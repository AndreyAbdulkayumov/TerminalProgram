using Core.Models.Modbus.DataTypes;

namespace MessageBusTypes.ModbusClient;

public class ModbusReadMessage
{
    public readonly string? Sender;

    public readonly byte SlaveID;
    public readonly ushort Address;
    public readonly ModbusReadFunction Function;
    public readonly int NumberOfRegisters;
    public readonly bool CheckSum_IsEnable;

    public ModbusReadMessage(string? sender, byte slaveID, ushort address, ModbusReadFunction function, int numberOfRegisters, bool checkSum_IsEnable)
    {
        Sender = sender;
        SlaveID = slaveID;
        Address = address;
        Function = function;
        NumberOfRegisters = numberOfRegisters;
        CheckSum_IsEnable = checkSum_IsEnable;
    }
}
