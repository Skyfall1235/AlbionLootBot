namespace AlbionLootBot.Services
{
    public class JsonService
    {
        private readonly string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "config.json");

        public Dictionary<ulong, TaxConfig> Configs { get; private set; }

        public JsonService()
        {
            if (!Directory.Exists("data")) Directory.CreateDirectory("data");
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
}
