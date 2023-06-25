using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class SerialPort_Info
    {
        public string? COMPort { get; set; }
        public string? BaudRate { get; set; }
        public bool BaudRate_IsCustom { get; set; }
        public string? BaudRate_Custom { get; set; }
        public string? Parity { get; set; }
        public string? DataBits { get; set; }
        public string? StopBits { get; set; }
    }

    public class IP_Info
    {
        public string? IP_Address { get; set; }
        public string? Port { get; set; }
    }

    public class DeviceData : ICloneable
    {
        public const string ConnectionName_SerialPort = "SerialPort";
        public const string ConnectionName_Ethernet = "Ethernet";

        public string? GlobalEncoding { get; set; }

        public string? TimeoutWrite { get; set; }
        public string? TimeoutRead { get; set; }

        public string? TypeOfConnection { get; set; }

        public SerialPort_Info? Connection_SerialPort { get; set; }
        public IP_Info? Connection_IP { get; set; }

        public object Clone()
        {
            SerialPort_Info? SerialPort;

            if (Connection_SerialPort != null)
            {
                SerialPort = new SerialPort_Info()
                {
                    COMPort = this.Connection_SerialPort.COMPort,
                    BaudRate = this.Connection_SerialPort.BaudRate,
                    BaudRate_IsCustom = this.Connection_SerialPort.BaudRate_IsCustom,
                    BaudRate_Custom = this.Connection_SerialPort.BaudRate_Custom,
                    Parity = this.Connection_SerialPort.Parity,
                    DataBits = this.Connection_SerialPort.DataBits,
                    StopBits = this.Connection_SerialPort.StopBits
                };
            }

            else
            {
                SerialPort = null;
            }

            IP_Info? IP;

            if (Connection_IP != null)
            {
                IP = new IP_Info()
                {
                    IP_Address = this.Connection_IP.IP_Address,
                    Port = this.Connection_IP.Port
                };
            }

            else
            {
                IP = null;
            }

            return new DeviceData()
            {
                GlobalEncoding = this.GlobalEncoding,

                TimeoutWrite = this.TimeoutWrite,
                TimeoutRead = this.TimeoutRead,

                TypeOfConnection = this.TypeOfConnection,

                Connection_SerialPort = SerialPort,

                Connection_IP = IP
            };
        }
    }
}
