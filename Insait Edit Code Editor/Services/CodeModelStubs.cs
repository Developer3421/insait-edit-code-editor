using System;
using System.Collections.Generic;
using Insait_Edit_C_Sharp.Controls;

namespace Insait_Edit_C_Sharp.Services;

/// <summary>
/// Kind of quick-fix suggestion. Kept as a minimal stub after Roslyn removal.
/// </summary>
public enum QuickFixKind
{
    AddUsing,
    InsertCode,
    RemoveCode,
    InstallNuGet,
    RoslynFix,
    GenerateType,
    GenerateMember,
    Other
}

/// <summary>
/// A single quick-fix action suggestion. Minimal stub — no Roslyn backing.
/// </summary>
public sealed class QuickFixSuggestion
{
    public string Title { get; set; } = string.Empty;
    public QuickFixKind Kind { get; set; } = QuickFixKind.Other;
    public string? InsertText { get; set; }
    public int InsertOffset { get; set; }
    public string? NamespaceName { get; set; }
    public string? NuGetPackage { get; set; }
    public string? RoslynAction { get; set; }
}

/// <summary>
/// A single file change produced by a rename operation.
/// </summary>
public sealed class RenameChange
{
    public string FilePath { get; set; } = string.Empty;
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public string NewText { get; set; } = string.Empty;
}

/// <summary>
/// Result of a rename-symbol operation.
/// </summary>
public sealed class RenameResult
{
    public string OldName { get; set; } = string.Empty;
    public string NewName { get; set; } = string.Empty;
    public List<RenameChange> Changes { get; set; } = new();
}

/// <summary>
/// Event args raised by the editor when inline diagnostics are updated.
/// </summary>
public sealed class InlineDiagnosticsUpdatedEventArgs : EventArgs
{
    public string FilePath { get; set; } = string.Empty;
    public IReadOnlyList<DiagnosticSpan> Diagnostics { get; set; } = Array.Empty<DiagnosticSpan>();
}