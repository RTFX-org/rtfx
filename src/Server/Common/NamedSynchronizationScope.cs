using System.Diagnostics;

namespace Rtfx.Server.Common;

public sealed class NamedSynchronizationScope : IDisposable
{
    private static readonly string _mutexNamePrefix;

    private readonly Mutex _mutex;

    static NamedSynchronizationScope()
    {
        var currentProcess = Process.GetCurrentProcess();
        _mutexNamePrefix = $"{currentProcess.ProcessName}:{currentProcess.Id}:";
    }

    private NamedSynchronizationScope(string name)
    {
        _mutex = new Mutex(false, $"{_mutexNamePrefix}{name}");
        _mutex.WaitOne();
    }

    public static NamedSynchronizationScope Enter(string name) => new(name);

    public void Dispose()
    {
        _mutex.ReleaseMutex();
        _mutex.Dispose();
    }
}
