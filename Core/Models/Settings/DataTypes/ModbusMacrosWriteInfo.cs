namespace Core.Models.Settings.DataTypes;

public class ModbusMacrosWriteInfo
{
    public byte[]? WriteBuffer { get; set; }
    public int NumberOfWriteRegisters { get; set; }
    public string? FloatNumberFormat { get; set; }
    public int[]? FloatStartByteIndices { get; set; }
}
