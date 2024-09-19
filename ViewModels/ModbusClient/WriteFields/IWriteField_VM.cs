using ViewModels.Validation;

namespace ViewModels.ModbusClient.WriteFields
{
    public class WriteData
    {
        public readonly byte[] Data;
        public readonly int NumberOfRegisters;

        public WriteData(byte[] data, int numberOfRegisters)
        {
            Data = data;
            NumberOfRegisters = numberOfRegisters;
        }
    }

    public interface IWriteField_VM
    {
        WriteData GetData();
        bool HasValidationErrors { get; }
        string? ValidationMessage { get; }
    }
}
