using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace CynVee_Code_Editor;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        Editor.Options.HighlightCurrentLine = true;
        Editor.TextArea.Caret.PositionChanged += EditorCaret_PositionChanged;
    }
    
    private string _filePath;
    private string _folderPath;
    
    
    // MenuBar functions
    // "File"
    private void ExitFileFolder_OnClick(object? sender, RoutedEventArgs e)
    {
        _filePath = null;
        _folderPath = null;
        
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
    private void ListViewButton_OnClick(object? sender, RoutedEventArgs e)
    {
        FileList.IsVisible = !FileList.IsVisible;
        if (Editor.GetValue(Grid.ColumnSpanProperty) is 1)
        {
            Editor.SetValue(Grid.ColumnSpanProperty, 2);
        }
        else if (Editor.GetValue(Grid.ColumnSpanProperty) is 2)
        {
            Editor.SetValue(Grid.ColumnSpanProperty, 1);
        }
    }
    private void ListMoveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Editor.GetValue(Grid.ColumnProperty) is 0)
        {
            Editor.SetValue(Grid.ColumnProperty, 1);
            FileList.SetValue(Grid.ColumnProperty, 0);
            //FileList.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Right);
        }
        else if (Editor.GetValue(Grid.ColumnProperty) is 1)
        {
            Editor.SetValue(Grid.ColumnProperty, 0);
            FileList.SetValue(Grid.ColumnProperty, 1);
            //FileList.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Right);
        }
    }
    // "Debug"
    private void GridLinesButton_OnClick(object? sender, RoutedEventArgs e)
    {
        MainGrid.ShowGridLines = !MainGrid.ShowGridLines;
    }
    
    
    // Functions for left side buttons
    private void SaveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("SaveButton_OnClick");
        if (_filePath != null)
        {
            StreamWriter writer = new StreamWriter(_filePath);
            writer.Write(Editor.Text);
            writer.Close();
        }
        else
        {
            Console.WriteLine("No file selected");
        }
    }
    private void LoadButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("LoadButton_OnClick");
        LoadFromList();
    }
    private void DeleteButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("DeleteButton_OnClick");
    }

    // Functions for right side buttons
    private async void OpenFileButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("OpenFileButton_OnClick");
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
            
            string selectedFile = _filePath.ToString();
            Editor.Clear();
            try
            {
                using StreamReader reader = new(selectedFile);
                string text = reader.ReadToEnd();
                Editor.Text = text;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            RefreshSyntax();
        }
    }
    private async void OpenFolderButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("OpenFolderButton_OnClick");
        var topLevel = TopLevel.GetTopLevel(this);
        var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Open Folder",
            AllowMultiple = false
        });
        _folderPath = folder.First().Path.LocalPath;
        FolderPathBlock.Text = "Currently Selected File: " + _folderPath;
        RefreshList();
    }
    private void SettingsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("SettingsButton_OnClick");
    }

    // Functions for Editor and FileList
    private async void FileList_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        LoadFromList();
    }
    
    // Non-Event functions
    private void LoadFromList()
    {
        if (FileList.SelectedItem != null)
        {
            string selectedFile = FileList.SelectedItem.ToString();
            _filePath = selectedFile;
            Editor.Clear();

            try
            {
                using StreamReader reader = new(selectedFile);
                string text = reader.ReadToEnd();
                Editor.Text = text;
                reader.Close();
                FilePathBlock.Text = "Currently Selected File: " + selectedFile;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        RefreshSyntax();
    }
    private void RefreshList()
    {
        Console.WriteLine("RefreshList");
        if (_folderPath != null)
        {
            string[] files = Directory.GetFiles(_folderPath);

            FileList.Items.Clear();
            foreach (string file in files)
            {
                FileList.Items.Add(file);
            }
        }
    }
    private void RefreshSyntax()
    {
        try
        {
            var _textEditor = this.FindControl<TextEditor>("Editor");
        
            var  _registryOptions = new RegistryOptions(ThemeName.DarkPlus);
        
            var _textMateInstallation = _textEditor.InstallTextMate(_registryOptions);
        
            string extension = _registryOptions.GetLanguageByExtension(Path.GetExtension(_filePath)).Id;
        
            _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(extension));
            LanguageTextBlock.Text = ("Detected Language: " + extension.ToUpper());
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
    }
}