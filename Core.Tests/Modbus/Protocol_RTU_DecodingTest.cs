using Core.Models.Modbus;
using Core.Models.Modbus.Message;

namespace Core.Tests.Modbus
{
    public class Protocol_RTU_DecodingTest
    {
        private ModbusMessage Message = new ModbusRTU_Message();

        [Fact]
        public void Test_Func_01_Success()
        {
            byte[] DataArray_Expected = new byte[] 
            { 
                0x11,   // Slave ID
                0x01,   // Function Number
                0x05,    // Number of bytes next
                0xCD,    // Data bytes
                0x6B,
                0xB2,
                0x0E,
                0x1B
            };
            
            ModbusResponse Response_Actual = Message.DecodingMessage(Function.ReadCoilStatus, DataArray_Expected);

            byte[] DataArray_Actual = new byte[3 + Response_Actual.Data.Length];

            DataArray_Actual[0] = Response_Actual.SlaveID;
            DataArray_Actual[1] = Response_Actual.Command;
            DataArray_Actual[2] = (byte)Response_Actual.LengthOfData;

            Array.Copy(Response_Actual.Data, 0, DataArray_Actual, 3, Response_Actual.Data.Length);

            Assert.Equal(DataArray_Expected, DataArray_Actual);
        }

        [Fact]
        public void Test_Func_01_Error()
        {
            Assert.Throws<ModbusException>(new Action(() =>
            {
                byte[] TestMessage = new byte[]
                {
                    0x11,   // Slave ID
                    0x81,   // Error Marker + Function Number
                    0x05,   // Error Code
                };

                Message.DecodingMessage(Function.ReadCoilStatus, TestMessage);
            }));
        }

        [Fact]
        public void Test_Func_02_Success()
        {
            byte[] TestMessage = new byte[]
            {
                0x11,   // Slave ID
                0x02,   // Function Number
                0x03,    // Number of bytes next
                0xAC,    // Data bytes
                0xDB,
                0x35
            };

            // Записываем байты по порядку SlaveID, Command, LengthOfData, DataBytes
            byte[] DataArray_Expected = new byte[] { 0x11, 0x01, 0x05, 0xCD, 0x6B, 0xB2, 0x0E, 0x1B };

            ModbusResponse Response_Actual = Message.DecodingMessage(Function.ReadCoilStatus, TestMessage);

            byte[] DataArray_Actual = new byte[3 + Response_Actual.Data.Length];

            DataArray_Actual[0] = Response_Actual.SlaveID;
            DataArray_Actual[1] = Response_Actual.Command;
            DataArray_Actual[2] = (byte)Response_Actual.LengthOfData;

            Array.Copy(Response_Actual.Data, 0, DataArray_Actual, 3, Response_Actual.Data.Length);

            Assert.Equal(DataArray_Expected, DataArray_Actual);
        }

        [Fact]
        public void Test_Func_02_Error()
        {
            Assert.Throws<ModbusException>(new Action(() =>
            {
                byte[] TestMessage = new byte[]
                {
                    0x11,   // Slave ID
                    0x82,   // Error Marker + Function Number
                    0x01,   // Error Code
                };

                Message.DecodingMessage(Function.ReadCoilStatus, TestMessage);
            }));
        }
    }
}
