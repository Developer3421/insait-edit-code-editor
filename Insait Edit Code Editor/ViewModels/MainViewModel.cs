using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Threading;
using Insait_Edit_C_Sharp.Models;

namespace Insait_Edit_C_Sharp.ViewModels;

/// <summary>
/// Main view model for the IDE
/// </summary>
public class MainViewModel : INotifyPropertyChanged, IDisposable
{
    private Project? _currentProject;
    private EditorTab? _activeTab;
    private string _statusText = "Ready";
    private bool _isBuildInProgress;
    private string _searchQuery = string.Empty;
    private string? _currentProjectPath;
    private int _errorsCount;
    private int _warningsCount;
    private int _messagesCount;

    // File system watcher for automatic refresh
    private FileSystemWatcher? _fileWatcher;
    private Timer? _refreshDebounceTimer;
    private bool _refreshPending;
    private readonly object _refreshLock = new object();

    // Action to invoke UI refresh on dispatcher
    public Action? RefreshTreeAction { get; set; }

    public MainViewModel()
    {
        Tabs = new ObservableCollection<EditorTab>();
        RecentFiles = new ObservableCollection<string>();
        Problems = new ObservableCollection<DiagnosticItem>();
        OutputLines = new ObservableCollection<string>();
        FileTreeItems = new ObservableCollection<FileTreeItem>();

        // Subscribe to Problems collection changes
        Problems.CollectionChanged += (s, e) => UpdateProblemsCounts();
    }

    #region Properties

    public Project? CurrentProject
    {
        get => _currentProject;
        set => SetProperty(ref _currentProject, value);
    }

    public ObservableCollection<EditorTab> Tabs { get; }

    public EditorTab? ActiveTab
    {
        get => _activeTab;
        set => SetProperty(ref _activeTab, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public bool IsBuildInProgress
    {
        get => _isBuildInProgress;
        set => SetProperty(ref _isBuildInProgress, value);
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    public string? CurrentProjectPath
    {
        get => _currentProjectPath;
        set => SetProperty(ref _currentProjectPath, value);
    }

    public int ErrorsCount
    {
        get => _errorsCount;
        set => SetProperty(ref _errorsCount, value);
    }

    public int WarningsCount
    {
        get => _warningsCount;
        set => SetProperty(ref _warningsCount, value);
    }

    public int MessagesCount
    {
        get => _messagesCount;
        set => SetProperty(ref _messagesCount, value);
    }

    public int ProblemsCount => ErrorsCount + WarningsCount + MessagesCount;

    public bool HasProblems => ProblemsCount > 0;
    public bool HasErrors => ErrorsCount > 0;

    public ObservableCollection<string> RecentFiles { get; }
    public ObservableCollection<DiagnosticItem> Problems { get; }
    public ObservableCollection<string> OutputLines { get; }
    public ObservableCollection<FileTreeItem> FileTreeItems { get; }

    private int _suppressProblemUpdates;

    public void ReplaceProblems(IEnumerable<DiagnosticItem> items)
    {
        _suppressProblemUpdates++;
        Problems.Clear();
        foreach (var item in items)
            Problems.Add(item);
        _suppressProblemUpdates--;
        if (_suppressProblemUpdates == 0)
            UpdateProblemsCounts();
    }

    private void UpdateProblemsCounts()
    {
        if (_suppressProblemUpdates > 0) return;
        ErrorsCount = 0;
        WarningsCount = 0;
        MessagesCount = 0;
        foreach (var problem in Problems)
        {
            if (problem.Severity == DiagnosticSeverity.Error)
                ErrorsCount++;
            else if (problem.Severity == DiagnosticSeverity.Warning)
                WarningsCount++;
            else
                MessagesCount++;
        }
        OnPropertyChanged(nameof(ProblemsCount));
        OnPropertyChanged(nameof(HasProblems));
        OnPropertyChanged(nameof(HasErrors));
    }

    #endregion

    #region Methods

    /// <summary>
    /// Load a project folder into the file tree (fire and forget - for backward compatibility)
    /// </summary>
    public void LoadProjectFolder(string folderPath)
    {
        // Fire and forget - call async version
        _ = LoadProjectFolderAsync(folderPath);
    }

    /// <summary>
    /// Load a project folder into the file tree asynchronously
    /// </summary>
    public async Task LoadProjectFolderAsync(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            // Try to get directory from file path
            if (File.Exists(folderPath))
            {
                folderPath = Path.GetDirectoryName(folderPath) ?? folderPath;
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() => StatusText = $"Directory not found: {folderPath}");
                return;
            }
        }

        folderPath = Path.GetFullPath(folderPath);
        FileTreeItem.SetAllowedRootPaths(new[] { folderPath });

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            CurrentProjectPath = folderPath;
            FileTreeItems.Clear();
            StatusText = "Loading...";
        });

        var rootItem = await Task.Run(() => FileTreeItem.FromDirectory(folderPath, loadChildren: true));
        rootItem.IsExpanded = true;
        await Dispatcher.UIThread.InvokeAsync(() => FileTreeItems.Add(rootItem));

        // Initialize file watcher
        InitializeFileWatcher(folderPath);

        var folderName = Path.GetFileName(folderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        await Dispatcher.UIThread.InvokeAsync(() => StatusText = $"Opened folder: {folderName}");
    }

    /// <summary>
    /// Initialize file system watcher for automatic tree refresh
    /// </summary>
    private void InitializeFileWatcher(string folderPath)
    {
        // Dispose existing watcher
        _fileWatcher?.Dispose();
        _refreshDebounceTimer?.Dispose();

        // File watcher is disabled by default to prevent automatic reloads
        // User can manually reload the project if needed
        System.Diagnostics.Debug.WriteLine("File watcher is disabled. Project will not reload automatically.");
    }

    public void StopFileWatcher()
    {
        try
        {
            if (_fileWatcher != null)
            {
                _fileWatcher.EnableRaisingEvents = false;
                _fileWatcher.Dispose();
                _fileWatcher = null;
            }

            if (_refreshDebounceTimer != null)
            {
                _refreshDebounceTimer.Stop();
                _refreshDebounceTimer.Dispose();
                _refreshDebounceTimer = null;
            }

            lock (_refreshLock)
            {
                _refreshPending = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error stopping file watcher: {ex.Message}");
        }
    }

    private void OnFileSystemChanged(object sender, FileSystemEventArgs e)
    {
        // Skip changes in excluded directories
        if (ShouldIgnorePath(e.FullPath)) return;

        RequestDebouncedRefresh();
    }

    private void OnFileSystemRenamed(object sender, RenamedEventArgs e)
    {
        // Skip changes in excluded directories
        if (ShouldIgnorePath(e.FullPath) && ShouldIgnorePath(e.OldFullPath)) return;

        RequestDebouncedRefresh();
    }

    private bool ShouldIgnorePath(string path)
    {
        var lowerPath = path.ToLowerInvariant();
        return lowerPath.Contains("\\bin\\") || lowerPath.EndsWith("\\bin") ||
               lowerPath.Contains("\\obj\\") || lowerPath.EndsWith("\\obj") ||
               lowerPath.Contains("\\.git\\") || lowerPath.EndsWith("\\.git") ||
               lowerPath.Contains("\\.vs\\")  || lowerPath.EndsWith("\\.vs")  ||
               lowerPath.Contains("\\.idea\\") || lowerPath.EndsWith("\\.idea") ||
               lowerPath.Contains("\\node_modules\\") || lowerPath.EndsWith("\\node_modules");
    }

    private void RequestDebouncedRefresh()
    {
        lock (_refreshLock)
        {
            _refreshPending = true;
            _refreshDebounceTimer?.Stop();
            _refreshDebounceTimer?.Start();
        }
    }

    private void OnRefreshDebounceTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        lock (_refreshLock)
        {
            if (_refreshPending)
            {
                _refreshPending = false;
                // Invoke refresh on UI thread
                RefreshTreeAction?.Invoke();
            }
        }
    }

    /// <summary>
    /// Refresh the file tree, preserving expanded state
    /// </summary>
    public void RefreshFileTree()
    {
        // Fire and forget async version
        _ = RefreshFileTreeAsync();
    }

    /// <summary>
    /// Refresh the file tree asynchronously, preserving expanded state
    /// </summary>
    public async Task RefreshFileTreeAsync()
    {
        if (string.IsNullOrEmpty(CurrentProjectPath) || !Directory.Exists(CurrentProjectPath))
        {
            FileTreeItem.SetAllowedRootPaths(null);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                FileTreeItems.Clear();
                StatusText = "No project folder to refresh";
            });
            return;
        }

        // Save expanded state
        var expandedPaths = new HashSet<string>();
        await Dispatcher.UIThread.InvokeAsync(() => CollectExpandedPaths(FileTreeItems, expandedPaths));

        // Reload the tree using solution-aware logic
        await LoadProjectFolderAsync(CurrentProjectPath);

        // Restore expanded state
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            RestoreExpandedPaths(FileTreeItems, expandedPaths);
            StatusText = "File tree refreshed";
        });
    }

    private void CollectExpandedPaths(ObservableCollection<FileTreeItem> items, HashSet<string> expandedPaths)
    {
        foreach (var item in items)
        {
            if (item.IsExpanded && item.IsDirectory)
            {
                expandedPaths.Add(item.FullPath);
                CollectExpandedPaths(item.Children, expandedPaths);
            }
        }
    }

    private void RestoreExpandedPaths(ObservableCollection<FileTreeItem> items, HashSet<string> expandedPaths)
    {
        foreach (var item in items)
        {
            if (item.IsDirectory && expandedPaths.Contains(item.FullPath))
            {
                // Load children first if not already loaded
                item.LoadChildren();
                item.IsExpanded = true;
                RestoreExpandedPaths(item.Children, expandedPaths);
            }
        }
    }

    /// <summary>
    /// Load a solution or project file
    /// </summary>
    public void LoadSolutionOrProject(string filePath)
    {
        if (!File.Exists(filePath)) return;

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            // Fire and forget async version
            _ = LoadProjectFolderAsync(directory);
        }

        StatusText = $"Loaded: {Path.GetFileName(filePath)}";
    }

    public void OpenFile(string filePath)
    {
        // Check if file is already open
        var existingTab = FindTabByPath(filePath);
        if (existingTab != null)
        {
            ActiveTab = existingTab;
            return;
        }

        // Create new tab
        var tab = new EditorTab
        {
            FileName = System.IO.Path.GetFileName(filePath),
            FilePath = filePath,
            Language = EditorTab.GetLanguageFromExtension(filePath),
        };

        // Read file content
        if (System.IO.File.Exists(filePath))
        {
            tab.Content = System.IO.File.ReadAllText(filePath);
        }

        Tabs.Add(tab);
        ActiveTab = tab;

        // Update recent files
        if (!RecentFiles.Contains(filePath))
        {
            RecentFiles.Insert(0, filePath);
            if (RecentFiles.Count > 10)
            {
                RecentFiles.RemoveAt(RecentFiles.Count - 1);
            }
        }

        StatusText = $"Opened: {tab.FileName}";
    }

    public void CloseTab(EditorTab tab)
    {
        var index = Tabs.IndexOf(tab);
        Tabs.Remove(tab);

        // Remove diagnostics for the closed file from the Problems panel
        if (!string.IsNullOrEmpty(tab.FilePath))
        {
            var toRemove = Problems
                .Where(p => string.Equals(p.FilePath, tab.FilePath, StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (var r in toRemove)
                Problems.Remove(r);
        }

        if (ActiveTab == tab && Tabs.Count > 0)
        {
            ActiveTab = Tabs[System.Math.Max(0, index - 1)];
        }
        else if (Tabs.Count == 0)
        {
            ActiveTab = null;
        }
    }

    public void SaveActiveTab()
    {
        if (ActiveTab == null || string.IsNullOrEmpty(ActiveTab.FilePath))
            return;

        System.IO.File.WriteAllText(ActiveTab.FilePath, ActiveTab.Content);
        ActiveTab.IsDirty = false;
        StatusText = $"Saved: {ActiveTab.FileName}";
    }

    public void SaveAllTabs()
    {
        foreach (var tab in Tabs)
        {
            if (tab.IsDirty && !string.IsNullOrEmpty(tab.FilePath))
            {
                System.IO.File.WriteAllText(tab.FilePath, tab.Content);
                tab.IsDirty = false;
            }
        }
        StatusText = "All files saved";
    }

    public EditorTab? FindTabByPath(string filePath)
    {
        foreach (var tab in Tabs)
        {
            if (tab.FilePath.Equals(filePath, System.StringComparison.OrdinalIgnoreCase))
            {
                return tab;
            }
        }
        return null;
    }

    public void AddOutput(string message)
    {
        OutputLines.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
    }

    public void ClearOutput()
    {
        OutputLines.Clear();
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion

    #region IDisposable

    private bool _disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _fileWatcher?.Dispose();
            _refreshDebounceTimer?.Dispose();
        }

        _disposed = true;
    }

    #endregion
}

/// <summary>
/// Represents a diagnostic/problem item
/// </summary>
public class DiagnosticItem
{
    public DiagnosticSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int Line { get; set; }
    public int Column { get; set; }
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Icon for severity
    /// </summary>
    public string SeverityIcon => Severity switch
    {
        DiagnosticSeverity.Error => "â›”",
        DiagnosticSeverity.Warning => "âš ",
        DiagnosticSeverity.Info => "â„¹",
        _ => "ðŸ’¡"
    };

    /// <summary>
    /// Color for severity
    /// </summary>
    public string SeverityColor => Severity switch
    {
        DiagnosticSeverity.Error => "#FFF38BA8",
        DiagnosticSeverity.Warning => "#FFF5A623",
        DiagnosticSeverity.Info => "#FF89B4FA",
        _ => "#FFA6E3A1"
    };

    /// <summary>
    /// Semi-transparent background color for severity badge
    /// </summary>
    public string SeverityBgColor => Severity switch
    {
        DiagnosticSeverity.Error => "#22F38BA8",
        DiagnosticSeverity.Warning => "#22F5A623",
        DiagnosticSeverity.Info => "#2289B4FA",
        _ => "#22A6E3A1"
    };

    /// <summary>
    /// Border color for severity left bar
    /// </summary>
    public string SeverityBarColor => Severity switch
    {
        DiagnosticSeverity.Error => "#CFF38BA8",
        DiagnosticSeverity.Warning => "#CFF5A623",
        DiagnosticSeverity.Info => "#CF89B4FA",
        _ => "#CFA6E3A1"
    };

    /// <summary>
    /// Location display string
    /// </summary>
    public string Location => $"{FileName}({Line},{Column})";
}

public enum DiagnosticSeverity
{
    Error,
    Warning,
    Info,
    Hint
}
