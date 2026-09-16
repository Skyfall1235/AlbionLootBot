using AlbionLootBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace AlbionLootBot.Modules
{
    public class AdminModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly InteractionService _interactionService;
        private readonly AdminService adminService;
        private readonly LootSplitService splitService;


        [SlashCommand("help", "Get a list of all slash commands this bot has. ")]
        public async Task ListCommands()
        {
            var commands = _interactionService.SlashCommands;
            var commandList = string.Join("\n", commands.Select(c => $"**/{c.Name}** - {c.Description}"));
            await RespondAsync($"### Available Commands:\n{commandList}");
        }



        [SlashCommand("setup", "Configure the Minecraft server settings.")]
        [DefaultMemberPermissions(GuildPermission.Administrator)]
        public async Task SetupCommandAsync(
            [Summary("name", "The name of the server")] string GuildName,
            [Summary("ip", "The IP address of the server")] string TaxRate,
            [Summary("channel", "The channel for updates")] SocketTextChannel? channel = null)
        {
            await DeferAsync(ephemeral: true);

        }



        [SlashCommand("playerlist", "Get your servers current registered player list.")]
        public async Task GetPlayersInGuildAsList(
            [Summary("Hide Message?", "Is this a public or private query?")] bool isEphemeral = true)
        {
            await DeferAsync(ephemeral: isEphemeral); //we will be ephemeral but give the option to have it be static
        }


    }
}