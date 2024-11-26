using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MessageBox_AvaloniaUI;
using MessageBox_Core;
using ViewModels;

namespace TerminalProgramBase;

public partial class NumberConverterWindow : Window
{
    private readonly IMessageBox Message;

    public NumberConverterWindow()
    {
        InitializeComponent();

        Message = new MessageBox(this, "Терминальная программа");

        DataContext = new NumberConverter_VM(Message);
    }

    private void Chrome_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    private void Button_Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}