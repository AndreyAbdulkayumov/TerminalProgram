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

        public MessageBox(Window Owner, string Title)
        {
            this.Owner = Owner;
            this.Title = Title;
        }

        public void Show(string Message, MessageType Type)
        {
            MessageBoxView window = new MessageBoxView(Message, Title, MessageBoxToolType.Default);

            Dispatcher.UIThread.Invoke(async () => await CallMessageBox(window));
        }

        public MessageBoxResult ShowYesNoDialog(string Message, MessageType Type)
        {
            MessageBoxView window = new MessageBoxView(Message, Title, MessageBoxToolType.YesNo);

            Dispatcher.UIThread.Invoke(async () => await CallMessageBox(window));

            return window.Result;
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
