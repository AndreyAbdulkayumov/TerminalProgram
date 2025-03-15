using System.Threading.Tasks;
using MessageBox_AvaloniaUI;
using MessageBox_Core;
using Services.Interfaces;
using TerminalProgramBase.Views.Macros.EditMacros;

namespace TerminalProgramBase.Services
{
    public class MessageBoxEditMacros : IMessageBoxEditMacros
    {
        public void Show(string message, MessageType type)
        {
            var messageBox = new MessageBox(EditMacrosWindow.Instance);

            messageBox.Show(message, type);
        }

        public async Task<MessageBoxResult> ShowYesNoDialog(string message, MessageType type)
        {
            var messageBox = new MessageBox(EditMacrosWindow.Instance);

            return await messageBox.ShowYesNoDialog(message, type);
        }
    }
}
