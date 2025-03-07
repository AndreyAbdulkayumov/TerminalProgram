using ReactiveUI;
using Core.Models.Settings.DataTypes;
using Core.Models.Settings.FileTypes;
using ViewModels.Macros.DataTypes;

namespace ViewModels.Macros.MacrosItemContext
{
    internal class ModbusMacrosItemContext : IMacrosContext
    {
        private readonly MacrosContent<MacrosCommandModbus> _content;

        public ModbusMacrosItemContext(MacrosContent<MacrosCommandModbus> content)
        {
            _content = content;
        }

        public MacrosData CreateContext()
        {
            Action action = () =>
            {
                if (_content.Commands == null)
                {
                    return;
                }

                MessageBus.Current.SendMessage(_content.Commands);
            };

            return new MacrosData(_content.MacrosName, action);
        }
    }
}
