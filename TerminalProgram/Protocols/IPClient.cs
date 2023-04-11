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
        public event EventHandler<DataFromDevice>? DataReceived;

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

        public void Connect(ConnectionInfo Info)
        {
            if (Info.Socket == null)
            {
                throw new Exception("Нет информации о настройках подключения по Ethernet.");
            }

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
            ProtocolMode? SelectedProtocol = ((MainWindow)Application.Current.MainWindow).SelectedProtocol;

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
            if (Stream == null)
            {
                return;
            }

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

                        DataFromDevice Data = new DataFromDevice(NumberOfReceiveBytes);

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

                ActionAfterCancel(CurrentStream);
            }

            catch (System.IO.IOException error)
            {
                MessageBox.Show(
                    "Возникла ошибка при асинхронном чтении у IP клиента.\n\n" + 
                    error.Message +
                    "\n\nТаймаут чтения: " + CurrentStream.ReadTimeout + " мс." +
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

        // В методе ReadAsync у класса NetworkStream сейчас невозможно отменить операцию чтения.
        // Поэтому при отмене асинхронной операции поток продолжает считывать данные (ReadTimeout == Timeout.Infinite).
        // Данная особенность порождает баг:
        // После подключения к хосту, при переходе из режима "Без протокола" в режим "Modbus"
        // клиент не может синхронно считать данные за отведенный таймаут. 
        // Это возникает из - за того, что эти данные уже прочитал поток неотменненый ранее. 
        // Эту особенность можно увидеть в методе ниже, если поставить таймаут на большее значение.
        private void ActionAfterCancel(NetworkStream CurrentStream)
        {
            try
            {
                if (CurrentStream == null)
                {
                    return;
                }

                CurrentStream.ReadTimeout = 1;
                CurrentStream?.Read(new byte[10], 0, 10);
            }

            catch (Exception)
            {
                CurrentStream.ReadTimeout = Timeout.Infinite;
            }
        }
    }
}
