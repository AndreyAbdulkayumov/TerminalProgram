namespace Core.Models.Modbus.DataTypes;

public struct ModbusResponse
{
    // Только для Modbus TCP
    public UInt16 OperationNumber;
    public UInt16 ProtocolID;
    public UInt16 LengthOfPDU;

    // Общая часть для всех типов Modbus протокола
    public byte SlaveID;

    // PDU - Protocol Data Unit
    public byte Command;
    public int LengthOfData;
    public byte[] Data;
}
