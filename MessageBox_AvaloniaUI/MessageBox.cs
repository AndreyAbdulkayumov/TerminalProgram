using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using MessageBox_AvaloniaUI.Views;
using MessageBox_Core;

namespace MessageBox_AvaloniaUI
{
    public class MessageBox : IMessageBox
    {
        private readonly Window Owner;

        private readonly string Title;

        public MessageBox(Window owner, string title)
        {
            Owner = owner;
            Title = title;
        }

        public void Show(string message, MessageType type)
        {
            Dispatcher.UIThread.Invoke(async () =>
            {
                var window = new MessageBoxView(message, Title, MessageBoxToolType.Default);

                await CallMessageBox(window);
            });
        }

        public async Task<MessageBoxResult> ShowYesNoDialog(string message, MessageType type)
        {
            return await Dispatcher.UIThread.Invoke(async () => 
            {
                var window = new MessageBoxView(message, Title, MessageBoxToolType.YesNo);

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
                window.Topmost = true;
                window.Show();
            }            
        }
    }
}
