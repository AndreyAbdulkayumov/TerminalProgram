using Core.Models.Settings.DataTypes;

namespace Core.Models.Settings.FileTypes;

public class NoProtocolCommandInfo
{
    public string? MacrosEncoding { get; set; }
    public string? Message { get; set; }
    public bool EnableCR { get; set; }
    public bool EnableLF { get; set; }
    public bool IsByteString { get; set; }
}

public class MacrosCommandNoProtocol : IMacrosCommand
{
    public string? Name { get; set; }
    public NoProtocolCommandInfo? Content { get; set; }
}

public class MacrosNoProtocol
{
    public List<MacrosContent<object, MacrosCommandNoProtocol>>? Items { get; set; }
}
