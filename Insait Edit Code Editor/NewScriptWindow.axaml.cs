using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Insait_Edit_C_Sharp.Services;
using Insait_Edit_C_Sharp.SetStandard;

namespace Insait_Edit_C_Sharp;

public partial class NewScriptWindow : Window
{
    private readonly string _defaultDirectory;

    public string? CreatedFilePath { get; private set; }

    public NewScriptWindow(string? defaultDirectory = null)
    {
        InitializeComponent();

        _defaultDirectory = !string.IsNullOrEmpty(defaultDirectory) && Directory.Exists(defaultDirectory)
            ? defaultDirectory
            : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        var parentBox = this.FindControl<TextBox>("ParentPathBox");
        if (parentBox != null) parentBox.Text = _defaultDirectory;

        UpdatePreview();
        ApplyLocalization();
    }

    private void ApplyLocalization()
    {
        var L = (Func<string, string>)LocalizationService.Get;

        var titleBar = this.FindControl<TextBlock>("TitleBarText");
        if (titleBar != null) titleBar.Text = L("NewScript.Title");

        var parentLabel = this.FindControl<TextBlock>("ParentLabel");
        if (parentLabel != null) parentLabel.Text = L("NewScript.Parent");
        var fileNameLabel = this.FindControl<TextBlock>("FileNameLabel");
        if (fileNameLabel != null) fileNameLabel.Text = L("NewScript.FileName");
        var extensionLabel = this.FindControl<TextBlock>("ExtensionLabel");
        if (extensionLabel != null) extensionLabel.Text = L("NewScript.Extension");
        var previewLabel = this.FindControl<TextBlock>("PreviewLabel");
        if (previewLabel != null) previewLabel.Text = L("NewScript.Preview");

        var browseBtn = this.FindControl<Button>("BrowseButton");
        if (browseBtn != null) browseBtn.Content = L("NewScript.Browse");

        var cancelBtn = this.FindControl<Button>("CancelButton");
        if (cancelBtn != null) cancelBtn.Content = L("NewScript.Cancel");
        var createBtn = this.FindControl<Button>("CreateButton");
        if (createBtn != null) createBtn.Content = L("NewScript.Create");
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
            Title = LocalizationService.Get("NewScript.Parent"),
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            var parentBox = this.FindControl<TextBox>("ParentPathBox");
            if (parentBox != null)
            {
                parentBox.Text = folders[0].Path.LocalPath;
                UpdatePreview();
            }
        }
    }

    private void Fields_Changed(object? sender, TextChangedEventArgs e)
    {
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        var nameBox = this.FindControl<TextBox>("FileNameBox");
        var extBox = this.FindControl<TextBox>("ExtensionBox");
        var parentBox = this.FindControl<TextBox>("ParentPathBox");
        var previewText = this.FindControl<TextBlock>("FilePreviewText");

        if (previewText == null) return;

        var name = nameBox?.Text?.Trim() ?? string.Empty;
        var ext = NormalizeExtension(extBox?.Text?.Trim() ?? string.Empty);
        var parent = parentBox?.Text?.Trim() ?? string.Empty;

        var fileName = string.IsNullOrWhiteSpace(name) ? string.Empty : name + ext;
        previewText.Text = string.IsNullOrWhiteSpace(fileName)
            ? string.Empty
            : Path.Combine(parent, fileName);
    }

    private static string NormalizeExtension(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        value = value.Trim();
        if (!value.StartsWith(".")) value = "." + value;
        return value;
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Create_Click(object? sender, RoutedEventArgs e)
    {
        var errorText = this.FindControl<TextBlock>("ErrorText");
        var parentBox = this.FindControl<TextBox>("ParentPathBox");
        var nameBox = this.FindControl<TextBox>("FileNameBox");
        var extBox = this.FindControl<TextBox>("ExtensionBox");

        if (errorText != null) errorText.IsVisible = false;

        var name = nameBox?.Text?.Trim() ?? string.Empty;
        var extension = NormalizeExtension(extBox?.Text?.Trim() ?? string.Empty);
        var parent = parentBox?.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
        {
            ShowError(LocalizationService.Get("NewScript.ErrorEmptyName"));
            return;
        }

        if (string.IsNullOrWhiteSpace(extension))
        {
            ShowError(LocalizationService.Get("NewScript.ErrorEmptyExtension"));
            return;
        }

        if (string.IsNullOrWhiteSpace(parent) || !Directory.Exists(parent))
        {
            ShowError(LocalizationService.Get("NewScript.ErrorParentNotFound"));
            return;
        }

        // Validate that the extension is supported by the editor
        var supported = SupportedFileTypes.All.Any(f =>
            string.Equals(f.Extension, extension, StringComparison.OrdinalIgnoreCase));

        if (!supported)
        {
            ShowError(string.Format(LocalizationService.Get("NewScript.ErrorUnknownExtension"), extension));
            return;
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        if (name.IndexOfAny(invalidChars) >= 0)
        {
            ShowError(LocalizationService.Get("NewScript.ErrorInvalidName"));
            return;
        }

        var filePath = Path.Combine(parent, name + extension);

        if (File.Exists(filePath))
        {
            ShowError(LocalizationService.Get("NewScript.ErrorExists"));
            return;
        }

        try
        {
            File.WriteAllText(filePath, string.Empty);
            CreatedFilePath = filePath;
            Close(CreatedFilePath);
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