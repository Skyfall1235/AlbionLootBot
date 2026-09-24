using AlbionLootBot.Services;
using Discord;
using Discord.Interactions;
using System.Text.RegularExpressions;

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
        public async Task AddSplitParticipant(IUser newParticipant, string splitName = "")
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
            int splitId = await splitService.GetSplitIdFromSessionNameAsync(splitName);
            await splitService.AddPlayerToExistingSplitAsync(splitId, newParticipant, guildId);

            //tell the user
        }

        //make split,
        [SlashCommand("CreateSplit", "")]
        public async Task CreateSplit(
            [Summary("Hide Message?", "Is this a public or private query?")] string splitName,
            [Summary("participants", "Space-separated list of @mentions for all group members")] string participantsInput)
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
            IUser user = Context.User;
            if (_configService.GetConfigFromContext(Context) == null)
            {
                throw new ArgumentException();//come back later :3
            }

            var matches = Regex.Matches(participantsInput, @"<@!?(\d+)>");
            var discordIds = matches
                .Select(m => ulong.Parse(m.Groups[1].Value))
                .Distinct()
                .ToList();

            List<IUser> users = new List<IUser>();
            foreach (ulong discordId in discordIds)
            {
                users.Add(Context.Guild.GetUser(discordId));
            }

            // Optional: Include the creator if they didn't tag themselves
            if (!discordIds.Contains(Context.User.Id))
            {
                discordIds.Add(Context.User.Id);
            }

            if (discordIds.Count == 0)
            {
                await FollowupAsync("No valid @user mentions found in the participants argument.", ephemeral: true);
                return;
            }
            TaxConfig? configContext = _configService.GetConfigFromContext(Context);
            double taxRate = 0;

            if (configContext != null)
            {
                taxRate = (double)configContext.GuildTaxRate;
            }

            await splitService.CreateSplitAsync(user, splitName, guildId, taxRate, users);
        }

        //delete split,
        [SlashCommand("DeleteSplit", "")]
        public async Task DeleteSplit(
            [Summary("Hide Message?", "Is this a public or private query?")] string splitName)
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
            //use guild is to ensure only delete splits within a guild.
            await splitService.DeleteSplit(splitName);
            //notify the user the completion status
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