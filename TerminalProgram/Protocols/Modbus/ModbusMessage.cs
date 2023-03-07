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
    public static partial class ModbusMessage
    {
        private static ulong PackageNumber = 0;

        public static byte[] Create_WriteType(ModbusWriteFunction WriteFunction, TypeOfModbus ModbusType, 
            byte SlaveID, UInt16 Address, UInt16[] Data, bool CRC_IsEnable, UInt16 Polynom)
        {
            if (ModbusType == TypeOfModbus.TCP)
            {
                if (Data.Length > 1)
                {
                    return ModbusTCP_Create_WriteTypeMessage_Multiple(WriteFunction,
                        SlaveID, Address, Data);
                }
                
                else
                {
                    return ModbusTCP_Create_WriteTypeMessage(WriteFunction,
                        SlaveID, Address, Data.First());
                }
            }

            else if (ModbusType == TypeOfModbus.RTU)
            {
                if (Data.Length > 1)
                {
                    return ModbusRTU_Create_WriteTypeMessage_Multiple(WriteFunction,
                        SlaveID, Address, Data, CRC_IsEnable, Polynom);
                }

                else
                {
                    return ModbusRTU_Create_WriteTypeMessage(WriteFunction,
                        SlaveID, Address, Data.First(), CRC_IsEnable, Polynom);
                }                
            }

            else
            {
                throw new Exception("При создании пакета Modbus выбран неизвестный тип протокола: " + ModbusType.ToString());
            }
        }

        public static byte[] Create_ReadType(ModbusReadFunction ReadFunction, TypeOfModbus ModbusType,
            byte SlaveID, UInt16 Address, int NumberOfRegisters, bool CRC_IsEnable, UInt16 Polynom)
        {
            if (ModbusType == TypeOfModbus.TCP)
            {
                return ModbusTCP_Create_ReadTypeMessage(ReadFunction,
                    SlaveID, Address, NumberOfRegisters);
            }

            else if (ModbusType == TypeOfModbus.RTU)
            {
                return ModbusRTU_Create_ReadTypeMessage(ReadFunction,
                    SlaveID, Address, NumberOfRegisters,
                    CRC_IsEnable, Polynom);
            }

            else
            {
                throw new Exception("При создании пакета Modbus выбран неизвестный тип протокола: " + ModbusType.ToString());
            }
        }

        public static ModbusResponse Decoding(TypeOfModbus ModbusType, 
            int CommandNumber, byte[] SourceArray)
        {
            if (ModbusType == TypeOfModbus.TCP)
            {
                return ModbusTCP_DecodingMessage(CommandNumber, SourceArray);
            }

            else if (ModbusType == TypeOfModbus.RTU)
            {
                return ModbusRTU_DecodingMessage(CommandNumber, SourceArray);
            }

            else
            {
                throw new Exception("При расшифровке пакета Modbus был выбран неизвестный тип протокола: " + 
                    ModbusType.ToString());
            }
        }
    }
}
