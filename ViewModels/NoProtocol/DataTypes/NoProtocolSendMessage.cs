using System.Text;

namespace ViewModels.NoProtocol.DataTypes;

public class NoProtocolSendMessage
{
    public readonly bool IsBytes;
    public readonly string? Message;
    public readonly bool EnableCR;
    public readonly bool EnableLF;
    public readonly Encoding SelectedEncoding;

    public NoProtocolSendMessage(bool isBytes, string? message, bool enableCR, bool enableLF, Encoding selectedEncoding)
    {
        IsBytes = isBytes;
        Message = message;
        EnableCR = enableCR;
        EnableLF = enableLF;
        SelectedEncoding = selectedEncoding;
    }
}
