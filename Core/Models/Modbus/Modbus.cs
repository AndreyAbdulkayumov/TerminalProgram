using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Core.Models;
using Core.Models.Modbus.Message;

namespace Core.Models.Modbus
{
    public struct ModbusResponse
    {
        // Только для Modbus TCP
        public UInt16 OperationNumber;
        public UInt16 ProtocolID;
        public UInt16 LengthOfPDU; 
        
        // Общая часть для всех типов Modbus протокола
        public byte SlaveID;

        // PDU - Protocol Data Unit
        public byte Command;
        public int LengthOfData;
        public byte[] Data;
    }

    public class Modbus
    {
        private static bool IsBusy = false;

        private readonly IConnection Device;

        public Modbus(IConnection ConnectedDevice)
        {
            Device = ConnectedDevice;
        }

        public void WriteRegister(ModbusWriteFunction WriteFunction, MessageData DataForWrite, 
            ModbusMessage Message, out ModbusResponse Response)
        {
            try
            {
                while (IsBusy) ;

                IsBusy = true;

                byte[] RX = new byte[20];

                byte[] TX = Message.CreateMessage(WriteFunction, DataForWrite);

                Device.Send(TX, TX.Length);

                Device.Receive(RX);

                Response = Message.DecodingMessage(WriteFunction, RX);

                IsBusy = false;
            }

            catch (ModbusException error)
            {
                IsBusy = false;
                throw new ModbusException(error);
            }

            catch (Exception error)
            {
                IsBusy = false;
                throw new Exception(error.Message);
            }
        }

        public UInt16[] ReadRegister(ModbusReadFunction ReadFunction, MessageData DataForRead,
            ModbusMessage Message, out ModbusResponse Response)
        {
            try
            {
                while (IsBusy) ;

                IsBusy = true;

                byte[] RX = new byte[100];

                byte[] TX = Message.CreateMessage(ReadFunction, DataForRead);

                Device.Send(TX, TX.Length);

                Device.Receive(RX);

                Response = Message.DecodingMessage(ReadFunction, RX);

                UInt16[] result;
                
                if (Response.Data.Length < 2)
                {
                    result = new UInt16[1];

                    result[0] = Response.Data[0];
                }

                else
                {
                    result = new UInt16[Response.Data.Length / 2];

                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i] = BitConverter.ToUInt16(Response.Data, i * 2);
                    }
                }

                IsBusy = false;

                return result;
            }

            catch (ModbusException error)
            {
                IsBusy = false;
                throw new ModbusException(error);
            }

            catch (Exception error)
            {
                IsBusy = false;
                throw new Exception(error.Message);
            }
        }
    }
}
