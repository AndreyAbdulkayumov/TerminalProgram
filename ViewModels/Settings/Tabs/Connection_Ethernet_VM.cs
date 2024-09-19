using System.Reactive.Linq;
using Core.Models.Settings;
using ReactiveUI;
using MessageBox_Core;
using ViewModels.Validation;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ViewModels.Settings.Tabs
{
    public class Connection_Ethernet_VM : ValidatedDateInput
    {
        private string? _ip_address = string.Empty;

        public string? IP_Address
        {
            get => _ip_address;
            set
            {
                this.RaiseAndSetIfChanged(ref _ip_address, value);
                ValidateInput(nameof(IP_Address), value);
            }
        }

        private string? _port = string.Empty;

        public string? Port
        {
            get => _port;
            set
            {
                this.RaiseAndSetIfChanged(ref _port, value);
                ValidateInput(nameof(Port), value);
            }
        }

        private readonly Model_Settings SettingsFile;

        private readonly Action<string, MessageType> Message;


        public Connection_Ethernet_VM(Settings_VM main_VM)
        {
            main_VM._settingsFileChanged += Main_VM_SettingsFileChanged;

            Message = main_VM.Message;

            SettingsFile = Model_Settings.Model;
        }

        private void Main_VM_SettingsFileChanged(object? sender, EventArgs e)
        {
            try
            {
                if (SettingsFile.Settings == null)
                {
                    throw new Exception("Не инициализирован файл настроек.");
                }

                if (SettingsFile.Settings.Connection_IP == null)
                {
                    IP_Address = null;
                    Port = null;

                    return;
                }

                IP_Address = SettingsFile.Settings.Connection_IP.IP_Address;
                Port = SettingsFile.Settings.Connection_IP.Port;
            }

            catch (Exception error)
            {
                Message.Invoke("Ошибка обновления значений на странице Ethernet.\n\n" + error.Message, MessageType.Error);
            }
        }

        protected override IEnumerable<string> GetShortErrorMessages(string fieldName, string? value)
        {
            List<ValidateMessage> errors = new List<ValidateMessage>();

            if (string.IsNullOrEmpty(value))
            {
                return errors.Select(message => message.Short);
            }

            switch (fieldName)
            {
                case nameof(IP_Address):
                    Check_IP_Address(value, errors);
                    break;

                case nameof(Port):
                    Check_Port(value, errors);
                    break;
            }            

            return errors.Select(message => message.Short);
        }

        private void Check_IP_Address(string value, List<ValidateMessage> errors)
        {
            // Регулярное выражение для проверки корректного IPv4 адреса
            string pattern = @"^(25[0-5]|2[0-4][0-9]|[1][0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|[1][0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|[1][0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|[1][0-9]{2}|[1-9]?[0-9])$";

            if (!Regex.IsMatch(value, pattern))
            {
                errors.Add(AllErrorMessages[IP_Address_Invalid]);
            }
        }

        private void Check_Port(string value, List<ValidateMessage> errors)
        {
            if (!StringValue.IsValidNumber(value, NumberStyles.Number, out uint _))
            {
                errors.Add(AllErrorMessages[DecError_uint]);
            }
        }
    }
}
