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
using Core.Models;
using Microsoft.Win32;
using ReactiveUI;
using View_WPF.Properties;

namespace View_WPF.ViewModels.Settings
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

        public ReactiveCommand<Unit, Task> Command_File_AddNew { get; }
        public ReactiveCommand<Unit, Task> Command_File_AddExisting { get; }
        public ReactiveCommand<Unit, Unit> Command_File_Delete { get; }
        public ReactiveCommand<Unit, Unit> Command_File_Save { get; }


        private Action<string, MessageType> Message;

        private readonly ConnectedHost Model;

        public event EventHandler<EventArgs>? SettingsFileChanged;

        public readonly ViewModel_Settings_Ethernet Ethernet_VM;
        public readonly ViewModel_Settings_SerialPort SerialPort_VM;

        private readonly Func<string, string?> Get_FilePath;
        private readonly Func<string> Get_NewFileName;

        public ViewModel_Settings(
            Action<string, MessageType> MessageBox,
            Func<string, string?> Get_FilePath_Handler,
            Func<string> Get_NewFileName_Handler)
        {
            Message = MessageBox;
            Get_FilePath = Get_FilePath_Handler;
            Get_NewFileName = Get_NewFileName_Handler;

            Model = ConnectedHost.Model;

            Command_Loaded = ReactiveCommand.CreateFromTask(Loaded_EventHandler);

            Command_File_AddNew = ReactiveCommand.Create(File_CreateNew_Handler);
            Command_File_AddExisting = ReactiveCommand.Create(File_AddExisting_Handler);
            Command_File_Delete = ReactiveCommand.Create(File_Delete_Handler);
            Command_File_Save = ReactiveCommand.Create(File_Save_Handler);

            this.WhenAnyValue(x => x.SelectedPreset)
                .Where(x => x != null)
                .Where(x => x != string.Empty)
                .Select(async value => await Model.ReadSettings(value))
                .Subscribe(UpdateUI);

            this.WhenAnyValue(x => x.WriteTimeout)
                .Where(x => x != null)
                .Where(x => x != string.Empty)
                .Select(CheckNumber)
                .Subscribe(result => WriteTimeout = result);
            
            this.WhenAnyValue(x => x.ReadTimeout)
                .Where(x => x != null)
                .Where(x => x != string.Empty)
                .Select(CheckNumber)
                .Subscribe(result => ReadTimeout = result);

            Ethernet_VM = new ViewModel_Settings_Ethernet(this, MessageBox);
            SerialPort_VM = new ViewModel_Settings_SerialPort(this, MessageBox);
        }

        public string CheckNumber(string Text)
        {
            if (UInt32.TryParse(Text, out _) == false)
            {
                Text = Text.Substring(0, Text.Length - 1);

                Message?.Invoke("Разрешается вводить только неотрицательные целые числа.", MessageType.Warning);
            }

            return Text;
        }

        private async void UpdateUI(Task task)
        {
            await task;

            DeviceData Settings = Model.Settings;

            SelectedEncoding = Settings.GlobalEncoding ?? string.Empty;

            WriteTimeout = Settings.TimeoutWrite ?? string.Empty;
            ReadTimeout = Settings.TimeoutRead ?? string.Empty;

            switch (Settings.TypeOfConnection)
            {
                case "SerialPort":
                    Selected_SerialPort = true;
                    break;

                case "Ethernet":
                    Selected_Ethernet = true;
                    break;
            }

            SettingsFileChanged?.Invoke(this, new EventArgs());
        }

        private async Task Loaded_EventHandler()
        {
            await UpdateListOfPresets();

            SelectedPreset = Presets.First();
        }

        private async Task UpdateListOfPresets()
        {
            string[] FileNames = await Model.GetSettings_FileNames();

            Presets.Clear();

            foreach (string element in FileNames)
            {
                Presets.Add(element);
            }
        }

        private async Task File_CreateNew_Handler()
        {
            string FileName = Get_NewFileName.Invoke();

            if (FileName != String.Empty)
            {
                await Model.SaveSettings(SystemOfSettings.GetDefault());

                await UpdateListOfPresets();

                SelectedPreset = Presets.Single(x => x == FileName);
            }
        }

        private async Task File_AddExisting_Handler()
        {
            try
            {
                string? FilePath = Get_FilePath.Invoke("Добавление уже существующего файла настроек");

                if (FilePath == null)
                {
                    return;
                }

                string destFilePath = SystemOfSettings.FolderPath_Settings + Path.GetFileName(FilePath);

                string FileName = Path.GetFileNameWithoutExtension(FilePath);

                File.Copy(FilePath, destFilePath);

                await UpdateListOfPresets();

                SelectedPreset = Presets.Single(x => x == FileName);
            }
            
            catch (Exception error)
            {
                Message.Invoke("Ошибка при добавлении уже существующего файла.\n\n" + error.Message, MessageType.Error);
            }
        }

        private void File_Delete_Handler()
        {

        }

        private void File_Save_Handler()
        {

        }
    }
}
