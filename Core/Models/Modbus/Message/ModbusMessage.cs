namespace Core.Models.Modbus.Message
{
    public class ModbusActionDetails
    {
        public byte[]? RequestBytes;
        public byte[]? ResponseBytes;

        public DateTime Request_ExecutionTime;
        public DateTime Response_ExecutionTime;
    }    

    public abstract class ModbusMessage
    {
        /***********************************************/

        // Должны быть определены в наследниках:
        // реализациях протокола Modbus RTU, ASCII, TCP

        public abstract string ProtocolName { get; }

        public abstract byte[] CreateMessage(ModbusFunction Function, MessageData Data);
        public abstract ModbusResponse DecodingMessage(ModbusFunction Function, byte[] SourceArray);

        //public abstract void DecodingClientMessage(int FunctionNumber, byte[] SourceArray);

        /***********************************************/

        protected ulong PackageNumber = 0;

        protected enum TypeOfModbus
        {
            TCP,
            RTU,
            ASCII
        }

        protected void CheckErrorCode(TypeOfModbus modbusType, ref ModbusResponse decoding, byte[] massive)
        {
            // Согласно документации на протокол Modbus:
            // Если значение в поле команды больше 0x80, то это ошибка.
            // Значение команды = значение в поле команды - 0x80

            if (decoding.Command > 0x80)
            {
                int functionCode = decoding.Command - 0x80;

                decoding.Data = new byte[1]; // Код ошибки занимает 1 байт

                // Modbus TCP
                // [0],[1] - Package ID, [2],[3] - Modbus ID, [4],[5] - Length of PDU
                // [6] - Slave ID, [7] - Command, [8] - Error code
                if (modbusType == TypeOfModbus.TCP)
                {
                    decoding.Data[0] = massive[8];
                }

                // Modbus RTU / ASCII 
                // [0] - Slave ID, [1] - Command, [2] - Error code,
                // [3] - CheckSum_low, [4] - CheckSum_high
                else
                {
                    decoding.Data[0] = massive[2];
                }

                GetModbusException(decoding.Data[0], (byte)functionCode);
            }
        }

        protected byte[] ReverseLowAndHighBytesInWords(byte[] sourceArray)
        {
            if (sourceArray.Length < 2)
            {
                return sourceArray;
            }

            byte temp;

            for (int i = 0; i < sourceArray.Length; i += 2)
            {
                temp = sourceArray[i];
                sourceArray[i] = sourceArray[i + 1];
                sourceArray[i + 1] = temp;
            }

            return sourceArray;
        }

        private void GetModbusException(byte errorCode, byte functionCode)
        {
            switch (errorCode)
            {
                case 1:
                    throw new ModbusException(functionCode, errorCode,
                        "Принятый код функции не может быть обработан.");

                case 2:
                    throw new ModbusException(functionCode, errorCode,
                        "Адрес данных, указанный в запросе, недоступен.");

                case 3:
                    throw new ModbusException(functionCode, errorCode,
                        "Значение, содержащееся в поле данных запроса, " +
                        "является недопустимой величиной.");

                case 4:
                    throw new ModbusException(functionCode, errorCode,
                        "Невосстанавливаемая ошибка имела место, " +
                        "пока ведомое устройство пыталось выполнить затребованное действие.");

                case 5:
                    throw new ModbusException(functionCode, errorCode,
                        "Ведомое устройство приняло запрос и обрабатывает его, " +
                        "но это требует много времени. " +
                        "Этот ответ предохраняет ведущее устройство от генерации ошибки тайм-аута.");

                case 6:
                    throw new ModbusException(functionCode, errorCode,
                        "Ведомое устройство занято обработкой команды. " +
                        "Ведущее устройство должно повторить сообщение позже, когда ведомое освободится.");

                case 7:
                    throw new ModbusException(functionCode, errorCode,
                        "Ведомое устройство не может выполнить программную функцию, заданную в запросе. " +
                        "Этот код возвращается для неуспешного программного запроса, " +
                        "использующего функции с номерами 13 или 14. " +
                        "Ведущее устройство должно запросить диагностическую информацию " +
                        "или информацию об ошибках от ведомого.");

                case 8:
                    throw new ModbusException(functionCode, errorCode,
                        "Ведомое устройство при чтении расширенной памяти обнаружило ошибку контроля четности. " +
                        "Master может повторить запрос позже, " +
                        "но обычно в таких случаях требуется ремонт оборудования.");

                case 10:
                    throw new ModbusException(functionCode, errorCode,
                        "Шлюз неправильно настроен или перегружен запросами.");

                case 11:
                    throw new ModbusException(functionCode, errorCode,
                        "Slave устройства нет в сети или от него нет ответа.");

                default:
                    throw new Exception("Код функции: " + functionCode.ToString() + "\n" +
                        "Неизвестная ошибка Modbus (Код " + errorCode + ")");
            }
        }
    }
}
