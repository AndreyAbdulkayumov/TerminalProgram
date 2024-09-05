using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using Core.Models.Settings;
using MessageBox_Core;
using ViewModels.Settings.Tabs;

namespace ViewModels.Settings
{
    public class Settings_VM : ReactiveObject
    {
        public event EventHandler<EventArgs>? _settingsFileChanged;

        private ObservableCollection<string> _presets = new ObservableCollection<string>();

        public ObservableCollection<string> Presets
        {
            get => _presets;
            set => this.RaiseAndSetIfChanged(ref _presets, value);
        }

        private string _selectedPreset = string.Empty;

        public string SelectedPreset
        {
            get => _selectedPreset;
            set => this.RaiseAndSetIfChanged(ref _selectedPreset, value);
        }

        private readonly Connection_VM _tab_Connection_VM;

        public Connection_VM Tab_Connection_VM
        {
            get => _tab_Connection_VM;
        }

        private readonly NoProtocol_VM _tab_NoProtocol_VM;

        public NoProtocol_VM Tab_NoProtocol_VM
        {
            get => _tab_NoProtocol_VM;
        }

        private readonly Modbus_VM _tab_Modbus_VM;

        public Modbus_VM Tab_Modbus_VM
        {
            get => _tab_Modbus_VM;
        }

        private readonly UI_VM _tab_UI_VM;

        public UI_VM Tab_UI_VM
        {
            get => _tab_UI_VM;
        }

        public ReactiveCommand<Unit, Unit> Command_Loaded { get; }

        public ReactiveCommand<Unit, Unit> Command_File_AddNew { get; }
        public ReactiveCommand<Unit, Unit> Command_File_AddExisting { get; }
        public ReactiveCommand<Unit, Unit> Command_File_Delete { get; }
        public ReactiveCommand<Unit, Unit> Command_File_Save { get; }

        public readonly Action<string, MessageType> Message;

        private readonly Func<string, MessageType, Task<MessageBoxResult>> MessageDialog;
        private readonly Func<string, Task<string?>> Get_FilePath;
        private readonly Func<Task<string?>> Get_NewFileName;

        private readonly Model_Settings SettingsFile;


        public Settings_VM(
            Action<string, MessageType> messageBox,
            Func<string, MessageType, Task<MessageBoxResult>> messageBoxDialog,
            Func<string, Task<string?>> get_FilePath_Handler,
            Func<Task<string?>> get_NewFileName_Handler,
            Action set_Dark_Theme_Handler,
            Action set_Light_Theme_Handler
            )
        {
            Message = messageBox;
            MessageDialog = messageBoxDialog;
            Get_FilePath = get_FilePath_Handler;
            Get_NewFileName = get_NewFileName_Handler;

            SettingsFile = Model_Settings.Model;

            _tab_Connection_VM = new Connection_VM(this);
            _tab_NoProtocol_VM = new NoProtocol_VM();
            _tab_Modbus_VM = new Modbus_VM();
            _tab_UI_VM = new UI_VM(set_Dark_Theme_Handler, set_Light_Theme_Handler, messageBox);

            Command_Loaded = ReactiveCommand.Create(Loaded_EventHandler);

            Command_File_AddNew = ReactiveCommand.CreateFromTask(File_CreateNew_Handler);
            Command_File_AddExisting = ReactiveCommand.CreateFromTask(File_AddExisting_Handler);
            Command_File_Delete = ReactiveCommand.CreateFromTask(File_Delete_Handler);
            Command_File_Save = ReactiveCommand.Create(File_Save_Handler);

            this.WhenAnyValue(x => x.SelectedPreset)
                .WhereNotNull()
                .Where(x => x != string.Empty)
                .Subscribe(UpdateUI);
        }

        private void UpdateUI(string fileName)
        {
            DeviceData settings = SettingsFile.ReadPreset(fileName);

            Tab_NoProtocol_VM.SelectedEncoding = settings.GlobalEncoding ?? string.Empty;

            Tab_Modbus_VM.WriteTimeout = settings.TimeoutWrite ?? string.Empty;
            Tab_Modbus_VM.ReadTimeout = settings.TimeoutRead ?? string.Empty;

            switch (settings.TypeOfConnection)
            {
                case DeviceData.ConnectionName_SerialPort:
                    Tab_Connection_VM.Selected_SerialPort = true;
                    break;

                case DeviceData.ConnectionName_Ethernet:
                    Tab_Connection_VM.Selected_Ethernet = true;
                    break;
            }

            _settingsFileChanged?.Invoke(this, new EventArgs());
        }

        private void Loaded_EventHandler()
        {
            UpdateListOfPresets();

            SelectedPreset = Presets.Single(x => x == CommonUI_VM.SettingsDocument);
        }

        private void UpdateListOfPresets()
        {
            string[] fileNames = SettingsFile.FindFilesOfPresets();

            Presets.Clear();

            foreach (string element in fileNames)
            {
                Presets.Add(element);
            }
        }

        private async Task File_CreateNew_Handler()
        {
            string? fileName = await Get_NewFileName();

            if (fileName != null && fileName != String.Empty)
            {
                SettingsFile.SavePreset(fileName, DeviceData.GetDefault());

                UpdateListOfPresets();

                SelectedPreset = Presets.Single(x => x == fileName);
            }
        }

        private async Task File_AddExisting_Handler()
        {
            try
            {
                string? filePath = await Get_FilePath.Invoke("Добавление уже существующего файла настроек");

                if (filePath == null)
                {
                    return;
                }

                string fileName = SettingsFile.CopyFrom(filePath);

                UpdateListOfPresets();

                SelectedPreset = Presets.Single(x => x == fileName);
            }
            
            catch (Exception error)
            {
                Message.Invoke("Ошибка при добавлении уже существующего файла.\n\n" + error.Message, MessageType.Error);
            }
        }

        private async Task File_Delete_Handler()
        {
            try
            {
                if (Presets.Count <= 1)
                {
                    Message.Invoke("Нельзя удалить единственный файл.\nПопробуйте его изменить.", MessageType.Warning);
                    return;
                }

                MessageBoxResult dialogResult = await MessageDialog("Вы действительно желайте удалить файл " + SelectedPreset + "?", MessageType.Warning);

                if (dialogResult != MessageBoxResult.Yes)
                {
                    return;
                }

                SettingsFile.Delete(SelectedPreset);

                UpdateListOfPresets();

                SelectedPreset = Presets.First();
            }

            catch (Exception error)
            {
                Message.Invoke("Ошибка удаления файла настроек.\n\n" + error.Message, MessageType.Error);
            }
        }

        private void File_Save_Handler()
        {
            try
            {
                string connectionType = Tab_Connection_VM.Selected_SerialPort ? DeviceData.ConnectionName_SerialPort : DeviceData.ConnectionName_Ethernet;

                var data = new DeviceData()
                {
                    GlobalEncoding = Tab_NoProtocol_VM.SelectedEncoding,

                    TimeoutWrite = Tab_Modbus_VM.WriteTimeout,
                    TimeoutRead = Tab_Modbus_VM.ReadTimeout,

                    TypeOfConnection = connectionType,
                                        
                    Connection_SerialPort = new SerialPort_Info()
                    {
                        COMPort = Tab_Connection_VM.Connection_SerialPort_VM.Selected_COM_Port,
                        BaudRate = Tab_Connection_VM.Connection_SerialPort_VM.Selected_BaudRate,
                        BaudRate_IsCustom = Tab_Connection_VM.Connection_SerialPort_VM.BaudRate_IsCustom,
                        BaudRate_Custom = Tab_Connection_VM.Connection_SerialPort_VM.Custom_BaudRate_Value,
                        Parity = Tab_Connection_VM.Connection_SerialPort_VM.Selected_Parity,
                        DataBits = Tab_Connection_VM.Connection_SerialPort_VM.Selected_DataBits,
                        StopBits = Tab_Connection_VM.Connection_SerialPort_VM.Selected_StopBits
                    },

                    Connection_IP = new IP_Info()
                    {
                        IP_Address = Tab_Connection_VM.Connection_Ethernet_VM.IP_Address,
                        Port = Tab_Connection_VM.Connection_Ethernet_VM.Port
                    }
                };

                SettingsFile.SavePreset(SelectedPreset, data);

                CommonUI_VM.SettingsDocument = SelectedPreset;

                if (Tab_Connection_VM.Connection_SerialPort_VM.Selected_COM_Port == null ||
                    Tab_Connection_VM.Connection_SerialPort_VM.Selected_COM_Port == String.Empty)
                {
                    Tab_Connection_VM.Connection_SerialPort_VM.Message_PortNotFound_IsVisible = true;
                }

                Message.Invoke("Настройки успешно сохранены!", MessageType.Information);
            }

            catch (Exception error)
            {
                Message.Invoke("Ошибка сохранения файла настроек.\n\n" + error.Message, MessageType.Error);
            }
        }
    }
}
