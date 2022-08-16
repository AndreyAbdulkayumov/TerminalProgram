using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace TerminalProgram.Protocols
{
    public enum CommunicationInterface
    {
        SerialPort,
        Ethernet
    }

    public class DataFromDevice : EventArgs
    {
        public string RX;
    }

    public class Connection
    {
        public string DeviceName { get; set; }

        /// <summary>
        /// Переменная, показывающая открыто ли соединение с устройством.
        /// <para>true - соединение открыто.</para>
        /// </summary>
        public bool IsConnected { get; private set; } = false;
        /// <summary>
        /// Переменная, показывающая была ли ошибка соединения с устройством. true - была ошибка.
        /// </summary>
        public bool ErrorConnection { get; private set; } = false;
        /// <summary>
        /// Переменная, показывающая тип подключения. Известные типы находятся в перечислении CommunicationInterface в базовом классе.
        /// </summary>
        public CommunicationInterface TypeOfConnection { get; private set; }

        private SerialPort DeviceSerialPort = null;
        private Socket DeviceLAN = null;

        public event EventHandler<DataFromDevice> AsyncDataReceived;

        private const int ReadBufferSize = 40;
        private byte[] ReceivedBytes = new byte[ReadBufferSize];

        private const int TIMER_INTERVAL_MS = 10;
        private const int TIMEOUT_CONNECTION_SEC = 200;
        private System.Timers.Timer ConnectionTimeoutTimer = new System.Timers.Timer(TIMER_INTERVAL_MS);
        private int TimeoutCounter = 0;

        private void ConnectionTimeoutTimer_Tick(object sender, EventArgs e)
        {
            TimeoutCounter++;

            if (TimeoutCounter == TIMEOUT_CONNECTION_SEC)
            {
                ConnectionTimeoutTimer.Stop();
                ErrorConnection = true;
            }
        }

        private void WaitResponse()
        {
            TimeoutCounter = 0;
            ErrorConnection = false;
            ConnectionTimeoutTimer.Start();

            if (DeviceSerialPort != null)
            {
                do
                {
                    if (ErrorConnection == true)
                    {
                        ErrorConnection = false;
                        TimeoutCounter = 0;
                        Disconnect();
                        throw new Exception("Прибор " + DeviceName + " не отвечает.");
                    }
                } while (DeviceSerialPort.BytesToRead == 0);
            }
            
            else if (DeviceLAN != null)
            {
                do
                {
                    if (ErrorConnection == true)
                    {
                        ErrorConnection = false;
                        TimeoutCounter = 0;
                        Disconnect();
                        throw new Exception("Прибор " + DeviceName + " не отвечает.");
                    }
                } while (DeviceLAN.Available == 0);
            }

            else
            {
                throw new Exception("Неизвестный тип подключения у " + DeviceName);
            }

            TimeoutCounter = 0;
            ConnectionTimeoutTimer.Stop();
        }

        /// <summary>
        /// Инициализации и открытие СОМ - порта.
        /// </summary>
        /// <param name="COM_Port"></param>
        /// <param name="BaudRate"></param>
        /// <param name="parity"></param>
        /// <param name="DataBits"></param>
        /// <param name="stopBits"></param>
        public void Connect(string COM_Port, int BaudRate, string parity, int DataBits, string stopBits)
        {
            InitAndOpenPort(COM_Port, BaudRate, parity, DataBits, stopBits);
            IsConnected = true;
        }
        /// <summary>
        /// Инициализации и открытие сокета.
        /// </summary>
        /// <param name="IPstr"></param>
        /// <param name="Port"></param>
        public void Connect(string IPstr, int Port)
        {
            CreateAndOpenSocket(IPstr, Port);
            IsConnected = true;
        }

        private void InitAndOpenPort(string COM_Port, int BaudRate, string parity, int DataBits, string stopBits)
        {
            DeviceSerialPort = new SerialPort();

            Parity SelectedParity;

            switch (parity)
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
                    throw new Exception("Неправильно задана четность для " + DeviceName);
            }

            StopBits SelectedStopBits;

            switch (stopBits)
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
                    throw new Exception("Неправильно задано колличество стоп - бит для " + DeviceName);
            }

            try
            {
                DeviceSerialPort.PortName = COM_Port;
                DeviceSerialPort.BaudRate = BaudRate;
                DeviceSerialPort.Parity = SelectedParity;
                DeviceSerialPort.DataBits = DataBits;
                DeviceSerialPort.StopBits = SelectedStopBits;

                ConnectionTimeoutTimer = new System.Timers.Timer(TIMER_INTERVAL_MS);
                ConnectionTimeoutTimer.Elapsed += ConnectionTimeoutTimer_Tick;
                ConnectionTimeoutTimer.Enabled = true;
                ConnectionTimeoutTimer.Stop();

                DeviceSerialPort.Open();
            }

            catch (ArgumentException)
            {
                ClosePort();
                throw new Exception("Неправильно задано имя COM порта для " + DeviceName);
            }

            catch (UnauthorizedAccessException)
            {
                ClosePort();
                throw new Exception("Отказ в доступе к порту для " + DeviceName);
            }

            catch (InvalidOperationException)
            {
                throw new Exception("Порт для " + DeviceName + " уже открыт");
            }

            catch (IOException)
            {
                ClosePort();
                throw new Exception("Не удалось подключиться к " + DeviceName);
            }

            catch (Exception error)
            {
                ClosePort();
                throw new Exception(error.Message);
            }

            ErrorConnection = false;
            TypeOfConnection = CommunicationInterface.SerialPort;

            DeviceSerialPort.DataReceived += SerialPort_AsyncReceived;
        }

        private void CreateAndOpenSocket(string IPstr, int Port)
        {
            IPAddress IP = IPAddress.Parse(IPstr);
            DeviceLAN = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            byte[] RX = new byte[10];

            IAsyncResult result = DeviceLAN.BeginConnect(IP, Port, null, null);

            bool SuccessConnect = result.AsyncWaitHandle.WaitOne(500, true);

            if (SuccessConnect == true)
            {
                DeviceLAN.EndConnect(result);
            }

            else
            {
                DeviceLAN.Close();
                throw new Exception("Не удалось подключиться к устройству " + DeviceName + " по Ethernet.");
            }

            ErrorConnection = false;
            TypeOfConnection = CommunicationInterface.Ethernet;
        }

        /// <summary>
        /// Закрытие соединения с устройством
        /// </summary>
        public void Disconnect()
        {
            switch (TypeOfConnection)
            {
                case CommunicationInterface.SerialPort:
                    ClosePort();
                    break;

                case CommunicationInterface.Ethernet:
                    CloseSocket();
                    break;

                default:
                    throw new Exception("Попытка закрыть соединение по неизвестному или неопределенному интерфейсу");
            }

            IsConnected = false;
        }

        private void ClosePort()
        {
            if (DeviceSerialPort != null && DeviceSerialPort.IsOpen == true)
            {
                DeviceSerialPort.Close();
            }
        }

        private void CloseSocket()
        {
            if (DeviceLAN != null)
            {
                DeviceLAN.Close();
            }
        }
        
        public void Send(string command)
        {
            try
            {
                if (TypeOfConnection == CommunicationInterface.SerialPort)
                {
                    DeviceSerialPort.WriteLine(command);
                }

                else if (TypeOfConnection == CommunicationInterface.Ethernet)
                {
                    DeviceLAN.Send(Encoding.ASCII.GetBytes(command));
                }

                else
                {
                    throw new Exception("Попытка обратиться к соединению по неизвестному или неопределенному интерфейсу");
                }
            }

            catch (Exception error)
            {
                throw new Exception(error.Message + "\nОшибка при отправке команды " + command +
                    "\nУстройство: " + DeviceName + "\nИнтерфейс связи: " + TypeOfConnection.ToString());
            }
        }

        public void Send(byte[] BytesArray)
        {
            try
            {
                if (TypeOfConnection == CommunicationInterface.SerialPort)
                {
                    DeviceSerialPort.Write(BytesArray, 0, BytesArray.Length);
                }

                else if (TypeOfConnection == CommunicationInterface.Ethernet)
                {
                    DeviceLAN.Send(BytesArray);
                }

                else
                {
                    throw new Exception("Попытка обратиться к соединению по неизвестному или неопределенному интерфейсу");
                }
            }

            catch (Exception error)
            {
                throw new Exception(error.Message + "\nОшибка при отправке команды " + BytesArray.ToString() +
                    "\nУстройство: " + DeviceName + "\nИнтерфейс связи: " + TypeOfConnection.ToString());
            }
        }

        private void SerialPort_AsyncReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (AsyncDataReceived != null)
            {
                DataFromDevice Data = new DataFromDevice
                {
                    RX = DeviceSerialPort.ReadExisting()
                };

                AsyncDataReceived(this, Data);
            }
        }

        public string Receive()
        {
            try
            {
                WaitResponse();

                if (TypeOfConnection == CommunicationInterface.SerialPort)
                {
                    if (DeviceSerialPort.BytesToRead > ReadBufferSize)
                    {
                        throw new Exception("Превышение указанного размера буфера. Возможно следует вручную увеличить его объем.");
                    } 

                    DeviceSerialPort.Read(ReceivedBytes, 0, DeviceSerialPort.BytesToRead);
                }

                else if (TypeOfConnection == CommunicationInterface.Ethernet)
                {
                    DeviceLAN.Receive(ReceivedBytes);
                }

                else
                {
                    throw new Exception("Попытка обратиться к соединению по неизвестному или неопределенному интерфейсу");
                }

                return BitConverter.ToString(ReceivedBytes);
            }

            catch (Exception error)
            {
                throw new Exception(error.Message + "\nОшибка при попытке чтения ответа." +
                    "\nУстройство: " + DeviceName + "\nИнтерфейс связи: " + TypeOfConnection.ToString());
            } 
        }

        public void Receive(byte[] RX)
        {
            try
            {
                WaitResponse();

                if (TypeOfConnection == CommunicationInterface.SerialPort)
                {
                    DeviceSerialPort.Read(RX, 0, DeviceSerialPort.BytesToRead);
                }

                else if (TypeOfConnection == CommunicationInterface.Ethernet)
                {
                    DeviceLAN.Receive(RX);
                }

                else
                {
                    throw new Exception("Попытка обратиться к соединению по неизвестному или неопределенному интерфейсу");
                }
            }

            catch (Exception error)
            {
                throw new Exception(error.Message + "\nОшибка при попытке чтения ответа." +
                    "\nУстройство: " + DeviceName + "\nИнтерфейс связи: " + TypeOfConnection.ToString());
            }
        }

    }
}
