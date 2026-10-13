using AlbionLootBot.Models;
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
            [Summary(description: "Space-separated list of @mentions for all group members")] string participantsInput = "")
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
            IUser user = Context.User;
            if (_configService.GetConfigFromContext(Context) == null)
            {
                //OOOPS
                //send notice to user that they arent in a guild?
                await FollowupAsync("The Lootsplit Maker is not part of a registered guild, " +
                    "and therefore cannot pull a tax rate. Tax will be set to 0 for this split.",
                    ephemeral: true);
            }

            var discordIds = new List<ulong> { Context.User.Id };

            if (!string.IsNullOrEmpty(participantsInput))
            {
                var matches = Regex.Matches(participantsInput, @"<@!?(\d+)>");
                foreach (Match match in matches)
                {
                    if (ulong.TryParse(match.Groups[1].Value, out var id))
                    {
                        discordIds.Add(id);
                    }
                }
                discordIds = discordIds.Distinct().ToList();
            }

            List<IUser> users = new List<IUser>();
            foreach (ulong discordId in discordIds)
            {
                users.Add(Context.Guild.GetUser(discordId));
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

            Lootsplit newSplit = new()
            {
                SessionName = "default"
            };
            try
            {
                newSplit = await splitService.CreateSplitAsync(user, splitName, guildId, taxRate, users);
            }
            catch
            {

            }
            await FollowupAsync($"Split {newSplit.SessionName} created! ID - {newSplit.Id}", ephemeral: false);
            await FollowupAsync($"this will require multiple steps probably, and may be combined in the future :)", ephemeral: true);
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
            await FollowupAsync($"Split '{splitName}' deleted!", ephemeral: false);
        }

        //see all available splits
        [SlashCommand("view-active-splits", "b")]
        public async Task ViewActiveSplits()
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
            Player? player = await splitService.GetPlayerDBInfo(Context.User);
            if (player == null)
            {
                await FollowupAsync("User not found in Database.");
                return;
            }
            //use guild is to ensure only delete splits within a guild.
            await splitService.FindRunningSplitsForUser(player.Id);
            //notify the user the completion status
            await FollowupAsync($"The following Split Sessions are active for {}: ", ephemeral: false);
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
        //eventualyl we could have it so the tax is per PERSON to each guild
        //via using tax via guild and the aformentioned per person calulation logic
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