using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class ConnectedHost
    {
        public bool HostIsConnect 
        { 
            get 
            { 
                if (Client == null)
                {
                    return false;
                }

                return Client.IsConnected; 
            } 
        }

        private IConnection? Client = null;

        public void Connect(string FilePath)
        {
            switch ("Ethernet")
            {
                case "SerialPort":

                    Client = new SerialPortClient();

                    Client.Connect(new ConnectionInfo(new SerialPortInfo(
                        "COM6",
                        "9600",
                        "None",
                        "8",
                        "1"
                        ),
                        Encoding.ASCII));

                    break;

                case "Ethernet":

                    Client = new IPClient();

                    Client.Connect(new ConnectionInfo(new SocketInfo(
                        "127.168.0.3",
                        "802"
                        ),
                        Encoding.ASCII));

                    break;

                default:
                    throw new Exception("В файле настроек задан неизвестный интерфейс связи.");
            }
        }

        public async Task Disconnect()
        {
            if (Client == null)
            {
                return;
            }

            await Client.Disconnect();
        }
    }
}
