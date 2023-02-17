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

            if (CommandNumber == 0x04)
            {
                DecodingResponse.LengthOfData = SourceArray[8];

                DecodingResponse.Data = new byte[DecodingResponse.LengthOfData];

                Array.Copy(SourceArray, 9, DecodingResponse.Data, 0, DecodingResponse.LengthOfData);

                DecodingResponse.Data = ReverseArray(DecodingResponse.Data);
            }

            else if (CommandNumber == 0x06)
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

            if (CommandNumber == 0x04)
            {
                DecodingResponse.LengthOfData = SourceArray[2];

                DecodingResponse.Data = new byte[DecodingResponse.LengthOfData];

                Array.Copy(SourceArray, 3, DecodingResponse.Data, 0, DecodingResponse.LengthOfData);

                DecodingResponse.Data = ReverseArray(DecodingResponse.Data);
            }

            else if (CommandNumber == 0x06)
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
            if (Decoding.Command > 128)
            {
                int FunctionCode = Decoding.Command - 128;

                Decoding.Data = new byte[1]; // Код ошибки занимает 1 байт

                if (ModbusType == TypeOfModbus.TCP)
                {
                    Array.Copy(massive, 8, Decoding.Data, 0, 1);
                }

                else
                {
                    Array.Copy(massive, 3, Decoding.Data, 0, 1);
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

        private static byte[] ReverseArray(byte[] SourceArray)
        {
            byte[] temp = new byte[SourceArray.Length];

            for (int i = 0; i < SourceArray.Length; i++)
            {
                temp[i] = SourceArray[SourceArray.Length - 1 - i];
            }

            return temp;
        }
    }
}
