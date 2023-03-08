using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace TerminalProgram.Protocols.Modbus.Message
{
    public class ModbusRTU_Message : ModbusMessage
    {
        public override string ProtocolName { get; } = "Modbus RTU";

        public override byte[] CreateMessage(ModbusFunction Function, MessageData Data)
        {
            byte[] PDU = Modbus_PDU.Create(Function, Data);

            byte[] TX;

            if (Data.CRC_IsEnable)
            {
                TX = new byte[3 + PDU.Length];
            }

            else
            {
                TX = new byte[1 + PDU.Length];
            }

            // Slave ID
            TX[0] = Data.SlaveID;

            Array.Copy(PDU, 0, TX, 7, PDU.Length);

            // CRC
            if (Data.CRC_IsEnable)
            {
                byte[] CRC = CRC_16.Calculate(TX, Data.Polynom);
                TX[TX.Length - 1] = CRC[0];
                TX[TX.Length] = CRC[1];
            }

            return TX;
        }

        public override ModbusResponse DecodingMessage(ModbusFunction Function, byte[] SourceArray)
        {
            ModbusResponse DecodingResponse = new ModbusResponse
            {
                SlaveID = SourceArray[0],
                Command = SourceArray[1]
            };

            CheckErrorCode(TypeOfModbus.RTU, ref DecodingResponse, SourceArray);

            if (Function is ModbusReadFunction)
            {
                DecodingResponse.LengthOfData = SourceArray[2];

                if (DecodingResponse.LengthOfData == 0)
                {
                    throw new Exception("Длина информационной части пакета равна 0.\n" +
                        "Код функции: " + Function.Number.ToString() + "\n" +
                        "Возможно нарушение целостности пакета Modbus RTU.");
                }

                DecodingResponse.Data = new byte[DecodingResponse.LengthOfData];

                // Согласно документации на протокол Modbus:
                // В пакете ответном пакете Modbus RTU на команду чтения (0х04)
                // Информационная часть начинается с 3 байта.

                Array.Copy(SourceArray, 3, DecodingResponse.Data, 0, DecodingResponse.LengthOfData);

                DecodingResponse.Data = ReverseLowAndHighBytes(DecodingResponse.Data);
            }

            else if (Function is ModbusReadFunction)
            {
                DecodingResponse.LengthOfData = -1;
            }

            else
            {
                throw new Exception("Неподдерживаемый код Modbus команды (Код: " + Function.Number + ")");
            }

            return DecodingResponse;
        }
    }
}
