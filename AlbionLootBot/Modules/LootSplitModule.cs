using AlbionLootBot.Dtos;
using AlbionLootBot.Models;
using AlbionLootBot.Services;
using Discord;
using Discord.Interactions;
using System.Text.RegularExpressions;

namespace AlbionLootBot.Modules
{
    public partial class LootSplitModule(InteractionService interactionService, LootSplitService splitService, ConfigService config, PlayerManagementService playerManagementService) : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly InteractionService _interactionService = interactionService;
        private readonly LootSplitService splitService = splitService;
        private readonly PlayerManagementService playerManagement = playerManagementService;
        private readonly ConfigService _configService = config;

        //add member + remove member from split
        [SlashCommand("AddParticipant", "Add a participant to a currently open split.")]
        public async Task AddSplitParticipant(
            [Summary(description: "the @mention of a member to be added to this split")] IUser newParticipant,
            [Summary(description: "What is the name for this lootsplit?")] string splitName)

        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
            int splitId;
            try
            {
                splitId = await splitService.GetSplitIdFromSessionNameAsyncDB(splitName);
                await splitService.AddPlayerToExistingSplitAsync(splitId, newParticipant, guildId);
                await ReplyAsync($"Player: <@{newParticipant.Id}> has been added to the lootsplit: {splitName}");
            }
            catch (InvalidOperationException IOEx)
            {
                //alt alert
                await FollowupAsync($"Operation Failed : {IOEx.Message}");
            }
            catch (Exception ex)
            {
                await FollowupAsync($"An uknown exeception has occurred, please contact the admin with this message: {ex.Message}");
            }
        }

        [SlashCommand("RemoveParticipant", "Remove a member from a currently open lootsplit")]
        public async Task RemoveSplitParticipant(
            [Summary(description: "the @mention of a member to be added to this split")] IUser userToBeRemoved,
            [Summary(description: "What is the name for this lootsplit?")] string splitName)
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
            int splitId = 0;//default to prevent errors, 
            try
            {
                splitId = await splitService.GetSplitIdFromSessionNameAsyncDB(splitName);
                await splitService.RemovePlayerFromExistingSplitAsync(splitId, userToBeRemoved, guildId);
                await ReplyAsync($"Player: <@{userToBeRemoved.Id}> has been removed from the lootsplit: {splitName}");
            }
            catch (Exception ex)
            {
                await FollowupAsync($"An uknown exeception has occurred, please contact the admin with this message: {ex.Message}");
            }
        }

        //make split,
        [SlashCommand("CreateSplit", "Create new lootsplit for you and potentially your friends!")]
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

            Lootsplit newSplit = new() { SessionName = "default" };

            try
            {
                newSplit = await splitService.CreateSplitAsync(user, splitName, guildId, taxRate, users);
                await ReplyAsync($"Split {newSplit.SessionName} created! ID: {newSplit.Id}");
                await FollowupAsync($"this will require multiple steps probably, and may be combined in the future :)", ephemeral: true);
            }
            catch (InvalidOperationException IOEx)
            {
                await FollowupAsync(IOEx.Message);
            }
        }

        //delete split,
        [SlashCommand("DeleteSplit", "this does not currently have confirmation. If you do this, statistics for your guild may be lost.")]
        public async Task DeleteSplit(
            [Summary(description: "What is the name of the lootsplit?")] string splitName)
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
            //use guild is to ensure only delete splits within a guild.
            await splitService.DeleteSplit(splitName);
            //notify the user the completion status
            await FollowupAsync($"Split '{splitName}' deleted!", ephemeral: false);
        }

        //see all available splits for the user
        [SlashCommand("ViewActiveSplits", "See your active splits")]
        public async Task ViewUserActiveSplits()
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
            try
            {
                Player player = await ConfirmUserExists(Context);
                ICollection<Lootsplit> lootsplitList = await splitService.FindRunningSplitsForUserAsyncDB(player.Id);

                string sessionNamesList = string.Join(", ", lootsplitList.Select(s => s.SessionName));

                await FollowupAsync($"The following Split Sessions are active for <@{Context.User.Id}>: {sessionNamesList}");
            }
            catch (UserNotFoundException ex)
            {
                // Since you deferred the response earlier, use FollowupAsync to send the error message
                await FollowupAsync($"Could not load your active splits: {ex.Message}", ephemeral: true);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"An Unknown error occured with the active split logic, please message admin with this info : {ex.Message}");
            }
        }

        //see all active splits for admin
        [SlashCommand("ViewAllActiveSplits", "Admin view of all active splits")]
        public async Task ViewAllActiveSplits()
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
            try
            {
                Player player = await ConfirmUserExists(Context);
                ICollection<Lootsplit> lootsplitList = await splitService.FindAllSplitsForUser(player.Id);

                string sessionNamesList = string.Join(", ", lootsplitList.Select(s => s.SessionName));

                await FollowupAsync($"The following Split Sessions are active for <@{Context.User.Id}>: {sessionNamesList}");
            }
            catch (UserNotFoundException ex)
            {
                // Since you deferred the response earlier, use FollowupAsync to send the error message
                await FollowupAsync($"Could not load your active splits: {ex.Message}", ephemeral: true);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"An Unknown error occured with the active split logic, please message admin with this info : {ex.Message}");
            }
        }

        //add to split total w/ items,
        [SlashCommand("AddItemValue", "Add Silver or Item vlaue to the split")]
        public async Task AddItemValueToSplit(
            [Summary(description: "What is the name for this lootsplit?")] string sessionName,
            [Summary(description: "Silver or Items?")] TaxableEntity entityType,
            [Summary(description: "Silver Value")] int itemValue)
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;
            LootItemDTO newDto = new(entityType, itemValue);
            int existingSplitId;
            try
            {
                //add player to pslit if they arent in it already

                existingSplitId = await splitService.GetSplitIdFromSessionNameAsyncDB(sessionName);
                await splitService.AddItemToLootsplitAsyncDB(existingSplitId, newDto);
                await splitService.AddPlayerToExistingSplitAsync(existingSplitId, Context.User, guildId);
                await FollowupAsync($"{entityType} item with silver value of {itemValue} added to {sessionName}");
            }
            catch (Exception ex)
            {
                await FollowupAsync(ex.Message);
            }

        }

        //calculate split,
        //eventualyl we could have it so the tax is per PERSON to each guild
        //via using tax via guild and the aformentioned per person calulation logic
        [SlashCommand("CalculateSplit", "Calculate the lootsplit per person AND close the lootsplit")]
        public async Task CalculateSplit(
            [Summary(description: "What is the name of this lootsplit?")] string splitName)
        {
            await DeferAsync();
            ulong guildId = Context.Guild.Id;

            int splitId = 0;//default to prevent errors, 
            try
            {
                splitId = await splitService.GetSplitIdFromSessionNameAsyncDB(splitName);
            }
            catch
            {
                await FollowupAsync($"Unable to find lootsplit : '{splitName}' in database");
            }

            try
            {
                CalculationEntity? calcEntity = await splitService.CompleteSplitAndCalculatePayout(splitId, Context.User, guildId);
                await DiscordResponse.SendCalculationResultAsync(Context, calcEntity, splitName);
            }
            catch (Exception ex)
            {
                await FollowupAsync($"An unexpected error occured : {ex}");
            }
        }

        //review split value :) (we have a full crud app now)
        [SlashCommand("GetCurrentSplit", "Get the full split value currently")]
        public async Task GetCurrentSplit(
            [Summary(description: "What is the name of this lootsplit?")] string splitName)
        {
            await DeferAsync();
            //ulong guildId = Context.Guild.Id;
            Lootsplit split;
            try
            {
                split = await splitService.ViewActiveSplitContents(splitName);
                long totalSilver = split.TotalSilver;
                await FollowupAsync($"The total value of {splitName}'s silver value is {totalSilver} silver");
            }
            catch (Exception ex)
            {
                await FollowupAsync(ex.Message);
            }

        }
    }


    public partial class AuditAndManagementModule(InteractionService interactionService, LootSplitService splitService, ConfigService config, PlayerManagementService playerManagementService) : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly InteractionService _interactionService = interactionService;
        private readonly LootSplitService splitService = splitService;
        private readonly PlayerManagementService playerManagement = playerManagementService;
        private readonly ConfigService _configService = config;
    }