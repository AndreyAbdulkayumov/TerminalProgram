using Core.Models.Settings.DataTypes;
using Core.Models.Settings.FileTypes;
using ReactiveUI;
using ViewModels.Macros.DataTypes;

namespace ViewModels.Macros;

public class ViewItemContext<T1, T2> : IMacrosContext
{
    private readonly MacrosContent<T1, T2> _content;

    public ViewItemContext(MacrosContent<T1, T2> content)
    {
        _content = content;
    }

    public MacrosViewItemData CreateContext()
    {
        if (_content.AdditionalData is ModbusAdditionalData modbusAdditions &&
            _content.Commands is List<MacrosCommandModbus> macrosCommands)
        {
            if (macrosCommands != null && modbusAdditions.UseCommonSlaveId)
            {
                foreach (var command in macrosCommands)
                {
                    if (command.Content == null)
                        continue;

                    command.Content.SlaveID = modbusAdditions.CommonSlaveId;
                }
            }
        }

        Action action = () =>
        {
            if (_content.Commands == null)
            {
                return;
            }

            MessageBus.Current.SendMessage(_content);
        };

        return new MacrosViewItemData(_content.MacrosName ?? string.Empty, action);
    }
}
