using Core.Models.Settings.DataTypes;
using Core.Models.Settings.FileTypes;
using ViewModels.Helpers;
using ViewModels.Macros.DataTypes;
using ViewModels.NoProtocol;

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
            Func<Task> action = async () =>
            {
                if (NoProtocol_VM.Instance == null)
                {
                    return;
                }

                if (_content.Commands == null)
                {
                    return;
                }

                foreach (var command in _content.Commands)
                {
                    if (command.Content == null)
                        continue;
                    
                    await NoProtocol_VM.Instance.NoProtocol_Send(command.Content.IsByteString, command.Content.Message, command.Content.EnableCR, command.Content.EnableLF, AppEncoding.GetEncoding(command.Content.MacrosEncoding));
                }                
            };

            return new MacrosData(_content.MacrosName, action);
        }
    }
}
