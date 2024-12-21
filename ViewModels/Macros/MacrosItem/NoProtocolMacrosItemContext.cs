using Core.Models.Settings.FileTypes;
using ViewModels.NoProtocol;

namespace ViewModels.Macros.MacrosItem
{
    internal class NoProtocolMacrosItemContext : IMacrosContext
    {
        private string? _macrosName;
        private string? _message;
        private bool _enableCR;
        private bool _enableLF;

        public NoProtocolMacrosItemContext(MacrosNoProtocolItem info)
        {
            _macrosName = info.Name;
            _message = info.Message;
            _enableCR = info.EnableCR;
            _enableLF = info.EnableLF;
        }

        public MacrosData CreateContext()
        {
            Func<Task> action = async () =>
            {
                if (NoProtocol_VM.Instance == null)
                {
                    return;
                }

                await NoProtocol_VM.Instance.NoProtocol_Send(false, _message, _enableCR, _enableLF);
            };

            return new MacrosData(_macrosName, action);
        }
    }
}
