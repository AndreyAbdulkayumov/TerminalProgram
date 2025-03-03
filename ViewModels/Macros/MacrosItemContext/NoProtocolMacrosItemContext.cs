using Core.Models.Settings.DataTypes;
using Core.Models.Settings.FileTypes;
using ReactiveUI;
using ViewModels.Macros.DataTypes;

namespace ViewModels.Macros.MacrosItemContext
{
    internal class NoProtocolMacrosItemContext : IMacrosContext
    {
        private readonly MacrosContent<MacrosCommandNoProtocol> _content;

        public NoProtocolMacrosItemContext(MacrosContent<MacrosCommandNoProtocol> content)
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
