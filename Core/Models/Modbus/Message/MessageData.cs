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

        public ReadTypeMessage(byte SlaveID, UInt16 Address, int NumberOfRegisters, bool CheckSum_IsEnable, UInt16 Polynom = 0xA001)
        {
            this.SlaveID = SlaveID;
            this.Address = Address;
            this.NumberOfRegisters = NumberOfRegisters;
            this.CheckSum_IsEnable = CheckSum_IsEnable;
            this.Polynom = Polynom;
        }
    }

    public class WriteTypeMessage : MessageData
    {
        public UInt16[] WriteData;

        public WriteTypeMessage(byte SlaveID, UInt16 Address, UInt16[] WriteData, bool CheckSum_IsEnable, UInt16 Polynom = 0xA001)
        {
            this.SlaveID = SlaveID;
            this.Address = Address;
            this.WriteData = WriteData;
            this.CheckSum_IsEnable = CheckSum_IsEnable;
            this.Polynom = Polynom;
        }
    }
}
