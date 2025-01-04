
using Core.Models.Settings.DataTypes;

namespace Core.Models.Settings.FileTypes
{
    public class MacrosModbusItem : IMacrosItem
    {
        public string? Name { get; set; }
        public byte SlaveID { get; set; }
        public bool CheckSum_IsEnable { get; set; }
        public ushort Address { get; set; }
        public int FunctionNumber { get; set; }
        public int NumberOfReadRegisters { get; set; }
        public ModbusMacrosWriteInfo? WriteInfo { get; set; }
    }

    public class MacrosModbus
    {
        public List<MacrosModbusItem>? Items { get; set; }
    }
}
