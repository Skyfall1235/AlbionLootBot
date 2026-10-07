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


        //+ remove member from split,
        [SlashCommand("add-split-participant", "a")]
        public async Task AddSplitParticipant(
            [Summary(description: "the @mention of a member to be added to this split")] IUser newParticipant,
            [Summary(description: "What is the name for this lootsplit?")] string splitName)

        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
            int splitId = await splitService.GetSplitIdFromSessionNameAsync(splitName);
            await splitService.AddPlayerToExistingSplitAsync(splitId, newParticipant, guildId);

            //tell the user
        }

        [SlashCommand("remove-split-participant", "Adds a member to a given lootsplit")]
        public async Task RemoveSplitParticipant(
            [Summary(description: "the @mention of a member to be added to this split")] IUser userToBeRemoved,
            [Summary(description: "What is the name for this lootsplit?")] string splitName)
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
            int splitId = await splitService.GetSplitIdFromSessionNameAsync(splitName);
            await splitService.RemovePlayerFromExistingSplitAsync(splitId, userToBeRemoved, guildId);
        }

        //make split,
        [SlashCommand("create-split", "a")]
        public async Task CreateSplit(
            [Summary(description: "What is the name for this lootsplit?")] string splitName,
            [Summary(description: "Space-separated list of @mentions for all group members")] string participantsInput)
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
            IUser user = Context.User;
            if (_configService.GetConfigFromContext(Context) == null)
            {
                //OOOPS
                //send notice to user that they arent in a guild?
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
        [SlashCommand("delete-split", "a")]
        public async Task DeleteSplit(
            [Summary(description: "What is the name for this lootsplit?")] string splitName)
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
            //use guild is to ensure only delete splits within a guild.
            await splitService.DeleteSplit(splitName);
            //notify the user the completion status
        }

        //add to split total w/ items,
        [SlashCommand("add-item-value", "a")]
        public async Task AddItemValueToSplit(
            [Summary(description: "What is the name for this lootsplit?")] string splitName,
            [Summary(description: "X")] int itemValue)
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
        }

        //add to split total with silver bags.
        [SlashCommand("add-silver-value", "a")]
        public async Task AddSilverValueToSplit(
            [Summary(description: "What is the name for this lootsplit?")] string splitName,
            [Summary(description: "X")] int silverValue)
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
        }

        //calculate split,
        [SlashCommand("calculate-split", "a")]
        public async Task CalculateSplit(
            [Summary(description: "What is the name for this lootsplit?")] string splitName)
        {
            await DeferAsync();
            //ulong guildId = Context.Guild.Id;
        }

        //review split value :) (we have a full crud app now)
        [SlashCommand("get-current-split", "a")]
        public async Task GetCurrentSplit(
            [Summary(description: "What is the name for this lootsplit?")] string splitName)
        {
            await DeferAsync();
            //ulong guildId = Context.Guild.Id;
        }




    }
}