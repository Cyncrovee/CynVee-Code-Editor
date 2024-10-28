using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Document;
using AvaloniaEdit.Highlighting;

namespace CynVee_Code_Editor;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    
    private string _filePath;
    private string _folderPath;
    
    // MenuBar "File" functions
    private void Exit(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
    
    // MenuBar "Edit" functions
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
            string ext = Path.GetExtension(file.First().Path.LocalPath.ToString());
            Console.WriteLine(ext);
            switch (ext)
            {
                case ".cs":
                    Editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
                    break;
                case ".html":
                    Editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("HTML");
                    break;
                case ".axaml" or ".xaml" or ".xml":
                    Editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");
                    break;
                case ".java":
                    Editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Java");
                    break;
                case ".js":
                    Editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("JavaScript");
                    break;
            }
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
    
    // Misc functions
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    private void SystemThemeItem_OnClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("SystemThemeItem_OnClick");
    }
}