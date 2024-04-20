using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using Core.Models.Settings;
using MessageBox_Core;
using ViewModels.MainWindow;

namespace ViewModels.Settings
{
    public class ViewModel_Settings : ReactiveObject
    {
        private object? _currentViewModel;

        public object? CurrentViewModel
        {
            get => _currentViewModel;
            set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
        }

        private const string ThemeName_Dark = "Темная";
        private const string ThemeName_Light = "Светлая";

        private ObservableCollection<string> _themes = new ObservableCollection<string>()
        {
            ThemeName_Dark, ThemeName_Light
        };

        public ObservableCollection<string> Themes
        {
            get => _themes;
            set => this.RaiseAndSetIfChanged(ref _themes, value);
        }

        private string _selectedTheme = string.Empty;

        public string SelectedTheme
        {
            get => _selectedTheme;
            set => this.RaiseAndSetIfChanged(ref _selectedTheme, value);
        }

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
        private readonly Func<string, MessageType, MessageBoxResult> MessageDialog;
        private readonly Func<string, string?> Get_FilePath;
        private readonly Func<string> Get_NewFileName;

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
            Func<string, MessageType, MessageBoxResult> MessageBoxDialog,
            Func<string, string?> Get_FilePath_Handler,
            Func<string> Get_NewFileName_Handler,
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
            _tab_UI_VM = new ViewModel_Tab_UI(Set_Dark_Theme_Handler, Set_Light_Theme_Handler);

            Command_Loaded = ReactiveCommand.Create(Loaded_EventHandler);

            Command_File_AddNew = ReactiveCommand.Create(File_CreateNew_Handler);
            Command_File_AddExisting = ReactiveCommand.Create(File_AddExisting_Handler);
            Command_File_Delete = ReactiveCommand.Create(File_Delete_Handler);
            Command_File_Save = ReactiveCommand.Create(File_Save_Handler);


            this.WhenAnyValue(x => x.SelectedTheme)
                .WhereNotNull()
                .Subscribe(ThemeName =>
                {
                    switch (ThemeName)
                    {
                        case ThemeName_Dark:
                            ViewModel_CommonUI.ThemeName = ViewModel_CommonUI.ThemeName_Dark;
                            break;

                        case ThemeName_Light:
                            ViewModel_CommonUI.ThemeName = ViewModel_CommonUI.ThemeName_Light;
                            break;
                    }
                });

            this.WhenAnyValue(x => x.SelectedPreset)
                .WhereNotNull()
                .Where(x => x != string.Empty)
                .Subscribe(UpdateUI);
        }

        private void UpdateUI(string FileName)
        {
            DeviceData Settings = SettingsFile.Read(FileName);

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
            if (ViewModel_CommonUI.ThemeName == ViewModel_CommonUI.ThemeName_Dark)
            {
                SelectedTheme = ThemeName_Dark;
            }

            else if (ViewModel_CommonUI.ThemeName == ViewModel_CommonUI.ThemeName_Light)
            {
                SelectedTheme = ThemeName_Light;
            }

            UpdateListOfPresets();

            SelectedPreset = Presets.Single(x => x == ViewModel_CommonUI.SettingsDocument);
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

        private void File_CreateNew_Handler()
        {
            string FileName = Get_NewFileName.Invoke();

            if (FileName != String.Empty)
            {
                SettingsFile.Save(FileName, DeviceData.GetDefault());

                UpdateListOfPresets();

                SelectedPreset = Presets.Single(x => x == FileName);
            }
        }

        private void File_AddExisting_Handler()
        {
            try
            {
                string? FilePath = Get_FilePath.Invoke("Добавление уже существующего файла настроек");

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

        private void File_Delete_Handler()
        {
            try
            {
                if (Presets.Count <= 1)
                {
                    Message.Invoke("Нельзя удалить единственный файл.\nПопробуйте его изменить.", MessageType.Warning);
                    return;
                }

                MessageBoxResult DialogResult = MessageDialog("Вы действительно желайте удалить файл " + SelectedPreset + "?", MessageType.Warning);

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

                SettingsFile.Save(SelectedPreset, Data);

                ViewModel_CommonUI.SettingsDocument = SelectedPreset;

                if (Tab_Connection_VM.Connection_SerialPort_VM.Selected_COM_Port != String.Empty)
                {
                    Tab_Connection_VM.Connection_SerialPort_VM.Message_PortNotFound_IsVisible = false;
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
