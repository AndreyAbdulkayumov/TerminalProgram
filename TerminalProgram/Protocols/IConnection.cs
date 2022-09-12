using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalProgram.Protocols
{
    public interface IConnection
    {
        event EventHandler<DataFromDevice> DataReceived;

        bool IsConnected { get; }

        void Connect(ConnectionInfo Info);
        void Disconnect();
        void Send(string Message);
        void Send(byte[] Message);
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

        //  Значение -1 обознает бесконечный таймаут
        public readonly int TimeoutWrite;
        public readonly int TimeoutRead;

        public ConnectionInfo(SocketInfo Info)
        {
            Socket = Info;
            TimeoutWrite = -1;
            TimeoutRead = -1;
        }

        public ConnectionInfo(SocketInfo Info, int TimeoutWrite, int TimeoutRead)
        {
            Socket = Info;
            this.TimeoutWrite = TimeoutWrite;
            this.TimeoutRead = TimeoutRead;
        }

        public ConnectionInfo(SerialPortInfo Info) 
        { 
            SerialPort = Info;
            TimeoutWrite = -1;
            TimeoutRead = -1;
        }

        public ConnectionInfo(SerialPortInfo Info, int TimeoutWrite, int TimeoutRead)
        {
            SerialPort = Info;
            this.TimeoutWrite = TimeoutWrite;
            this.TimeoutRead = TimeoutRead;
        }
    }
}
