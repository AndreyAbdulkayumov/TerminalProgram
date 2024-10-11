using Core.Models.AppUpdateSystem;
using MessageBox_Core;
using ReactiveUI;
using System.Reactive;

namespace ViewModels
{
    public class AboutApp_VM : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit>? Command_CheckUpdate { get; }
        public ReactiveCommand<Unit, Unit> Command_MakeDonate { get; }

        private readonly IMessageBox _messageBox;

        private readonly Model_AppUpdateSystem _model;

        public AboutApp_VM(IMessageBox messageBox, Version? appVersion)
        {
            _messageBox = messageBox;

            _model = Model_AppUpdateSystem.Model;

            if (appVersion != null)
            {
                Command_CheckUpdate = ReactiveCommand.CreateFromTask(async () => await CheckUpdate(appVersion));
                Command_CheckUpdate.ThrownExceptions.Subscribe(error => _messageBox.Show($"Ошибка проверки обновлений:\n\n{error.Message}", MessageType.Error));
            }            

            Command_MakeDonate = ReactiveCommand.Create(_model.GoToDonatePage);
            Command_MakeDonate.ThrownExceptions.Subscribe(error => _messageBox.Show($"Ошибка :(\n\n{error.Message}", MessageType.Error));
        }

        private async Task CheckUpdate(Version appVersion)
        {
            LastestVersionInfo? info = await _model.IsUpdateAvailable(appVersion);

            if (info != null)
            {
                if (!string.IsNullOrEmpty(info.Version))
                {
                    string downloadLink = _model.GetDownloadLink(info);

                    if (await _messageBox.ShowYesNoDialog($"Доступна новая версия приложения - {info.Version}\nПерейти на страницу скачивания?", MessageType.Information) == MessageBoxResult.Yes)
                    {
                        _model.GoToWebPage(downloadLink);
                    }
                    return;
                }

                throw new Exception("Нарушена целостность файла с информацией об обновлении.");
            }

            _messageBox.Show("Вы используйте актуальную версию приложения!", MessageType.Information);
        }
    }
}
