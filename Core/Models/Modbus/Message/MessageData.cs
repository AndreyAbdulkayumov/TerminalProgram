namespace Core.Models.Modbus.Message
{
    public abstract class MessageData
    {
        public byte SlaveID;
        public UInt16 Address;
        public bool CheckSum_IsEnable;
        public UInt16 Polynom;
        public int NumberOfRegisters;
    }

    public class ReadTypeMessage : MessageData
    {
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
        public byte[] WriteData;

        public WriteTypeMessage(byte slaveID, UInt16 address, byte[] writeData, int numberOfRegisters, bool checkSum_IsEnable, UInt16 polynom = 0xA001)
        {
            SlaveID = slaveID;
            Address = address;
            WriteData = writeData;
            NumberOfRegisters = numberOfRegisters;
            CheckSum_IsEnable = checkSum_IsEnable;
            Polynom = polynom;
        }
    }
}
