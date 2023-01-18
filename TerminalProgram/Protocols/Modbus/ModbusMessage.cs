using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Shapes;

namespace TerminalProgram.Protocols.Modbus
{
    public static class ModbusMessage
    {
        private static ulong PackageNumber = 0;

        /**********************************************/
        //
        // Write type message
        //
        /**********************************************/

        public static byte[] Create_WriteType(TypeOfModbus ModbusType, 
            byte SlaveID, UInt16 Address, UInt16 Data, bool CRC_IsEnable, UInt16 Polynom)
        {
            if (ModbusType == TypeOfModbus.TCP)
            {
                return ModbusTCP_Create_WriteTypeMessage(SlaveID, Address, Data, CRC_IsEnable, Polynom);
            }

            else if (ModbusType == TypeOfModbus.RTU)
            {
                return ModbusRTU_Create_WriteTypeMessage(SlaveID, Address, Data, CRC_IsEnable, Polynom);
            }

            else
            {
                throw new Exception("При создании пакета Modbus выбран неизвестный тип протокола: " + ModbusType.ToString());
            }
        }

        private static byte[] ModbusTCP_Create_WriteTypeMessage(
            byte SlaveID, UInt16 Address, UInt16 Data, bool CRC_IsEnable, UInt16 Polynom)
        {
            byte[] TX;

            if (CRC_IsEnable)
            {
                TX = new byte[14];
            }

            else
            {
                TX = new byte[12];
            }

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
            // Write 1 register
            TX[7] = 0x06;
            // address 
            TX[8] = AddressArray[1];
            TX[9] = AddressArray[0];
            // value
            TX[10] = DataArray[1];
            TX[11] = DataArray[0];
            // CRC
            if (CRC_IsEnable)
            {
                byte[] CRC = CRC_16.Calculate(TX, Polynom);
                TX[12] = CRC[1];
                TX[13] = CRC[0];
            }

            return TX;
        }

        private static byte[] ModbusRTU_Create_WriteTypeMessage(
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
            // Write 1 register
            TX[1] = 0x06;
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
                TX[6] = CRC[1];
                TX[7] = CRC[0];
            }

            return TX;
        }

        /**********************************************/
        //
        // Read type message
        //
        /**********************************************/

        public static byte[] Create_ReadType(TypeOfModbus ModbusType,
            byte SlaveID, UInt16 Address, int NumberOfRegisters, bool CRC_IsEnable, UInt16 Polynom)
        {
            if (ModbusType == TypeOfModbus.TCP)
            {
                return ModbusTCP_Create_ReadTypeMessage(SlaveID, Address, NumberOfRegisters, CRC_IsEnable, Polynom);
            }

            else if (ModbusType == TypeOfModbus.RTU)
            {
                return ModbusRTU_Create_ReadTypeMessage(SlaveID, Address, NumberOfRegisters, CRC_IsEnable, Polynom);
            }

            else
            {
                throw new Exception("При создании пакета Modbus выбран неизвестный тип протокола: " + ModbusType.ToString());
            }
        }

        private static byte[] ModbusTCP_Create_ReadTypeMessage(
            byte SlaveID, UInt16 Address, int NumberOfRegisters, bool CRC_IsEnable, UInt16 Polynom)
        {
            byte[] TX;

            if (CRC_IsEnable)
            {
                TX = new byte[14];
            }

            else
            {
                TX = new byte[12];
            }

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
            // Read 1 register
            TX[7] = 0x04;
            // address 
            TX[8] = AddressArray[1];
            TX[9] = AddressArray[0];
            // value
            TX[10] = NumberOfRegistersArray[1];
            TX[11] = NumberOfRegistersArray[0];
            // CRC
            if (CRC_IsEnable)
            {
                byte[] CRC = CRC_16.Calculate(TX, Polynom);
                TX[12] = CRC[1];
                TX[13] = CRC[0];
            }

            return TX;
        }

        private static byte[] ModbusRTU_Create_ReadTypeMessage(
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
            // Read 1 register
            TX[1] = 0x04;
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
                TX[6] = CRC[1];
                TX[7] = CRC[0];
            }

            return TX;
        }
    }
}
