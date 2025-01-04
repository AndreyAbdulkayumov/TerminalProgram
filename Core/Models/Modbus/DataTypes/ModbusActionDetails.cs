namespace Core.Models.Modbus.DataTypes
{
    public class ModbusActionDetails
    {
        public byte[]? RequestBytes;
        public byte[]? ResponseBytes;

        public DateTime Request_ExecutionTime;
        public DateTime Response_ExecutionTime;
    }
}
