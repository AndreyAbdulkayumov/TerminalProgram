using MessageBox_AvaloniaUI;
using MessageBox_Core;
using Services.Interfaces;
using System.Threading.Tasks;
using TerminalProgramBase.Views.Settings;

namespace TerminalProgramBase.Services
{
    public class MessageBoxSettings : IMessageBoxSettings
    {
        public void Show(string message, MessageType type)
        {
            var messageBox = new MessageBox(SettingsWindow.Instance);

            messageBox.Show(message, type);
        }

        public async Task<MessageBoxResult> ShowYesNoDialog(string message, MessageType type)
        {
            var messageBox = new MessageBox(SettingsWindow.Instance);

            return await messageBox.ShowYesNoDialog(message, type);
        }
    }
}
