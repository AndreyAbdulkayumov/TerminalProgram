using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MessageBox_AvaloniaUI;
using MessageBox_Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ViewModels.Macros;
using ViewModels.Macros.DataTypes;

namespace TerminalProgram.Views.Macros;

public partial class MacrosWindow : Window
{
    private static bool _isOpen = false;

    private readonly IMessageBox _messageBox;

    private readonly Macros_VM _viewModel;

    private readonly Border? _resizeIcon;

    public MacrosWindow()
    {
        InitializeComponent();

        _messageBox = new MessageBox(this, "Макросы");

        _viewModel = new Macros_VM(_messageBox, OpenEditMacrosWindow, GetFolderPath, GetFilePath);

        _resizeIcon = this.FindControl<Border>("Border_ResizeIcon");

        DataContext = _viewModel;
    }

    private async Task<object?> OpenEditMacrosWindow(List<EditCommandParameters>? allParameters)
    {
        //var window = new EditMacrosWindow(parameters);

        //await MainWindow.OpenWindowWithDimmer(async () =>
        //{
        //    await window.ShowDialog(this);
        //},
        //Grid_Workspace);

        //return window.GetData();

        var window = new EditMacrosWindow(allParameters);

        await MainWindow.OpenWindowWithDimmer(async () =>
        {
            await window.ShowDialog(this);
        },
        Grid_Workspace);

        return window.GetData();
    }

    private async Task<string?> GetFolderPath(string WindowTitle)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        TopLevel? topLevel = TopLevel.GetTopLevel(this);

        if (topLevel != null)
        {
            var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = WindowTitle,
                AllowMultiple = false
            });

            if (folder != null && folder.Count > 0)
            {
                return folder.First().TryGetLocalPath();
            }
        }

        return null;
    }

    private async Task<string?> GetFilePath(string WindowTitle)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        TopLevel? topLevel = TopLevel.GetTopLevel(this);

        if (topLevel != null)
        {
            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = WindowTitle,
                FileTypeFilter = [new FilePickerFileType("Файл макросов") { Patterns = ["*.json"] }],
                AllowMultiple = false
            });

            if (files.Count >= 1)
            {
                return files.First().TryGetLocalPath();
            }
        }

        return null;
    }

    /// <summary>
    /// Использовать для открытия окна в единственном экземпляре.
    /// </summary>
    public static void ShowWindow(Window owner)
    {
        if (_isOpen)
        {
            return;
        }

        var window = new MacrosWindow();

        window.Show(owner);

        _isOpen = true;
    }

    private void Window_Closing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
    {
        _isOpen = false;
    }

    private void Chrome_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    private void Button_Minimize_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Button_Maximize_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        if (_resizeIcon != null)
        {
            _resizeIcon.IsVisible = WindowState == WindowState.Normal ? true : false;
        }
    }

    private void Button_Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ResizeIcon_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Cursor = new(StandardCursorType.BottomRightCorner);
        BeginResizeDrag(WindowEdge.SouthEast, e);
        Cursor = new(StandardCursorType.Arrow);
    }
}