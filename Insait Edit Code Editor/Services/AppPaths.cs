using System;
using System.IO;

namespace Insait_Edit_C_Sharp.Services;

/// <summary>
/// Central definition of the application's data and log directories.
/// All user data lives in %AppData%\Insait Edit Code Editor and all
/// local/log data lives in %LocalAppData%\Insait Edit Code Editor, so the
/// app's file system is kept separate from the rest of the system.
/// </summary>
public static class AppPaths
{
    /// <summary>Display/company name used as the AppData folder name.</summary>
    public const string FolderName = "Insait Edit Code Editor";

    /// <summary>
    /// Roaming application data directory for databases and settings
    /// (e.g. %AppData%\Insait Edit Code Editor).
    /// </summary>
    public static string DataDir { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        FolderName);

    /// <summary>
    /// Local application data directory for logs and cached data
    /// (e.g. %LocalAppData%\Insait Edit Code Editor).
    /// </summary>
    public static string LocalDataDir { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        FolderName);

    /// <summary>Directory where the app writes its log files.</summary>
    public static string LogsDir { get; } = Path.Combine(LocalDataDir, "Logs");

    static AppPaths()
    {
        try
        {
            Directory.CreateDirectory(DataDir);
            Directory.CreateDirectory(LocalDataDir);
            Directory.CreateDirectory(LogsDir);
        }
        catch
        {
        }
    }
}