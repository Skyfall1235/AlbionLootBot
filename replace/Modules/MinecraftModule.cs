using ASimpleMinecraftUpdatesBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace ASimpleMinecraftUpdatesBot.Modules
{
    public class MinecraftModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly MineStatService _mcService;
        private readonly ConfigService _configService;
        private readonly InteractionService _interactionService;


        public MinecraftModule(MineStatService mcService, ConfigService configService, InteractionService interactionService)
        {
            _mcService = mcService;
            _configService = configService;
            _interactionService = interactionService;
        }

        [SlashCommand("help", "Get a list of all slash commands this bot has. ")]
        public async Task ListCommands()
        {
            var commands = _interactionService.SlashCommands;
            var commandList = string.Join("\n", commands.Select(c => $"**/{c.Name}** - {c.Description}"));
            await RespondAsync($"### Available Commands:\n{commandList}");
        }

        [SlashCommand("status", "Get the current status of the Minecraft server.")]
        public async Task StatusCommandAsync()
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
            string guildName = Context.Guild.Name;
            BotConfig? config = _configService.GetConfigFromContext(Context);
            if(config is null)
            {
                await FollowupAsync("❌ **Server Config Not Found.**");
                return;
            }

            Console.WriteLine($"Status command run in: {guildName} ({guildId})");
            
            var status = await _mcService.GetFullStatus(config);
            if (status.IsOnline)
            {
                await FollowupAsync($"✅ **Server Online!** Players: `{status.CurrentOverMaxPlayers}`");
            }
            else
            {
                await FollowupAsync("❌ **Server Offline.** Check the IP or port settings.");
            }
        }

        [SlashCommand("setup", "Configure the Minecraft server settings.")]
        [DefaultMemberPermissions(GuildPermission.Administrator)]
        public async Task SetupCommandAsync(
            [Summary("name", "The name of the server")] string serverName,
            [Summary("ip", "The IP address of the server")] string ip,
            [Summary("port", "The port (default is 25565)")] ushort port = 25565,
            [Summary("channel", "The channel for updates")] SocketTextChannel? channel = null)
        {
            await DeferAsync(ephemeral: true);
            try
            {
                ulong guildId = Context.Guild.Id;
                _configService.UpdateConfig(guildId, serverName, ip, port, channel?.Id);
                await FollowupAsync($"✅ **Settings Saved!**\nIP: `{ip}`\nPort: `{port}`\nChannel: {channel?.Mention ?? "None"}");
            }
            catch (Exception ex)
            {
                await FollowupAsync($"❌ **Error saving config:** {ex.Message}");
            }
        }

        [SlashCommand("pinghost", "Check if the physical server is reachable.")]
        public async Task PingHostAsync()
        {
            await DeferAsync();
            BotConfig? config = _configService.GetConfigFromContext(Context);
            if (config is null)
            {
                await FollowupAsync("❌ **Server Config Not Found.**");
                return;
            }
            bool isAlive = await _mcService.PingComputerAsync(config.MinecraftIp);
            await FollowupAsync(isAlive ? "🖥️ Host is reachable!" : "💀 Host is unreachable (Offline or Firewall blocking).");
        }

        [SlashCommand("address", "Get your servers currently configured MC address and port.")]
        public async Task GetServerAddressAndPort(
            [Summary("Hide Message?", "Is this a public or private query?")] bool isEphemeral = true)
        {
            await DeferAsync(ephemeral: isEphemeral); //we will be ephemeral but give the option to have it be static
            BotConfig? config = _configService.GetConfigFromContext(Context);
            if (config is not null)
            {
                await FollowupAsync($"❌ **Server Address:** {config.MinecraftIp}:{config.Port}");
                return;
            }
        }

        [SlashCommand("playerlist", "Get your servers current player list.")]
        public async Task GetPlayerList(
            [Summary("Hide Message?", "Is this a public or private query?")] bool isEphemeral = true)
        {
            await DeferAsync(ephemeral: isEphemeral); //we will be ephemeral but give the option to have it be static
            BotConfig? config = _configService.GetConfigFromContext(Context);
            if (config is null)
            {
                await FollowupAsync("❌ **Server Config Not Found.**");
                return;
            }
            bool isAlive = await _mcService.GetAliveStatus(config);
            if (!isAlive)
            {
                await FollowupAsync("❌ **Server is not online.**");
                return;
            }
            string[] playerList = await _mcService.GetPlayerList(config);
            string playerListReadable = string.Join(", ", playerList);
            await FollowupAsync($"**Online Players:** {playerListReadable}");
        }


    }
}