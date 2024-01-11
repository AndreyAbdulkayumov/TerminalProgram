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

    public partial class NotificationSource
    {
        public event EventHandler<NotificationArgs>? TX_Notification;
        public event EventHandler<NotificationArgs>? RX_Notification;

        private ulong TX_Counter;
        private ulong RX_Counter;

        private Task? TX_Notification_ControlTask;
        private Task? RX_Notification_ControlTask;

        private readonly int TX_ViewLatency_ms;
        private readonly int RX_ViewLatency_ms;
        private readonly int CheckInterval_ms;

        private CancellationTokenSource? NotificationCancelSource;


        public NotificationSource(int TX_ViewLatency_ms, int RX_ViewLatency_ms, int CheckInterval_ms)
        {
            this.TX_ViewLatency_ms = TX_ViewLatency_ms;
            this.RX_ViewLatency_ms= RX_ViewLatency_ms;
            this.CheckInterval_ms = CheckInterval_ms;
        }

        public void TransmitEvent()
        {
            TX_Counter++;
        }

        public void ReceiveEvent()
        {
            RX_Counter++;
        }

        public void StartMonitor()
        {
            NotificationCancelSource = new CancellationTokenSource();

            TX_Notification_ControlTask = Task.Run(() => TX_Notification_Control(NotificationCancelSource.Token));
            RX_Notification_ControlTask = Task.Run(() => RX_Notification_Control(NotificationCancelSource.Token));            
        }

        public async Task StopMonitor()
        {
            NotificationCancelSource?.Cancel();

            if (TX_Notification_ControlTask != null && RX_Notification_ControlTask != null)
            {
                await Task.WhenAll(TX_Notification_ControlTask, RX_Notification_ControlTask);
            }            
        }

        private async Task TX_Notification_Control(CancellationToken NotificationCancel)
        {
            try
            {
                bool IsStarted = false;

                while (true)
                {
                    if (TX_Counter > 0)
                    {
                        TX_Counter = 0;

                        if (IsStarted == false)
                        {
                            TX_Notification?.Invoke(null, new NotificationArgs(true));

                            IsStarted = true;
                        }
                    }

                    else if (IsStarted)
                    {
                        await Task.Delay(TX_ViewLatency_ms);

                        TX_Notification?.Invoke(null, new NotificationArgs(false));

                        IsStarted = false;
                    }

                    await Task.Delay(CheckInterval_ms);

                    NotificationCancel.ThrowIfCancellationRequested();
                }
            }

            // Задача отменена
            catch (OperationCanceledException)
            {                
                TX_Notification?.Invoke(null, new NotificationArgs(false));
            }
        }

        private async Task RX_Notification_Control(CancellationToken NotificationCancel)
        {
            try
            {
                bool IsStarted = false;

                while (true)
                {
                    if (RX_Counter > 0)
                    {
                        RX_Counter = 0;

                        if (IsStarted == false)
                        {
                            RX_Notification?.Invoke(null, new NotificationArgs(true));

                            IsStarted = true;
                        }
                    }

                    else if (IsStarted)
                    {
                        await Task.Delay(RX_ViewLatency_ms);

                        RX_Notification?.Invoke(null, new NotificationArgs(false));

                        IsStarted = false;
                    }

                    await Task.Delay(CheckInterval_ms);

                    NotificationCancel.ThrowIfCancellationRequested();
                }
            }

            // Задача отменена
            catch (OperationCanceledException)
            {
                RX_Notification?.Invoke(null, new NotificationArgs(false));
            }
        }
    }
}
