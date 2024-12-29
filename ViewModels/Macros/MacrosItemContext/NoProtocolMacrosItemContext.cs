using Core.Models.Settings.FileTypes;
using ViewModels.Helpers;
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

                await NoProtocol_VM.Instance.NoProtocol_Send(_content.IsByteString, _content.Message, _content.EnableCR, _content.EnableLF, AppEncoding.GetEncoding(_content.MacrosEncoding));
            };

            return new MacrosData(_content.Name, action);
        }
    }
}
