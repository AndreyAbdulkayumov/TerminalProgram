using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Windows;

namespace TerminalProgram.Protocols
{
    public class IPClient : IConnection
    {
        public event EventHandler<DataFromDevice> DataReceived;

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

        private NetworkStream Stream = null;
        private TcpClient Client = null;

        private Task ReadThread = null;
        private CancellationTokenSource ReadCancelSource = null;


        public async void SetReadMode(ReadMode Mode)
        {
            switch (Mode)
            {
                case ReadMode.Async:

                    if (IsConnected)
                    {
                        ReadCancelSource = new CancellationTokenSource();

                        ReadThread = Task.Run(() => AsyncThread_Read(Stream, ReadCancelSource.Token));
                    }                    

                    break;

                case ReadMode.Sync:

                    if (IsConnected && ReadCancelSource != null)
                    {
                        ReadCancelSource.Cancel();

                        await Task.WhenAll(ReadThread);

                        await Stream.FlushAsync();
                    }
                    
                    break;

                default:
                    throw new Exception("У клиента задан неизвестный режим чтения: " + Mode.ToString());
            }
        }

        public void Connect(ConnectionInfo Info)
        {
            if (Info.Socket.IP == null || Info.Socket.Port == null)
            {
                throw new Exception(
                    (Info.Socket.IP == null ? "Не задан IP адрес.\n" : "") +
                    (Info.Socket.Port == null ? "Не задан Порт." : "")
                    );
            }

            if (Int32.TryParse(Info.Socket.Port, out int Port) == false)
            {
                throw new Exception("Не удалось преобразовать номер порта в целочисленное значение.\n" +
                    "Полученный номер порта: " + Info.Socket.Port);
            }

            Client = new TcpClient();

            IAsyncResult result = Client.BeginConnect(Info.Socket.IP, Port, null, null);

            bool SuccessConnect = result.AsyncWaitHandle.WaitOne(500, true);

            if (SuccessConnect == true)
            {
                Client.EndConnect(result);
            }

            else
            {
                Client.Close();

                throw new Exception("Не удалось подключиться к серверу.\n\n" +
                    "IP адрес: " + Info.Socket.IP + "\n" +
                    "Порт: " + Info.Socket.Port);
            }

            Stream = Client.GetStream();

            IsConnected = true;
        }

        public async Task Disconnect()
        {
            if (((MainWindow)Application.Current.MainWindow).SelectedProtocol.CurrentReadMode == ReadMode.Async)
            {
                ReadCancelSource.Cancel();

                await Task.WhenAll(ReadThread);
            }            

            Stream?.Close();

            Client?.Close();

            IsConnected = false;                
        }

        public void Send(byte[] Message, int NumberOfBytes)
        {
            try
            {
                if (IsConnected)
                {
                    Stream.Write(Message, 0, NumberOfBytes);
                }
            }

            catch (Exception error)
            {
                throw new Exception("Ошибка отправки данных:\n\n" + error.Message + "\n\n" +
                    "Таймаут передачи: " +
                    (Stream.WriteTimeout == Timeout.Infinite ? 
                    "бесконечно" : (Stream.WriteTimeout.ToString() + " мс.")));
            }
        }

        public void Receive(byte[] RX)
        {
            try
            {
                if (IsConnected)
                {
                    int NumberOfReceivedBytes = 0;

                    do
                    {
                        if (NumberOfReceivedBytes > RX.Length)
                        {
                            break;
                        }

                        NumberOfReceivedBytes = Stream.Read(RX, NumberOfReceivedBytes, RX.Length);

                    } while (Stream.DataAvailable);
                }
            }

            catch (Exception error)
            {
                throw new Exception("Ошибка приема данных:\n\n" + error.Message + "\n\n" +
                    "Таймаут приема: " +
                    (Stream.ReadTimeout == Timeout.Infinite ?
                    "бесконечно" : (Stream.ReadTimeout.ToString() + " мс.")));
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

                while(true)
                {
                    ReadCancel.ThrowIfCancellationRequested();

                    if (DataReceived != null && CurrentStream != null)
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

                        NumberOfReceiveBytes = ReadResult.Result;

                        ReadCancel.ThrowIfCancellationRequested();

                        DataFromDevice Data = new DataFromDevice()
                        {
                            RX = new byte[NumberOfReceiveBytes]
                        };

                        for (int i = 0; i < NumberOfReceiveBytes; i++)
                        {
                            Data.RX[i] = BufferRX[i];
                        }
                                                
                        DataReceived?.Invoke(this, Data);

                        Array.Clear(BufferRX, 0, NumberOfReceiveBytes);
                    }
                }
            }

            catch (OperationCanceledException)
            {
                //  Возникает при отмене задачи.
                //  По правилам отмены асинхронных задач это исключение можно игнорировать.
            }

            catch (System.IO.IOException error)
            {
                MessageBox.Show(
                    "Возникла ошибка при асинхронном чтении у IP клиента.\n\n" + 
                    error.Message +
                    "\n\nТаймаут чтения: " + Stream.ReadTimeout + " мс." +
                    "\n\nЧтение данных прекращено. Возможно вам стоит изменить настройки и переподключиться.",
                    "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            catch (Exception error)
            {
                // TODO: Как правильно обработать это исключение?

                MessageBox.Show("Возникла НЕОБРАБОТАННАЯ ошибка " +
                    "при асинхронном чтении у IP клиента.\n\n" + error.Message +
                    "\n\nКлиент был отключен.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
