using Core.Models.Modbus.DataTypes;

namespace ViewModels.ModbusClient.MessageBusTypes
{
    public class ModbusWriteMessage
    {
        public readonly byte SlaveID;
        public readonly ushort Address;
        public readonly ModbusWriteFunction Function;
        public readonly byte[]? WriteData;
        public readonly int NumberOfRegisters;
        public readonly bool CheckSum_IsEnable;

        public ModbusWriteMessage(byte slaveID, ushort address, ModbusWriteFunction function, byte[]? writeData, int numberOfRegisters, bool checkSum_IsEnable)
        {
            SlaveID = slaveID;
            Address = address;
            Function = function;
            WriteData = writeData;
            NumberOfRegisters = numberOfRegisters;
            CheckSum_IsEnable = checkSum_IsEnable;
        }
    }
}
