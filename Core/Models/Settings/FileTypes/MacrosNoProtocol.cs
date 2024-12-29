using Core.Models.Settings.DataTypes;

namespace Core.Models.Settings.FileTypes
{
    public class MacrosNoProtocolItem : IMacrosItem
    {
        public string? Name { get; set; }
        public string? Message { get; set; }
        public bool IsByteString { get; set; }
        public bool EnableCR { get; set; }
        public bool EnableLF { get; set; }
    }

    public class MacrosNoProtocol
    {
        public List<MacrosNoProtocolItem>? Items { get; set; }
    }
}
