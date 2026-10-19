using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Insait_Edit_C_Sharp.Services;

namespace Insait_Edit_C_Sharp;

public partial class NewFolderWindow : Window
{
    private readonly string _defaultParentDirectory;

    public string? CreatedFolderPath { get; private set; }

    public NewFolderWindow(string? defaultParentDirectory = null)
    {
        InitializeComponent();

        _defaultParentDirectory = !string.IsNullOrEmpty(defaultParentDirectory) && Directory.Exists(defaultParentDirectory)
            ? defaultParentDirectory
            : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        var parentBox = this.FindControl<TextBox>("ParentPathBox");
        if (parentBox != null) parentBox.Text = _defaultParentDirectory;

        UpdatePreview();
        ApplyLocalization();
    }

    private void ApplyLocalization()
    {
        var L = (Func<string, string>)LocalizationService.Get;

        var titleBar = this.FindControl<TextBlock>("TitleBarText");
        if (titleBar != null) titleBar.Text = L("NewFolder.Title");

        var parentLabel = this.FindControl<TextBlock>("ParentLabel");
        if (parentLabel != null) parentLabel.Text = L("NewFolder.Parent");
        var folderNameLabel = this.FindControl<TextBlock>("FolderNameLabel");
        if (folderNameLabel != null) folderNameLabel.Text = L("NewFolder.Name");
        var previewLabel = this.FindControl<TextBlock>("PreviewLabel");
        if (previewLabel != null) previewLabel.Text = L("NewFolder.Preview");

        var browseBtn = this.FindControl<Button>("BrowseButton");
        if (browseBtn != null) browseBtn.Content = L("NewFolder.Browse");

        var cancelBtn = this.FindControl<Button>("CancelButton");
        if (cancelBtn != null) cancelBtn.Content = L("NewFolder.Cancel");
        var createBtn = this.FindControl<Button>("CreateButton");
        if (createBtn != null) createBtn.Content = L("NewFolder.Create");
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void BrowseParent_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel == null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = LocalizationService.Get("NewFolder.Parent"),
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            var parentBox = this.FindControl<TextBox>("ParentPathBox");
            if (parentBox != null)
            {
                parentBox.Text = folders[0].Path.LocalPath;
            }
        }
    }

    private void FolderName_Changed(object? sender, TextChangedEventArgs e)
    {
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        var nameBox = this.FindControl<TextBox>("FolderNameBox");
        var parentBox = this.FindControl<TextBox>("ParentPathBox");
        var previewText = this.FindControl<TextBlock>("FolderPreviewText");

        if (previewText == null) return;

        var name = nameBox?.Text?.Trim() ?? string.Empty;
        var parent = parentBox?.Text?.Trim() ?? string.Empty;

        previewText.Text = string.IsNullOrWhiteSpace(name)
            ? string.Empty
            : Path.Combine(parent, name);
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Create_Click(object? sender, RoutedEventArgs e)
    {
        var errorText = this.FindControl<TextBlock>("ErrorText");
        var parentBox = this.FindControl<TextBox>("ParentPathBox");
        var nameBox = this.FindControl<TextBox>("FolderNameBox");

        if (errorText != null) errorText.IsVisible = false;

        var name = nameBox?.Text?.Trim() ?? string.Empty;
        var parent = parentBox?.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
        {
            ShowError(LocalizationService.Get("NewFolder.ErrorEmptyName"));
            return;
        }

        if (string.IsNullOrWhiteSpace(parent) || !Directory.Exists(parent))
        {
            ShowError(LocalizationService.Get("NewFolder.ErrorParentNotFound"));
            return;
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        if (name.IndexOfAny(invalidChars) >= 0)
        {
            ShowError(LocalizationService.Get("NewFolder.ErrorInvalidName"));
            return;
        }

        var folderPath = Path.Combine(parent, name);

        if (Directory.Exists(folderPath))
        {
            ShowError(LocalizationService.Get("NewFolder.ErrorExists"));
            return;
        }

        try
        {
            Directory.CreateDirectory(folderPath);
            CreatedFolderPath = folderPath;
            Close(CreatedFolderPath);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void ShowError(string message)
    {
        var errorText = this.FindControl<TextBlock>("ErrorText");
        if (errorText == null) return;
        errorText.Text = message;
        errorText.IsVisible = true;
    }
}
