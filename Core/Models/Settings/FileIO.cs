using System.Text.Json;

namespace Core.Models.Settings
{
    internal static class FileIO
    {
        public static void Save<T>(string FilePath, T Data)
        {
            if (File.Exists(FilePath) == false)
            {
                File.Create(FilePath).Close();
            }

            File.WriteAllText(FilePath, string.Empty);

            var Options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            using (FileStream Stream = new FileStream(FilePath, FileMode.Open))
            {
                JsonSerializer.Serialize(Stream, Data, Options);
            }
        }

        public static T? Read<T>(string FilePath)
        {
            if (File.Exists(FilePath) == false)
            {
                throw new Exception("Файл настроек не существует.\n\n" + "Путь: " + FilePath);
            }

            T? Data;

            try
            {
                using (FileStream Stream = new FileStream(FilePath, FileMode.Open))
                {
                    Data = JsonSerializer.Deserialize<T>(Stream);
                }
            }

            // Создание настроек по умолчанию, если в файле некоректные данные.
            catch (JsonException)
            {
                return default;
            }

            return Data;
        }

        public static T ReadOrCreateDefault<T>(string FilePath, T DefaultData)
        {
            try
            {
                T? Data = Read<T>(FilePath);

                if (Data == null)
                {
                    Save(FilePath, DefaultData);

                    Data = DefaultData;
                }

                return Data;
            }
            
            catch (Exception)
            {
                return DefaultData;
            }
        }
    }
}
