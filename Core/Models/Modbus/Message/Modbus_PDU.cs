namespace Core.Models.Modbus.Message
{
    public static class Modbus_PDU
    {
        public static byte[] Create(ModbusFunction Function, MessageData Data)
        {
            WriteTypeMessage? DataForWrite = Data as WriteTypeMessage;
            ReadTypeMessage? DataForRead = Data as ReadTypeMessage;

            if (DataForRead != null && Function.Number >= 1 && Function.Number <= 4)
            {
                return Create_Read(Function, DataForRead);
            }

            else if (DataForWrite != null && (Function.Number == 5 || Function.Number == 6))
            {
                return Create_Write(Function, DataForWrite);
            }

            else if (DataForWrite != null && (Function.Number == 15 || Function.Number == 16))
            {
                return Create_WriteMultiple(Function, DataForWrite);
            }

            else
            {
                throw new Exception("Ошибка при формировании PDU.\n" +
                    "Неподдерживаемый код функции: " + Function.Number);
            }
        }

        private static byte[] Create_Read(ModbusFunction ReadFunction, ReadTypeMessage Data)
        {
            byte[] PDU = new byte[5];

            byte[] AddressArray = BitConverter.GetBytes(Data.Address);
            byte[] NumberOfRegistersArray = BitConverter.GetBytes(Data.NumberOfRegisters);

            // Function number
            PDU[0] = ReadFunction.Number;
            // Address 
            PDU[1] = AddressArray[1];
            PDU[2] = AddressArray[0];
            // Amount of readed registers
            PDU[3] = NumberOfRegistersArray[1];
            PDU[4] = NumberOfRegistersArray[0];

            return PDU;
        }

        private static byte[] Create_Write(ModbusFunction WriteFunction, WriteTypeMessage Data)
        {
            byte[] PDU = new byte[5];

            byte[] AddressArray = BitConverter.GetBytes(Data.Address);
            byte[] DataArray = BitConverter.GetBytes(Data.WriteData[0]);

            // Function number
            PDU[0] = WriteFunction.Number;
            // Address 
            PDU[1] = AddressArray[1];
            PDU[2] = AddressArray[0];
            // Data
            PDU[3] = DataArray[1];
            PDU[4] = DataArray[0];

            return PDU;
        }

        private static byte[] Create_WriteMultiple(ModbusFunction WriteFunction, WriteTypeMessage Data)
        {
            byte[] PDU = new byte[6 + Data.WriteData.Length * 2];

            byte[] AddressArray = BitConverter.GetBytes(Data.Address);
            byte[] NumberOfRegistersArray = BitConverter.GetBytes(Data.WriteData.Length);

            // Function number
            PDU[0] = WriteFunction.Number;
            // Address 
            PDU[1] = AddressArray[1];
            PDU[2] = AddressArray[0];
            // Amount of write registers
            PDU[3] = NumberOfRegistersArray[1];
            PDU[4] = NumberOfRegistersArray[0];
            // Amount of byte next
            PDU[5] = (byte)(Data.WriteData.Length * 2);

            byte[] DataArray;
            int ElementCounter = 6;

            foreach (UInt16 element in Data.WriteData)
            {
                DataArray = BitConverter.GetBytes(element);

                PDU[ElementCounter] = DataArray[1];
                PDU[ElementCounter + 1] = DataArray[0];

                ElementCounter += 2;
            }

            return PDU;
        }
    }
}
