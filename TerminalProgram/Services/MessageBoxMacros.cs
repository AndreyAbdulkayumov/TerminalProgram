using System.Threading.Tasks;
using MessageBox_AvaloniaUI;
using MessageBox_Core;
using TerminalProgramBase.Views.Macros;
using Services.Interfaces;

namespace TerminalProgramBase.Services
{
    public class MessageBoxMacros : IMessageBoxMacros
    {
        public void Show(string message, MessageType type)
        {
            var messageBox = new MessageBox(MacrosWindow.Instance);

            messageBox.Show(message, type);
        }

        public async Task<MessageBoxResult> ShowYesNoDialog(string message, MessageType type)
        {
            var messageBox = new MessageBox(MacrosWindow.Instance);

            return await messageBox.ShowYesNoDialog(message, type);
        }
    }
}
