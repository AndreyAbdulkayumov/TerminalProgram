using MessageBox_AvaloniaUI;
using MessageBox_Core;
using System.Threading.Tasks;
using TerminalProgramBase.Views;
using Services.Interfaces;

namespace TerminalProgramBase.Services
{
    public class MessageBoxMainWindow : IMessageBoxMainWindow
    {
        public void Show(string message, MessageType type)
        {
            var messageBox = new MessageBox(MainWindow.Instance);

            messageBox.Show(message, type);
        }

        public async Task<MessageBoxResult> ShowYesNoDialog(string message, MessageType type)
        {
            var messageBox = new MessageBox(MainWindow.Instance);

            return await messageBox.ShowYesNoDialog(message, type);
        }
    }
}
