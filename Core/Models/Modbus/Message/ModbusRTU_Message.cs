using Core.Models.Modbus.DataTypes;

namespace Core.Models.Modbus.Message;

public class ModbusRTU_Message : ModbusMessage
{
    public override string ProtocolName { get; } = "Modbus RTU";

    public override byte[] CreateMessage(ModbusFunction function, MessageData data)
    {
        byte[] PDU = Modbus_PDU.Create(function, data);

        byte[] TX;

        if (data.CheckSum_IsEnable)
        {
            TX = new byte[3 + PDU.Length];
        }

        else
        {
            TX = new byte[1 + PDU.Length];
        }

        // Slave ID
        TX[0] = data.SlaveID;

        Array.Copy(PDU, 0, TX, 1, PDU.Length);

        // CRC16
        if (data.CheckSum_IsEnable)
        {
            byte[] CRC16 = CheckSum.Calculate_CRC16(TX, data.Polynom);
            TX[TX.Length - 2] = CRC16[0];  // Предпоследний элемент
            TX[TX.Length - 1] = CRC16[1];  // Последний элемент
        }

        return TX;
    }

    public override ModbusResponse DecodingMessage(ModbusFunction currentFunction, byte[] sourceArray)
    {
        var decodingResponse = new ModbusResponse
        {
            SlaveID = sourceArray[0],
            Command = sourceArray[1]
        };

        CheckErrorCode(TypeOfModbus.RTU, ref decodingResponse, sourceArray);

        if (currentFunction is ModbusReadFunction)
        {
            decodingResponse.LengthOfData = sourceArray[2];

            if (decodingResponse.LengthOfData == 0)
            {
                throw new Exception("Длина информационной части пакета равна 0.\n" +
                    "Код функции: " + currentFunction.Number.ToString() + "\n" +
                    "Возможно нарушение целостности пакета Modbus RTU.");
            }

            decodingResponse.Data = new byte[decodingResponse.LengthOfData];

            // Согласно документации на протокол Modbus:
            // В ответном пакете Modbus RTU на команды чтения
            // информационная часть начинается с 3 байта.

            Array.Copy(sourceArray, 3, decodingResponse.Data, 0, decodingResponse.LengthOfData);

            // Реверс байтов не нужен функциям, работающими с флагами (номера 1 и 2).
            if (currentFunction != Function.ReadCoilStatus &&
                currentFunction != Function.ReadDiscreteInputs)
            {
                decodingResponse.Data = ReverseLowAndHighBytesInWords(decodingResponse.Data);
            }
        }

        else if (currentFunction is ModbusWriteFunction)
        {
            decodingResponse.LengthOfData = -1;
        }

        else
        {
            throw new Exception("Неподдерживаемый код Modbus команды (Код: " + currentFunction.Number + ")");
        }

        return decodingResponse;
    }
}
