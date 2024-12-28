using Core.Models.Settings.FileTypes;
using ViewModels.Macros.DataTypes;
using ViewModels.NoProtocol;

namespace ViewModels.Macros.MacrosItemContext
{
    internal class NoProtocolMacrosItemContext : IMacrosContext
    {
        private readonly MacrosNoProtocolItem _content;

        public NoProtocolMacrosItemContext(MacrosNoProtocolItem content)
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

                await NoProtocol_VM.Instance.NoProtocol_Send(false, _content.Message, _content.EnableCR, _content.EnableLF);
            };

            return new MacrosData(_content.Name, action);
        }
    }
}
