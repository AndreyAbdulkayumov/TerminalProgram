using System.Reactive.Linq;
using Core.Models.Settings;
using ReactiveUI;
using MessageBox_Core;

namespace ViewModels.Settings
{
    public class ViewModel_Tab_Connection_Ethernet : ReactiveObject
    {
        private string? _ip_address = String.Empty;

        public string? IP_Address
        {
            get => _ip_address;
            set => this.RaiseAndSetIfChanged(ref _ip_address, value);
        }

        private string? _port = String.Empty;

        public string? Port
        {
            get => _port;
            set => this.RaiseAndSetIfChanged(ref _port, value);
        }

        private readonly Model_Settings SettingsFile;

        private readonly Action<string, MessageType> Message;


        public ViewModel_Tab_Connection_Ethernet(ViewModel_Settings Main_VM)
        {
            Main_VM.SettingsFileChanged += Main_VM_SettingsFileChanged;

            Message = Main_VM.Message;
            
            SettingsFile = Model_Settings.Model;

            this.WhenAnyValue(x => x.Port)
                .WhereNotNull()
                .Where(x => x != String.Empty)
                .Select(x => StringValue.CheckNumber(x, System.Globalization.NumberStyles.Number, out UInt16 _))
                .Subscribe(result => Port = result);
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
    }
}
