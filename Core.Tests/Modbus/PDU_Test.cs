using Core.Models.Modbus.DataTypes;
using Core.Models.Modbus.Message;

namespace Core.Tests.Modbus
{
    public class PDU_Test
    {
        [Fact]
        public void Test_Func_01()
        {
            CheckReadFunction(
                SelectedFunction:  Function.ReadCoilStatus,
                Address:           16,
                NumberOfRegisters: 2
                );
        }

        [Fact]
        public void Test_Func_02()
        {
            CheckReadFunction(
                SelectedFunction:  Function.ReadDiscreteInputs,
                Address:           2,
                NumberOfRegisters: 4
                );
        }

        [Fact]
        public void Test_Func_03()
        {
            CheckReadFunction(
                SelectedFunction:  Function.ReadHoldingRegisters,
                Address:           0,
                NumberOfRegisters: 3
                );
        }

        [Fact]
        public void Test_Func_04()
        {
            CheckReadFunction(
                SelectedFunction:  Function.ReadInputRegisters,
                Address:           45,
                NumberOfRegisters: 7
                );
        }

        [Fact]
        public void Test_Func_05()
        {
            CheckSingleWriteFunction(
                SelectedFunction: Function.ForceSingleCoil,
                Address:          82,
                WriteData:        0x6891
                );
        }

        [Fact]
        public void Test_Func_06()
        {
            CheckSingleWriteFunction(
                SelectedFunction: Function.PresetSingleRegister,
                Address:          76,
                WriteData:        0x5942
                );
        }

        [Fact]
        public void Test_Func_16()
        {
            CheckMultiplyWriteFunction(
                SelectedFunction: Function.PresetMultipleRegisters,
                Address:          79,
                WriteData:        new UInt16[] { 0x0F50, 0x4575, 0x0789, 0x0030 }
                );
        }


        // Общий функционал

        private void CheckReadFunction(ModbusReadFunction SelectedFunction, UInt16 Address, UInt16 NumberOfRegisters)
        {
            MessageData Data = new ReadTypeMessage(
                    16,
                    Address,
                    NumberOfRegisters,
                    true
                    );

            byte[] BytesArray_Actual = Modbus_PDU.Create(SelectedFunction, Data);

            byte[] AddressBytes = ModbusField.Get_Address(Address);
            byte[] NumberOfRegistersBytes = ModbusField.Get_NumberOfRegisters(NumberOfRegisters);

            byte[] BytesArray_Expected = new byte[]
            {
                SelectedFunction.Number,
                AddressBytes[1],
                AddressBytes[0],
                NumberOfRegistersBytes[1],
                NumberOfRegistersBytes[0]
            };

            Assert.Equal(BytesArray_Expected, BytesArray_Actual);
        }

        private void CheckSingleWriteFunction(ModbusWriteFunction SelectedFunction, UInt16 Address, UInt16 WriteData)
        {
            UInt16[] WriteDataArray = new UInt16[] { WriteData };

            byte[] bytes = BitConverter.GetBytes(WriteData);

            MessageData Data = new WriteTypeMessage(
                16,
                Address,
                bytes,
                1,
                true
                );

            byte[] BytesArray_Actual = Modbus_PDU.Create(SelectedFunction, Data);

            byte[] AddressBytes = ModbusField.Get_Address(Address);
            byte[] WriteDataBytes = ModbusField.Get_WriteData(WriteDataArray);

            if (WriteDataBytes.Length != 2)
            {
                throw new Exception("При записи одного регистра поле данных должно содержать только 2 байта.");
            }

            byte[] BytesArray_Expected = new byte[]
            {
                SelectedFunction.Number,
                AddressBytes[1],
                AddressBytes[0],
                WriteDataBytes[0],
                WriteDataBytes[1]
            };

            Assert.Equal(BytesArray_Expected, BytesArray_Actual);
        }

        private void CheckMultiplyWriteFunction(ModbusWriteFunction SelectedFunction, UInt16 Address, UInt16[] WriteData)
        {
            byte[] bytes = WriteData.SelectMany(BitConverter.GetBytes).ToArray();

            MessageData Data = new WriteTypeMessage(
                16,
                Address,
                bytes,
                WriteData.Length,
                true
                );

            byte[] BytesArray_Actual = Modbus_PDU.Create(SelectedFunction, Data);

            byte[] AddressBytes = ModbusField.Get_Address(Address);
            byte[] NumberOfRegisters = ModbusField.Get_NumberOfRegisters((UInt16)WriteData.Length);
            byte[] WriteDataBytes = ModbusField.Get_WriteData(WriteData);

            if (WriteDataBytes.Length != WriteData.Length * 2)
            {
                throw new Exception("Неправильное количество байт в поле данных.");
            }

            byte[] BytesArray_Expected = new byte[6 + WriteDataBytes.Length];

            BytesArray_Expected[0] = SelectedFunction.Number;
            BytesArray_Expected[1] = AddressBytes[1];
            BytesArray_Expected[2] = AddressBytes[0];
            BytesArray_Expected[3] = NumberOfRegisters[1];
            BytesArray_Expected[4] = NumberOfRegisters[0];
            BytesArray_Expected[5] = (byte)WriteDataBytes.Length;

            Array.Copy(WriteDataBytes, 0, BytesArray_Expected, 6, WriteDataBytes.Length);

            Assert.Equal(BytesArray_Expected, BytesArray_Actual);
        }
    }
}