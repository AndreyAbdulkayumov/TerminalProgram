using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Clients
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
        /// Событие ошибки в потоке чтения (асинхронный режим). 
        /// После появления события чтение заканчивается.
        /// </summary>
        event EventHandler<string> ErrorInReadThread;

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
        public readonly byte[] RX;

        public DataFromDevice(int RX_ArrayLength)
        {
            RX = new byte[RX_ArrayLength];
        }
    }

    public interface ITypeOfInfo
    {

    }

    public class SocketInfo : ITypeOfInfo
    {
        public string? IP;
        public string? Port;

        public SocketInfo(string? IP, string? Port)
        {
            this.IP = IP;
            this.Port = Port;
        }
    }

    public class SerialPortInfo : ITypeOfInfo
    {
        public string? COM_Port;
        public string? BaudRate;
        public string? Parity;
        public string? DataBits;
        public string? StopBits;

        public SerialPortInfo(string? COM_Port, string? BaudRate, string? Parity, string? DataBits, string? StopBits)
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
        public ITypeOfInfo Info;

        public readonly Encoding GlobalEncoding;

        public ConnectionInfo(SocketInfo Info, Encoding GlobalEncoding)
        {
            this.Info = Info;
            this.GlobalEncoding = GlobalEncoding;
        }

        public ConnectionInfo(SerialPortInfo Info, Encoding GlobalEncoding)
        {
            this.Info = Info;
            this.GlobalEncoding = GlobalEncoding;
        }
    }
}
