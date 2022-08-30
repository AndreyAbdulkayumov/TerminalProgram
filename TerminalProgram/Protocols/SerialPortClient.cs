using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalProgram.Protocols
{
    public class SerialPortClient : IConnection
    {
        public event EventHandler<DataFromDevice> DataReceived;

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

        private SerialPort DeviceSerialPort = null;

        public void Connect(ConnectionInfo Info)
        {
            try
            {
                if (Info.SerialPort.COM_Port == null ||
                Info.SerialPort.BaudRate == null ||
                Info.SerialPort.Parity == null ||
                Info.SerialPort.DataBits == null ||
                Info.SerialPort.StopBits == null)
                {
                    throw new Exception(
                        (Info.SerialPort.COM_Port == null ? "Не задан СОМ порт.\n" : "") +
                        (Info.SerialPort.BaudRate == null ? "Не задано BaudRate.\n" : "") +
                        (Info.SerialPort.Parity == null ? "Не задано Parity.\n" : "") +
                        (Info.SerialPort.DataBits == null ? "Не задано DataBits\n" : "") +
                        (Info.SerialPort.StopBits == null ? "Не задано StopBits\n" : "")
                        );
                }

                DeviceSerialPort = new SerialPort();

                if (Int32.TryParse(Info.SerialPort.BaudRate, out int BaudRate) == false)
                {
                    throw new Exception("Не удалось преобразовать значение BaudRate в целочисленное значение.\n" +
                        "Полученное значение BaudRate: " + Info.SerialPort.BaudRate);
                }

                Parity SelectedParity;

                switch (Info.SerialPort.Parity)
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

                if (Int32.TryParse(Info.SerialPort.DataBits, out int DataBits) == false)
                {
                    throw new Exception("Не удалось преобразовать значение DataBits в целочисленное значение.\n" +
                        "Полученное значение DataBits: " + Info.SerialPort.DataBits);
                }

                StopBits SelectedStopBits;

                switch (Info.SerialPort.StopBits)
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

                try
                {
                    DeviceSerialPort.PortName = Info.SerialPort.COM_Port;
                    DeviceSerialPort.BaudRate = BaudRate;
                    DeviceSerialPort.Parity = SelectedParity;
                    DeviceSerialPort.DataBits = DataBits;
                    DeviceSerialPort.StopBits = SelectedStopBits;

                    DeviceSerialPort.WriteTimeout = 500;
                    DeviceSerialPort.ReadTimeout = 1000;

                    DeviceSerialPort.Open();
                }

                catch (ArgumentException)
                {
                    Disconnect();
                    throw new Exception("Неправильно задано имя COM порта.");
                }

                catch (UnauthorizedAccessException)
                {
                    Disconnect();
                    throw new Exception("Отказ в доступе к СОМ порту.");
                }

                catch (InvalidOperationException)
                {
                    throw new Exception("СОМ порт уже открыт");
                }

                catch (IOException)
                {
                    Disconnect();
                    throw new Exception("Не удалось подключиться к СОМ порту: " + Info.SerialPort.COM_Port);
                }

                catch (Exception error)
                {
                    Disconnect();
                    throw new Exception(error.Message);
                }

                DeviceSerialPort.DataReceived += DeviceSerialPort_DataReceived;
            }

            catch (Exception error)
            {
                throw new Exception("Ошибка подключения:\n" + error.Message);
            }
        }

        private void DeviceSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (DataReceived != null)
            {
                DataFromDevice Data = new DataFromDevice
                {
                    RX = DeviceSerialPort.ReadExisting()
                };

                DataReceived(this, Data);
            }
        }

        public void Disconnect()
        {
            try
            {
                if (DeviceSerialPort != null && DeviceSerialPort.IsOpen)
                {
                    DeviceSerialPort.Close();
                }
            }

            catch(Exception error)
            {
                throw new Exception("Не удалось отключиться от СОМ порта.\n\n" + error.Message);
            }
         }

        public void Send(string Message)
        {
            try
            {
                DeviceSerialPort.Write(Message);
            }

            catch (Exception error)
            {
                throw new Exception("Ошибка при отправке сообщения: " + Message + "\n\n" + error.Message);
            }
        }

        public void Send(byte[] Message)
        {
            try
            {
                DeviceSerialPort.Write(Message, 0, Message.Length);
            }

            catch (Exception error)
            {
                throw new Exception("Ошибка при отправке сообщения: " + Message + "\n\n" + error.Message);
            }
        }

        public void Receive(byte[] Data)
        {
            try
            {
                Task.Delay(100);
                DeviceSerialPort.Read(Data, 0, DeviceSerialPort.BytesToRead);
            }

            catch(Exception error)
            {
                throw new Exception("Ошибка при приеме сообщения.\n\n" + error.Message);
            }
        }

        
    }
}
