using Core.Models.Settings;
using Core.Models.Settings.FileTypes;
using MessageBox_Core;
using ReactiveUI;
using Services.Interfaces;
using System.Reactive;

namespace ViewModels.Settings.Tabs
{
    public class AppSettings_VM : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> Select_Dark_Theme { get; }
        public ReactiveCommand<Unit, Unit> Select_Light_Theme { get; }

        private bool _checkAppUpdateAfterStart;

        public bool CheckAppUpdateAfterStart
        {
            get => SettingsFile.AppData.CheckUpdateAfterStart;
            set
            {
                SettingsFile.AppData.CheckUpdateAfterStart = value;
                this.RaiseAndSetIfChanged(ref _checkAppUpdateAfterStart, value);                
            }
        }

        private readonly Model_Settings SettingsFile;

        private readonly IUIService _uiServices;
        private readonly IMessageBoxSettings _messageBox;

        public AppSettings_VM(IUIService uiServices, IMessageBoxSettings messageBox)
        {
            _uiServices = uiServices ?? throw new ArgumentNullException(nameof(uiServices));
            _messageBox = messageBox ?? throw new ArgumentNullException(nameof(messageBox));

            SettingsFile = Model_Settings.Model;

            Select_Dark_Theme = ReactiveCommand.Create(SetDarkTheme);
            Select_Dark_Theme.ThrownExceptions.Subscribe(error => _messageBox.Show("Не удалось корректно переключиться на темную тему.\n\n" + error.Message, MessageType.Error));

            Select_Light_Theme = ReactiveCommand.Create(SetLightTheme);
            Select_Light_Theme.ThrownExceptions.Subscribe(error => _messageBox.Show("Не удалось корректно переключиться на светлую тему.\n\n" + error.Message, MessageType.Error));
        }

        private void SetDarkTheme()
        {
            _uiServices.Set_Dark_Theme();

            SettingsFile.AppData.ThemeName = AppTheme.Dark;
        }

        private void SetLightTheme()
        {
            _uiServices.Set_Light_Theme();

            SettingsFile.AppData.ThemeName = AppTheme.Light;
        }
    }
}
