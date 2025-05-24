using Core.Models.Settings.DataTypes;
using Core.Models.Settings.FileTypes;

namespace ViewModels.Macros;

internal static class MacrosHelper
{
    // Необходимо реализовывать глубокое копирование контента, иначе возможно изменение элементов первоначальной коллекции.
    internal static MacrosContent<T1, T2> GetWithAdditionalData<T1, T2>(MacrosContent<T1, T2> macrosContent)
    {
        if (macrosContent is MacrosContent<ModbusAdditionalData, MacrosCommandModbus> modbusContent)
        {
            var updatedContent = (object)ApplyAdditionalData_Modbus(modbusContent);
            return (MacrosContent<T1, T2>)updatedContent;
        }

        return macrosContent;
    }

    private static MacrosContent<ModbusAdditionalData, MacrosCommandModbus> ApplyAdditionalData_Modbus(MacrosContent<ModbusAdditionalData, MacrosCommandModbus> macrosContent)
    {
        if (macrosContent.Commands == null ||
            macrosContent.AdditionalData == null ||
            !macrosContent.AdditionalData.UseCommonSlaveId)
        {
            return macrosContent;
        }

        var changedContent = new MacrosContent<ModbusAdditionalData, MacrosCommandModbus>()
        {
            MacrosName = macrosContent.MacrosName,
            AdditionalData =  new ModbusAdditionalData()
            {
                UseCommonSlaveId = macrosContent.AdditionalData.UseCommonSlaveId,
                CommonSlaveId = macrosContent.AdditionalData.CommonSlaveId,
            },
            Commands = new List<MacrosCommandModbus>(),
        };

        foreach (var command in macrosContent.Commands)
        {
            changedContent.Commands.Add(new MacrosCommandModbus()
            {
                Name = command.Name,
                Content = command.Content?.Clone() as ModbusCommandContent
            });
        }

        foreach (var command in changedContent.Commands)
        {
            if (command.Content == null)
                continue;

            command.Content.SlaveID = changedContent.AdditionalData.CommonSlaveId;
        }

        return changedContent;
    }
}
