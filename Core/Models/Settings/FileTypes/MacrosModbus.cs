
using Core.Models.Settings.DataTypes;

namespace Core.Models.Settings.FileTypes;

public class ModbusCommandInfo
{
    public byte SlaveID { get; set; }
    public bool CheckSum_IsEnable { get; set; }
    public ushort Address { get; set; }
    public int FunctionNumber { get; set; }
    public int NumberOfReadRegisters { get; set; }
    public ModbusMacrosWriteInfo? WriteInfo { get; set; }
}

public class MacrosCommandModbus : IMacrosCommand
{
    public string? Name { get; set; }
    public ModbusCommandInfo? Content { get; set; }
}

public class ModbusAdditionalData
{
    public bool UseCommonSlaveId { get; set; }
    public byte CommonSlaveId { get; set; }
}

public class MacrosModbus
{
    public List<MacrosContent<ModbusAdditionalData, MacrosCommandModbus>>? Items { get; set; }
}
