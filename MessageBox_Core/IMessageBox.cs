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

    public enum MessageBoxResult
    {
        Default,
        Yes,
        No
    }

    public interface IMessageBox
    {
        void Show(string Message, MessageType Type);
        MessageBoxResult ShowYesNoDialog(string Message, MessageType Type);
    }
}
