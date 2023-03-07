using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalProgram.Protocols.Modbus
{
    public static partial class ModbusMessage
    {
        private static ModbusResponse ModbusTCP_DecodingMessage(int CommandNumber, byte[] SourceArray)
        {
            ModbusResponse DecodingResponse = new ModbusResponse();

            byte[] temp = new byte[2];

            temp[0] = SourceArray[1];
            temp[1] = SourceArray[0];
            DecodingResponse.OperationNumber = (UInt16)BitConverter.ToInt16(temp, 0);
            temp[0] = SourceArray[3];
            temp[1] = SourceArray[2];
            DecodingResponse.ProtocolID = (UInt16)BitConverter.ToInt16(temp, 0);
            temp[0] = SourceArray[5];
            temp[1] = SourceArray[4];
            DecodingResponse.LengthOfPDU = (UInt16)BitConverter.ToInt16(temp, 0);
            DecodingResponse.SlaveID = SourceArray[6];
            DecodingResponse.Command = SourceArray[7];

            CheckErrorCode(TypeOfModbus.TCP, ref DecodingResponse, SourceArray);

            if (CommandNumber == ModbusCommand.ReadInputRegisters)
            {
                DecodingResponse.LengthOfData = SourceArray[8];

                if (DecodingResponse.LengthOfData == 0)
                {
                    throw new Exception("Длина информационной части пакета равна 0.\n" +
                        "Код функции: " + CommandNumber.ToString() + "\n" +
                        "Возможно нарушение целостности пакета Modbus TCP.");
                }

                DecodingResponse.Data = new byte[DecodingResponse.LengthOfData];

                // Согласно документации на протокол Modbus:
                // В пакете ответном пакете Modbus TCP на команду чтения (0х04)
                // Информационная часть начинается с 9 байта.
                Array.Copy(SourceArray, 9, DecodingResponse.Data, 0, DecodingResponse.LengthOfData);

                DecodingResponse.Data = ReverseLowAndHighBytes(DecodingResponse.Data);
            }

            else if (CommandNumber == ModbusCommand.PresetSingleRegister)
            {
                DecodingResponse.LengthOfData = -1;
            }

            else
            {
                throw new Exception("Неподдерживаемый код Modbus команды (Код: " + CommandNumber + ")");
            }

            return DecodingResponse;
        }

        private static ModbusResponse ModbusRTU_DecodingMessage(int CommandNumber, byte[] SourceArray)
        {
            ModbusResponse DecodingResponse = new ModbusResponse();

            DecodingResponse.SlaveID = SourceArray[0];
            DecodingResponse.Command = SourceArray[1];

            CheckErrorCode(TypeOfModbus.RTU, ref DecodingResponse, SourceArray);

            if (CommandNumber == ModbusCommand.ReadInputRegisters)
            {
                DecodingResponse.LengthOfData = SourceArray[2];

                if (DecodingResponse.LengthOfData == 0)
                {
                    throw new Exception("Длина информационной части пакета равна 0.\n" +
                        "Код функции: " + CommandNumber.ToString() + "\n" +
                        "Возможно нарушение целостности пакета Modbus RTU.");
                }

                DecodingResponse.Data = new byte[DecodingResponse.LengthOfData];

                // Согласно документации на протокол Modbus:
                // В пакете ответном пакете Modbus RTU на команду чтения (0х04)
                // Информационная часть начинается с 3 байта.

                Array.Copy(SourceArray, 3, DecodingResponse.Data, 0, DecodingResponse.LengthOfData);

                DecodingResponse.Data = ReverseLowAndHighBytes(DecodingResponse.Data);
            }

            else if (CommandNumber == ModbusCommand.PresetSingleRegister)
            {
                DecodingResponse.LengthOfData = -1;
            }

            else
            {
                throw new Exception("Неподдерживаемый код Modbus команды (Код: " + CommandNumber + ")");
            }            

            return DecodingResponse;
        }

        private static void CheckErrorCode(TypeOfModbus ModbusType, ref ModbusResponse Decoding, byte[] massive)
        {
            // Согласно документации на протокол Modbus:
            // Если значение в поле команды больше 0x80, то это ошибка.
            // Значение команды = значение в поле команды - 0x80

            if (Decoding.Command > 0x80)
            {
                int FunctionCode = Decoding.Command - 0x80;

                Decoding.Data = new byte[1]; // Код ошибки занимает 1 байт

                // Modbus TCP
                // [0],[1] - Package ID, [2],[3] - Modbus ID, [4],[5] - Length of PDU
                // [6] - Slave ID, [7] - Command, [8] - Error code
                if (ModbusType == TypeOfModbus.TCP)
                {
                    Decoding.Data[0] = massive[8];
                }

                // Modbus RTU 
                // [0] - Slave ID, [1] - Command, [2] - Error code,
                // [3] - CRC_low, [4] - CRC_high
                else
                {
                    Decoding.Data[0] = massive[2];
                }                

                switch (Decoding.Data[0])
                {
                    case 1:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Ошибка Modbus: " +
                            "Принятый код функции не может быть обработан (Код 1).");

                    case 2:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Ошибка Modbus: " +
                            "Адрес данных, указанный в запросе, недоступен (Код 2).");

                    case 3:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Ошибка Modbus: " +
                            "Значение, содержащееся в поле данных запроса, является недопустимой величиной (Код 3).");

                    case 4:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Ошибка Modbus: " +
                            "Невосстанавливаемая ошибка имела место, " +
                            "пока ведомое устройство пыталось выполнить затребованное действие (Код 4).");

                    case 5:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Ошибка Modbus: " +
                            "Ведомое устройство приняло запрос и обрабатывает его, " +
                            "но это требует много времени. " +
                            "Этот ответ предохраняет ведущее устройство от генерации ошибки тайм-аута (Код 5).");

                    case 6:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Ошибка Modbus: " +
                            "Ведомое устройство занято обработкой команды. " +
                            "Ведущее устройство должно повторить сообщение позже, когда ведомое освободится. (Код 6).");

                    case 7:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Ошибка Modbus: " +
                            "Ведомое устройство не может выполнить программную функцию, заданную в запросе. " +
                            "Этот код возвращается для неуспешного программного запроса, " +
                            "использующего функции с номерами 13 или 14. " +
                            "Ведущее устройство должно запросить диагностическую информацию " +
                            "или информацию об ошибках от ведомого (Код 7).");

                    case 8:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Ошибка Modbus: " +
                            "Ведомое устройство при чтении расширенной памяти обнаружило ошибку контроля четности. " +
                            "Master может повторить запрос позже, " +
                            "но обычно в таких случаях требуется ремонт оборудования (Код 8).");

                    case 10:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Ошибка Modbus: " +
                            "Шлюз неправильно настроен или перегружен запросами (Код 10).");

                    case 11:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Ошибка Modbus: " +
                            "Slave устройства нет в сети или от него нет ответа (Код 11).");

                    default:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Неизвестная ошибка Modbus (Код " + Decoding.Data[0] + ")");
                }
            }
        }

        private static byte[] ReverseLowAndHighBytes(byte[] SourceArray)
        {
            byte temp;

            for (int i = 0; i < SourceArray.Length; i += 2)
            {
                temp = SourceArray[i];
                SourceArray[i] = SourceArray[i + 1];
                SourceArray[i + 1] = temp;
            }

            return SourceArray;
        }
    }
}
