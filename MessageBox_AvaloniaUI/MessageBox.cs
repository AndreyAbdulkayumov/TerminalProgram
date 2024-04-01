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

            Dispatcher.UIThread.Invoke(() => window.ShowDialog(Owner));
        }

        public MessageBoxResult ShowYesNoDialog(string Message, MessageType Type)
        {
            MessageBoxView window = new MessageBoxView(Message, Title, MessageBoxToolType.YesNo);

            Dispatcher.UIThread.Invoke(() => window.ShowDialog(Owner));

            return window.Result;
        }
    }
}
