using Core.Models.Modbus.DataTypes;

namespace ViewModels.ModbusClient.MessageBusTypes;

public class ModbusReadMessage
{
    public readonly byte SlaveID;
    public readonly ushort Address;
    public readonly ModbusReadFunction Function;
    public readonly int NumberOfRegisters;
    public readonly bool CheckSum_IsEnable;

    public ModbusReadMessage(byte slaveID, ushort address, ModbusReadFunction function, int numberOfRegisters, bool checkSum_IsEnable)
    {
        SlaveID = slaveID;
        Address = address;
        Function = function;
        NumberOfRegisters = numberOfRegisters;
        CheckSum_IsEnable = checkSum_IsEnable;
    }
}
