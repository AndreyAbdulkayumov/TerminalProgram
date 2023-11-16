using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox_Core
{
    public enum MessageType
    {
        Error,
        Warning,
        Information
    }

    public interface IMessageBox
    {
        void Show(string Message, MessageType Type);
        bool ShowDialog(string Message, MessageType Type);
    }
}
