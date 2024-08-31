﻿using System.Text;

namespace Core.Clients
{
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
        public readonly ITypeOfInfo Info;

        public readonly Encoding GlobalEncoding;

        public ConnectionInfo(SocketInfo info, Encoding globalEncoding)
        {
            Info = info;
            GlobalEncoding = globalEncoding;
        }

        public ConnectionInfo(SerialPortInfo info, Encoding globalEncoding)
        {
            Info = info;
            GlobalEncoding = globalEncoding;
        }
    }
}