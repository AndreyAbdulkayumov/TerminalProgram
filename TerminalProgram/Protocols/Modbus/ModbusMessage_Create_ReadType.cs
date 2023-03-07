using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalProgram.Protocols.Modbus
{
    public static partial class ModbusMessage
    {
        private static byte[] ModbusTCP_Create_ReadTypeMessage(ModbusReadFunction ReadFunction,
            byte SlaveID, UInt16 Address, int NumberOfRegisters)
        {
            byte[] TX;

            TX = new byte[12];

            byte[] AddressArray = BitConverter.GetBytes(Address);
            byte[] PackageNumberArray = BitConverter.GetBytes(PackageNumber);
            byte[] NumberOfRegistersArray = BitConverter.GetBytes(NumberOfRegisters);

            // Номер транзакции
            TX[0] = PackageNumberArray[1];
            TX[1] = PackageNumberArray[0];
            // Modbus ID
            TX[2] = 0x00;
            TX[3] = 0x00;
            // Длина PDU
            TX[4] = 0x00;
            TX[5] = 0x06;
            // Slave ID
            TX[6] = SlaveID;
            // Command
            TX[7] = ReadFunction.Number;
            // address 
            TX[8] = AddressArray[1];
            TX[9] = AddressArray[0];
            // value
            TX[10] = NumberOfRegistersArray[1];
            TX[11] = NumberOfRegistersArray[0];

            return TX;
        }

        private static byte[] ModbusRTU_Create_ReadTypeMessage(ModbusReadFunction ReadFunction,
            byte SlaveID, UInt16 Address, int NumberOfRegisters, bool CRC_IsEnable, UInt16 Polynom)
        {
            byte[] TX;

            if (CRC_IsEnable)
            {
                TX = new byte[8];
            }

            else
            {
                TX = new byte[6];
            }

            byte[] AddressArray = BitConverter.GetBytes(Address);
            byte[] NumberOfRegistersArray = BitConverter.GetBytes(NumberOfRegisters);

            // Slave ID
            TX[0] = SlaveID;
            // Command
            TX[1] = ReadFunction.Number;
            // address 
            TX[2] = AddressArray[1];
            TX[3] = AddressArray[0];
            // value
            TX[4] = NumberOfRegistersArray[1];
            TX[5] = NumberOfRegistersArray[0];
            // CRC
            if (CRC_IsEnable)
            {
                byte[] CRC = CRC_16.Calculate(TX, Polynom);
                TX[6] = CRC[0];
                TX[7] = CRC[1];
            }

            return TX;
        }
    }
}
