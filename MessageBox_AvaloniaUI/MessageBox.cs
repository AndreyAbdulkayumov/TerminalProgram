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

        private const string Title = "Терминальная программа";

        public MessageBox(Window owner)
        {
            Owner = owner;
        }

        public void Show(string message, MessageType messageType)
        {
            Dispatcher.UIThread.Invoke(async () =>
            {
                var window = new MessageBoxView(message, Title, messageType, MessageBoxToolType.Default);

                await CallMessageBox(window);
            });
        }

        public async Task<MessageBoxResult> ShowYesNoDialog(string message, MessageType messageType)
        {
            return await Dispatcher.UIThread.Invoke(async () => 
            {
                var window = new MessageBoxView(message, Title, messageType, MessageBoxToolType.YesNo);

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
}
