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

        private Thread ReadThread = null;

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

            if (ReadThread == null)
            {
                ReadThread = new Thread(new ParameterizedThreadStart(ReadThread_Handler))
                {
                    IsBackground = true
                };

                ReadThread.Start();
            }

            IsConnected = true;
        }

        public void Disconnect()
        {
            if (ReadThread != null)
            {
                ReadThread = null;
            }

            if (Stream != null)
            {
                Stream.Close();
            }
                
            if (Client != null)
            {
                Client.Close();
            }            

            IsConnected = false;                
        }

        public void Send(string Message)
        {
            if (IsConnected)
            {
                byte[] Data = Encoding.ASCII.GetBytes(Message);

                Stream.WriteAsync(Data, 0, Data.Length);
            }
        }

        public void Send(byte[] Message)
        {
            if (IsConnected)
            {
                Stream.WriteAsync(Message, 0, Message.Length);
            }
        }

        public void Receive(byte[] RX)
        {
            if (IsConnected)
            {
                Stream.ReadTimeout = 1000;
                Stream.Read(RX, 0, RX.Length);
                Stream.ReadTimeout = Timeout.Infinite;
            }
        }

        private void ReadThread_Handler(object x)
        {
            try
            {
                byte[] BufferRX = new byte[50]; 
                
                while(true)
                {
                    if (DataReceived != null && Stream != null)
                    {
                        Stream.Read(BufferRX, 0, BufferRX.Length);

                        DataFromDevice Data = new DataFromDevice();

                        Data.RX = new byte[BufferRX.Length];

                        for(int i = 0; i < BufferRX.Length; i++)
                        {
                            Data.RX[i] = BufferRX[i];
                        }

                        DataReceived(this, Data);

                        Array.Clear(BufferRX, 0, BufferRX.Length);
                    }
                }
            }

            catch(Exception error)
            {
                // TODO: Как правильно обработать это исключение?

                //Disconnect();

                //MessageBox.Show("Возникла ошибка при асинхронном чтении у IP клиента.\n\n" + error.Message +
                //    "\n\nКлиент был отключен.", "Ошибка",
                //    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
