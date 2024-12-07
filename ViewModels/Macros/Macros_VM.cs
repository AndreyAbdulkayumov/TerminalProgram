using MessageBox_Core;

namespace ViewModels.Macros
{
    public class Macros_VM
    {

        private readonly IMessageBox _messageBox;

        public Macros_VM(IMessageBox messageBox)
        {
            _messageBox = messageBox;
        }
    }
}
