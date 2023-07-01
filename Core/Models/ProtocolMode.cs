using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models.Settings;

namespace Core.Models
{
    public abstract class ProtocolMode
    {
        public int WriteTimeout { get; protected set; } = Timeout.Infinite;
        public int ReadTimeout { get; protected set; } = Timeout.Infinite;

        public ReadMode CurrentReadMode { get; protected set; } = ReadMode.Async;

        public virtual void InitMode(IConnection? Client)
        {
            if (Client == null || Client.IsConnected == false)
            {
                return;
            }

            Client.SetReadMode(CurrentReadMode);

            Client.WriteTimeout = WriteTimeout;
            Client.ReadTimeout = ReadTimeout;
        }
    }

    public class ProtocolMode_NoProtocol : ProtocolMode
    {
        public ProtocolMode_NoProtocol(IConnection? Client)
        {
            CurrentReadMode = ReadMode.Async;

            WriteTimeout = 500;
            ReadTimeout = Timeout.Infinite;

            InitMode(Client);
        }
    }

    public class ProtocolMode_Modbus : ProtocolMode
    {
        public ProtocolMode_Modbus(IConnection? Client, DeviceData? Settings)
        {
            CurrentReadMode = ReadMode.Sync;

            UpdateTimeouts(Settings);

            InitMode(Client);
        }

        public void UpdateTimeouts(DeviceData? Settings)
        {
            if (Settings != null)
            {
                this.WriteTimeout = Convert.ToInt32(Settings.TimeoutWrite);
                this.ReadTimeout = Convert.ToInt32(Settings.TimeoutRead);
            }            
        }
    }
}
