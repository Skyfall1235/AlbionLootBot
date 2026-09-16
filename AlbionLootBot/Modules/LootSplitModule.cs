using AlbionLootBot.Services;
using Discord.Interactions;

namespace AlbionLootBot.Modules
{
    public class LootSplitModule(InteractionService interactionService, AdminService adminService, LootSplitService splitService) : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly InteractionService _interactionService = interactionService;
        private readonly AdminService adminService = adminService;
        private readonly LootSplitService splitService = splitService;


        [SlashCommand("status", "Get the current status of the Minecraft server.")]
        public async Task StatusCommandAsync()
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
            string guildName = Context.Guild.Name;
        }

    }
}