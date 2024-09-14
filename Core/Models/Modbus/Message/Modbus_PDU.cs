namespace Core.Models.Modbus.Message
{
    public static class Modbus_PDU
    {
        public static byte[] Create(ModbusFunction function, MessageData data)
        {
            WriteTypeMessage? dataForWrite = data as WriteTypeMessage;
            ReadTypeMessage? dataForRead = data as ReadTypeMessage;

            if (dataForRead != null && function.Number >= 1 && function.Number <= 4)
            {
                return Create_Read(function, dataForRead);
            }

            else if (dataForWrite != null && (function.Number == 5 || function.Number == 6))
            {
                return Create_Write(function, dataForWrite);
            }

            else if (dataForWrite != null && (function.Number == 15 || function.Number == 16))
            {
                return Create_WriteMultiple(function, dataForWrite);
            }

            else
            {
                throw new Exception("Ошибка при формировании PDU.\n" +
                    "Неподдерживаемый код функции: " + function.Number);
            }
        }

        private static byte[] Create_Read(ModbusFunction readFunction, ReadTypeMessage data)
        {
            byte[] PDU = new byte[5];

            byte[] addressArray = BitConverter.GetBytes(data.Address);
            byte[] numberOfRegistersArray = BitConverter.GetBytes(data.NumberOfRegisters);

            // Function number
            PDU[0] = readFunction.Number;
            // Address 
            PDU[1] = addressArray[1];
            PDU[2] = addressArray[0];
            // Amount of readed registers
            PDU[3] = numberOfRegistersArray[1];
            PDU[4] = numberOfRegistersArray[0];

            return PDU;
        }

        private static byte[] Create_Write(ModbusFunction writeFunction, WriteTypeMessage data)
        {
            byte[] PDU = new byte[5];

            byte[] addressArray = BitConverter.GetBytes(data.Address);

            // Function number
            PDU[0] = writeFunction.Number;
            // Address 
            PDU[1] = addressArray[1];
            PDU[2] = addressArray[0];
            // Data
            PDU[3] = data.WriteData[1];
            PDU[4] = data.WriteData[0];

            return PDU;
        }

        private static byte[] Create_WriteMultiple(ModbusFunction writeFunction, WriteTypeMessage data)
        {
            byte[] PDU = new byte[6 + data.WriteData.Length];

            byte[] addressArray = BitConverter.GetBytes(data.Address);
            byte[] numberOfRegistersArray = BitConverter.GetBytes(data.NumberOfRegisters);

            // Function number
            PDU[0] = writeFunction.Number;
            // Address 
            PDU[1] = addressArray[1];
            PDU[2] = addressArray[0];
            // Amount of write registers
            PDU[3] = numberOfRegistersArray[1];
            PDU[4] = numberOfRegistersArray[0];
            // Amount of byte next
            PDU[5] = (byte)data.WriteData.Length;

            int elementCounter = 6;

            foreach (byte element in data.WriteData)
            {
                PDU[elementCounter] = element;
                elementCounter++;
            }

            return PDU;
        }
    }
}
