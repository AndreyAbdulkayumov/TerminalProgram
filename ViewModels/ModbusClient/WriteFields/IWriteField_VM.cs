using ViewModels.ModbusClient.DataTypes;

namespace ViewModels.ModbusClient.WriteFields
{
    public interface IWriteField_VM
    {
        WriteData GetData();
        void SetData(WriteData data);
        bool HasValidationErrors { get; }
        string? ValidationMessage { get; }
    }
}
