using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace View_WPF.ViewModels
{
    public enum MessageType
    {
        Error,
        Warning,
        Information
    }

    public delegate void ViewMessage(string Message, MessageType Type);

    public delegate void StateUI_Connected();
    public delegate void StateUI_Disconnected();

    public delegate void ActionAfter_Receive();
}
