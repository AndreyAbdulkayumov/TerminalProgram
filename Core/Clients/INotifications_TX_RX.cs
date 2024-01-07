using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Clients
{
    public class NotificationArgs : EventArgs
    {
        public readonly bool IsStarted;

        public NotificationArgs(bool IsStarted)
        {
            this.IsStarted = IsStarted;
        }
    }

    public interface INotifications_TX_RX
    {
        event EventHandler<NotificationArgs>? TX_Notification;

        ulong TX_Counter { get; }

        Task TX_Notification_ControlTask { get; }

        event EventHandler<NotificationArgs>? RX_Notification;

        ulong RX_Counter { get; }

        Task RX_Notification_ControlTask { get; }
    }
}
