namespace Rtfx.Server;

public static class Globals
{
    static Globals()
    {
        var processPath = Environment.ProcessPath;
        var dir = Path.GetDirectoryName(processPath);
        if (dir is null || Path.GetFileName(processPath)?.Contains("dotnet") == true)
            dir = Environment.CurrentDirectory;

        DataPath = dir;
    }

    public static string DataPath { get; }
}
