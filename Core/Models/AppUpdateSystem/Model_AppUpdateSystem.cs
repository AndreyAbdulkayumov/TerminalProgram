using System.Diagnostics;
using System.Text.Json;

namespace Core.Models.AppUpdateSystem
{
    public class Model_AppUpdateSystem
    {
        private static Model_AppUpdateSystem? _model;

        public static Model_AppUpdateSystem Model
        {
            get => _model ?? (_model = new Model_AppUpdateSystem());
        }

        private const string UrlLastestVersion = "https://andreyabdulkayumov.github.io/TerminalProgram_Website/lastestVersion.json";

        private const string UrlDonate = "https://andreyabdulkayumov.github.io/TerminalProgram_Website/donate.html";

        public Model_AppUpdateSystem()
        {

        }

        /// <summary>
        /// Проверяет наличие новой версии приложения. Если используется актуальная версия приложения, то возвращается null.
        /// </summary>
        /// <param name="currentVersion"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<LastestVersionInfo?> IsUpdateAvailable(Version currentVersion)
        {
            HttpClient client = new HttpClient();

            var response = await client.GetStringAsync(UrlLastestVersion);

            var info = JsonSerializer.Deserialize<LastestVersionInfo>(response);

            if (info != null && !string.IsNullOrEmpty(info.Version) && !string.IsNullOrEmpty(info.DownloadLink))
            {
                var lastestVersion = new Version(info.Version);

                if (lastestVersion > currentVersion)
                {
                    return info;
                }

                return null;
            }

            throw new Exception("Нарушена целостность файла с информацией о последней версии приложения.");
        }

        /// <summary>
        /// Переход на страницу пожертвований
        /// </summary>
        public void GoToDonatePage()
        {
            GoToWebPage(UrlDonate);
        }

        /// <summary>
        /// Переход на страницу по заданному URL.
        /// </summary>
        /// <param name="url"></param>
        public void GoToWebPage(string? url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true // Позволяет открыть URL в стандартном браузере
            });
        }
    }
}
