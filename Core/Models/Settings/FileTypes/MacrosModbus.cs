
using Core.Models.Settings.DataTypes;

namespace Core.Models.Settings.FileTypes;

public class ModbusCommandContent : ICloneable
{
    public byte SlaveID { get; set; }
    public bool CheckSum_IsEnable { get; set; }
    public ushort Address { get; set; }
    public int FunctionNumber { get; set; }
    public int NumberOfReadRegisters { get; set; }
    public ModbusMacrosWriteInfo? WriteInfo { get; set; }

    public object Clone()
    {
        return new ModbusCommandContent()
        {
            SlaveID = SlaveID,
            CheckSum_IsEnable = CheckSum_IsEnable,
            Address = Address,
            FunctionNumber = FunctionNumber,
            NumberOfReadRegisters = NumberOfReadRegisters,
            WriteInfo = new ModbusMacrosWriteInfo()
            {
                WriteBuffer = WriteInfo?.WriteBuffer,
                NumberOfWriteRegisters = WriteInfo?.NumberOfWriteRegisters ?? 0,
                FloatNumberFormat = WriteInfo?.FloatNumberFormat,
                FloatStartByteIndices = WriteInfo?.FloatStartByteIndices
            }
        };
    }
}

public class MacrosCommandModbus : IMacrosCommand
{
    public string? Name { get; set; }
    public ModbusCommandContent? Content { get; set; }
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
