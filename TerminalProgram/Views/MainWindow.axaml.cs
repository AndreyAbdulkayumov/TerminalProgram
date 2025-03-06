using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MessageBox_Core;
using TerminalProgramBase.Views.Macros;
using System.Diagnostics;
using System.IO;

namespace TerminalProgramBase.Views;

public partial class MainWindow : Window
{
    public static MainWindow? Instance { get; private set; }
    public static Grid? Workspace { get; private set; }

    private readonly Border? _resizeIcon;

    public MainWindow()
    {
        InitializeComponent();

        Instance = this;
        Workspace = this.FindControl<Grid>("Grid_Workspace");

        _resizeIcon = this.FindControl<Border>("Border_ResizeIcon");
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

    /********************************************************/
    //
    //  Обработчики кнопок
    //
    /********************************************************/

    private void Button_Macros_Click(object? sender, RoutedEventArgs e)
    {
        MacrosWindow.ShowWindow(this);
    }
}
