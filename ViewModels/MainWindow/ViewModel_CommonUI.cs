﻿using Core.Clients;
using Core.Models;
using Core.Models.Settings;
using ReactiveUI;
using MessageBox_Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ViewModels.MainWindow
{
    public class DocArgs : EventArgs
    {
        public readonly string? FilePath;

        public DocArgs(string? FilePath)
        {
            this.FilePath = FilePath;
        }
    }

    public class ViewModel_CommonUI : ReactiveObject
    {
        public bool IsConnected
        {
            get => Model.HostIsConnect;
        }

        public static event EventHandler<DocArgs>? SettingsDocument_Changed;

        private static string? _settingsDocument;

        public static string? SettingsDocument
        {
            get
            {
                return _settingsDocument;
            }

            set
            {
                _settingsDocument = value;
                SettingsDocument_Changed?.Invoke(null, new DocArgs(value));
            }
        }

        public static event EventHandler<DocArgs>? ThemeName_Changed;

        private static string? _themeName;

        public static string? ThemeName
        {
            get
            {
                return _themeName;
            }

            set
            {
                _themeName = value;
                ThemeName_Changed?.Invoke(null, new DocArgs(value));
            }
        }

        public static string? ThemeName_Dark;
        public static string? ThemeName_Light;

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

        public ReactiveCommand<Unit, Unit> Command_UpdatePresets { get; }

        public ReactiveCommand<Unit, Unit> Command_ProtocolMode_NoProtocol { get; }
        public ReactiveCommand<Unit, Unit> Command_ProtocolMode_Modbus { get; }

        public ReactiveCommand<Unit, Unit> Command_Connect { get; }
        public ReactiveCommand<Unit, Unit> Command_Disconnect { get; }


        private readonly ConnectedHost Model;
        private readonly Model_Settings SettingsFile;

        private readonly Action<string, MessageType> Message;
        private readonly Action SetUI_Connected;
        private readonly Action SetUI_Disconnected;
        private readonly Func<string[], string> Select_AvailablePresetFile;

        

        public ViewModel_CommonUI(
            Action<string, MessageType> MessageBox,
            Action UI_Connected_Handler,
            Action UI_Disconnected_Handler,
            Func<string[], string> Select_AvailablePresetFile_Handler,
            string? SettingsDocument,
            string? CurrentThemeName,
            string? ThemeName_Dark,
            string? ThemeName_Light
            )
        {
            Message = MessageBox;
            SetUI_Connected = UI_Connected_Handler;
            SetUI_Disconnected = UI_Disconnected_Handler;
            Select_AvailablePresetFile = Select_AvailablePresetFile_Handler;
            ViewModel_CommonUI.SettingsDocument = SettingsDocument;
            ViewModel_CommonUI.ThemeName = CurrentThemeName;
            ViewModel_CommonUI.ThemeName_Dark = ThemeName_Dark;
            ViewModel_CommonUI.ThemeName_Light = ThemeName_Light;

            Model = ConnectedHost.Model;
            SettingsFile = Model_Settings.Model;

            StringValue.ShowMessageView = Message;

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;

            this.WhenAnyValue(x => x.SelectedPreset)
                .WhereNotNull()
                .Where(x => x != string.Empty)
                .Subscribe(PresetName =>
                {
                    try
                    {
                        SettingsFile.Read(PresetName);
                        SettingsDocument = PresetName;
                    }

                    catch (Exception error)
                    {
                        Message.Invoke("Ошибка выбора пресета.\n\n" + error.Message, MessageType.Error);
                    }
                });

            Command_UpdatePresets = ReactiveCommand.Create(UpdateListOfPresets);
            Command_UpdatePresets.ThrownExceptions.Subscribe(error => Message.Invoke("Ошибка обновления списка пресетов.\n\n" + error.Message, MessageType.Error));

            Command_ProtocolMode_NoProtocol = ReactiveCommand.Create(Model.SetProtocol_NoProtocol);
            Command_ProtocolMode_NoProtocol.ThrownExceptions.Subscribe(error => Message.Invoke(error.Message, MessageType.Error));

            Command_ProtocolMode_Modbus = ReactiveCommand.Create(Model.SetProtocol_Modbus);
            Command_ProtocolMode_Modbus.ThrownExceptions.Subscribe(error => Message.Invoke(error.Message, MessageType.Error));

            Command_Connect = ReactiveCommand.Create(Connect_Handler);
            Command_Connect.ThrownExceptions.Subscribe(error => Message.Invoke(error.Message, MessageType.Error));

            Command_Disconnect = ReactiveCommand.CreateFromTask(Model.Disconnect);
            Command_Disconnect.ThrownExceptions.Subscribe(error => Message.Invoke(error.Message, MessageType.Error));


            // Действия после запуска приложения

            SetUI_Disconnected.Invoke();
        }

        private void Model_DeviceIsConnect(object? sender, ConnectArgs e)
        {
            SetUI_Connected.Invoke();
        }

        private void Model_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            SetUI_Disconnected.Invoke();
        }

        private void UpdateListOfPresets()
        {
            string[] FileNames = SettingsFile.FindFilesOfPresets();

            Presets.Clear();

            foreach (string element in FileNames)
            {
                Presets.Add(element);
            }

            if (SettingsDocument == null || Presets.Contains(SettingsDocument) == false)
            {
                Message.Invoke("Файл настроек " + SettingsDocument + " не существует в папке " + Model_Settings.FolderPath_Settings +
                    "\n\nНажмите ОК и выберите один из доступных файлов в появившемся окне.", MessageType.Warning);

                SettingsDocument = Select_AvailablePresetFile(Presets.ToArray());
            }

            SelectedPreset = Presets.Single(x => x == SettingsDocument);
        }

        private void Connect_Handler()
        {
            if (SettingsFile.Settings == null)
            {
                throw new Exception("Настройки не инициализированы.");
            }

            DeviceData Settings = (DeviceData)SettingsFile.Settings.Clone();

            ConnectionInfo Info;

            switch (Settings.TypeOfConnection)
            {
                case DeviceData.ConnectionName_SerialPort:

                    Info = new ConnectionInfo(new SerialPortInfo(
                        Settings.Connection_SerialPort?.COMPort,
                        Settings.Connection_SerialPort?.BaudRate_IsCustom == true ?
                            Settings.Connection_SerialPort?.BaudRate_Custom : Settings.Connection_SerialPort?.BaudRate,
                        Settings.Connection_SerialPort?.Parity,
                        Settings.Connection_SerialPort?.DataBits,
                        Settings.Connection_SerialPort?.StopBits
                        ),
                        GetEncoding(Settings.GlobalEncoding));

                    break;

                case DeviceData.ConnectionName_Ethernet:

                    Info = new ConnectionInfo(new SocketInfo(
                        Settings.Connection_IP?.IP_Address,
                        Settings.Connection_IP?.Port
                        ),
                        GetEncoding(Settings.GlobalEncoding));

                    break;

                default:
                    throw new Exception("В файле настроек задан неизвестный интерфейс связи.");
            }

            Model.Connect(Info);
        }

        public Encoding GetEncoding(string? EncodingName)
        {
            switch (EncodingName)
            {
                case "ASCII":
                    return Encoding.ASCII;

                case "Unicode":
                    return Encoding.Unicode;

                case "UTF-32":
                    return Encoding.UTF32;

                case "UTF-8":
                    return Encoding.UTF8;

                default:
                    throw new Exception("Задан неизвестный тип кодировки: " + EncodingName);
            }
        }
    }
}