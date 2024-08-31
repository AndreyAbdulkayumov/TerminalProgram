namespace Core.Models.Modbus.Message
{
    public abstract class MessageData
    {
        public byte SlaveID;
        public UInt16 Address;
        public bool CheckSum_IsEnable;
        public UInt16 Polynom;
    }

    public class ReadTypeMessage : MessageData
    {
        public int NumberOfRegisters;

        public ReadTypeMessage(byte slaveID, UInt16 address, int numberOfRegisters, bool checkSum_IsEnable, UInt16 polynom = 0xA001)
        {
            SlaveID = slaveID;
            Address = address;
            NumberOfRegisters = numberOfRegisters;
            CheckSum_IsEnable = checkSum_IsEnable;
            Polynom = polynom;
        }
    }

    public class WriteTypeMessage : MessageData
    {
        public UInt16[] WriteData;

        public WriteTypeMessage(byte slaveID, UInt16 address, UInt16[] writeData, bool checkSum_IsEnable, UInt16 polynom = 0xA001)
        {
            SlaveID = slaveID;
            Address = address;
            WriteData = writeData;
            CheckSum_IsEnable = checkSum_IsEnable;
            Polynom = polynom;
        }
    }
}
