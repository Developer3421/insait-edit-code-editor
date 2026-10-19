using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Insait_Edit_C_Sharp.Controls;
using Insait_Edit_C_Sharp.Services;

namespace Insait_Edit_C_Sharp.InsaitCodeEditor;

/// <summary>
/// Insait Edit Code Editor — plain-text editor (Avalonia TextBox).
/// Roslyn / IntelliSense removed; opens and edits any file.
/// </summary>
public partial class InsaitEditor : UserControl
{
    private TextBox _textBox = null!;
    private TextBlock _lineNumbers = null!;
    private ScrollViewer? _textBoxScroller;
    private bool _scrollerSynced;

    private string _currentFilePath = "untitled.cs";
    private bool   _isDirty;
    private bool   _suppressChangeEvents;
    private List<DiagnosticSpan> _diagnosticSpans = new();

    // ── Public events (kept for MainWindow wiring) ─────────────────────
    public event EventHandler?                                 EditorReady;
    public event EventHandler?                                 ContentChanged;
    public event EventHandler<ContentChangedEventArgs>?        ContentChangedWithValue;
    public event EventHandler<CursorPositionChangedEventArgs>? CursorPositionChanged;
    public event EventHandler<NuGetInstallRequestedEventArgs>? NuGetInstallRequested;
    public event EventHandler<GoToDefinitionRequestedEventArgs>? GoToDefinitionRequested;
    public event EventHandler<RenameCompletedEventArgs>?       RenameCompleted;
    public event EventHandler<InlineDiagnosticsUpdatedEventArgs>? DiagnosticsUpdated;

    // ── Public properties ──────────────────────────────────────────────
    public bool   IsDirty        => _isDirty;
    public bool   CanUndo        => _textBox?.CanUndo ?? false;
    public bool   CanRedo        => _textBox?.CanRedo ?? false;
    public string CurrentContent => _textBox.Text ?? string.Empty;
    public UndoRedoManager UndoRedoManager { get; } = new();
    public IReadOnlyList<DiagnosticSpan> Diagnostics => _diagnosticSpans;
    public (int line, int col) CursorPosition => CurrentCursorPosition();

    public InsaitEditor()
    {
        InitializeComponent();
        _textBox = this.FindControl<TextBox>("TextArea") ?? throw new InvalidOperationException("TextArea not found");
        _lineNumbers = this.FindControl<TextBlock>("LineNumbersText") ?? throw new InvalidOperationException("LineNumbersText not found");

        _textBox.TextChanged += (_, _) =>
        {
            if (_suppressChangeEvents) return;
            _isDirty = true;
            UpdateLineNumbers();
            ContentChanged?.Invoke(this, EventArgs.Empty);
            ContentChangedWithValue?.Invoke(this, new ContentChangedEventArgs(_textBox.Text ?? string.Empty));
            UndoRedoManager.RaiseStateChangedForEditor();
        };
        _textBox.PropertyChanged += (_, e) =>
        {
            if (e.Property == Avalonia.Controls.TextBox.CaretIndexProperty)
            {
                var (l, c) = CurrentCursorPosition();
                CursorPositionChanged?.Invoke(this, new CursorPositionChangedEventArgs(l, c));
            }
        };

        Loaded += (_, _) => SyncScroller();
        DetachedFromVisualTree += (_, _) =>
        {
            if (_textBoxScroller != null)
            {
                _textBoxScroller.ScrollChanged -= OnScroller_ScrollChanged;
                _textBoxScroller = null;
            }
        };

        Dispatcher.UIThread.Post(() => EditorReady?.Invoke(this, EventArgs.Empty), DispatcherPriority.Loaded);
    }

    /// <summary>Locate the TextBox's internal ScrollViewer so the line gutter follows scrolling.</summary>
    private void SyncScroller()
    {
        if (_scrollerSynced) return;
        _scrollerSynced = true;

        _textBoxScroller = _textBox.FindDescendantOfType<ScrollViewer>();
        if (_textBoxScroller != null)
        {
            _textBoxScroller.ScrollChanged += OnScroller_ScrollChanged;
        }

        UpdateLineNumbers();
    }

    private void OnScroller_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        UpdateLineNumbers();
    }

    private void UpdateLineNumbers()
    {
        var text = _textBox.Text ?? string.Empty;
        int lineCount = text.Length == 0 ? 1 : text.Count(c => c == '\n') + 1;

        var sb = new StringBuilder(lineCount * 2);
        for (int i = 1; i <= lineCount; i++)
        {
            sb.Append(i);
            if (i < lineCount) sb.Append('\n');
        }

        _lineNumbers.Text = sb.ToString();

        // Grow/shrink gutter width to fit the largest number
        int digits = lineCount.ToString().Length;
        int width = 20 + digits * 10;
        var border = _lineNumbers.Parent as Border;
        if (border != null) border.Width = width;

        // Keep the numbers vertically in sync with the editor's scroll offset.
        // The gutter itself lives outside the TextBox scroller, so mirror the offset.
        if (_textBoxScroller != null)
        {
            double y = _textBoxScroller.Offset.Y;
            _lineNumbers.Margin = new Thickness(0, -y, 0, 0);
        }
    }

    private (int line, int col) CurrentCursorPosition()
    {
        var text = _textBox.Text ?? string.Empty;
        int idx = Math.Clamp(_textBox.CaretIndex, 0, text.Length);
        int line = 1, col = 1;
        for (int i = 0; i < idx; i++)
        {
            if (text[i] == '\n') { line++; col = 1; }
            else col++;
        }
        return (line, col);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  Public API
    // ═══════════════════════════════════════════════════════════════════
    public void SetContent(string content, string language = "plaintext")
    {
        _suppressChangeEvents = true;
        try
        {
            _textBox.Text = content ?? string.Empty;
            _isDirty = false;
            UndoRedoManager.Clear();
            UpdateLineNumbers();
        }
        finally
        {
            // Keep suppression active until the pending layout pass so any
            // deferred TextChanged raised from the programmatic set is ignored.
            Dispatcher.UIThread.Post(() => _suppressChangeEvents = false, DispatcherPriority.Loaded);
        }
    }

    public void SetFilePath(string path)
    {
        _currentFilePath = path;
    }

    public Task<string> GetContentAsync() => Task.FromResult(_textBox.Text ?? string.Empty);
    public void MarkAsSaved()           => _isDirty = false;
    public void SetReadOnly(bool v)     => _textBox.IsReadOnly = v;
    public void SetFontSize(int sz)     => _textBox.FontSize = sz;
    public void FocusEditor()           => _textBox.Focus();
public void Undo()                  { _textBox.Undo(); UpdateUndoState(); }
    public void Redo()                  { _textBox.Redo(); UpdateUndoState(); }
    public void Find()                    { }
    public void Replace()               { }
    public void FormatDocument()        { }
    public void SetLanguage(string _)   { }

    private void UpdateUndoState()
    {
        UndoRedoManager.RaiseStateChangedForEditor();
    }
    public void GoToLine(int line, int col = 1)
    {
        if (line <= 1 && col <= 1) { _textBox.CaretIndex = 0; return; }
        var text = _textBox.Text ?? string.Empty;
        int idx = 0, currentLine = 1;
        int target = Math.Max(1, line);
        while (idx < text.Length && currentLine < target)
        {
            if (text[idx] == '\n') currentLine++;
            idx++;
        }
        int colIdx = Math.Max(1, col) - 1;
        idx = Math.Min(idx + colIdx, text.Length);
        _textBox.CaretIndex = idx;
        _textBox.Focus();
    }
    public void TriggerCompletion()     { }
    public void LoadProjectReferences(string path) { }

    public (bool HasSelection, int CharCount, int LineCount) GetSelectionInfo()
    {
        string sel = _textBox.SelectedText ?? string.Empty;
        int lines = sel.Length == 0 ? 0 : sel.Count(c => c == '\n') + 1;
        return (sel.Length > 0, sel.Length, lines);
    }

    public void SetProjectContext(string? projectDir) { }

    public void ApplyExternalDiagnostics(IEnumerable<DiagnosticSpan> spans)
    {
        _diagnosticSpans = spans?.ToList() ?? new List<DiagnosticSpan>();
        DiagnosticsUpdated?.Invoke(this, new InlineDiagnosticsUpdatedEventArgs
        {
            FilePath = _currentFilePath,
            Diagnostics = _diagnosticSpans,
        });
    }

    public void ClearDiagnostics()
    {
        var had = _diagnosticSpans.Count > 0;
        _diagnosticSpans.Clear();
        if (had)
        {
            DiagnosticsUpdated?.Invoke(this, new InlineDiagnosticsUpdatedEventArgs
            {
                FilePath = _currentFilePath,
                Diagnostics = _diagnosticSpans,
            });
        }
    }

    public void OpenAutoFixWindow() { }
}

// ── Event args ──────────────────────────────────────────────────────────

public sealed class NuGetInstallRequestedEventArgs : EventArgs
{
    public string PackageName { get; }
    public NuGetInstallRequestedEventArgs(string packageName) => PackageName = packageName;
}

public sealed class GoToDefinitionRequestedEventArgs : EventArgs
{
    public string FilePath { get; }
    public int Line { get; }
    public int Column { get; }
    public GoToDefinitionRequestedEventArgs(string filePath, int line, int column)
    { FilePath = filePath; Line = line; Column = column; }
}

public sealed class RenameCompletedEventArgs : EventArgs
{
    public RenameResult Result { get; }
    public RenameCompletedEventArgs(RenameResult result) => Result = result;
}