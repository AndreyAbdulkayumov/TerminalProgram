using ReactiveUI;
using System.Reactive;
using MessageBox_Core;
using Core.Models.Settings;
using Core.Models.Settings.FileTypes;
using Services.Interfaces;

namespace ViewModels.Settings.Tabs
{
    public class AppSettings_VM : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> Select_Dark_Theme { get; }
        public ReactiveCommand<Unit, Unit> Select_Light_Theme { get; }

        private bool _checkAppUpdateAfterStart;

        public bool CheckAppUpdateAfterStart
        {
            get => _settingsModel.AppData.CheckUpdateAfterStart;
            set
            {
                _settingsModel.AppData.CheckUpdateAfterStart = value;
                this.RaiseAndSetIfChanged(ref _checkAppUpdateAfterStart, value);                
            }
        }

        private readonly IUIService _uiServices;
        private readonly IMessageBoxSettings _messageBox;
        private readonly Model_Settings _settingsModel;

        public AppSettings_VM(IUIService uiServices, IMessageBoxSettings messageBox, Model_Settings settingsModel)
        {
            _uiServices = uiServices ?? throw new ArgumentNullException(nameof(uiServices));
            _messageBox = messageBox ?? throw new ArgumentNullException(nameof(messageBox));
            _settingsModel = settingsModel ?? throw new ArgumentNullException(nameof(settingsModel));

            Select_Dark_Theme = ReactiveCommand.Create(SetDarkTheme);
            Select_Dark_Theme.ThrownExceptions.Subscribe(error => _messageBox.Show($"Не удалось корректно переключиться на темную тему.\n\n{error.Message}", MessageType.Error, error));

            Select_Light_Theme = ReactiveCommand.Create(SetLightTheme);
            Select_Light_Theme.ThrownExceptions.Subscribe(error => _messageBox.Show($"Не удалось корректно переключиться на светлую тему.\n\n{error.Message}", MessageType.Error, error));
        }

        private void SetDarkTheme()
        {
            _uiServices.Set_Dark_Theme();

            _settingsModel.AppData.ThemeName = AppTheme.Dark;
        }

        private void SetLightTheme()
        {
            _uiServices.Set_Light_Theme();

            _settingsModel.AppData.ThemeName = AppTheme.Light;
        }
    }
}
