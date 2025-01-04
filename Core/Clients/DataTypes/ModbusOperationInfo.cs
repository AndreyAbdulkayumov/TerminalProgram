namespace Core.Clients.DataTypes
{
    public class ModbusOperationInfo
    {
        public readonly DateTime ExecutionTime;
        public readonly byte[]? ResponseBytes;

        public ModbusOperationInfo(DateTime executionTime, byte[]? responseBytes)
        {
            ExecutionTime = executionTime;
            ResponseBytes = responseBytes;
        }
    }
}
