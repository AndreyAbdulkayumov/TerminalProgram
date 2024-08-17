namespace ViewModels.MainWindow.Modbus
{
    public class BinaryRepresentation_ItemData
    {
        public string? Address { get; set; }
        public BinaryDataItemGroup[]? BinaryData { get; set; }
    }

    public class BinaryDataItemGroup
    {
        public BinaryDataItem[]? GroupData { get; set; }
    }

    public class BinaryDataItem
    {
        public string? Bit { get; set; } = "0";
        public bool IsChange { get; set; } = true;
    }
}
