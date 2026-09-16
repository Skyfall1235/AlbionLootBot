using Discord.Interactions;

namespace AlbionLootBot.Services
{
    public class TaxConfig
    {
        public ulong GuildId;
        public string GuildName = string.Empty;
        //public string GuildName;
        public int GuildTaxRate = 0;
        public ulong TargetChannelId = 0;

        public TaxConfig(ulong guildId = 0, string guildName = "", int guildTax = 0, ulong targetChannelId = 0)
        {
            GuildId = guildId;
            GuildName = guildName;
            GuildTaxRate = guildTax;
            TargetChannelId = targetChannelId;
        }
    }

    public class ConfigService
    {
        readonly JsonService? _jsonService;

        public ConfigService(JsonService service)
        {
            _jsonService = service ?? throw new ArgumentNullException(nameof(service));
        }

        public void UpdateConfig(ulong guildId, string? name = "", ulong? channelId = null)
        {
            Console.WriteLine("Updating Config with new values");
            if (!_jsonService!.Configs.TryGetValue(guildId, out var currentConfig))
            {
                currentConfig = new TaxConfig
                {
                    GuildName = "New Guild"
                };
                _jsonService.Configs.TryAdd(guildId, currentConfig);
            }

            if (!string.IsNullOrWhiteSpace(name)) currentConfig.GuildName = name;
            if (channelId.HasValue) currentConfig.TargetChannelId = channelId.Value;
            SaveConfigAndLog(guildId, currentConfig);
        }

        //update the tax rate of a gbuild for splits
        public void UpdateTaxRate(ulong guildId, int newTaxRate)
        {
            Console.WriteLine("Updating Config with new values");
            if (!_jsonService!.Configs.TryGetValue(guildId, out TaxConfig? currentConfig))
            {
                throw new ArgumentNullException(nameof(guildId));
            }
            if (currentConfig != null)
            {
                currentConfig.GuildTaxRate = newTaxRate;
                SaveConfigAndLog(guildId, currentConfig);
            }
            else throw new ArgumentException($"Current config of guild {guildId} is null after being retrieved from config file");
        }

        //seperate save and log config
        protected void SaveConfigAndLog(ulong guildId, TaxConfig currentConfig)
        {
            if (_jsonService != null)
            {
                _jsonService.SaveConfig(guildId, currentConfig);
                Console.WriteLine($"[Config] Success: {currentConfig.GuildName} updated.");
            }
            else throw new InvalidOperationException("JsonService Failed to Start");
        }

        public void SaveToFile()
        {
            _jsonService?.SaveTextToFile();
        }

        public TaxConfig? GetConfigFromContext(SocketInteractionContext context)
        {
            Console.WriteLine($"Reteriving config from service via Discord Context...");
            ulong guildId = context.Guild.Id;
            return RetriveConfigFromJson(guildId);
        }

        private TaxConfig? RetriveConfigFromJson(ulong guildId)
        {
            bool success = _jsonService!.Configs.TryGetValue(guildId, out var serverConfig);
            if (!success || serverConfig is null || serverConfig is not TaxConfig)
            {
                Console.WriteLine($"Config for server {guildId} Not found.");
                return null;
            }
            Console.WriteLine($"Found config for server: {guildId}");
            return serverConfig;
        }
    }
}