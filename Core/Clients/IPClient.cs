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
                if (_stream != null)
                {
                    return _stream.WriteTimeout;
                }

                return 0;
            }

            set
            {
                if (_stream != null)
                {
                    _stream.WriteTimeout = value;
                }
            }
        }

        public int ReadTimeout
        {
            get
            {
                if (_stream != null)
                {
                    return _stream.ReadTimeout;
                }

                return 0;
            }

            set
            {
                if (_stream != null)
                {
                    _stream.ReadTimeout = value;
                }
            }
        }

        public NotificationSource Notifications { get; private set; }

        private NetworkStream? _stream;
        private TcpClient? _client;

        private Task? _readThread;
        private CancellationTokenSource? _readCancelSource;     


        public IPClient()
        {
            Notifications = new NotificationSource(
                TX_ViewLatency_ms: 100,
                RX_ViewLatency_ms: 100,
                CheckInterval_ms: 10
                );
        }

        public void SetReadMode(ReadMode mode)
        {
            switch (mode)
            {
                case ReadMode.Async:

                    if (_stream != null && IsConnected)
                    {
                        _readCancelSource = new CancellationTokenSource();

                        _readThread = Task.Run(() => AsyncThread_Read(_stream, _readCancelSource.Token));
                    }

                    break;

                case ReadMode.Sync:

                    if (_stream != null && IsConnected)
                    {
                        _readCancelSource?.Cancel();

                        if (_readThread != null)
                        {
                            Task waitCancel = Task.WhenAll(_readThread);

                            Task flushTask = _stream.FlushAsync();

                            Task.WaitAll(waitCancel, flushTask);
                        }
                    }

                    break;

                default:
                    throw new Exception("У клиента задан неизвестный режим чтения: " + mode.ToString());
            }
        }

        public void Connect(ConnectionInfo information)
        {
            var socketInfo = information.Info as SocketInfo;

            if (socketInfo == null)
            {
                throw new Exception("Нет информации о настройках подключения по Ethernet.");
            }

            if (string.IsNullOrEmpty(socketInfo.IP) || 
                string.IsNullOrEmpty(socketInfo.Port))
            {
                throw new Exception(
                    (string.IsNullOrEmpty(socketInfo.IP) ? "IP-адрес не задан.\n" : "") +
                    (string.IsNullOrEmpty(socketInfo.Port) ? "Порт не задан." : "")
                    );
            }

            if (int.TryParse(socketInfo.Port, out int Port) == false)
            {
                throw new Exception("Не удалось преобразовать номер порта в целочисленное значение.\n" +
                    "Полученный номер порта: " + socketInfo.Port);
            }

            _client = new TcpClient();

            IAsyncResult result = _client.BeginConnect(socketInfo.IP, Port, null, null);

            if (result.AsyncWaitHandle.WaitOne(500, true) == true)
            {
                _client.EndConnect(result);
            }

            else
            {
                _client.Close();

                throw new Exception("Не удалось подключиться к серверу.\n\n" +
                    "IP адрес: " + socketInfo.IP + "\n" +
                    "Порт: " + socketInfo.Port);
            }

            _stream = _client.GetStream();

            Notifications.StartMonitor();

            IsConnected = true;
        }

        public async Task Disconnect()
        {
            ProtocolMode? selectedProtocol = ConnectedHost.SelectedProtocol;

            if (selectedProtocol != null && selectedProtocol.CurrentReadMode == ReadMode.Async)
            {
                _readCancelSource?.Cancel();

                if (_readThread != null)
                {
                    await Task.WhenAll(_readThread).ConfigureAwait(false);
                }
            }

            _stream?.Close();

            _client?.Close();

            await Notifications.StopMonitor();

            IsConnected = false;
        }

        public async Task<ModbusOperationInfo> Send(byte[] message, int numberOfBytes)
        {
            if (_stream == null)
            {
                return new ModbusOperationInfo(DateTime.Now, null);
            }

            var executionTime = new DateTime();

            try
            {
                if (IsConnected)
                {
                    await _stream.WriteAsync(message, 0, numberOfBytes);

                    executionTime = DateTime.Now;

                    Notifications.TransmitEvent();
                }

                return new ModbusOperationInfo(executionTime, null);
            }

            catch (Exception error)
            {
                throw new Exception("Ошибка отправки данных:\n\n" + error.Message + "\n\n" +
                    "Таймаут передачи: " +
                    (_stream.WriteTimeout == Timeout.Infinite ?
                    "бесконечно" : _stream.WriteTimeout.ToString() + " мс."));
            }
        }

        public async Task<ModbusOperationInfo> Receive()
        {
            if (_stream == null)
            {
                return new ModbusOperationInfo(DateTime.Now, Array.Empty<byte>());
            }

            var receivedBytes = new List<byte>();

            DateTime executionTime = new DateTime();

            try
            {
                if (IsConnected)
                {
                    byte[] buffer;

                    int numberOfReceivedBytes;

                    bool isFirstPackage = true;

                    do
                    {
                        buffer = new byte[100];

                        // Асинхронная операция не среагирует на срабатывание таймаута чтения.
                        // Поэтому чтобы предотвратить зависание программы на этом моменте, заведен токен отмены.
                        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(_stream.ReadTimeout));

                        try
                        {
                            numberOfReceivedBytes = await _stream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                        }

                        catch (OperationCanceledException)
                        {
                            throw new Exception("Хост не ответил за указанный таймаут.");
                        }                        

                        if (isFirstPackage)
                        {
                            executionTime = DateTime.Now;
                            isFirstPackage = false;
                        }                        

                        Array.Resize(ref buffer, numberOfReceivedBytes);

                        receivedBytes.AddRange(buffer);

                    } while (_stream.DataAvailable);

                    executionTime = DateTime.Now;

                    if (receivedBytes.Count > 0)
                    {
                        Notifications.ReceiveEvent();
                    }
                }

                return new ModbusOperationInfo(executionTime, receivedBytes.ToArray());
            }

            catch (Exception error)
            {
                throw new Exception("Ошибка приема данных:\n\n" + error.Message + "\n\n" +
                    "Таймаут приема: " +
                    (_stream.ReadTimeout == Timeout.Infinite ?
                    "бесконечно" : _stream.ReadTimeout.ToString() + " мс."));
            }
        }

        private async Task AsyncThread_Read(NetworkStream currentStream, CancellationToken readCancel)
        {
            try
            {
                byte[] bufferRX = new byte[currentStream.Socket.ReceiveBufferSize];

                int numberOfReceiveBytes;

                Task<int> readResult;

                Task waitCancel = Task.Run(async () =>
                {
                    while (readCancel.IsCancellationRequested == false)
                    {
                        await Task.Delay(50);
                    }
                });

                Task completedTask;

                while (true)
                {
                    readCancel.ThrowIfCancellationRequested();

                    if (currentStream != null)
                    {
                        /// Метод асинхронного чтения у объекта класса NetworkStream
                        /// почему то не обрабатывает событие отмены у токена отмены.
                        /// Судя по формумам это происходит из - за того что внутри метода происходят 
                        /// неуправляемые вызовы никоуровневого API (в MSDN об этом не сказано).
                        /// Поэтому для отслеживания состояния токена отмены была создана задача WaitCancel.

                        readResult = currentStream.ReadAsync(bufferRX, 0, bufferRX.Length, readCancel);
                        
                        completedTask = await Task.WhenAny(readResult, waitCancel).ConfigureAwait(false);
                        
                        if (completedTask == waitCancel)
                        {
                            throw new OperationCanceledException();
                        }

                        readCancel.ThrowIfCancellationRequested();

                        numberOfReceiveBytes = readResult.Result;

                        var Data = new DataFromDevice(numberOfReceiveBytes);

                        for (int i = 0; i < numberOfReceiveBytes; i++)
                        {
                            Data.RX[i] = bufferRX[i];
                        }

                        DataReceived?.Invoke(this, Data);

                        Notifications.ReceiveEvent();

                        Array.Clear(bufferRX, 0, numberOfReceiveBytes);
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
