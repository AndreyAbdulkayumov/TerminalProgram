using MessageBox_AvaloniaUI;
using MessageBox_Core;
using System.Threading.Tasks;
using TerminalProgramBase.Views;
using Services.Interfaces;

namespace TerminalProgramBase.Services
{
    public class MessageBoxMainWindow : IMessageBoxMainWindow
    {
        private readonly IMessageBox _messageBox;

        public MessageBoxMainWindow()
        {
            _messageBox = new MessageBox(MainWindow.Instance);
        }

        public void Show(string message, MessageType type)
        {
            _messageBox.Show(message, type);
        }

        public async Task<MessageBoxResult> ShowYesNoDialog(string message, MessageType type)
        {
            return await _messageBox.ShowYesNoDialog(message, type);
        }
    }
}
