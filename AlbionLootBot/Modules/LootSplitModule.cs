using AlbionLootBot.Services;
using Discord.Interactions;

namespace AlbionLootBot.Modules
{
    public class LootSplitModule(InteractionService interactionService, AdminService adminService, LootSplitService splitService, ConfigService config) : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly InteractionService _interactionService = interactionService;
        private readonly AdminService adminService = adminService;
        private readonly LootSplitService splitService = splitService;
        private readonly ConfigService _configService = config;
        //add member
        [SlashCommand("AddSplitParticipant", "Adds a member to a given lootsplit")]
        public async Task AddMemberToSplit()
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
            //get split from name of split?
        }
        //+ remove member from split,
        [SlashCommand("AddSplitParticipant", "")]
        public async Task AddSplitParticipant()
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;

        }
        //make split,
        [SlashCommand("CreateSplit", "")]
        public async Task CreateSplit()
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
            if (_configService.GetConfigFromContext(Context) == null)
            {
                throw new ArgumentException();//come back later :3
            }

            double taxRate = (double)_configService.GetConfigFromContext(Context).GuildTaxRate;

            splitService.CreateSplitAsync();
        }
        //delete split,
        [SlashCommand("DeleteSplit", "")]
        public async Task DeleteSplit(
            [Summary("Hide Message?", "Is this a public or private query?")] string splitName)
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
            //use guild is to ensure only delete splits within a guild.
        }
        //add to split total w/ items,
        [SlashCommand("AddItemValue", "")]
        public async Task AddItemValueToSplit(
            [Summary("Hide Message?", "Is this a public or private query?")] string splitName,
            [Summary("Hide Message?", "Is this a public or private query?")] int itemValue)
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
        }
        //add to split total with silver bags.
        [SlashCommand("AddSilverValue", "")]
        public async Task AddSilverValueToSplit(
            [Summary("Hide Message?", "Is this a public or private query?")] string splitName,
            [Summary("Hide Message?", "Is this a public or private query?")] int silverValue)
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
        }
        //calculate split,
        [SlashCommand("CalculateSplit", "")]
        public async Task CalculateSplit(
            [Summary("Hide Message?", "Is this a public or private query?")] string splitName)
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
        }
        //review split value :) (we have a full crud app now)
        [SlashCommand("GetCurrentSplit", "")]
        public async Task GetCurrentSplit(
            [Summary("Hide Message?", "Is this a public or private query?")] string splitName)
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
        }




    }
}