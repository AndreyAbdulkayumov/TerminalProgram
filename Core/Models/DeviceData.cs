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
        public string? BaudRate_IsCustom { get; set; }
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
        public string? GlobalEncoding { get; set; }

        public string? TimeoutWrite { get; set; }
        public string? TimeoutRead { get; set; }

        public string? TypeOfConnection { get; set; }

        public SerialPort_Info? Connection_SerialPort { get; set; }
        public IP_Info? Connection_IP { get; set; }

        public object Clone()
        {
            return new DeviceData()
            {
                GlobalEncoding = this.GlobalEncoding,

                TimeoutWrite = this.TimeoutWrite,
                TimeoutRead = this.TimeoutRead,

                TypeOfConnection = this.TypeOfConnection,

                Connection_SerialPort = new SerialPort_Info()
                {
                    COMPort = this.Connection_SerialPort?.COMPort,
                    BaudRate = this.Connection_SerialPort?.BaudRate,
                    BaudRate_IsCustom = this.Connection_SerialPort?.BaudRate_IsCustom,
                    BaudRate_Custom = this.Connection_SerialPort?.BaudRate_Custom,
                    Parity = this.Connection_SerialPort?.Parity,
                    DataBits = this.Connection_SerialPort?.DataBits,
                    StopBits = this.Connection_SerialPort?.StopBits,
                },

                Connection_IP = new IP_Info()
                {
                    IP_Address = this.Connection_IP?.IP_Address,
                    Port = this.Connection_IP?.Port
                }
            };
        }
    }
}
