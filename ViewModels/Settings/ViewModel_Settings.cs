using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using Core.Models.Settings;
using MessageBox_Core;

namespace ViewModels.Settings
{
    public class ViewModel_Settings : ReactiveObject
    {
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

        public event EventHandler<EventArgs>? SettingsFileChanged;


        private readonly ViewModel_Tab_Connection _tab_Connection_VM;

        public ViewModel_Tab_Connection Tab_Connection_VM
        {
            get => _tab_Connection_VM;
        }

        private readonly ViewModel_Tab_NoProtocol _tab_NoProtocol_VM;

        public ViewModel_Tab_NoProtocol Tab_NoProtocol_VM
        {
            get => _tab_NoProtocol_VM;
        }

        private readonly ViewModel_Tab_Modbus _tab_Modbus_VM;

        public ViewModel_Tab_Modbus Tab_Modbus_VM
        {
            get => _tab_Modbus_VM;
        }

        private readonly ViewModel_Tab_UI _tab_UI_VM;

        public ViewModel_Tab_UI Tab_UI_VM
        {
            get => _tab_UI_VM;
        }       


        public ViewModel_Settings(
            Action<string, MessageType> MessageBox,
            Func<string, MessageType, Task<MessageBoxResult>> MessageBoxDialog,
            Func<string, Task<string?>> Get_FilePath_Handler,
            Func<Task<string?>> Get_NewFileName_Handler,
            Action Set_Dark_Theme_Handler,
            Action Set_Light_Theme_Handler
            )
        {
            Message = MessageBox;
            MessageDialog = MessageBoxDialog;
            Get_FilePath = Get_FilePath_Handler;
            Get_NewFileName = Get_NewFileName_Handler;

            SettingsFile = Model_Settings.Model;

            _tab_Connection_VM = new ViewModel_Tab_Connection(this);
            _tab_NoProtocol_VM = new ViewModel_Tab_NoProtocol();
            _tab_Modbus_VM = new ViewModel_Tab_Modbus();
            _tab_UI_VM = new ViewModel_Tab_UI(Set_Dark_Theme_Handler, Set_Light_Theme_Handler, MessageBox);

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

        private void UpdateUI(string FileName)
        {
            DeviceData Settings = SettingsFile.ReadPreset(FileName);

            Tab_NoProtocol_VM.SelectedEncoding = Settings.GlobalEncoding ?? string.Empty;

            Tab_Modbus_VM.WriteTimeout = Settings.TimeoutWrite ?? string.Empty;
            Tab_Modbus_VM.ReadTimeout = Settings.TimeoutRead ?? string.Empty;

            switch (Settings.TypeOfConnection)
            {
                case DeviceData.ConnectionName_SerialPort:
                    Tab_Connection_VM.Selected_SerialPort = true;
                    break;

                case DeviceData.ConnectionName_Ethernet:
                    Tab_Connection_VM.Selected_Ethernet = true;
                    break;
            }

            SettingsFileChanged?.Invoke(this, new EventArgs());
        }

        private void Loaded_EventHandler()
        {
            UpdateListOfPresets();

            SelectedPreset = Presets.Single(x => x == CommonUI_VM.SettingsDocument);
        }

        private void UpdateListOfPresets()
        {
            string[] FileNames = SettingsFile.FindFilesOfPresets();

            Presets.Clear();

            foreach (string element in FileNames)
            {
                Presets.Add(element);
            }
        }

        private async Task File_CreateNew_Handler()
        {
            string? FileName = await Get_NewFileName();

            if (FileName != null && FileName != String.Empty)
            {
                SettingsFile.SavePreset(FileName, DeviceData.GetDefault());

                UpdateListOfPresets();

                SelectedPreset = Presets.Single(x => x == FileName);
            }
        }

        private async Task File_AddExisting_Handler()
        {
            try
            {
                string? FilePath = await Get_FilePath.Invoke("Добавление уже существующего файла настроек");

                if (FilePath == null)
                {
                    return;
                }

                string FileName = SettingsFile.CopyFrom(FilePath);

                UpdateListOfPresets();

                SelectedPreset = Presets.Single(x => x == FileName);
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

                MessageBoxResult DialogResult = await MessageDialog("Вы действительно желайте удалить файл " + SelectedPreset + "?", MessageType.Warning);

                if (DialogResult != MessageBoxResult.Yes)
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
                string ConnectionType = Tab_Connection_VM.Selected_SerialPort ? DeviceData.ConnectionName_SerialPort : DeviceData.ConnectionName_Ethernet;

                DeviceData Data = new DeviceData()
                {
                    GlobalEncoding = Tab_NoProtocol_VM.SelectedEncoding,

                    TimeoutWrite = Tab_Modbus_VM.WriteTimeout,
                    TimeoutRead = Tab_Modbus_VM.ReadTimeout,

                    TypeOfConnection = ConnectionType,
                                        
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

                SettingsFile.SavePreset(SelectedPreset, Data);

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
