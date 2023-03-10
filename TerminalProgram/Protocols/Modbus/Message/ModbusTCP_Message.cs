using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalProgram.Protocols.Modbus.Message
{
    public class ModbusTCP_Message : ModbusMessage
    {
        public override string ProtocolName { get; } = "Modbus TCP";

        public override byte[] CreateMessage(ModbusFunction Function, MessageData Data)
        {
            byte[] PDU = Modbus_PDU.Create(Function, Data);

            byte[] TX;

            TX = new byte[7 + PDU.Length];

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
            TX[6] = Data.SlaveID;

            Array.Copy(PDU, 0, TX, 7, PDU.Length);

            return TX;
        }

        public override ModbusResponse DecodingMessage(ModbusFunction Function, byte[] SourceArray)
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

            if (Function is ModbusReadFunction)
            {
                DecodingResponse.LengthOfData = SourceArray[8];

                if (DecodingResponse.LengthOfData == 0)
                {
                    throw new Exception("Длина информационной части пакета равна 0.\n" +
                        "Код функции: " + Function.Number.ToString() + "\n" +
                        "Возможно нарушение целостности пакета Modbus TCP.");
                }

                DecodingResponse.Data = new byte[DecodingResponse.LengthOfData];

                // Согласно документации на протокол Modbus:
                // В ответном пакете Modbus TCP на команды чтения
                // информационная часть начинается с 9 байта.
                Array.Copy(SourceArray, 9, DecodingResponse.Data, 0, DecodingResponse.LengthOfData);

                DecodingResponse.Data = ReverseLowAndHighBytes(DecodingResponse.Data);
            }

            else if (Function is ModbusWriteFunction)
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
