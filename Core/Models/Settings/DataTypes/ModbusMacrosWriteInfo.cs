namespace Core.Models.Settings.DataTypes
{
    public class ModbusMacrosWriteInfo
    {
        public byte[]? WriteBuffer { get; set; }
        public int NumberOfWriteRegisters { get; set; }
        public int[]? FloatByteIndices { get; set; }
    }
}
