using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using MessageBox_AvaloniaUI.ViewModels;
using MessageBox_AvaloniaUI.Views;
using MessageBox_Core;

namespace MessageBox_AvaloniaUI;

public class MessageBox : IMessageBox
{
    private readonly Window Owner;

    private const string Title = "Терминальная программа";

    private readonly string? _appVersion;

    public MessageBox(Window owner, string? appVersion)
    {
        Owner = owner;
        _appVersion = appVersion;
    }

    public void Show(string message, MessageType messageType, Exception? error = null)
    {
        Dispatcher.UIThread.Invoke(async () =>
        {
            var window = new MessageBoxWindow(message, Title, messageType, MessageBoxToolType.Default, _appVersion, error);

            await CallMessageBox(window);
        });
    }

    public async Task<MessageBoxResult> ShowYesNoDialog(string message, MessageType messageType, Exception? error = null)
    {
        return await Dispatcher.UIThread.Invoke(async () =>
        {
            var window = new MessageBoxWindow(message, Title, messageType, MessageBoxToolType.YesNo, _appVersion, error);

            await CallMessageBox(window);

            return window.Result;
        });
    }

    private async Task CallMessageBox(Window window)
    {
        if (Owner.IsVisible)
        {
            await window.ShowDialog(Owner);
        }

        else
        {
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Show();
        }
    }
}
