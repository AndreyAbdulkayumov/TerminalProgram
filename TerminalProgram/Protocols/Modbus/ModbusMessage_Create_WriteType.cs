using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalProgram.Protocols.Modbus
{
    public static partial class ModbusMessage
    {
        private static byte[] ModbusTCP_Create_WriteTypeMessage(ModbusWriteFunction WriteFunction,
            byte SlaveID, UInt16 Address, UInt16 Data)
        {
            byte[] TX;

            TX = new byte[12];

            byte[] AddressArray = BitConverter.GetBytes(Address);
            byte[] DataArray = BitConverter.GetBytes(Data);
            byte[] PackageNumberArray = BitConverter.GetBytes(PackageNumber);

            PackageNumber++;

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
            TX[7] = WriteFunction.Number;
            // address 
            TX[8] = AddressArray[1];
            TX[9] = AddressArray[0];
            // value
            TX[10] = DataArray[1];
            TX[11] = DataArray[0];

            return TX;
        }

        private static byte[] ModbusTCP_Create_WriteTypeMessage_Multiple(ModbusWriteFunction WriteFunction,
            byte SlaveID, UInt16 Address, UInt16[] Data)
        {
            byte[] TX;

            // NumberOfRegisters * 2 - т.к. регистры 16 - битные.
            TX = new byte[13 + Data.Length * 2];

            byte[] AddressArray = BitConverter.GetBytes(Address);
            byte[] PackageNumberArray = BitConverter.GetBytes(PackageNumber);
            byte[] LengthOfPDU = BitConverter.GetBytes(Data.Length + 7);
            byte[] NumberOfRegistersArray = BitConverter.GetBytes(Data.Length);

            PackageNumber++;

            // Номер транзакции
            TX[0] = PackageNumberArray[1];
            TX[1] = PackageNumberArray[0];
            // Modbus ID
            TX[2] = 0x00;
            TX[3] = 0x00;
            // Длина PDU
            TX[4] = LengthOfPDU[1];
            TX[5] = LengthOfPDU[0];
            // Slave ID
            TX[6] = SlaveID;
            // Command
            TX[7] = WriteFunction.Number;
            // address 
            TX[8] = AddressArray[1];
            TX[9] = AddressArray[0];
            // value
            TX[10] = NumberOfRegistersArray[1];
            TX[11] = NumberOfRegistersArray[0];

            TX[12] = (byte)(Data.Length * 2);

            byte[] DataArray;
            int ElementCounter = 13;

            foreach (UInt16 element in Data)
            {
                DataArray = BitConverter.GetBytes(element);

                TX[ElementCounter] = DataArray[1];
                TX[ElementCounter + 1] = DataArray[0];

                ElementCounter += 2;
            }

            return TX;
        }

        private static byte[] ModbusRTU_Create_WriteTypeMessage(ModbusWriteFunction WriteFunction,
            byte SlaveID, UInt16 Address, UInt16 Data, bool CRC_IsEnable, UInt16 Polynom)
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
            byte[] DataArray = BitConverter.GetBytes(Data);

            // Slave ID
            TX[0] = SlaveID;
            // Command
            TX[1] = WriteFunction.Number;
            // address 
            TX[2] = AddressArray[1];
            TX[3] = AddressArray[0];
            // value
            TX[4] = DataArray[1];
            TX[5] = DataArray[0];
            // CRC
            if (CRC_IsEnable)
            {
                byte[] CRC = CRC_16.Calculate(TX, Polynom);
                TX[6] = CRC[0];
                TX[7] = CRC[1];
            }

            return TX;
        }

        private static byte[] ModbusRTU_Create_WriteTypeMessage_Multiple(ModbusWriteFunction WriteFunction,
            byte SlaveID, UInt16 Address, UInt16[] Data,
            bool CRC_IsEnable, UInt16 Polynom)
        {
            byte[] TX;

            // NumberOfRegisters * 2 - т.к. регистры 16 - битные.
            if (CRC_IsEnable)
            {
                TX = new byte[8 + Data.Length * 2];
            }

            else
            {
                TX = new byte[6 + Data.Length * 2];
            }

            byte[] AddressArray = BitConverter.GetBytes(Address);
            byte[] NumberOfRegistersArray = BitConverter.GetBytes(Data.Length);

            // Slave ID
            TX[0] = SlaveID;
            // Command
            TX[1] = WriteFunction.Number;
            // address 
            TX[2] = AddressArray[1];
            TX[3] = AddressArray[0];
            // value
            TX[4] = NumberOfRegistersArray[1];
            TX[5] = NumberOfRegistersArray[0];

            byte[] DataArray;
            int ElementCounter = 6;

            foreach (UInt16 element in Data)
            {
                DataArray = BitConverter.GetBytes(element);

                TX[ElementCounter] = DataArray[1];
                TX[ElementCounter + 1] = DataArray[0];

                ElementCounter += 2;
            }

            // CRC
            if (CRC_IsEnable)
            {
                byte[] CRC = CRC_16.Calculate(TX, Polynom);
                TX[TX.Length - 1] = CRC[0];
                TX[TX.Length] = CRC[1];
            }

            return TX;
        }
    }
}
