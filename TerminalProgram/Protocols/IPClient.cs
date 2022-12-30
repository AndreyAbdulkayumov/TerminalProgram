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

        private NetworkStream Stream = null;
        private TcpClient Client = null;

        private Task ReadThread = null;
        private CancellationTokenSource ReadCancelSource;

        public bool IsConnected { get; private set; } = false;

        public IPClient()
        {
            
        }

        public void Connect(ConnectionInfo Info)
        {
            if (Info.Socket.IP == null || Info.Socket.Port == null)
            {
                throw new Exception(
                    (Info.Socket.IP == null ? "Не был задан IP адрес\n." : "") +
                    (Info.Socket.Port == null ? "Не был задан порт." : "")
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

            Stream.WriteTimeout = Info.TimeoutWrite;
            Stream.ReadTimeout = Info.TimeoutRead;

            ReadCancelSource = new CancellationTokenSource();

            ReadThread = Task.Run(() => AsyncThread_Read(Stream, ReadCancelSource.Token));

            IsConnected = true;
        }

        public async void Disconnect()
        {
            ReadCancelSource.Cancel();

            await Task.WhenAll(ReadThread);

            Stream?.Close();

            Client?.Close();

            IsConnected = false;                
        }

        public void Send(string Message)
        {
            try
            {
                if (IsConnected)
                {
                    byte[] Data = Encoding.ASCII.GetBytes(Message);

                    Stream.WriteAsync(Data, 0, Data.Length);
                }
            }
            
            catch(Exception error)
            {
                throw new Exception("Ошибка отправки данных:\n\n" + error.Message + "\n\n" + 
                    "Таймаут передачи: " + 
                    (Stream.WriteTimeout == Timeout.Infinite ?
                    "бесконечно" : (Stream.WriteTimeout.ToString() + " мс.")));
            }
        }

        public void Send(byte[] Message)
        {
            try
            {
                if (IsConnected)
                {
                    Stream.WriteAsync(Message, 0, Message.Length);
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
                    Stream.Read(RX, 0, RX.Length);
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

                Task WaitCancel = Task.Run(() =>
                {
                    while (ReadCancel.IsCancellationRequested == false) ;
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

                        DataFromDevice Data = new DataFromDevice();

                        Data.RX = new byte[NumberOfReceiveBytes];

                        for(int i = 0; i < NumberOfReceiveBytes; i++)
                        {
                            Data.RX[i] = BufferRX[i];
                        }

                        DataReceived(this, Data);

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
