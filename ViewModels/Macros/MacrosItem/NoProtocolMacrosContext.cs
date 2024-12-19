using ViewModels.NoProtocol;

namespace ViewModels.Macros.MacrosItem
{
    internal class NoProtocolMacrosContext : IMacrosContext
    {
        private string _macrosName;
        private string _message;
        private bool _enableCR;
        private bool _enableLF;

        public NoProtocolMacrosContext(string macrosName, string message, bool enableCR, bool enableLF)
        {
            _macrosName = macrosName;
            _message = message;
            _enableCR = enableCR;
            _enableLF = enableLF;
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
