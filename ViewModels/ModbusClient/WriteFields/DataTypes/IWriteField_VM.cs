using Core.Models.Settings.DataTypes;
using ViewModels.ModbusClient.DataTypes;

namespace ViewModels.ModbusClient.WriteFields.DataTypes;

public interface IWriteField_VM
{
    WriteData GetData();
    void SetDataFromMacros(ModbusMacrosWriteInfo data);
    ModbusMacrosWriteInfo GetMacrosData();
    bool HasValidationErrors { get; }
    string? ValidationMessage { get; }
}
