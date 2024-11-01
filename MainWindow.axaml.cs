using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace CynVee_Code_Editor;

public partial class MainWindow : Window
{
    public class UserSettings
    {
        public string? ThemeSetting { get; set; }
    }
    public MainWindow()
    {
        InitializeComponent();

        var settingsFilePath = Directory.GetCurrentDirectory() + "\\CynVee-Code-Editor-Settings.json";
        
        if (File.Exists(settingsFilePath))
        {
            Console.WriteLine("Settings file found");
            Console.WriteLine(settingsFilePath);
            
        }
        else
        {
            Console.WriteLine("No settings file found, creating a new one...");
            using FileStream fileStream = File.Open(settingsFilePath, FileMode.Append);
            using StreamWriter file = new StreamWriter(fileStream);
            file.Close();
            var themeSetting = new UserSettings
            {
                ThemeSetting = "Default"
            };
            var jsonString = JsonSerializer.Serialize(themeSetting);
            var writer = new StreamWriter(_settingsFile);
            writer.Write(jsonString);
            writer.Close();
        }
        
        RefreshThemeSetting();
        
        Editor.Options.AllowToggleOverstrikeMode = true;
        Editor.Options.HighlightCurrentLine = true;
        Editor.TextArea.Caret.PositionChanged += EditorCaret_PositionChanged;
    }
    
    
    private readonly string _settingsFile = Directory.GetCurrentDirectory() + "\\CynVee-Code-Editor-Settings.json";
    private string _filePath = string.Empty;
    private string _folderPath = string.Empty;
    
    
    // MenuBar functions
    // "File"
    private async void SaveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_filePath != string.Empty)
        {
            var writer = new StreamWriter(_filePath);
            await writer.WriteAsync(Editor.Text);
            writer.Close();
            Console.WriteLine("File saved");
        }
        else
        {
            Console.WriteLine("No file selected");
        }
    }
    private async void SaveAsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save File"
        });

        if (file is not null)
        {
            await using var stream = await file.OpenWriteAsync();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(Editor.Text);
            _filePath = file.Path.LocalPath;
            RefreshFileInformation();
        }
        else
        {
            Console.WriteLine("No file created");
        }
    }
    private void ExitFileFolder_OnClick(object? sender, RoutedEventArgs e)
    {
        _filePath = string.Empty;
        _folderPath = string.Empty;
        
        Editor.Clear();
        FileList.Items.Clear();
        
        FolderPathBlock.Text = "Currently Selected Folder: ";
        FilePathBlock.Text = "Currently Selected File: ";
    }
    private void Exit(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
    // "Edit"
    private void UndoButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Editor.Undo();
    }
    private void RedoButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Editor.Redo();
    }
    private void SelectAll_OnClick(object? sender, RoutedEventArgs e)
    {
        Editor.SelectAll();
    }
    private void Cut(object? sender, RoutedEventArgs e)
    {
        Editor.Cut();
    }
    private void Copy(object? sender, RoutedEventArgs e)
    {
        Editor.Copy();
    }
    private void Paste(object? sender, RoutedEventArgs e)
    {
        Editor.Paste();
    }
    private void Clear(object? sender, RoutedEventArgs e)
    {
        Editor.Clear();
    }
    // "View"
    private void HighlightRowButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Editor.Options.HighlightCurrentLine = !Editor.Options.HighlightCurrentLine;
    }
    private void SpacesButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Editor.Options.ShowSpaces = !Editor.Options.ShowSpaces;
    }
    private void TabSpacesButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Editor.Options.ShowTabs = !Editor.Options.ShowTabs;
    }
    private void ColumnRulerButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Editor.Options.ShowColumnRulers = !Editor.Options.ShowColumnRulers;
    }
    private void EndOfLineButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Editor.Options.ShowEndOfLine = !Editor.Options.ShowEndOfLine;
    }
    private void ListViewButton_OnClick(object? sender, RoutedEventArgs e)
    {
        FileList.IsVisible = !FileList.IsVisible;
        switch (Editor.GetValue(Grid.ColumnSpanProperty))
        {
            case 1:
                Editor.SetValue(Grid.ColumnSpanProperty, 2);
                break;
            case 2:
                Editor.SetValue(Grid.ColumnSpanProperty, 1);
                break;
        }
    }
    private void ListMoveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        switch (Editor.GetValue(Grid.ColumnProperty))
        {
            case 0:
                Editor.SetValue(Grid.ColumnProperty, 1);
                FileList.SetValue(Grid.ColumnProperty, 0);
                //FileList.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Right);
                break;
            case 1:
                Editor.SetValue(Grid.ColumnProperty, 0);
                FileList.SetValue(Grid.ColumnProperty, 1);
                //FileList.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Right);
                break;
        }
    }
    private void MoveStatusBarButton_OnClick(object? sender, RoutedEventArgs e)
    {
        switch (StatusBar.GetValue(Grid.RowProperty))
        {
            case 5:
                StatusBar.SetValue(Grid.RowProperty, 3);
                break;
            case 3:
                StatusBar.SetValue(Grid.RowProperty, 5);
                break;
        }
    }
    // "Debug"
    private void GridLinesButton_OnClick(object? sender, RoutedEventArgs e)
    {
        MainGrid.ShowGridLines = !MainGrid.ShowGridLines;
    }

    
    // Functions for right side buttons
    private async void OpenFileButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var file = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false
        });

        if (file.Count >= 1)
        {
            _filePath = file.First().Path.LocalPath;
            FilePathBlock.Text = "Currently Selected File: " + _filePath;
            
            string selectedFile = _filePath;
            Editor.Clear();
            try
            {
                using StreamReader reader = new(selectedFile);
                var text = reader.ReadToEndAsync().Result;
                Editor.Text = text;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            RefreshFileInformation();
        }
        else
        {
            Console.WriteLine("No file selected");
        }
    }
    private async void OpenFolderButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Open Folder",
            AllowMultiple = false
        });

        if (folder.Count >= 1)
        {
            _folderPath = folder[0].Path.LocalPath;
            FolderPathBlock.Text = "Currently Selected File: " + _folderPath;
            RefreshList();
        }
        else
        {
            Console.WriteLine("No folder selected");
        }
    }
    private void SystemThemeItem_OnClick(object? sender, RoutedEventArgs e)
    {
        RequestedThemeVariant = ThemeVariant.Default;
        RefreshFileInformation();
        var themeSetting = new UserSettings
        {
            ThemeSetting = "Default"
        };
        var jsonString = JsonSerializer.Serialize(themeSetting);
        var writer = new StreamWriter(_settingsFile);
        writer.Write(jsonString);
        writer.Close();
    }
    private void LightThemeItem_OnClick(object? sender, RoutedEventArgs e)
    {
        RequestedThemeVariant = ThemeVariant.Light;
        RefreshFileInformation();
        var themeSetting = new UserSettings
        {
            ThemeSetting = "Light"
        };
        var jsonString = JsonSerializer.Serialize(themeSetting);
        var writer = new StreamWriter(_settingsFile);
        writer.Write(jsonString);
        writer.Close();
    }
    private void DarkThemeItem_OnClick(object? sender, RoutedEventArgs e)
    {
        RequestedThemeVariant = ThemeVariant.Dark;
        RefreshFileInformation();
        var themeSetting = new UserSettings
        {
            ThemeSetting = "Dark"
        };
        var jsonString = JsonSerializer.Serialize(themeSetting);
        var writer = new StreamWriter(_settingsFile);
        writer.Write(jsonString);
        writer.Close();
    }

    
    // Functions for Editor and FileList
    private void EditorCaret_PositionChanged(object? sender, EventArgs e)
    {
        StatusText.Text = "Line: " + Editor.TextArea.Caret.Line + ", Column: " + Editor.TextArea.Caret.Column + " | ";
    }
    private void FileList_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        LoadFromList();
    }
    
    
    // Non-Event functions
    private void RefreshThemeSetting()
    {
        var jsonString = File.ReadAllText(_settingsFile);
        var userSettings = JsonSerializer.Deserialize<UserSettings>(jsonString);
        switch (userSettings.ThemeSetting)
        {
            case null:
                RequestedThemeVariant = ThemeVariant.Default;
                break;
            case "Default":
                RequestedThemeVariant = ThemeVariant.Default;
                break;
            case "Light":
                RequestedThemeVariant = ThemeVariant.Light;
                break;
            case "Dark":
                RequestedThemeVariant = ThemeVariant.Dark;
                break;
        }
    }
    private void LoadFromList()
    {
        if (FileList.SelectedItem != null)
        {
            var selectedFile = FileList.SelectedItem.ToString();
            _filePath = selectedFile;
            Editor.Clear();

            try
            {
                using StreamReader reader = new(selectedFile);
                var text = reader.ReadToEnd();
                Editor.Text = text;
                reader.Close();
                FilePathBlock.Text = "Currently Selected File: " + selectedFile;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        else
        {
            Console.WriteLine("No file selected");
        }
        RefreshFileInformation();
    }
    private void RefreshList()
    {
        if (_folderPath != string.Empty)
        {
            string[] files = Directory.GetFiles(_folderPath);

            FileList.Items.Clear();
            foreach (string file in files)
            {
                FileList.Items.Add(file);
            }
        }
        else
        {
            Console.WriteLine("No folder selected");
        }
    }
    private void RefreshFileInformation()
    {
        try
        {
            if (ActualThemeVariant == ThemeVariant.Default)
            {
                var textEditor = this.FindControl<TextEditor>("Editor");
                var registryOptions = new RegistryOptions(ThemeName.DarkPlus);
                var textMateInstallation = textEditor.InstallTextMate(registryOptions);
                string languageExtension = registryOptions.GetLanguageByExtension(Path.GetExtension(_filePath)).Id;
        
                textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(languageExtension));
                LanguageStatusText.Text= ("Language: " + languageExtension.ToUpper());
            }
            else if (ActualThemeVariant == ThemeVariant.Light)
            {
                var textEditor = this.FindControl<TextEditor>("Editor");
                var registryOptions = new RegistryOptions(ThemeName.LightPlus);
                var textMateInstallation = textEditor.InstallTextMate(registryOptions);
                string languageExtension = registryOptions.GetLanguageByExtension(Path.GetExtension(_filePath)).Id;
        
                textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(languageExtension));
                LanguageStatusText.Text= ("Language: " + languageExtension.ToUpper());
            }
            else if (ActualThemeVariant == ThemeVariant.Dark)
            {
                var textEditor = this.FindControl<TextEditor>("Editor");
                var registryOptions = new RegistryOptions(ThemeName.DarkPlus);
                var textMateInstallation = textEditor.InstallTextMate(registryOptions);
                string languageExtension = registryOptions.GetLanguageByExtension(Path.GetExtension(_filePath)).Id;
        
                textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(languageExtension));
                LanguageStatusText.Text= ("Language: " + languageExtension.ToUpper());
            }
        }
        catch (Exception)
        {
            LanguageStatusText.Text= ("Language: " + "Not a Programming Language/Language Not Supported");
            Console.WriteLine("Not a Programming Language/Language Not Supported");
        }
        var fileExtension = Path.GetExtension(_filePath);
        FileExtensionText.Text = ("File Extension: " + fileExtension + " | ");
    }
}