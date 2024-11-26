using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using Core.Models.Settings;
using MessageBox_Core;
using ViewModels.Settings.Tabs;
using ViewModels.Validation;
using System.Text;
using ViewModels.FloatNumber;

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

        private readonly AppSettings_VM _tab_AppSettings_VM;

        public AppSettings_VM Tab_AppSettings_VM
        {
            get => _tab_AppSettings_VM;
        }

        private readonly ReactiveObject[] _allTabs;

        public ReactiveCommand<Unit, Unit> Command_Loaded { get; }

        public ReactiveCommand<Unit, Unit> Command_File_AddNew { get; }
        public ReactiveCommand<Unit, Unit> Command_File_AddExisting { get; }
        public ReactiveCommand<Unit, Unit> Command_File_Delete { get; }
        public ReactiveCommand<Unit, Unit> Command_File_Save { get; }

        private readonly IMessageBox _messageBox;

        private readonly Func<string, Task<string?>> Get_FilePath;
        private readonly Func<Task<string?>> Get_NewFileName;

        private readonly Model_Settings SettingsFile;


        public Settings_VM(
            IMessageBox messageBox,
            Func<string, Task<string?>> get_FilePath_Handler,
            Func<Task<string?>> get_NewFileName_Handler,
            Action set_Dark_Theme_Handler,
            Action set_Light_Theme_Handler
            )
        {
            _messageBox = messageBox;
            Get_FilePath = get_FilePath_Handler;
            Get_NewFileName = get_NewFileName_Handler;

            SettingsFile = Model_Settings.Model;

            _tab_Connection_VM = new Connection_VM(this, messageBox);
            _tab_NoProtocol_VM = new NoProtocol_VM();
            _tab_Modbus_VM = new Modbus_VM();
            _tab_AppSettings_VM = new AppSettings_VM(set_Dark_Theme_Handler, set_Light_Theme_Handler, messageBox);

            _allTabs = [
                Tab_Connection_VM.Connection_SerialPort_VM,
                Tab_Connection_VM.Connection_Ethernet_VM,
                Tab_NoProtocol_VM,
                Tab_Modbus_VM,
                Tab_AppSettings_VM
            ];

            Command_Loaded = ReactiveCommand.Create(Loaded_EventHandler);

            Command_File_AddNew = ReactiveCommand.CreateFromTask(File_CreateNew_Handler);
            Command_File_AddExisting = ReactiveCommand.CreateFromTask(File_AddExisting_Handler);
            Command_File_Delete = ReactiveCommand.CreateFromTask(File_Delete_Handler);
            Command_File_Save = ReactiveCommand.Create(File_Save_Handler);

            this.WhenAnyValue(x => x.SelectedPreset)
                .WhereNotNull()
                .Where(x => !string.IsNullOrEmpty(x))
                .Subscribe(UpdateUI);
        }

        private void UpdateUI(string fileName)
        {
            DeviceData settings = SettingsFile.ReadPreset(fileName);

            Tab_NoProtocol_VM.SelectedEncoding = settings.GlobalEncoding ?? DeviceData.GlobalEncoding_Default;
            Tab_NoProtocol_VM.SelectedReceiveBufferSize = settings.ReceiveBufferSize ?? DeviceData.ReceiveBufferSize_Default;

            Tab_Modbus_VM.WriteTimeout = settings.TimeoutWrite ?? DeviceData.TimeoutWrite_Default;
            Tab_Modbus_VM.ReadTimeout = settings.TimeoutRead ?? DeviceData.TimeoutRead_Default;

            Tab_Modbus_VM.FloatFormat = FloatHelper.GetFloatNumberFormatOrDefault(settings.FloatNumberFormat);

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

            if (!string.IsNullOrEmpty(fileName))
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

                if (string.IsNullOrEmpty(filePath))
                {
                    return;
                }

                string fileName = SettingsFile.CopyFrom(filePath);

                UpdateListOfPresets();

                SelectedPreset = Presets.Single(x => x == fileName);
            }
            
            catch (Exception error)
            {
                _messageBox.Show("Ошибка при добавлении уже существующего файла.\n\n" + error.Message, MessageType.Error);
            }
        }

        private async Task File_Delete_Handler()
        {
            try
            {
                if (Presets.Count <= 1)
                {
                    _messageBox.Show("Нельзя удалить единственный файл.\nПопробуйте его изменить.", MessageType.Warning);
                    return;
                }

                MessageBoxResult dialogResult = await _messageBox.ShowYesNoDialog("Вы действительно желайте удалить файл " + SelectedPreset + "?", MessageType.Warning);

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
                _messageBox.Show("Ошибка удаления файла настроек.\n\n" + error.Message, MessageType.Error);
            }
        }

        private void File_Save_Handler()
        {
            try
            {
                string connectionType = Tab_Connection_VM.Selected_SerialPort ? DeviceData.ConnectionName_SerialPort : DeviceData.ConnectionName_Ethernet;

                string? validationMessage = CheckTabFields(connectionType);

                if (!string.IsNullOrEmpty(validationMessage))
                {
                    _messageBox.Show(validationMessage, MessageType.Warning);
                    return;
                }

                string floatFormat = string.Empty;

                switch (Tab_Modbus_VM.FloatFormat)
                {
                    case FloatNumberFormat.AB_CD:
                        floatFormat = DeviceData.FloatWriteFormat_AB_CD;
                        break;

                    case FloatNumberFormat.BA_DC:
                        floatFormat = DeviceData.FloatWriteFormat_BA_DC;
                        break;

                    case FloatNumberFormat.CD_AB:
                        floatFormat = DeviceData.FloatWriteFormat_CD_AB;
                        break;

                    case FloatNumberFormat.DC_BA:
                        floatFormat = DeviceData.FloatWriteFormat_DC_BA;
                        break;
                }

                var data = new DeviceData()
                {
                    TypeOfConnection = connectionType,

                    Connection_SerialPort = new SerialPort_Info()
                    {
                        Port = Tab_Connection_VM.Connection_SerialPort_VM.Selected_SerialPort,
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
                    },

                    GlobalEncoding = Tab_NoProtocol_VM.SelectedEncoding,
                    ReceiveBufferSize = Tab_NoProtocol_VM.SelectedReceiveBufferSize,

                    TimeoutWrite = Tab_Modbus_VM.WriteTimeout,
                    TimeoutRead = Tab_Modbus_VM.ReadTimeout,
                    FloatNumberFormat = floatFormat,                    
                };

                SettingsFile.SavePreset(SelectedPreset, data);

                CommonUI_VM.SettingsDocument = SelectedPreset;

                if (string.IsNullOrEmpty(Tab_Connection_VM.Connection_SerialPort_VM.Selected_SerialPort))
                {
                    Tab_Connection_VM.Connection_SerialPort_VM.Message_PortNotFound_IsVisible = true;
                }

                _tab_Connection_VM.Connection_SerialPort_VM.ReScan_SerialPorts(data.Connection_SerialPort);

                _messageBox.Show("Настройки успешно сохранены!", MessageType.Information);
            }

            catch (Exception error)
            {
                _messageBox.Show("Ошибка сохранения файла настроек.\n\n" + error.Message, MessageType.Error);
            }
        }

        private string? CheckTabFields(string connectionType)
        {
            StringBuilder message = new StringBuilder();

            IEnumerable<ReactiveObject> neededTabs = GetNeededTabs(connectionType);

            foreach (var tab in neededTabs)
            {
                var validationTab = tab as ValidatedDateInput;

                if (validationTab != null && validationTab.HasErrors)
                {
                    foreach (KeyValuePair<string, ValidateMessage> element in validationTab.ActualErrors)
                    {
                        message.AppendLine($"[{(tab as IValidationFieldInfo)?.GetFieldViewName(element.Key)}]\n{validationTab.GetFullErrorMessage(element.Key)}\n");
                    }
                }
            }

            if (message.Length > 0)
            {
                message.Insert(0, "Ошибки валидации\n\n");
                return message.ToString().TrimEnd('\r', '\n');
            }

            return null;
        }

        private IEnumerable<ReactiveObject> GetNeededTabs(string connectionType)
        {
            switch (connectionType)
            {
                case DeviceData.ConnectionName_SerialPort:
                    return _allTabs.Where(tab => tab is not Connection_Ethernet_VM);

                case DeviceData.ConnectionName_Ethernet:
                    return _allTabs.Where(tab => tab is not Connection_SerialPort_VM);

                default:
                    return _allTabs;
            }
        }
    }
}
