using System.IO.Ports;
using Core.Clients.DataTypes;
using Core.Models;

namespace Core.Clients;

public class SerialPortClient : IConnection
{
    public event EventHandler<byte[]>? DataReceived;
    public event EventHandler<Exception>? ErrorInReadThread;

    public bool IsConnected
    {
        get
        {
            if (_deviceSerialPort == null || _deviceSerialPort.IsOpen == false)
            {
                return false;
            }

            return true;
        }
    }

    public int WriteTimeout
    {
        get
        {
            if (_deviceSerialPort != null)
            {
                return _deviceSerialPort.WriteTimeout;
            }

            return 0;
        }

        set
        {
            if (_deviceSerialPort != null)
            {
                _deviceSerialPort.WriteTimeout = value;
            }
        }
    }

    public int ReadTimeout
    {
        get
        {
            if (_deviceSerialPort != null)
            {
                return _deviceSerialPort.ReadTimeout;
            }

            return 0;
        }

        set
        {
            if (_deviceSerialPort != null)
            {
                _deviceSerialPort.ReadTimeout = value;
            }
        }
    }

    public NotificationSource Notifications { get; private set; }

    private SerialPort? _deviceSerialPort;

    private Task? _readThread;
    private CancellationTokenSource? _readCancelSource;


    public SerialPortClient()
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

                if (_deviceSerialPort != null && IsConnected)
                {
                    _readCancelSource = new CancellationTokenSource();

                    _deviceSerialPort.BaseStream.WriteTimeout = 500;
                    _deviceSerialPort.BaseStream.ReadTimeout = -1;   // Бесконечно

                    _readThread = Task.Run(() => AsyncThread_Read(_deviceSerialPort.BaseStream, _readCancelSource.Token));
                }

                break;

            case ReadMode.Sync:

                if (_deviceSerialPort != null && IsConnected)
                {
                    _readCancelSource?.Cancel();

                    if (_readThread != null)
                    {
                        Task.WaitAll(_readThread);
                    }

                    _deviceSerialPort.DiscardInBuffer();
                    _deviceSerialPort.DiscardOutBuffer();
                }

                break;

            default:
                throw new Exception("У клиента задан неизвестный режим чтения: " + mode.ToString());
        }
    }

    public void Connect(ConnectionInfo information)
    {
        var portInfo = information.Info as SerialPortInfo;

        try
        {
            if (portInfo == null)
            {
                throw new Exception("Нет информации о настройках подключения по последовательному порту.");
            }

            if (string.IsNullOrEmpty(portInfo.Port) ||
                string.IsNullOrEmpty(portInfo.BaudRate) ||
                string.IsNullOrEmpty(portInfo.Parity) ||
                string.IsNullOrEmpty(portInfo.DataBits) ||
                string.IsNullOrEmpty(portInfo.StopBits))
            {
                throw new Exception(
                    (string.IsNullOrEmpty(portInfo.Port) ? "Не задан порт.\n" : "") +
                    (string.IsNullOrEmpty(portInfo.BaudRate) ? "Не задан BaudRate.\n" : "") +
                    (string.IsNullOrEmpty(portInfo.Parity) ? "Не задан Parity.\n" : "") +
                    (string.IsNullOrEmpty(portInfo.DataBits) ? "Не задан DataBits\n" : "") +
                    (string.IsNullOrEmpty(portInfo.StopBits) ? "Не задан StopBits\n" : "")
                    );
            }

            _deviceSerialPort = new SerialPort();

            if (int.TryParse(portInfo.BaudRate, out int BaudRate) == false)
            {
                throw new Exception("Не удалось преобразовать значение BaudRate в целочисленное значение.\n" +
                    "Полученное значение BaudRate: " + portInfo.BaudRate);
            }

            Parity selectedParity;

            switch (portInfo.Parity)
            {
                case "None":
                    selectedParity = Parity.None;
                    break;

                case "Even":
                    selectedParity = Parity.Even;
                    break;

                case "Odd":
                    selectedParity = Parity.Odd;
                    break;

                case "Space":
                    selectedParity = Parity.Space;
                    break;

                case "Mark":
                    selectedParity = Parity.Mark;
                    break;

                default:
                    throw new Exception("Неправильно задано значение Parity.");
            }

            if (int.TryParse(portInfo.DataBits, out int DataBits) == false)
            {
                throw new Exception("Не удалось преобразовать значение DataBits в целочисленное значение.\n" +
                    "Полученное значение DataBits: " + portInfo.DataBits);
            }

            StopBits selectedStopBits;

            switch (portInfo.StopBits)
            {
                case "1":
                    selectedStopBits = StopBits.One;
                    break;

                case "1.5":
                    selectedStopBits = StopBits.OnePointFive;
                    break;

                case "2":
                    selectedStopBits = StopBits.Two;
                    break;

                default:
                    throw new Exception("Неправильно задано значение StopBits");
            }

            _deviceSerialPort.PortName = portInfo.Port;
            _deviceSerialPort.BaudRate = BaudRate;
            _deviceSerialPort.Parity = selectedParity;
            _deviceSerialPort.DataBits = DataBits;
            _deviceSerialPort.StopBits = selectedStopBits;

            _deviceSerialPort.Open();

            Notifications.StartMonitor();
        }

        catch (Exception error)
        {
            _deviceSerialPort?.Close();

            string CommonMessage = "Не удалось подключиться к последовательному порту.\n\n";

            if (portInfo != null)
            {
                throw new Exception(CommonMessage +
                    "Данные подключения:" + "\n" +
                    "Port: " + portInfo.Port + "\n" +
                    "BaudRate: " + portInfo.BaudRate + "\n" +
                    "Parity: " + portInfo.Parity + "\n" +
                    "DataBits: " + portInfo.DataBits + "\n" +
                    "StopBits: " + portInfo.StopBits + "\n\n" +
                    error.Message);
            }

            throw new Exception(CommonMessage + error.Message);
        }
    }

    public async Task Disconnect()
    {
        try
        {
            if (_deviceSerialPort != null && _deviceSerialPort.IsOpen)
            {
                ProtocolMode? SelectedProtocol = ConnectedHost.SelectedProtocol;

                if (SelectedProtocol != null && SelectedProtocol.CurrentReadMode == ReadMode.Async)
                {
                    _readCancelSource?.Cancel();

                    if (_readThread != null)
                    {
                        await Task.WhenAll(_readThread).ConfigureAwait(false);

                        await Task.Delay(100);
                    }
                }

                _deviceSerialPort.Close();
            }

            await Notifications.StopMonitor();
        }

        catch (Exception error)
        {
            throw new Exception("Не удалось отключиться от СОМ порта.\n\n" + error.Message);
        }
    }

    public async Task<ModbusOperationInfo> Send(byte[] message, int numberOfBytes)
    {
        if (_deviceSerialPort == null)
        {
            return new ModbusOperationInfo(DateTime.Now, null);
        }

        DateTime ExecutionTime = new DateTime();

        try
        {
            if (IsConnected)
            {
                await _deviceSerialPort.BaseStream.WriteAsync(message, 0, numberOfBytes);

                ExecutionTime = DateTime.Now;

                Notifications.TransmitEvent();
            }

            return new ModbusOperationInfo(ExecutionTime, null);
        }

        catch (Exception error)
        {
            throw new Exception("Ошибка отправки данных:\n\n" + error.Message + "\n\n" +
                "Таймаут передачи: " +
                (_deviceSerialPort.WriteTimeout == Timeout.Infinite ?
                "бесконечно" : _deviceSerialPort.WriteTimeout.ToString() + " мс."));
        }
    }

    public async Task<ModbusOperationInfo> Receive()
    {
        if (_deviceSerialPort == null)
        {
            return new ModbusOperationInfo(DateTime.Now, Array.Empty<byte>());
        }

        var ReceivedBytes = new List<byte>();

        DateTime ExecutionTime = new DateTime();

        try
        {
            if (IsConnected)
            {
                // т.к. передача данных по СОМ порту медленная, то
                // в первый раз метод Read примет только часть сообщения.
                // Оставшаяся часть сообщения будет считана повторными вызовами метода Read.
                // Метод Read будет вызываться пока не опустеет буфер приема (DeviceSerialPort.BytesToRead = 0).
                // Задержка нужна для того, чтобы буфер приема успел заполниться данными.
                // Для использования небольших скоростей передачи данных (Baud Rate)
                // значение задержки взято с запасом.

                byte[] buffer;

                int numberOfReceivedBytes;

                bool isFirstPackage = true;

                do
                {
                    buffer = new byte[100];

                    numberOfReceivedBytes = _deviceSerialPort.Read(buffer, 0, buffer.Length);

                    if (isFirstPackage)
                    {
                        ExecutionTime = DateTime.Now;
                        isFirstPackage = false;
                    }

                    Array.Resize(ref buffer, numberOfReceivedBytes);

                    ReceivedBytes.AddRange(buffer);

                    await Task.Delay(70);

                } while (_deviceSerialPort.BytesToRead > 0);

                if (ReceivedBytes.Count > 0)
                {
                    Notifications.ReceiveEvent();
                }
            }

            return new ModbusOperationInfo(ExecutionTime, ReceivedBytes.ToArray());
        }

        catch (TimeoutException error)
        {
            throw new TimeoutException(error.Message);
        }

        catch (Exception error)
        {
            throw new Exception("Ошибка приема данных:\n\n" + error.Message + "\n\n" +
                "Таймаут приема: " + _deviceSerialPort.ReadTimeout + " мс.");
        }
    }

    private async Task AsyncThread_Read(Stream? currentStream, CancellationToken readCancel)
    {
        try
        {
            if (currentStream == null)
                throw new InvalidOperationException("Поток чтения не инициализирован.");

            byte[] bufferRX = new byte[65536];

            int numberOfReceiveBytes;

            Task<int> readResult;

            var waitCancel = Task.Delay(Timeout.Infinite, readCancel);

            Task completedTask;

            while (true)
            {
                readCancel.ThrowIfCancellationRequested();

                /// Метод асинхронного чтения у объекта класса Stream, 
                /// который содержится в объекте класса SerialPort,
                /// почему то не обрабатывает событие отмены у токена отмены.
                /// Возможно это происходит из - за того что внутри метода происходят 
                /// неуправляемые вызовы никоуровневого API.
                /// Поэтому для отслеживания состояния токена отмены была создана задача WaitCancel.

                readResult = currentStream.ReadAsync(bufferRX, 0, bufferRX.Length, readCancel);

                completedTask = await Task.WhenAny(readResult, waitCancel).ConfigureAwait(false);

                if (completedTask == waitCancel)
                    throw new OperationCanceledException();

                numberOfReceiveBytes = readResult.Result;

                DataReceived?.Invoke(this, bufferRX.Take(numberOfReceiveBytes).ToArray());

                Notifications.ReceiveEvent();

                Array.Clear(bufferRX, 0, numberOfReceiveBytes);
            }
        }

        catch (OperationCanceledException)
        {
            //  Возникает при отмене задачи.
            //  По правилам отмены асинхронных задач это исключение можно игнорировать.
        }

        catch (Exception error)
        {
            ErrorInReadThread?.Invoke(this, error);
        }
    }
}
