using System;
using System.IO;
using System.Text.Json;

namespace GUI2CHD
{
    public class Settings
    {
        public string OutputFolder { get; set; } = string.Empty;
        public string TempFolder { get; set; } = Path.Combine(Path.GetTempPath(), "GUI2CHD_Temp");
        public string LastInputFilesFolder { get; set; } = string.Empty;
        public string LastOutputFolder { get; set; } = string.Empty;
        public string Language { get; set; } = "ru";

        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "GUI2CHD",
            "settings.json"
        );

        public static Settings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
                }
            }
            catch (Exception)
            {
                // В случае ошибки возвращаем настройки по умолчанию
            }
            return new Settings();
        }

        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(SettingsPath) ?? string.Empty;
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception)
            {
                // Игнорируем ошибки сохранения
            }
        }
    }
} 