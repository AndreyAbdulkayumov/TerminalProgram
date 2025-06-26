using MessageBox.Core;

namespace MessageBusTypes.Macros;

public class MacrosActionResponse
{
    public readonly string? Name;
    public readonly string? Sender;
    public readonly bool ActionSuccess;

    public readonly string? Message;
    public readonly MessageType Type = MessageType.Information;
    public readonly Exception? Error;


    public MacrosActionResponse(string? name, string? sender, bool actionSuccess, string message, MessageType messageType, Exception? error)
    {
        Name = name;
        Sender = sender;
        ActionSuccess = actionSuccess;

        Message = message;
        Type = messageType;
        Error = error;
    }
}
