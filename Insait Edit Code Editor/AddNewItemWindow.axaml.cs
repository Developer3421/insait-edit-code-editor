using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Insait_Edit_C_Sharp.Services;

namespace Insait_Edit_C_Sharp;

public partial class AddNewItemWindow : Window
{
    private readonly string _targetDirectory;

    public string? CreatedFilePath { get; private set; }

    public AddNewItemWindow(string targetDirectory, string? defaultNamespace = null)
    {
        InitializeComponent();

        _targetDirectory = targetDirectory;

        var locationText = this.FindControl<TextBlock>("LocationPreviewText");
        if (locationText != null)
        {
            locationText.Text = _targetDirectory;
        }

        UpdatePreview();
        ApplyLocalization();
    }

    private void ApplyLocalization()
    {
        var L = (Func<string, string>)LocalizationService.Get;
        var titleBar = this.FindControl<TextBlock>("TitleBarText");
        if (titleBar != null) titleBar.Text = L("AddItem.Title");
        var nameLabel = this.FindControl<TextBlock>("NameLabel");
        if (nameLabel != null) nameLabel.Text = L("AddItem.Name");
        var previewLabel = this.FindControl<TextBlock>("PreviewLabel");
        if (previewLabel != null) previewLabel.Text = L("AddItem.Preview");
        var locLabel = this.FindControl<TextBlock>("LocationLabel");
        if (locLabel != null) locLabel.Text = L("AddItem.Location");
        var cancelBtn = this.FindControl<Button>("CancelButton");
        if (cancelBtn != null) cancelBtn.Content = L("AddItem.Cancel");
        var createBtn = this.FindControl<Button>("CreateButton");
        if (createBtn != null) createBtn.Content = L("AddItem.Add");
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

    private void ItemName_Changed(object? sender, TextChangedEventArgs e)
    {
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        var nameBox = this.FindControl<TextBox>("ItemNameBox");
        var previewText = this.FindControl<TextBlock>("FilePreviewText");

        if (nameBox == null || previewText == null) return;

        var name = nameBox.Text?.Trim() ?? "NewFile.txt";
        previewText.Text = name;
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Create_Click(object? sender, RoutedEventArgs e)
    {
        var nameBox = this.FindControl<TextBox>("ItemNameBox");
        if (nameBox == null) return;

        var fileName = nameBox.Text?.Trim() ?? "NewFile.txt";
        if (string.IsNullOrWhiteSpace(fileName)) return;

        // Default to .txt if no extension is given
        if (Path.GetExtension(fileName).Length == 0)
        {
            fileName += ".txt";
        }

        var filePath = Path.Combine(_targetDirectory, fileName);

        try
        {
            File.WriteAllText(filePath, string.Empty);
            CreatedFilePath = filePath;
            Close(CreatedFilePath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating file: {ex.Message}");
        }
    }
}