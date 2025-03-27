using Core.Models.Modbus.DataTypes;
using Core.Models.Modbus.Message;

namespace Core.Tests.Modbus
{
    public class Protocol_TCP_CreateTest
    {
        private ModbusMessage Message = new ModbusTCP_Message();

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
                WriteData:         0xFF00
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
        public void Test_Func_0F()
        {
            CheckMultiplyWriteCoilsFunction(
                packageNumber:     0,
                slaveID:           32,
                address:           73,
                bitArray:          new int[] { 0, 0, 1, 1, 1, 0, 1 }
                );
        }

        [Fact]
        public void Test_Func_16()
        {
            CheckMultiplyWriteRegistersFunction(
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
                false
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
            // Количество байт далее (байт SlaveID + байты PDU)
            BytesArray_Expected[4] = 0;
            BytesArray_Expected[5] = 6;
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

            byte[] bytes = BitConverter.GetBytes(WriteData);

            MessageData Data = new WriteTypeMessage(
                SlaveID,
                Address,
                bytes,
                1,
                false
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
            // Количество байт далее (байт SlaveID + байты PDU)
            BytesArray_Expected[4] = 0;
            BytesArray_Expected[5] = 6;
            BytesArray_Expected[6] = SlaveID;
            BytesArray_Expected[7] = SelectedFunction.Number;
            BytesArray_Expected[8] = AddressBytes[1];
            BytesArray_Expected[9] = AddressBytes[0];
            BytesArray_Expected[10] = WriteDataBytes[0];
            BytesArray_Expected[11] = WriteDataBytes[1];

            Assert.Equal(BytesArray_Expected, BytesArray_Actual);
        }

        private void CheckMultiplyWriteCoilsFunction(UInt16 packageNumber, byte slaveID, UInt16 address, int[] bitArray)
        {
            ModbusWriteFunction selectedFunction = Function.ForceMultipleCoils;

            (byte[] writeBytes, int numberOfCoils) = ModbusField.Get_WriteDataFromMultipleCoils(bitArray);

            MessageData data = new WriteTypeMessage(
                slaveID,
                address,
                writeBytes,
                numberOfCoils,
                false
                );

            byte[] bytesArray_Actual = Message.CreateMessage(selectedFunction, data);

            byte[] bytesArray_Expected = new byte[13 + writeBytes.Length];

            // PDU - 6 байт + байт SlaveID + байты данных
            byte[] slaveID_PDU_Size_Bytes = BitConverter.GetBytes((UInt16)(7 + writeBytes.Length));

            byte[] packageNumberArray = BitConverter.GetBytes(packageNumber);
            byte[] addressBytes = ModbusField.Get_Address(address);
            byte[] numberOfRegisters = ModbusField.Get_NumberOfRegisters((UInt16)numberOfCoils);            

            bytesArray_Expected[0] = packageNumberArray[1];
            bytesArray_Expected[1] = packageNumberArray[0];
            // Modbus ID
            bytesArray_Expected[2] = 0;
            bytesArray_Expected[3] = 0;
            // Количество байт далее (байт SlaveID + байты PDU)
            bytesArray_Expected[4] = slaveID_PDU_Size_Bytes[1];
            bytesArray_Expected[5] = slaveID_PDU_Size_Bytes[0];
            bytesArray_Expected[6] = slaveID;
            bytesArray_Expected[7] = selectedFunction.Number;
            bytesArray_Expected[8] = addressBytes[1];
            bytesArray_Expected[9] = addressBytes[0];
            bytesArray_Expected[10] = numberOfRegisters[1];
            bytesArray_Expected[11] = numberOfRegisters[0];
            bytesArray_Expected[12] = (byte)writeBytes.Length;

            Array.Copy(writeBytes, 0, bytesArray_Expected, 13, writeBytes.Length);

            Assert.Equal(bytesArray_Expected, bytesArray_Actual);
        }

        private void CheckMultiplyWriteRegistersFunction(UInt16 PackageNumber, byte SlaveID, UInt16 Address, UInt16[] WriteData)
        {
            ModbusWriteFunction selectedFunction = Function.PresetMultipleRegisters;

            byte[] bytes = WriteData.SelectMany(BitConverter.GetBytes).ToArray();

            MessageData Data = new WriteTypeMessage(
                SlaveID,
                Address,
                bytes,
                WriteData.Length,
                false
                );

            byte[] BytesArray_Actual = Message.CreateMessage(selectedFunction, Data);

            byte[] PackageNumberArray = BitConverter.GetBytes(PackageNumber);
            byte[] AddressBytes = ModbusField.Get_Address(Address);
            byte[] NumberOfRegisters = ModbusField.Get_NumberOfRegisters((UInt16)WriteData.Length);
            byte[] WriteDataBytes = ModbusField.Get_WriteData(WriteData);

            // PDU - 6 байт + байт SlaveID + байты данных
            byte[] SlaveID_PDU_Size_Bytes = BitConverter.GetBytes((UInt16)(7 + WriteDataBytes.Length));

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
            // Количество байт далее (байт SlaveID + байты PDU)
            BytesArray_Expected[4] = SlaveID_PDU_Size_Bytes[1];
            BytesArray_Expected[5] = SlaveID_PDU_Size_Bytes[0];
            BytesArray_Expected[6] = SlaveID;
            BytesArray_Expected[7] = selectedFunction.Number;
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
