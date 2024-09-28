namespace Core.Clients
{
    public partial class NotificationSource
    {
        public async Task DemoVisualization()
        {
            while (true)
            {
                await TX_RX_View();

                await Task.Delay(500);

                await TX_RX_View();

                await Task.Delay(600);

                await Only_TX_View(1);

                await Task.Delay(300);

                await Only_RX_View(5);

                await Task.Delay(1000);
            }
        }

        private async Task TX_RX_View()
        {
            Task TxView, RxView;

            TxView = Task.Run(async () =>
            {
                TX_Notification?.Invoke(null, new NotificationArgs(true));

                await Task.Delay(TX_ViewLatency_ms);

                TX_Notification?.Invoke(null, new NotificationArgs(false));
            });

            await Task.Delay(5);

            RxView = Task.Run(async () =>
            {
                RX_Notification?.Invoke(null, new NotificationArgs(true));

                await Task.Delay(RX_ViewLatency_ms);

                RX_Notification?.Invoke(null, new NotificationArgs(false));
            });

            await Task.WhenAll(TxView, RxView);
        }

        private async Task Only_TX_View(int TX_Count)
        {
            while (--TX_Count >= 0)
            {
                TX_Notification?.Invoke(null, new NotificationArgs(true));

                await Task.Delay(TX_ViewLatency_ms);

                TX_Notification?.Invoke(null, new NotificationArgs(false));

                await Task.Delay(300);
            }
        }

        private async Task Only_RX_View(int RX_Count)
        {
            while (--RX_Count >= 0)
            {
                RX_Notification?.Invoke(null, new NotificationArgs(true));

                await Task.Delay(RX_ViewLatency_ms);

                RX_Notification?.Invoke(null, new NotificationArgs(false));

                await Task.Delay(300);
            }
        }
    }
}
