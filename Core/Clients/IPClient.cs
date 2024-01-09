using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using Core.Models;

namespace Core.Clients
{
    public class IPClient : IConnection
    {
        public event EventHandler<DataFromDevice>? DataReceived;
        public event EventHandler<string>? ErrorInReadThread;

        public bool IsConnected { get; private set; } = false;

        public int WriteTimeout
        {
            get
            {
                if (Stream != null)
                {
                    return Stream.WriteTimeout;
                }

                return 0;
            }

            set
            {
                if (Stream != null)
                {
                    Stream.WriteTimeout = value;
                }
            }
        }

        public int ReadTimeout
        {
            get
            {
                if (Stream != null)
                {
                    return Stream.ReadTimeout;
                }

                return 0;
            }

            set
            {
                if (Stream != null)
                {
                    Stream.ReadTimeout = value;
                }
            }
        }

        private NetworkStream? Stream = null;
        private TcpClient? Client = null;

        private Task? ReadThread = null;
        private CancellationTokenSource? ReadCancelSource = null;

        public NotificationSource Notifications { get; private set; }


        public IPClient()
        {
            Notifications = new NotificationSource(
                TX_ViewLatency_ms: 100,
                RX_ViewLatency_ms: 100,
                CheckInterval_ms: 10
                );
        }

        public void SetReadMode(ReadMode Mode)
        {
            switch (Mode)
            {
                case ReadMode.Async:

                    if (Stream != null && IsConnected)
                    {
                        ReadCancelSource = new CancellationTokenSource();

                        ReadThread = Task.Run(() => AsyncThread_Read(Stream, ReadCancelSource.Token));
                    }

                    break;

                case ReadMode.Sync:

                    if (Stream != null && IsConnected)
                    {
                        ReadCancelSource?.Cancel();

                        if (ReadThread != null)
                        {
                            Task WaitCancel = Task.WhenAll(ReadThread);

                            Task FlushTask = Stream.FlushAsync();

                            Task.WaitAll(WaitCancel, FlushTask);
                        }
                    }

                    break;

                default:
                    throw new Exception("У клиента задан неизвестный режим чтения: " + Mode.ToString());
            }
        }

        public void Connect(ConnectionInfo Information)
        {
            SocketInfo? SocketInfo = Information.Info as SocketInfo;

            if (SocketInfo == null)
            {
                throw new Exception("Нет информации о настройках подключения по Ethernet.");
            }

            if (SocketInfo.IP == null || SocketInfo.IP == String.Empty || 
                SocketInfo.Port == null || SocketInfo.Port == String.Empty)
            {
                throw new Exception(
                    (SocketInfo.IP == null || SocketInfo.IP == String.Empty ? "IP адрес не задан.\n" : "") +
                    (SocketInfo.Port == null || SocketInfo.Port == String.Empty ? "Порт не задан." : "")
                    );
            }

            if (int.TryParse(SocketInfo.Port, out int Port) == false)
            {
                throw new Exception("Не удалось преобразовать номер порта в целочисленное значение.\n" +
                    "Полученный номер порта: " + SocketInfo.Port);
            }

            Client = new TcpClient();

            IAsyncResult result = Client.BeginConnect(SocketInfo.IP, Port, null, null);

            bool SuccessConnect = result.AsyncWaitHandle.WaitOne(500, true);

            if (SuccessConnect == true)
            {
                Client.EndConnect(result);
            }

            else
            {
                Client.Close();

                throw new Exception("Не удалось подключиться к серверу.\n\n" +
                    "IP адрес: " + SocketInfo.IP + "\n" +
                    "Порт: " + SocketInfo.Port);
            }

            Stream = Client.GetStream();

            Notifications.StartMonitor();

            IsConnected = true;
        }

        public async Task Disconnect()
        {
            ProtocolMode? SelectedProtocol = ConnectedHost.SelectedProtocol;

            if (SelectedProtocol != null && SelectedProtocol.CurrentReadMode == ReadMode.Async)
            {
                ReadCancelSource?.Cancel();

                if (ReadThread != null)
                {
                    await Task.WhenAll(ReadThread).ConfigureAwait(false);
                }
            }

            Stream?.Close();

            Client?.Close();

            await Notifications.StopMonitor();

            IsConnected = false;
        }

        public void Send(byte[] Message, int NumberOfBytes)
        {
            if (Stream == null)
            {
                return;
            }

            try
            {
                if (IsConnected)
                {
                    Stream.Write(Message, 0, NumberOfBytes);

                    Notifications.TransmitEvent();
                }
            }

            catch (Exception error)
            {
                throw new Exception("Ошибка отправки данных:\n\n" + error.Message + "\n\n" +
                    "Таймаут передачи: " +
                    (Stream.WriteTimeout == Timeout.Infinite ?
                    "бесконечно" : Stream.WriteTimeout.ToString() + " мс."));
            }
        }

        public int Receive(byte[] RX)
        {
            if (Stream == null)
            {
                return 0;
            }

            int NumberOfReceivedBytes = 0;

            try
            {
                if (IsConnected)
                {
                    do
                    {
                        if (NumberOfReceivedBytes > RX.Length)
                        {
                            return RX.Length;
                        }

                        NumberOfReceivedBytes += Stream.Read(RX, NumberOfReceivedBytes, RX.Length);

                    } while (Stream.DataAvailable);

                    Notifications.ReceiveEvent();
                }

                return NumberOfReceivedBytes;
            }

            catch (Exception error)
            {
                throw new Exception("Ошибка приема данных:\n\n" + error.Message + "\n\n" +
                    "Таймаут приема: " +
                    (Stream.ReadTimeout == Timeout.Infinite ?
                    "бесконечно" : Stream.ReadTimeout.ToString() + " мс."));
            }
        }

        private async Task AsyncThread_Read(NetworkStream CurrentStream, CancellationToken ReadCancel)
        {
            try
            {
                byte[] BufferRX = new byte[50];

                int NumberOfReceiveBytes;

                Task<int> ReadResult;

                Task WaitCancel = Task.Run(async () =>
                {
                    while (ReadCancel.IsCancellationRequested == false)
                    {
                        await Task.Delay(50);
                    }
                });

                Task CompletedTask;

                while (true)
                {
                    ReadCancel.ThrowIfCancellationRequested();

                    if (CurrentStream != null)
                    {
                        /// Метод асинхронного чтения у объекта класса NetworkStream
                        /// почему то не обрабатывает событие отмены у токена отмены.
                        /// Судя по формумам это происходит из - за того что внутри метода происходят 
                        /// неуправляемые вызовы никоуровневого API (в MSDN об этом не сказано).
                        /// Поэтому для отслеживания состояния токена отмены была создана задача WaitCancel.

                        ReadResult = CurrentStream.ReadAsync(BufferRX, 0, BufferRX.Length, ReadCancel);
                        
                        CompletedTask = await Task.WhenAny(ReadResult, WaitCancel).ConfigureAwait(false);
                        
                        if (CompletedTask == WaitCancel)
                        {
                            throw new OperationCanceledException();
                        }

                        ReadCancel.ThrowIfCancellationRequested();

                        NumberOfReceiveBytes = ReadResult.Result;

                        DataFromDevice Data = new DataFromDevice(NumberOfReceiveBytes);

                        for (int i = 0; i < NumberOfReceiveBytes; i++)
                        {
                            Data.RX[i] = BufferRX[i];
                        }

                        DataReceived?.Invoke(this, Data);

                        Notifications.ReceiveEvent();

                        Array.Clear(BufferRX, 0, NumberOfReceiveBytes);
                    }
                }
            }

            catch (OperationCanceledException)
            {
                //  Возникает при отмене задачи.
                //  По правилам отмены асинхронных задач это исключение можно игнорировать.
            }

            catch (Exception error)
            {
                ErrorInReadThread?.Invoke(this, "Возникла ошибка при асинхронном чтении у IP клиента.\n\n" +
                    "Прием данных прекращен.\n\n" + error.Message);
            }
        }
    }
}
