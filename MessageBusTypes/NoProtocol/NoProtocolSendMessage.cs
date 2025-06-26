using System.Text;

namespace MessageBusTypes.NoProtocol;

public class NoProtocolSendMessage
{
    public readonly string? Sender;

    public readonly bool IsBytes;
    public readonly string? Message;
    public readonly bool EnableCR;
    public readonly bool EnableLF;
    public readonly Encoding SelectedEncoding;

    public NoProtocolSendMessage(string? sender, bool isBytes, string? message, bool enableCR, bool enableLF, Encoding selectedEncoding)
    {
        Sender = sender;
        IsBytes = isBytes;
        Message = message;
        EnableCR = enableCR;
        EnableLF = enableLF;
        SelectedEncoding = selectedEncoding;
    }
}
