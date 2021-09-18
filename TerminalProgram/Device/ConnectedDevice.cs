using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;

namespace TerminalProgram.Device
{
    public enum TypeOfConnection
    {
        NoDefined,
        SerialPort,
        Ethernet
    }


    

    public class DataFromDevice : EventArgs
    {
        public string RX;
    }

    public class ConnectedDevice
    {
        /// <summary>
        /// Переменная, хранящая в себе тип соединения с устройством.
        /// </summary>
        public TypeOfConnection SelectedConnection = TypeOfConnection.NoDefined;
        
        /// <summary>
        /// Переменная, показывающая открыто ли соединение с устройством.
        /// <para>true - соединение открыто.</para>
        /// </summary>
        public bool IsConnected { get; private set; } = false;

        private SerialPort DeviceSerialPort = null;
        private Socket DeviceLAN = null;

        private const int TimeoutLANConnection = 1000; // В миллисекундах

        public event EventHandler<DataFromDevice> SerialPortReceived;

        public void Connect(string COM_Port, int BaudRate, string parity, int DataBits, string stopBits)
        {
            try
            {
                SerialPort_Connect(COM_Port, BaudRate, parity, DataBits, stopBits);
                IsConnected = true;
                SelectedConnection = TypeOfConnection.SerialPort;
            }

            catch (Exception error)
            {
                throw new Exception(error.Message);
            }
        }

        public void Connect(string IPstr, int Port)
        {
            try
            {
                Ethernet_Connect(IPstr, Port);
                IsConnected = true;
                SelectedConnection = TypeOfConnection.Ethernet;
            }

            catch (Exception error)
            {
                throw new Exception(error.Message);
            }
        }
        

        public void Disconnect()
        {
            switch (SelectedConnection)
            {
                case TypeOfConnection.SerialPort:
                    SerialPort_Disconnect();
                    break;

                case TypeOfConnection.Ethernet:
                    Ethernet_Disconnect();
                    break;

                default:
                    throw new Exception("Не задан тип соединения или выбран ранее неизвестный.");
            }

            IsConnected = false;
            SelectedConnection = TypeOfConnection.NoDefined;

        }

        private void SerialPort_Connect(string COM_Port, int BaudRate, string parity, int DataBits, string stopBits)
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
                    throw new Exception("Неправильно задана четность у порта " + COM_Port + ".");
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
                    throw new Exception("Неправильно задано колличество стоп - бит у порта " + COM_Port + ".");
            }

            try
            {
                DeviceSerialPort.PortName = COM_Port;
                DeviceSerialPort.BaudRate = BaudRate;
                DeviceSerialPort.Parity = SelectedParity;
                DeviceSerialPort.DataBits = DataBits;
                DeviceSerialPort.StopBits = SelectedStopBits;

                DeviceSerialPort.Open();
            }

            catch (ArgumentException)
            {
                DeviceSerialPort.Close();
                throw new Exception("Неправильно задано имя COM порта (Задано: " + COM_Port + ".");
            }

            catch (UnauthorizedAccessException)
            {
                DeviceSerialPort.Close();
                throw new Exception("Отказ в доступе к порту (" + COM_Port + ").");
            }

            catch (InvalidOperationException)
            {
                throw new Exception("Этот порт (" + COM_Port + ") уже открыт.");
            }

            catch (IOException)
            {
                DeviceSerialPort.Close();
                throw new Exception("Не удалось подключиться к прибору через " + COM_Port + ".");
            }

            catch (Exception error)
            {
                DeviceSerialPort.Close();
                throw new Exception(error.Message);
            }

            DeviceSerialPort.DataReceived += DeviceSerialPort_DataReceived;
        }

        private void DeviceSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (SerialPortReceived != null)
            {
                DataFromDevice Data = new DataFromDevice
                {
                    RX = DeviceSerialPort.ReadExisting()
                };

                SerialPortReceived(this, Data);
            }
        }

        private void SerialPort_Disconnect()
        {
            if (DeviceSerialPort != null && DeviceSerialPort.IsOpen == true)
            {
                DeviceSerialPort.Close();
                DeviceSerialPort = null;
            }
        }

        private void Ethernet_Connect(string IPstr, int Port)
        {
            try
            {
                IPAddress IP = IPAddress.Parse(IPstr);
                DeviceLAN = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IAsyncResult result = DeviceLAN.BeginConnect(IP, Port, null, null);

                bool SuccessConnect = result.AsyncWaitHandle.WaitOne(TimeoutLANConnection, true);

                if (SuccessConnect == true)
                {
                    DeviceLAN.EndConnect(result);
                }

                else
                {
                    Ethernet_Disconnect();
                    throw new Exception("Не удалось подключиться к устройству по Ethernet" +
                        "\nЗаданный IP: " + IPstr +
                        "\nЗаданный порт: " + Port.ToString());
                }
            }

            catch (Exception error)
            {
                throw new Exception(error.Message);
            }

        }

        private void Ethernet_Disconnect()
        {
            if (DeviceLAN != null)
            {
                DeviceLAN.Close();
                DeviceLAN = null;
            }
        }


        public void Send(ref char[] Data)
        {
            switch (SelectedConnection)
            {
                case TypeOfConnection.SerialPort:
                    SerialPort_Send(ref Data);
                    break;

                case TypeOfConnection.Ethernet:
                    Ethernet_Send(ref Data);
                    break;

                default:
                    throw new Exception("Не задан тип соединения или выбран ранее неизвестный.");
            }
        }

        private void SerialPort_Send(ref char[] Data)
        {
            try
            {
                DeviceSerialPort.Write(Data, 0, Data.Length);
            }

            catch(Exception error)
            {
                throw new Exception(error.Message);
            }
            
        }

        private void Ethernet_Send(ref char[] Data)
        {

        }

    }
}
