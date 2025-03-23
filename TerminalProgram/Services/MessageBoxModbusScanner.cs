using MessageBox_AvaloniaUI;
using MessageBox_Core;
using Services.Interfaces;
using System.Threading.Tasks;
using TerminalProgramBase.Views;

namespace TerminalProgramBase.Services
{
    public class MessageBoxModbusScanner : IMessageBoxModbusScanner
    {
        public void Show(string message, MessageType type)
        {
            var messageBox = new MessageBox(ModbusScannerWindow.Instance);

            messageBox.Show(message, type);
        }

        public async Task<MessageBoxResult> ShowYesNoDialog(string message, MessageType type)
        {
            var messageBox = new MessageBox(ModbusScannerWindow.Instance);

            return await messageBox.ShowYesNoDialog(message, type);
        }
    }
}
