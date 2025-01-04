using System.Text.Json;
using System.Text.Encodings.Web;

namespace Core.Models.Settings
{
    internal static class FileIO
    {
        public static void Save<T>(string filePath, T data)
        {
            if (File.Exists(filePath) == false)
            {
                File.Create(filePath).Close();
            }

            File.WriteAllText(filePath, string.Empty);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            using var stream = new FileStream(filePath, FileMode.Open);
            
            JsonSerializer.Serialize(stream, data, options);            
        }

        public static T? Read<T>(string filePath)
        {
            if (File.Exists(filePath) == false)
            {
                throw new Exception("Файл настроек не существует.\n\n" + "Путь: " + filePath);
            }

            T? data;

            try
            {
                using var stream = new FileStream(filePath, FileMode.Open);
                
                data = JsonSerializer.Deserialize<T>(stream);
            }

            // Если в файле некоректные данные.
            catch (JsonException)
            {
                return default;
            }

            return data;
        }

        public static T ReadOrCreateDefault<T>(string filePath, T defaultData)
        {
            try
            {
                T? data = Read<T>(filePath);

                if (data == null)
                {
                    Save(filePath, defaultData);

                    data = defaultData;
                }

                return data;
            }
            
            catch (Exception)
            {
                return defaultData;
            }
        }
    }
}
