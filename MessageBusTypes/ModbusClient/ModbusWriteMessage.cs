using Core.Models.Modbus.DataTypes;

namespace MessageBusTypes.ModbusClient;

public class ModbusWriteMessage
{
    public readonly string? Sender;

    public readonly byte SlaveID;
    public readonly ushort Address;
    public readonly ModbusWriteFunction Function;
    public readonly byte[]? WriteData;
    public readonly int NumberOfRegisters;
    public readonly bool CheckSum_IsEnable;

    public ModbusWriteMessage(string? sender, byte slaveID, ushort address, ModbusWriteFunction function, byte[]? writeData, int numberOfRegisters, bool checkSum_IsEnable)
    {
        Sender = sender;
        SlaveID = slaveID;
        Address = address;
        Function = function;
        WriteData = writeData;
        NumberOfRegisters = numberOfRegisters;
        CheckSum_IsEnable = checkSum_IsEnable;
    }
}
