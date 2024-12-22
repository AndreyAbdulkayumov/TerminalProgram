﻿namespace Core.Models.Settings.FileTypes
{
    public class MacrosNoProtocolItem : IMacros
    {
        public string? Name { get; set; }
        public string? Message { get; set; }
        public bool EnableCR { get; set; }
        public bool EnableLF { get; set; }
    }

    public class MacrosNoProtocol
    {
        public List<MacrosNoProtocolItem>? Items { get; set; }
    }
}
