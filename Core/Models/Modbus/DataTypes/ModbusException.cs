namespace Core.Models.Modbus.DataTypes;

public class ModbusException : Exception
{
    public readonly byte ErrorCode;
    public readonly byte FunctionCode;

    public override string Message { get; }

    public readonly ModbusActionDetails Details = new ModbusActionDetails();

    public ModbusException(byte functionCode, byte errorCode, string message)
    {
        FunctionCode = functionCode;
        ErrorCode = errorCode;
        Message = message;
    }

    public ModbusException(ModbusException errorObject)
    {
        FunctionCode = errorObject.FunctionCode;
        ErrorCode = errorObject.ErrorCode;
        Message = errorObject.Message;
        Details.RequestBytes = errorObject.Details.RequestBytes;
        Details.ResponseBytes = errorObject.Details.ResponseBytes;
    }

    public ModbusException(byte functionCode, byte errorCode, string message,
        byte[] requestBytes, byte[] responseBytes)
    {
        FunctionCode = functionCode;
        ErrorCode = errorCode;
        Message = message;
        Details.RequestBytes = requestBytes;
        Details.ResponseBytes = responseBytes;
    }

    public ModbusException(ModbusException errorObject, byte[] requestBytes, byte[] responseBytes,
        DateTime request_ExecutionTime, DateTime response_ExecutionTime)
    {
        FunctionCode = errorObject.FunctionCode;
        ErrorCode = errorObject.ErrorCode;
        Message = errorObject.Message;
        Details.RequestBytes = requestBytes;
        Details.ResponseBytes = responseBytes;
        Details.Request_ExecutionTime = request_ExecutionTime;
        Details.Response_ExecutionTime = response_ExecutionTime;
    }
}

public class ModbusExceptionInfo : Exception
{
    public ModbusActionDetails? Details;
}
