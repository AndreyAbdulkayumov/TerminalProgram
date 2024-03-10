using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ReactiveUI;
using Core.Models.Settings;
using MessageBox_Core;
using ViewModels.MainWindow;

namespace ViewModels.Settings
{
    public class ViewModel_Settings : ReactiveObject
    {
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

        private readonly ObservableCollection<string> _typeOfEncoding = new ObservableCollection<string>()
        {
            "ASCII", "UTF-8", "UTF-32", "Unicode"
        };

        public ObservableCollection<string> TypeOfEncoding
        {
            get => _typeOfEncoding;
        }

        private string _selectedEncoding = string.Empty;

        public string SelectedEncoding
        {
            get => _selectedEncoding;
            set => this.RaiseAndSetIfChanged(ref _selectedEncoding, value);
        }

        private string _writeTimeout = string.Empty;

        public string WriteTimeout
        {
            get => _writeTimeout;
            set => this.RaiseAndSetIfChanged(ref _writeTimeout, value);
        }

        private string _readTimeout = string.Empty;

        public string ReadTimeout
        {
            get => _readTimeout;
            set => this.RaiseAndSetIfChanged(ref _readTimeout, value);
        }

        private bool _selected_SerialPort;

        public bool Selected_SerialPort
        {
            get => _selected_SerialPort;
            set => this.RaiseAndSetIfChanged(ref _selected_SerialPort, value);
        }

        private bool _selected_Ethernet;

        public bool Selected_Ethernet
        {
            get => _selected_Ethernet;
            set => this.RaiseAndSetIfChanged(ref _selected_Ethernet, value);
        }
        
        public ReactiveCommand<Unit, Unit> Command_Loaded { get; }

        public ReactiveCommand<Unit, Unit> Command_File_AddNew { get; }
        public ReactiveCommand<Unit, Unit> Command_File_AddExisting { get; }
        public ReactiveCommand<Unit, Unit> Command_File_Delete { get; }
        public ReactiveCommand<Unit, Unit> Command_File_Save { get; }

        public readonly Action<string, MessageType> Message;
        private readonly Func<string, MessageType, bool> MessageDialog;
        private readonly Func<string, string?> Get_FilePath;
        private readonly Func<string> Get_NewFileName;

        private readonly Model_Settings SettingsFile;

        public event EventHandler<EventArgs>? SettingsFileChanged;

        public readonly ViewModel_Settings_Ethernet Ethernet_VM;
        public readonly ViewModel_Settings_SerialPort SerialPort_VM;

        private ReactiveCommand<Unit, Unit> Select_Dark_Theme { get; }
        private ReactiveCommand<Unit, Unit> Select_Light_Theme { get; }


        public ViewModel_Settings(
            Action<string, MessageType> MessageBox,
            Func<string, MessageType, bool> MessageBoxDialog,
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

            Command_Loaded = ReactiveCommand.Create(Loaded_EventHandler);

            Command_File_AddNew = ReactiveCommand.Create(File_CreateNew_Handler);
            Command_File_AddExisting = ReactiveCommand.Create(File_AddExisting_Handler);
            Command_File_Delete = ReactiveCommand.Create(File_Delete_Handler);
            Command_File_Save = ReactiveCommand.Create(File_Save_Handler);

            Select_Dark_Theme = ReactiveCommand.Create(Set_Dark_Theme_Handler);
            Select_Light_Theme = ReactiveCommand.Create(Set_Light_Theme_Handler);

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

            this.WhenAnyValue(x => x.WriteTimeout)
                .WhereNotNull()
                .Where(x => x != string.Empty)
                .Select(x => StringValue.CheckNumber(x, System.Globalization.NumberStyles.Number, out UInt16 _))
                .Subscribe(result => WriteTimeout = result);
            
            this.WhenAnyValue(x => x.ReadTimeout)
                .WhereNotNull()
                .Where(x => x != string.Empty)
                .Select(x => StringValue.CheckNumber(x, System.Globalization.NumberStyles.Number, out UInt16 _))
                .Subscribe(result => ReadTimeout = result);

            Ethernet_VM = new ViewModel_Settings_Ethernet(this);
            SerialPort_VM = new ViewModel_Settings_SerialPort(this);
        }

        private void UpdateUI(string FileName)
        {
            DeviceData Settings = SettingsFile.Read(FileName);

            SelectedEncoding = Settings.GlobalEncoding ?? string.Empty;

            WriteTimeout = Settings.TimeoutWrite ?? string.Empty;
            ReadTimeout = Settings.TimeoutRead ?? string.Empty;

            switch (Settings.TypeOfConnection)
            {
                case DeviceData.ConnectionName_SerialPort:
                    Selected_SerialPort = true;
                    break;

                case DeviceData.ConnectionName_Ethernet:
                    Selected_Ethernet = true;
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

                bool DialogResult = MessageDialog("Вы действительно желайте удалить файл " + SelectedPreset + "?", MessageType.Warning);

                if (DialogResult == false)
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
                string ConnectionType = Selected_SerialPort ? DeviceData.ConnectionName_SerialPort : DeviceData.ConnectionName_Ethernet;

                DeviceData Data = new DeviceData()
                {
                    GlobalEncoding = this.SelectedEncoding,

                    TimeoutWrite = this.WriteTimeout,
                    TimeoutRead = this.ReadTimeout,

                    TypeOfConnection = ConnectionType,
                                        
                    Connection_SerialPort = new SerialPort_Info()
                    {
                        COMPort = SerialPort_VM.Selected_COM_Port,
                        BaudRate = SerialPort_VM.Selected_BaudRate,
                        BaudRate_IsCustom = SerialPort_VM.BaudRate_IsCustom,
                        BaudRate_Custom = SerialPort_VM.Custom_BaudRate_Value,
                        Parity = SerialPort_VM.Selected_Parity,
                        DataBits = SerialPort_VM.Selected_DataBits,
                        StopBits = SerialPort_VM.Selected_StopBits
                    },

                    Connection_IP = new IP_Info()
                    {
                        IP_Address = Ethernet_VM.IP_Address,
                        Port = Ethernet_VM.Port
                    }
                };

                SettingsFile.Save(SelectedPreset, Data);

                ViewModel_CommonUI.SettingsDocument = SelectedPreset;

                if (SerialPort_VM.Selected_COM_Port != String.Empty)
                {
                    SerialPort_VM.Message_PortNotFound_IsVisible = false;
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
