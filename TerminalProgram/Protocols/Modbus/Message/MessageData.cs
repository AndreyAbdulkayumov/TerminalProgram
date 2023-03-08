using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalProgram.Protocols.Modbus
{
    public class MessageData
    {
        public byte SlaveID;
        public UInt16 Address;

        public int NumberOfRegisters;

        public UInt16[] WriteData;

        public bool CRC_IsEnable;
        public UInt16 Polynom;

        /// <summary>
        /// Конструктор инициализации сообщения для чтения.
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <param name="Address"></param>
        /// <param name="NumberOfRegisters"></param>
        /// <param name="CRC_IsEnable"></param>
        /// <param name="Polynom"></param>
        public MessageData(byte SlaveID, UInt16 Address, int NumberOfRegisters, bool CRC_IsEnable, UInt16 Polynom)
        {
            this.SlaveID = SlaveID;
            this.Address = Address;
            this.NumberOfRegisters = NumberOfRegisters;
            this.CRC_IsEnable = CRC_IsEnable;
            this.Polynom = Polynom;
        }

        /// <summary>
        /// Конструктор инициализации сообщения для записи.
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <param name="Address"></param>
        /// <param name="WriteData"></param>
        /// <param name="CRC_IsEnable"></param>
        /// <param name="Polynom"></param>
        public MessageData(byte SlaveID, UInt16 Address, UInt16[] WriteData, bool CRC_IsEnable, UInt16 Polynom)
        {
            this.SlaveID = SlaveID;
            this.Address = Address;
            this.WriteData = WriteData;
            this.CRC_IsEnable = CRC_IsEnable;
            this.Polynom = Polynom;
        }
    }
}
