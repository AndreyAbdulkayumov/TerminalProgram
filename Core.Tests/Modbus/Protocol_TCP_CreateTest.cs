using Core.Models.Modbus;
using Core.Models.Modbus.Message;

namespace Core.Tests.Modbus
{
    public class Protocol_TCP_CreateTest
    {
        private ModbusMessage Message = new ModbusTCP_Message();

        private const UInt16 Polynom = 0xA001;

        // PackageNumber делать всегда равным 0

        [Fact]
        public void Test_Func_01()
        {
            CheckReadFunction(
                SelectedFunction:  Function.ReadCoilStatus,
                PackageNumber:     0,
                SlaveID:           156,
                Address:           12,
                NumberOfRegisters: 5
                );
        }

        [Fact]
        public void Test_Func_02()
        {
            CheckReadFunction(
                SelectedFunction:  Function.ReadDiscreteInputs,
                PackageNumber:     0,
                SlaveID:           172,
                Address:           12,
                NumberOfRegisters: 2
                );
        }

        [Fact]
        public void Test_Func_03()
        {
            CheckReadFunction(
                SelectedFunction:  Function.ReadHoldingRegisters,
                PackageNumber:     0,
                SlaveID:           16,
                Address:           42,
                NumberOfRegisters: 5
                );
        }

        [Fact]
        public void Test_Func_04()
        {
            CheckReadFunction(
                SelectedFunction:  Function.ReadInputRegisters,
                PackageNumber:     0,
                SlaveID:           6,
                Address:           19,
                NumberOfRegisters: 4
                );
        }

        [Fact]
        public void Test_Func_05()
        {
            CheckSingleWriteFunction(
                SelectedFunction:  Function.ForceSingleCoil,
                PackageNumber:     0,
                SlaveID:           13,
                Address:           96,
                WriteData:         0x0056
                );
        }

        [Fact]
        public void Test_Func_06()
        {
            CheckSingleWriteFunction(
                SelectedFunction:  Function.PresetSingleRegister,
                PackageNumber:     0,
                SlaveID:           18,
                Address:           63,
                WriteData:         0xAED5
                );
        }

        [Fact]
        public void Test_Func_16()
        {
            CheckMultiplyWriteFunction(
                SelectedFunction:  Function.PresetMultipleRegister,
                PackageNumber:     0,
                SlaveID:           18,
                Address:           63,
                WriteData:         new UInt16[] { 0xFFFF, 0x4586, 0x4000, 0x0568, 0xFAFD }
                );
        }



        // Общий функционал

        private void CheckReadFunction(ModbusReadFunction SelectedFunction, UInt16 PackageNumber,
            byte SlaveID, UInt16 Address, UInt16 NumberOfRegisters)
        {
            MessageData Data = new ReadTypeMessage(
                SlaveID,
                Address,
                NumberOfRegisters,
                false,
                Polynom
                );

            byte[] BytesArray_Actual = Message.CreateMessage(SelectedFunction, Data);

            byte[] PackageNumberArray = BitConverter.GetBytes(PackageNumber);
            byte[] AddressBytes = ModbusField.Get_Address(Address);
            byte[] NumberOfRegistersBytes = ModbusField.Get_NumberOfRegisters(NumberOfRegisters);

            byte[] BytesArray_Expected = new byte[12];

            BytesArray_Expected[0] = PackageNumberArray[1];
            BytesArray_Expected[1] = PackageNumberArray[0];
            // Modbus ID
            BytesArray_Expected[2] = 0;
            BytesArray_Expected[3] = 0;
            // Длина PDU в байтах (Количество байт после SlaveID)
            BytesArray_Expected[4] = 0;
            BytesArray_Expected[5] = 5;
            BytesArray_Expected[6] = SlaveID;
            BytesArray_Expected[7] = SelectedFunction.Number;
            BytesArray_Expected[8] = AddressBytes[1];
            BytesArray_Expected[9] = AddressBytes[0];
            BytesArray_Expected[10] = NumberOfRegistersBytes[1];
            BytesArray_Expected[11] = NumberOfRegistersBytes[0];

            Assert.Equal(BytesArray_Actual, BytesArray_Expected);
        }

        private void CheckSingleWriteFunction(ModbusWriteFunction SelectedFunction, UInt16 PackageNumber,
            byte SlaveID, UInt16 Address, UInt16 WriteData)
        {
            UInt16[] WriteDataArray = new UInt16[] { WriteData };

            MessageData Data = new WriteTypeMessage(
                SlaveID,
                Address,
                WriteDataArray,
                false,
                Polynom
                );

            byte[] BytesArray_Actual = Message.CreateMessage(SelectedFunction, Data);

            byte[] PackageNumberArray = BitConverter.GetBytes(PackageNumber);
            byte[] AddressBytes = ModbusField.Get_Address(Address);
            byte[] WriteDataBytes = ModbusField.Get_WriteData(WriteDataArray);

            if (WriteDataBytes.Length != 2)
            {
                throw new Exception("При записи одного регистра поле данных должно содержать только 2 байта.");
            }

            byte[] BytesArray_Expected = new byte[12];

            BytesArray_Expected[0] = PackageNumberArray[1];
            BytesArray_Expected[1] = PackageNumberArray[0];
            // Modbus ID
            BytesArray_Expected[2] = 0;
            BytesArray_Expected[3] = 0;
            // Длина PDU в байтах (Количество байт после SlaveID)
            BytesArray_Expected[4] = 0;
            BytesArray_Expected[5] = 5;
            BytesArray_Expected[6] = SlaveID;
            BytesArray_Expected[7] = SelectedFunction.Number;
            BytesArray_Expected[8] = AddressBytes[1];
            BytesArray_Expected[9] = AddressBytes[0];
            BytesArray_Expected[10] = WriteDataBytes[0];
            BytesArray_Expected[11] = WriteDataBytes[1];

            Assert.Equal(BytesArray_Expected, BytesArray_Actual);
        }

        private void CheckMultiplyWriteFunction(ModbusWriteFunction SelectedFunction, UInt16 PackageNumber,
            byte SlaveID, UInt16 Address, UInt16[] WriteData)
        {
            MessageData Data = new WriteTypeMessage(
                SlaveID,
                Address,
                WriteData,
                false,
                Polynom
                );

            byte[] BytesArray_Actual = Message.CreateMessage(SelectedFunction, Data);

            byte[] PackageNumberArray = BitConverter.GetBytes(PackageNumber);
            byte[] AddressBytes = ModbusField.Get_Address(Address);
            byte[] NumberOfRegisters = ModbusField.Get_NumberOfRegisters((UInt16)WriteData.Length);
            byte[] WriteDataBytes = ModbusField.Get_WriteData(WriteData);

            // PDU - 6 байт + байты данных
            byte[] PDU_Size_Bytes = BitConverter.GetBytes((UInt16)(6 + WriteDataBytes.Length));

            if (WriteDataBytes.Length != WriteData.Length * 2)
            {
                throw new Exception("Неправильное количество байт в поле данных.");
            }

            byte[] BytesArray_Expected = new byte[13 + WriteDataBytes.Length];

            BytesArray_Expected[0] = PackageNumberArray[1];
            BytesArray_Expected[1] = PackageNumberArray[0];
            // Modbus ID
            BytesArray_Expected[2] = 0;
            BytesArray_Expected[3] = 0;
            // Длина PDU в байтах
            BytesArray_Expected[4] = PDU_Size_Bytes[1];
            BytesArray_Expected[5] = PDU_Size_Bytes[0];
            BytesArray_Expected[6] = SlaveID;
            BytesArray_Expected[7] = SelectedFunction.Number;
            BytesArray_Expected[8] = AddressBytes[1];
            BytesArray_Expected[9] = AddressBytes[0];
            BytesArray_Expected[10] = NumberOfRegisters[1];
            BytesArray_Expected[11] = NumberOfRegisters[0];
            BytesArray_Expected[12] = (byte)WriteDataBytes.Length;

            Array.Copy(WriteDataBytes, 0, BytesArray_Expected, 13, WriteDataBytes.Length);

            Assert.Equal(BytesArray_Expected, BytesArray_Actual);
        }
    }
}
