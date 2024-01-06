using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Models;

namespace Core.Clients
{
    public class SerialPortClient : IConnection
    {
        public event EventHandler<DataFromDevice>? DataReceived;
        public event EventHandler<string>? ErrorInReadThread;

        public bool IsConnected
        {
            get
            {
                if (DeviceSerialPort == null || DeviceSerialPort.IsOpen == false)
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
                if (DeviceSerialPort != null)
                {
                    return DeviceSerialPort.WriteTimeout;
                }

                return 0;
            }

            set
            {
                if (DeviceSerialPort != null)
                {
                    DeviceSerialPort.WriteTimeout = value;
                }
            }
        }

        public int ReadTimeout
        {
            get
            {
                if (DeviceSerialPort != null)
                {
                    return DeviceSerialPort.ReadTimeout;
                }

                return 0;
            }

            set
            {
                if (DeviceSerialPort != null)
                {
                    DeviceSerialPort.ReadTimeout = value;
                }
            }
        }

        private SerialPort? DeviceSerialPort = null;

        private Task? ReadThread = null;
        private CancellationTokenSource? ReadCancelSource = null;


        public void SetReadMode(ReadMode Mode)
        {
            switch (Mode)
            {
                case ReadMode.Async:

                    if (DeviceSerialPort != null && IsConnected)
                    {
                        ReadCancelSource = new CancellationTokenSource();

                        DeviceSerialPort.BaseStream.WriteTimeout = 500;
                        DeviceSerialPort.BaseStream.ReadTimeout = -1;   // Бесконечно

                        ReadThread = Task.Run(() => AsyncThread_Read(DeviceSerialPort.BaseStream, ReadCancelSource.Token));
                    }

                    break;

                case ReadMode.Sync:

                    if (DeviceSerialPort != null && IsConnected)
                    {
                        ReadCancelSource?.Cancel();

                        if (ReadThread != null)
                        {
                            Task.WaitAll(ReadThread);
                        }

                        DeviceSerialPort.DiscardInBuffer();
                        DeviceSerialPort.DiscardOutBuffer();
                    }

                    break;

                default:
                    throw new Exception("У клиента задан неизвестный режим чтения: " + Mode.ToString());
            }
        }

        public void Connect(ConnectionInfo Information)
        {
            SerialPortInfo? PortInfo = Information.Info as SerialPortInfo;

            try
            {
                if (PortInfo == null)
                {
                    throw new Exception("Нет информации о настройках подключения по последовательному порту.");
                }

                if (PortInfo.COM_Port == null || PortInfo.COM_Port == String.Empty ||
                    PortInfo.BaudRate == null || PortInfo.BaudRate == String.Empty ||
                    PortInfo.Parity == null || PortInfo.Parity == String.Empty ||
                    PortInfo.DataBits == null || PortInfo.DataBits == String.Empty ||
                    PortInfo.StopBits == null || PortInfo.StopBits == String.Empty)
                {
                    throw new Exception(
                        (PortInfo.COM_Port == null || PortInfo.COM_Port == String.Empty ? "Не задан СОМ порт.\n" : "") +
                        (PortInfo.BaudRate == null || PortInfo.BaudRate == String.Empty ? "Не задан BaudRate.\n" : "") +
                        (PortInfo.Parity == null || PortInfo.Parity == String.Empty ? "Не задан Parity.\n" : "") +
                        (PortInfo.DataBits == null || PortInfo.DataBits == String.Empty ? "Не задан DataBits\n" : "") +
                        (PortInfo.StopBits == null || PortInfo.StopBits == String.Empty ? "Не задан StopBits\n" : "")
                        );
                }

                DeviceSerialPort = new SerialPort();

                if (int.TryParse(PortInfo.BaudRate, out int BaudRate) == false)
                {
                    throw new Exception("Не удалось преобразовать значение BaudRate в целочисленное значение.\n" +
                        "Полученное значение BaudRate: " + PortInfo.BaudRate);
                }

                Parity SelectedParity;

                switch (PortInfo.Parity)
                {
                    case "None":
                        SelectedParity = Parity.None;
                        break;

                    case "Even":
                        SelectedParity = Parity.Even;
                        break;

                    case "Odd":
                        SelectedParity = Parity.Odd;
                        break;

                    default:
                        throw new Exception("Неправильно задано значение Parity.");
                }

                if (int.TryParse(PortInfo.DataBits, out int DataBits) == false)
                {
                    throw new Exception("Не удалось преобразовать значение DataBits в целочисленное значение.\n" +
                        "Полученное значение DataBits: " + PortInfo.DataBits);
                }

                StopBits SelectedStopBits;

                switch (PortInfo.StopBits)
                {
                    case "0":
                        SelectedStopBits = StopBits.None;
                        break;

                    case "1":
                        SelectedStopBits = StopBits.One;
                        break;

                    case "1.5":
                        SelectedStopBits = StopBits.OnePointFive;
                        break;

                    case "2":
                        SelectedStopBits = StopBits.Two;
                        break;

                    default:
                        throw new Exception("Неправильно задано значение StopBits");
                }

                DeviceSerialPort.PortName = PortInfo.COM_Port;
                DeviceSerialPort.BaudRate = BaudRate;
                DeviceSerialPort.Parity = SelectedParity;
                DeviceSerialPort.DataBits = DataBits;
                DeviceSerialPort.StopBits = SelectedStopBits;

                DeviceSerialPort.Open();
            }

            catch (Exception error)
            {
                DeviceSerialPort?.Close();

                string CommonMessage = "Не удалось подключиться к СОМ порту.\n\n";

                if (PortInfo != null)
                {
                    throw new Exception(CommonMessage +
                        "Данные подключения:" + "\n" +
                        "COM - Port: " + PortInfo.COM_Port + "\n" +
                        "BaudRate: " + PortInfo.BaudRate + "\n" +
                        "Parity: " + PortInfo.Parity + "\n" +
                        "DataBits: " + PortInfo.DataBits + "\n" +
                        "StopBits: " + PortInfo.StopBits + "\n\n" +
                        error.Message);
                }

                throw new Exception(CommonMessage + error.Message);
            }
        }

        public async Task Disconnect()
        {
            try
            {
                if (DeviceSerialPort != null && DeviceSerialPort.IsOpen)
                {
                    ProtocolMode? SelectedProtocol = ConnectedHost.SelectedProtocol;

                    if (SelectedProtocol != null && SelectedProtocol.CurrentReadMode == ReadMode.Async)
                    {
                        ReadCancelSource?.Cancel();

                        if (ReadThread != null)
                        {
                            await Task.WhenAll(ReadThread).ConfigureAwait(false);

                            await Task.Delay(100);
                        }
                    }

                    DeviceSerialPort.Close();
                }
            }

            catch (Exception error)
            {
                throw new Exception("Не удалось отключиться от СОМ порта.\n\n" + error.Message);
            }
        }

        public void Send(byte[] Message, int NumberOfBytes)
        {
            if (DeviceSerialPort == null)
            {
                return;
            }

            try
            {
                if (IsConnected)
                {
                    DeviceSerialPort.Write(Message, 0, NumberOfBytes);
                }
            }

            catch (Exception error)
            {
                throw new Exception("Ошибка отправки данных:\n\n" + error.Message + "\n\n" +
                    "Таймаут передачи: " +
                    (DeviceSerialPort.WriteTimeout == Timeout.Infinite ?
                    "бесконечно" : DeviceSerialPort.WriteTimeout.ToString() + " мс."));
            }
        }

        public int Receive(byte[] Data)
        {
            if (DeviceSerialPort == null)
            {
                return 0;
            }

            int NumberOfReceivedBytes = 0;

            try
            {
                if (IsConnected)
                {
                    // т.к. передача данных по СОМ порту медленная, то
                    // в первый раз метод Read примет только часть сообщения.
                    // Оставшаяся часть сообщения будет считана повторным вызовом метода Read.
                    // Задержка нужна для того, чтобы буфер приема успел заполниться данными.
                    // Для использования небольших скоростей передачи данных (Baud Rate)
                    // значение задержки взято с запасом.

                    int FirstBytes = DeviceSerialPort.BytesToRead;

                    if (Data.Length > FirstBytes)
                    {
                        NumberOfReceivedBytes = FirstBytes;

                        DeviceSerialPort.Read(Data, 0, FirstBytes);

                        Thread.Sleep(50);

                        NumberOfReceivedBytes += DeviceSerialPort.BytesToRead;

                        DeviceSerialPort.Read(Data, FirstBytes, Data.Length);
                    }
                    
                    else
                    {
                        NumberOfReceivedBytes = Data.Length;

                        DeviceSerialPort.Read(Data, 0, Data.Length);
                    }
                }

                return NumberOfReceivedBytes;
            }

            catch (Exception error)
            {
                throw new Exception("Ошибка приема данных:\n\n" + error.Message + "\n\n" +
                    "Таймаут приема: " + DeviceSerialPort.ReadTimeout + " мс.");
            }
        }

        private async Task AsyncThread_Read(Stream CurrentStream, CancellationToken ReadCancel)
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
                        await Task.Delay(50, ReadCancel);
                    }
                });

                Task CompletedTask;

                while (true)
                {
                    ReadCancel.ThrowIfCancellationRequested();

                    if (CurrentStream != null)
                    {
                        /// Метод асинхронного чтения у объекта класса Stream, 
                        /// который содержится в объекте класса SerialPort,
                        /// почему то не обрабатывает событие отмены у токена отмены.
                        /// Возможно это происходит из - за того что внутри метода происходят 
                        /// неуправляемые вызовы никоуровневого API.
                        /// Поэтому для отслеживания состояния токена отмены была создана задача WaitCancel.

                        ReadResult = CurrentStream.ReadAsync(BufferRX, 0, BufferRX.Length, ReadCancel);

                        CompletedTask = await Task.WhenAny(ReadResult, WaitCancel).ConfigureAwait(false);

                        ReadCancel.ThrowIfCancellationRequested();

                        if (CompletedTask == WaitCancel)
                        {
                            throw new OperationCanceledException();
                        }

                        NumberOfReceiveBytes = ReadResult.Result;

                        DataFromDevice Data = new DataFromDevice(NumberOfReceiveBytes);

                        for (int i = 0; i < NumberOfReceiveBytes; i++)
                        {
                            Data.RX[i] = BufferRX[i];
                        }

                        ReadCancel.ThrowIfCancellationRequested();

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

            catch (Exception error)
            {
                ErrorInReadThread?.Invoke(this, "Возникла ошибка при асинхронном чтении у SerialPort клиента.\n\n" +
                    "Прием данных прекращен.\n\n" + error.Message);
            }
        }
    }
}
