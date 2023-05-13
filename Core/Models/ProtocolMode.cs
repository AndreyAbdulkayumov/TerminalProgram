using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public abstract class ProtocolMode
    {
        public int WriteTimeout { get; protected set; }
        public int ReadTimeout { get; protected set; }
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
            ReadTimeout = -1; // Бесконечно

            InitMode(Client);
        }
    }

    public class ProtocolMode_Modbus : ProtocolMode
    {
        public ProtocolMode_Modbus(IConnection? Client, DeviceData Settings)
        {
            CurrentReadMode = ReadMode.Sync;

            UpdateTimeouts(Settings);

            InitMode(Client);
        }

        public void UpdateTimeouts(DeviceData Settings)
        {
            WriteTimeout = Convert.ToInt32(Settings.TimeoutWrite);
            ReadTimeout = Convert.ToInt32(Settings.TimeoutRead);
        }
    }
}
