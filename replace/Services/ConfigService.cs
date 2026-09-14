using Discord.Interactions;

namespace ASimpleMinecraftUpdatesBot.Services
{
    public class BotConfig
    {
        public string ServerName { get; set; } = "";
        public string MinecraftIp { get; set; } = "127.0.0.1";
        public ushort Port { get; set; } = 25565;
        public ulong TargetChannelId { get; set; }
    }

    public class ConfigService
    {
        readonly JsonService? _jsonService;

        public ConfigService(JsonService service)
        {
            _jsonService = service ?? throw new ArgumentNullException(nameof(service));
        }

        public void UpdateConfig(ulong guildId, string? name = "", string? ip = null, ushort? port = null, ulong? channelId = null)
        {
            Console.WriteLine("Updating Config with new values");
            if (!_jsonService!.Configs.TryGetValue(guildId, out var currentConfig))
            {
                currentConfig = new BotConfig
                {
                    ServerName = "New Minecraft Server",
                    Port = 25565
                };
                _jsonService.Configs.TryAdd(guildId, currentConfig);
            }

            if (!string.IsNullOrWhiteSpace(name)) currentConfig.ServerName = name;
            if (!string.IsNullOrWhiteSpace(ip)) currentConfig.MinecraftIp = ip;
            if (port.HasValue) currentConfig.Port = port.Value;
            if (channelId.HasValue) currentConfig.TargetChannelId = channelId.Value;

            _jsonService.SaveConfig(guildId, currentConfig);
            Console.WriteLine($"[Config] Success: {currentConfig.ServerName} updated.");
        }

        public void SaveToFile()
        {
            _jsonService?.SaveTextToFile();
        }

        public BotConfig? GetConfigFromContext(SocketInteractionContext context)
        {
            Console.WriteLine($"Reteriving config from service via Discord Context...");
            ulong guildId = context.Guild.Id;
            return RetriveConfigFromJson(guildId);
        }

        private BotConfig? RetriveConfigFromJson(ulong guildId)
        {
            bool success = _jsonService!.Configs.TryGetValue(guildId, out var serverConfig);
            if (!success || serverConfig is null || serverConfig is not BotConfig)
            {
                Console.WriteLine($"Config for server {guildId} Not found.");
                return null;
            }
            Console.WriteLine($"Found config for server: {guildId}");
            return serverConfig;
        }
    }
}