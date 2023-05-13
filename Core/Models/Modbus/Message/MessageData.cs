using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Modbus.Message
{
    public abstract class MessageData
    {
        public byte SlaveID;
        public UInt16 Address;
        public bool CRC_IsEnable;
        public UInt16 Polynom;
    }

    public class ReadTypeMessage : MessageData
    {
        public int NumberOfRegisters;

        public ReadTypeMessage(byte SlaveID, UInt16 Address, int NumberOfRegisters, bool CRC_IsEnable, UInt16 Polynom)
        {
            this.SlaveID = SlaveID;
            this.Address = Address;
            this.NumberOfRegisters = NumberOfRegisters;
            this.CRC_IsEnable = CRC_IsEnable;
            this.Polynom = Polynom;
        }
    }

    public class WriteTypeMessage : MessageData
    {
        public UInt16[] WriteData;

        public WriteTypeMessage(byte SlaveID, UInt16 Address, UInt16[] WriteData, bool CRC_IsEnable, UInt16 Polynom)
        {
            this.SlaveID = SlaveID;
            this.Address = Address;
            this.WriteData = WriteData;
            this.CRC_IsEnable = CRC_IsEnable;
            this.Polynom = Polynom;
        }
    }
}
