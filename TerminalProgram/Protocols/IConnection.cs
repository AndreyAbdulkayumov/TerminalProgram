using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalProgram.Protocols
{
    public enum ReadMode
    {
        Async,
        Sync
    }

    public interface IConnection
    {
        /// <summary>
        /// Событие получения данных в режиме асинхронного чтения.
        /// </summary>
        event EventHandler<DataFromDevice> DataReceived;

        /// <summary>
        /// Возращает значение указывающее на то, подключен ли сейчас клиент к какому либо хосту или нетю
        /// </summary>
        /// <returns>Значение true. если клиент подключен.</returns>
        bool IsConnected { get; }

        /// <summary>
        /// Возвращает или задает время ожидания окончания записи в милисекундах.
        /// </summary>
        /// <returns>Возращает время ожидания выполнения операции записи в милисекундах</returns>
        int WriteTimeout { get; set; }
        /// <summary>
        /// Возвращает или задает время ожидания данных для чтения в милисекундах.
        /// </summary>
        /// <returns>Возращает время ожидания появления данных для чтения в милисекундах</returns>
        int ReadTimeout { get; set; }

        /// <summary>
        /// Установка синхронного или асинхронного режима чтения.
        /// </summary>
        /// <param name="Mode"></param>
        void SetReadMode(ReadMode Mode);
        /// <summary>
        /// Подключение к указанному хосту.
        /// </summary>
        /// <param name="Info"></param>
        void Connect(ConnectionInfo Info);
        /// <summary>
        /// Закрытие открытого соединения.
        /// </summary>
        Task Disconnect();
        /// <summary>
        /// Запись определенного колличества байт в открытое соединение.
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="NumberOfBytes"></param>
        void Send(byte[] Message, int NumberOfBytes);
        /// <summary>
        /// Сихронно считывает данные из соединения.
        /// </summary>
        /// <param name="Data"></param>
        void Receive(byte[] Data);
    }

    public class DataFromDevice : EventArgs
    {
        public byte[] RX;
    }

    public class SocketInfo
    {
        public string IP = null;
        public string Port = null;

        public SocketInfo(string IP, string Port)
        {
            this.IP = IP;
            this.Port = Port;
        }
    }

    public class SerialPortInfo
    {
        public string COM_Port = null;
        public string BaudRate = null;
        public string Parity = null;
        public string DataBits = null;
        public string StopBits = null;

        public SerialPortInfo(string COM_Port, string BaudRate, string Parity, string DataBits, string StopBits)
        {
            this.COM_Port = COM_Port;
            this.BaudRate = BaudRate;
            this.Parity = Parity;
            this.DataBits = DataBits;
            this.StopBits = StopBits;
        }
    }

    public class ConnectionInfo
    {
        public SocketInfo Socket;
        public SerialPortInfo SerialPort;

        public readonly Encoding GlobalEncoding;

        public ConnectionInfo(SocketInfo Info, Encoding GlobalEncoding)
        {
            Socket = Info;
            this.GlobalEncoding = GlobalEncoding;
        }

        public ConnectionInfo(SerialPortInfo Info, Encoding GlobalEncoding) 
        { 
            SerialPort = Info;
            this.GlobalEncoding = GlobalEncoding;
        }
    }
}
