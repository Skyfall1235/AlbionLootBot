using System.Text.Json;

namespace ASimpleMinecraftUpdatesBot.Services
{
    public class JsonService
    {
        private readonly string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "config.json");

        public Dictionary<ulong, BotConfig> Configs { get; private set; }

        public JsonService()
        {
            if (!Directory.Exists("data")) Directory.CreateDirectory("data");
            Configs = new();
            Configs = Load();
        }

        public Dictionary<ulong, BotConfig> Load()
        {
            if (!File.Exists(_path)) return new Dictionary<ulong, BotConfig>();
            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<Dictionary<ulong, BotConfig>>(json) ?? new Dictionary<ulong, BotConfig>();
        }

        public void SaveConfig(ulong guildId, BotConfig newConfig)
        {
            Configs.Add(guildId, newConfig);
            SaveTextToFile();
        }

        public void SaveTextToFile() => File.WriteAllText(_path, JsonSerializer.Serialize(Configs, new JsonSerializerOptions { WriteIndented = true }));
    }
}
