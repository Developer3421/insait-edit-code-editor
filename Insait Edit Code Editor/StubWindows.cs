using System;
using Avalonia.Controls;

namespace Insait_Edit_C_Sharp;

/// <summary>
/// Stub dialogs retained for compilation after C#/Roslyn/publish features were
/// removed. All no-ops — return a cancellation/none result.
/// </summary>

public class PublishWindow : Window
{
    public string? Result { get; }
    public PublishWindow(string projectPath)
    {
        Title = "Publish";
        Width = 480; Height = 320;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }
}

public class PublishProgressWindow : Window
{
    public PublishProgressWindow(object _publishService, object? profile)
    {
        Title = "Publish Progress";
        Width = 480; Height = 200;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    public void StartPublish() { }
}

public class MsixManagerWindow : Window
{
    public MsixManagerWindow(string? _)
    {
        Title = "MSIX Manager";
        Width = 520; Height = 400;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }
}

public class NewProjectWindow : Window
{
    public NewProjectWindow(string? _ = null)
    {
        Title = "New Project";
        Width = 520; Height = 400;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }
}

public class NewSolutionWindow : Window
{
    public NewSolutionWindow()
    {
        Title = "New Solution";
        Width = 520; Height = 400;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }
}

public class AddProjectToSolutionWindow : Window
{
    public AddProjectToSolutionWindow(string? _ = null)
    {
        Title = "Add Project";
        Width = 520; Height = 400;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }
}

public class ProjectPropertiesWindow : Window
{
    public ProjectPropertiesWindow(string? _ = null)
    {
        Title = "Project Properties";
        Width = 520; Height = 400;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }
}

public class SolutionPropertiesWindow : Window
{
    public SolutionPropertiesWindow(string? _ = null)
    {
        Title = "Solution Properties";
        Width = 520; Height = 400;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }
}