using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace CynVee_Code_Editor;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        Editor.Options.AllowToggleOverstrikeMode = true;
        Editor.Options.HighlightCurrentLine = true;
        
        Editor.TextArea.Caret.PositionChanged += EditorCaret_PositionChanged;
    }
    
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
            RefreshSyntax();
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
    private void SettingsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("SettingsButton_OnClick");
    }

    // Functions for Editor and FileList
    private void FileList_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        LoadFromList();
    }
    
    // Non-Event functions
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
        RefreshSyntax();
    }
    private void RefreshList()
    {
        Console.WriteLine("RefreshList");
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
    private void RefreshSyntax()
    {
        try
        {
            var textEditor = this.FindControl<TextEditor>("Editor");
        
            var  registryOptions = new RegistryOptions(ThemeName.DarkPlus);
        
            var textMateInstallation = textEditor.InstallTextMate(registryOptions);
        
            string extension = registryOptions.GetLanguageByExtension(Path.GetExtension(_filePath)).Id;
        
            textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(extension));
            LanguageTextBlock.Text = ("Detected Language: " + extension.ToUpper());
            LanguageStatusText.Text= ("Language: " + extension.ToUpper());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            LanguageTextBlock.Text = ("Detected Language Not Supported");
        }
    }
    
    //Misc Functions
    private void EditorCaret_PositionChanged(object? sender, EventArgs e)
    {
        CurrentLineTextBlock.Text = "Line: " + Editor.TextArea.Caret.Line + ", Column: " + Editor.TextArea.Caret.Column;
        StatusText.Text = "Line: " + Editor.TextArea.Caret.Line + ", Column: " + Editor.TextArea.Caret.Column + " | ";
    }
}