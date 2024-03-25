using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Modbus.Message
{
    public class ModbusActionDetails
    {
        public byte[]? RequestBytes;
        public byte[]? ResponseBytes;

        public DateTime? Request_ExecutionTime;
        public DateTime? Response_ExecutionTime;
    }

    public class ModbusException : Exception
    {
        public readonly byte ErrorCode;
        public readonly byte FunctionCode;

        public override string Message { get; }

        public readonly ModbusActionDetails Details = new ModbusActionDetails();

        public ModbusException(byte FunctionCode, byte ErrorCode, string Message)
        {
            this.FunctionCode = FunctionCode;
            this.ErrorCode = ErrorCode;
            this.Message = Message;
        }

        public ModbusException(ModbusException ErrorObject)
        {
            FunctionCode= ErrorObject.FunctionCode;
            ErrorCode = ErrorObject.ErrorCode;
            Message = ErrorObject.Message;
            Details.RequestBytes = ErrorObject.Details.RequestBytes;
            Details.ResponseBytes = ErrorObject.Details.ResponseBytes;
        }

        public ModbusException(byte FunctionCode, byte ErrorCode, string Message, 
            byte[] RequestBytes, byte[] ResponseBytes)
        {
            this.FunctionCode = FunctionCode;
            this.ErrorCode = ErrorCode;
            this.Message = Message;
            this.Details.RequestBytes = RequestBytes;
            this.Details.ResponseBytes = ResponseBytes;
        }

        public ModbusException(ModbusException ErrorObject, byte[] RequestBytes, byte[] ResponseBytes, 
            DateTime? Request_ExecutionTime, DateTime? Response_ExecutionTime)
        {
            FunctionCode = ErrorObject.FunctionCode;
            ErrorCode = ErrorObject.ErrorCode;
            Message = ErrorObject.Message;
            this.Details.RequestBytes = RequestBytes;
            this.Details.ResponseBytes = ResponseBytes;
            this.Details.Request_ExecutionTime = Request_ExecutionTime;
            this.Details.Response_ExecutionTime = Response_ExecutionTime;
        }
    }

    public class ModbusExceptionInfo : Exception
    {
        public ModbusActionDetails Details;
    }

    public abstract class ModbusMessage
    {
        /***********************************************/

        // Должны быть определены в наследниках:
        // реализациях протокола Modbus RTU, ASCII, TCP

        public abstract string ProtocolName { get; }

        public abstract byte[] CreateMessage(ModbusFunction Function, MessageData Data);
        public abstract ModbusResponse DecodingMessage(ModbusFunction Function, byte[] SourceArray);

        /***********************************************/

        protected ulong PackageNumber = 0;

        protected enum TypeOfModbus
        {
            TCP,
            RTU,
            ASCII
        }

        protected void CheckErrorCode(TypeOfModbus ModbusType, ref ModbusResponse Decoding, byte[] massive)
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

                // Modbus RTU / ASCII 
                // [0] - Slave ID, [1] - Command, [2] - Error code,
                // [3] - CheckSum_low, [4] - CheckSum_high
                else
                {
                    Decoding.Data[0] = massive[2];
                }

                GetModbusException(Decoding.Data[0], (byte)FunctionCode);
            }
        }

        protected byte[] ReverseLowAndHighBytesInWords(byte[] SourceArray)
        {
            if (SourceArray.Length < 2)
            {
                return SourceArray;
            }

            byte temp;

            for (int i = 0; i < SourceArray.Length; i += 2)
            {
                temp = SourceArray[i];
                SourceArray[i] = SourceArray[i + 1];
                SourceArray[i + 1] = temp;
            }

            return SourceArray;
        }

        private void GetModbusException(byte ErrorCode, byte FunctionCode)
        {
            switch (ErrorCode)
            {
                case 1:
                    throw new ModbusException(FunctionCode, ErrorCode,
                        "Принятый код функции не может быть обработан.");

                case 2:
                    throw new ModbusException(FunctionCode, ErrorCode,
                        "Адрес данных, указанный в запросе, недоступен.");

                case 3:
                    throw new ModbusException(FunctionCode, ErrorCode,
                        "Значение, содержащееся в поле данных запроса, " +
                        "является недопустимой величиной.");

                case 4:
                    throw new ModbusException(FunctionCode, ErrorCode,
                        "Невосстанавливаемая ошибка имела место, " +
                        "пока ведомое устройство пыталось выполнить затребованное действие.");

                case 5:
                    throw new ModbusException(FunctionCode, ErrorCode,
                        "Ведомое устройство приняло запрос и обрабатывает его, " +
                        "но это требует много времени. " +
                        "Этот ответ предохраняет ведущее устройство от генерации ошибки тайм-аута.");

                case 6:
                    throw new ModbusException(FunctionCode, ErrorCode,
                        "Ведомое устройство занято обработкой команды. " +
                        "Ведущее устройство должно повторить сообщение позже, когда ведомое освободится.");

                case 7:
                    throw new ModbusException(FunctionCode, ErrorCode,
                        "Ведомое устройство не может выполнить программную функцию, заданную в запросе. " +
                        "Этот код возвращается для неуспешного программного запроса, " +
                        "использующего функции с номерами 13 или 14. " +
                        "Ведущее устройство должно запросить диагностическую информацию " +
                        "или информацию об ошибках от ведомого.");

                case 8:
                    throw new ModbusException(FunctionCode, ErrorCode,
                        "Ведомое устройство при чтении расширенной памяти обнаружило ошибку контроля четности. " +
                        "Master может повторить запрос позже, " +
                        "но обычно в таких случаях требуется ремонт оборудования.");

                case 10:
                    throw new ModbusException(FunctionCode, ErrorCode,
                        "Шлюз неправильно настроен или перегружен запросами.");

                case 11:
                    throw new ModbusException(FunctionCode, ErrorCode,
                        "Slave устройства нет в сети или от него нет ответа.");

                default:
                    throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                        "Неизвестная ошибка Modbus (Код " + ErrorCode + ")");
            }
        }
    }
}
