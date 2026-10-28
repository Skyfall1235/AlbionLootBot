namespace AlbionLootBot.Services
{
    using System.Text.Json;

    public class JsonService
    {
        private static readonly string _dir = Environment.GetEnvironmentVariable("DATA_DIR") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AlbionLootBot");
        private readonly string _path = Path.Combine(_dir, "config.json");

        public Dictionary<ulong, TaxConfig> Configs { get; private set; }

        public JsonService()
        {
            Configs = new();
            Configs = Load();
        }

        public Dictionary<ulong, TaxConfig> Load()
        {
            if (!File.Exists(_path)) return new Dictionary<ulong, TaxConfig>();
            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<Dictionary<ulong, TaxConfig>>(json) ?? new Dictionary<ulong, TaxConfig>();
        }

        public void SaveConfig(ulong guildId, TaxConfig newConfig)
        {
            Configs.Add(guildId, newConfig);
            SaveTextToFile();
        }

        public void SaveTextToFile() => File.WriteAllText(_path, JsonSerializer.Serialize(Configs, new JsonSerializerOptions { WriteIndented = true }));
    }

    //temp, removed upon rework of config service
    public static class AppPaths
    {
        public static readonly string DataDir =
            Environment.GetEnvironmentVariable("DATA_DIR")
            ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");

        public static string File(string name) => Path.Combine(DataDir, name);
    }
}
