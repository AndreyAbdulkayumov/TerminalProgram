using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace CoreBus.Base.Views;

public partial class ServiceWindow : Window
{
    public string? SelectedFilePath { get; private set; }

    public ServiceWindow()
    {
        InitializeComponent();

        TextBlock_Description.Text = "ֲגוהטעו טלÿ פאיכא";
    }

    private void Chrome_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    private void Button_Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Button_Select_Click(object? sender, RoutedEventArgs e)
    {
        SelectedFilePath = TextBox_SelectFileName.Text;
        Close();
    }
}
