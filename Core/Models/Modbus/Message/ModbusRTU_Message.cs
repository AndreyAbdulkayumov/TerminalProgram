using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Modbus.Message
{
    public class ModbusRTU_Message : ModbusMessage
    {
        public override string ProtocolName { get; } = "Modbus RTU";

        public override byte[] CreateMessage(ModbusFunction Function, MessageData Data)
        {
            byte[] PDU = Modbus_PDU.Create(Function, Data);

            byte[] TX;

            if (Data.CheckSum_IsEnable)
            {
                TX = new byte[3 + PDU.Length];
            }

            else
            {
                TX = new byte[1 + PDU.Length];
            }

            // Slave ID
            TX[0] = Data.SlaveID;

            Array.Copy(PDU, 0, TX, 1, PDU.Length);

            // CRC16
            if (Data.CheckSum_IsEnable)
            {
                byte[] CRC16 = CheckSum.Calculate_CRC16(TX, Data.Polynom);
                TX[TX.Length - 2] = CRC16[0];  // Предпоследний элемент
                TX[TX.Length - 1] = CRC16[1];  // Последний элемент
            }

            return TX;
        }

        public override ModbusResponse DecodingMessage(ModbusFunction CurrentFunction, byte[] SourceArray)
        {
            ModbusResponse DecodingResponse = new ModbusResponse
            {
                SlaveID = SourceArray[0],
                Command = SourceArray[1]
            };

            CheckErrorCode(TypeOfModbus.RTU, ref DecodingResponse, SourceArray);

            if (CurrentFunction is ModbusReadFunction)
            {
                DecodingResponse.LengthOfData = SourceArray[2];

                if (DecodingResponse.LengthOfData == 0)
                {
                    throw new Exception("Длина информационной части пакета равна 0.\n" +
                        "Код функции: " + CurrentFunction.Number.ToString() + "\n" +
                        "Возможно нарушение целостности пакета Modbus RTU.");
                }

                DecodingResponse.Data = new byte[DecodingResponse.LengthOfData];

                // Согласно документации на протокол Modbus:
                // В ответном пакете Modbus RTU на команды чтения
                // информационная часть начинается с 3 байта.

                Array.Copy(SourceArray, 3, DecodingResponse.Data, 0, DecodingResponse.LengthOfData);


                // Реверс байтов не нужен функциям, работающими с флагами (номера 1 и 2).
                if (CurrentFunction != Function.ReadCoilStatus &&
                    CurrentFunction != Function.ReadDiscreteInputs)
                {
                    DecodingResponse.Data = ReverseLowAndHighBytesInWords(DecodingResponse.Data);
                }
            }

            else if (CurrentFunction is ModbusWriteFunction)
            {
                DecodingResponse.LengthOfData = -1;
            }

            else
            {
                throw new Exception("Неподдерживаемый код Modbus команды (Код: " + CurrentFunction.Number + ")");
            }

            return DecodingResponse;
        }
    }
}
