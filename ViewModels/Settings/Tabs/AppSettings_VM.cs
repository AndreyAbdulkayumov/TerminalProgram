using Core.Models.Settings;
using Core.Models.Settings.FileTypes;
using MessageBox_Core;
using ReactiveUI;
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


        public AppSettings_VM(Action set_Dark_Theme_Handler, Action set_Light_Theme_Handler, IMessageBox messageBox)
        {
            SettingsFile = Model_Settings.Model;

            Select_Dark_Theme = ReactiveCommand.Create(() =>
            {
                set_Dark_Theme_Handler();

                SettingsFile.AppData.ThemeName = AppTheme.Dark;
            });
            Select_Dark_Theme.ThrownExceptions.Subscribe(error => messageBox.Show("Не удалось корректно переключиться на темную тему.\n\n" + error.Message, MessageType.Error));

            Select_Light_Theme = ReactiveCommand.Create(() =>
            {
                set_Light_Theme_Handler();

                SettingsFile.AppData.ThemeName = AppTheme.Light;
            });
            Select_Light_Theme.ThrownExceptions.Subscribe(error => messageBox.Show("Не удалось корректно переключиться на светлую тему.\n\n" + error.Message, MessageType.Error));
        }
    }
}
