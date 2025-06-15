using MessageBox.Core;

namespace MessageBusTypes.Macros;

public class MacrosActionResponse
{
    public readonly string? Name;
    public readonly bool ActionSuccess;

    public readonly string? Message;
    public readonly MessageType Type = MessageType.Information;
    public readonly Exception? Error;


    public MacrosActionResponse(string? name, bool actionSuccess, string message, MessageType messageType, Exception? error)
    {
        Name = name;
        ActionSuccess = actionSuccess;

        Message = message;
        Type = messageType;
        Error = error;
    }
}
