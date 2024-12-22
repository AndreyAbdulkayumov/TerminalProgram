namespace Core.Models.Settings.FileTypes
{
    public class MacrosModbusItem : IMacros
    {
        public string? Name { get; set; }
        public byte SlaveID { get; set; }
        public ushort Address { get; set; }
        public int FunctionNumber { get; set; }
        public byte[]? WriteBuffer { get; set; }
        public int NumberOfRegisters { get; set; }
        public bool CheckSum_IsEnable { get; set; }
    }

    public class MacrosModbus
    {
        public List<MacrosModbusItem>? Items { get; set; }
    }
}
