namespace ViewModels.MainWindow.Modbus
{
    public class ModbusDataDisplayed
    {
        public UInt16 OperationID { get; set; }
        public string? FuncNumber { get; set; }
        public UInt16 Address { get; set; }
        public string? ViewAddress { get; set; }
        public UInt16[]? Data { get; set; }
        public string? ViewData { get; set; }
    }
}
