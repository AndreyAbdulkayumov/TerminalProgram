using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
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

        private SerialPort DeviceSerialPort = null;


        public void SetReadMode(ReadMode Mode)
        {
            switch(Mode)
            {
                case ReadMode.Async:
                    DeviceSerialPort.DataReceived += DeviceSerialPort_DataReceived;
                    break;

                case ReadMode.Sync:
                    DeviceSerialPort.DataReceived -= DeviceSerialPort_DataReceived;
                    break;

                default:
                    throw new Exception("У клиента задан неизвестный режим чтения: " + Mode.ToString());
            }
        }

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
            }

            catch (Exception error)
            {
                throw new Exception("Ошибка подключения:\n" + error.Message);
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
                throw new Exception("Ошибка отправки данных:\n\n" + error.Message + "\n\n" +
                    "Таймаут передачи: " +
                    (DeviceSerialPort.WriteTimeout == Timeout.Infinite ?
                    "бесконечно" : (DeviceSerialPort.WriteTimeout.ToString() + " мс.")));
            }
        }

        public void Send(byte[] Message, int NumberOfBytes)
        {
            try
            {
                DeviceSerialPort.Write(Message, 0, NumberOfBytes);
            }

            catch (Exception error)
            {
                throw new Exception("Ошибка отправки данных:\n\n" + error.Message + "\n\n" +
                    "Таймаут передачи: " +
                    (DeviceSerialPort.WriteTimeout == Timeout.Infinite ?
                    "бесконечно" : (DeviceSerialPort.WriteTimeout.ToString() + " мс.")));
            }
        }

        public void Receive(byte[] Data)
        {
            int SavedTimeout = ReadTimeout;

            try
            {
                // Если ожидать ответа с помощью таймаута, то в случае получения данных примется
                // только несколько первых байт. Поэтому таймаут реализуется с помощью задержки.
                // Таким образом, буфер приема успеет заполниться.
                
                ReadTimeout = 10;

                Thread.Sleep(SavedTimeout);

                DeviceSerialPort.Read(Data, 0, Data.Length);

                ReadTimeout = SavedTimeout;
            }

            catch (Exception error)
            {
                throw new Exception("Ошибка приема данных:\n\n" + error.Message + "\n\n" +
                    "Таймаут приема: " + SavedTimeout + " мс.");
            }
        }

        private void DeviceSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (DataReceived != null)
                {
                    DataFromDevice Data = new DataFromDevice()
                    {
                        RX = new byte[DeviceSerialPort.BytesToRead]
                    };

                    DeviceSerialPort.Read(Data.RX, 0, Data.RX.Length);

                    DataReceived(this, Data);
                }
            }

            catch (Exception error)
            {
                throw new Exception("Ошибка приема данных:\n\n" + error.Message + "\n\n" +
                    "Таймаут приема: " +
                    (DeviceSerialPort.ReadTimeout == Timeout.Infinite ?
                    "бесконечно" : (DeviceSerialPort.ReadTimeout.ToString() + " мс.")));
            }
        }
    }
}
