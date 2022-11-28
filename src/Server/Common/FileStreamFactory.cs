namespace Rtfx.Server.Common;

public static class FileStreamFactory
{
    private static List<FileLock> _locks = new();
    private static object _locksLock = new();

    private delegate void FileLockChangedEventHandler(string filePath, FileAccess fileAccess, FileShare fileShare);
    private static event FileLockChangedEventHandler? FileLockChanged;

    public interface IFileLock : IDisposable
    {
        string FilePath { get; }
        FileAccess FileAccess { get; }
        FileShare FileShare { get; }
    }

    public static async Task<FileStream> CreateAsync(string filePath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare, CancellationToken cancellationToken)
    {
        var fileLock = await ObtainFileLock(filePath, fileAccess, fileShare, cancellationToken);

        try
        {
            return new WrappedFileStream(filePath, fileMode, fileAccess, fileShare, (FileLock)fileLock);
        }
        catch
        {
            fileLock!.Dispose();
            throw;
        }
    }

    public static FileStream Create(IFileLock fileLock, FileMode fileMode)
    {
        return new FileStream(fileLock.FilePath, fileMode, fileLock.FileAccess, fileLock.FileShare);
    }

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created", Justification = "The FileLock is disposed when FileStream is closed.")]
    public static async Task<IFileLock> ObtainFileLock(string filePath, FileAccess fileAccess, FileShare fileShare, CancellationToken cancellationToken)
    {
        FileLock? fileLock = null;
        (FileAccess Access, FileShare Share) currentState;

        lock (_locksLock)
        {
            currentState = GetCurrentLockState(filePath);
            if (CanUse(currentState.Access, currentState.Share, fileAccess, fileShare))
                fileLock = CreateFileLock(filePath, fileAccess, fileShare, ref currentState);
        }

        if (fileLock is null)
            (fileLock, currentState.Access, currentState.Share) = await WaitAndCreateFileLock(filePath, fileAccess, fileShare, cancellationToken);

        FileLockChanged?.Invoke(filePath, currentState.Access, currentState.Share);
        return fileLock!;
    }

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created", Justification = "The FileLock is disposed when FileStream is closed.")]
    private static async Task<(FileLock Lock, FileAccess Access, FileShare Share)> WaitAndCreateFileLock(string filePath, FileAccess fileAccess, FileShare fileShare, CancellationToken cancellationToken)
    {
        FileLock? fileLock = null;
        (FileAccess Access, FileShare Share) currentState = (0, FileShare.ReadWrite);

        var tcs = new TaskCompletionSource();
        cancellationToken.Register(() => tcs.TrySetCanceled());

        void OnFileLockChanged(string stateFilePath, FileAccess accesState, FileShare shareState)
        {
            if (stateFilePath != filePath || !CanUse(accesState, shareState, fileAccess, fileShare))
                return;

            lock (_locksLock)
            {
                if (fileLock is not null)
                    return;

                currentState = GetCurrentLockState(filePath);
                if (!CanUse(currentState.Access, currentState.Share, fileAccess, fileShare))
                    return;
                fileLock = CreateFileLock(filePath, fileAccess, fileShare, ref currentState);
                tcs!.TrySetResult();
            }
        }

        FileLockChanged += OnFileLockChanged;

        try
        {
            await tcs.Task;
        }
        catch (OperationCanceledException)
        {
            // Make sure to remove potentially created file lock when task is cancelled
            if (fileLock is not null)
            {
                lock (_locksLock)
                {
                    if (_locks.Contains(fileLock))
                        _locks.Remove(fileLock);
                }
            }

            throw;
        }
        finally
        {
            FileLockChanged -= OnFileLockChanged;
        }

        return (fileLock!, currentState.Access, currentState.Share);
    }

    private static FileLock CreateFileLock(string filePath, FileAccess fileAccess, FileShare fileShare, ref (FileAccess Access, FileShare Share) currentState)
    {
        var fileLock = new FileLock(filePath, fileAccess, fileShare);
        _locks.Add(fileLock);
        currentState.Share &= fileShare;
        currentState.Access |= fileAccess;
        return fileLock;
    }

    private static (FileAccess Access, FileShare Share) GetCurrentLockState(string filePath)
    {
        var access = (FileAccess)0;
        var share = FileShare.ReadWrite;

        lock (_locksLock)
        {
            foreach (var @lock in _locks)
            {
                if (@lock.FilePath != filePath)
                    continue;

                access |= @lock.FileAccess;
                share &= @lock.FileShare;
            }
        }

        return (access, share);
    }

    private static bool CanUse(FileAccess accessSate, FileShare shareState, FileAccess fileAccess, FileShare fileShare)
    {
        return ((int)shareState & (int)fileAccess) == (int)fileAccess
            && ((int)accessSate | (int)fileShare) == (int)fileShare;
    }

    private sealed class FileLock : IFileLock
    {
        private bool _isDisposed;

        public FileLock(string filePath, FileAccess fileAccess, FileShare fileShare)
        {
            FilePath = filePath;
            FileAccess = fileAccess;
            FileShare = fileShare;
        }

        public string FilePath { get; }
        public FileAccess FileAccess { get; }
        public FileShare FileShare { get; }

        public void Dispose()
        {
            if (_isDisposed)
                return;
            _isDisposed = true;

            (FileAccess Access, FileShare Share) newState;
            lock (_locksLock)
            {
                _locks.Remove(this);
                newState = GetCurrentLockState(FilePath);
            }

            FileLockChanged?.Invoke(FilePath, newState.Access, newState.Share);
        }
    }

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don't dispose injected", Justification = "File lock should be disposed if FileStream is closed.")]
    private sealed class WrappedFileStream : FileStream
    {
        private readonly FileLock _fileLock;

        public WrappedFileStream(string path, FileMode mode, FileAccess access, FileShare share, FileLock fileLock)
            : base(path, mode, access, share)
        {
            _fileLock = fileLock;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                base.Dispose(disposing);
            }
            finally
            {
                if (disposing)
                    _fileLock.Dispose();
            }
        }

        public override async ValueTask DisposeAsync()
        {
            try
            {
                await base.DisposeAsync();
            }
            finally
            {
                _fileLock.Dispose();
            }
        }
    }
}
