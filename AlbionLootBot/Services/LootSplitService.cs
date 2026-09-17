using AlbionLootBot.Database;
using AlbionLootBot.Models;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

namespace AlbionLootBot.Services
{
    public class LootSplitService(InteractionService interactionService, AdminService adminService, LootBotDbContext context, ConfigService configService)
    {
        private readonly InteractionService _interactionService = interactionService;
        private readonly AdminService adminService = adminService;
        private readonly ConfigService _configService = configService;
        private readonly LootBotDbContext _context = context;

        public async Task<Lootsplit> CreateSplitAsync(
        IUser user,
        string sessionName,
        double taxRatePercent)
        {
            // 1. Guard against an existing open split
            var existingSplit = await _context.Lootsplits
                .FirstOrDefaultAsync(s => s.Status == LootsplitStatus.Open);

            if (existingSplit != null)
            {
                throw new InvalidOperationException($"An active loot split (**{existingSplit.SessionName}**, ID: `{existingSplit.Id}`) is already open!");
            }



            // 3. Create Lootsplit entity
            decimal taxDecimal = (decimal)(taxRatePercent / 100.0);

            var newSplit = new Lootsplit
            {
                SessionName = sessionName,
                TaxRate = taxDecimal,
                Status = LootsplitStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            // 4. Add creator as initial participant
            newSplit.Participants.Add(new LootsplitParticipant
            {
                PlayerId = player.Id
            });

            _context.Lootsplits.Add(newSplit);
            await _context.SaveChangesAsync();

            return newSplit;
        }



        public async Task MarkSplitCompleted(string splitName)
        {

        }

        protected async Task FindRunningSplitsForUser(string splitName, ulong UserId)
        {
            //find splits that this person is a part of 
            //we neet to get all splits the user is a part of, then
        }

        protected async Task AddPlayerToExistingSplitAsync(int existingSplitId, IUser user, ulong guildId)
        {
            //find or get the player
            LootSplitUserContext userContext = CreateUserContextDtoAsync(user, guildId);
            Player playerEntry = await FetchOrCreatePlayerAsync(userContext);

            //find if the participant is already in the split
            bool alreadyJoined = await _context.LootsplitParticipants
        .AnyAsync(p => p.LootsplitId == existingSplitId && p.PlayerId == playerEntry.Id);

            if (alreadyJoined)
            {
                throw new InvalidOperationException($"User is already inside of the Lootsplit id : {existingSplitId}");
            }

            //finally, save
            await SaveParticipantToSplitAsync(existingSplitId, playerEntry);
        }

        //name should be self explanatory, sometimes we know or dont know if they exist.
        protected async Task<Player> FetchOrCreatePlayerAsync(LootSplitUserContext userContext)
        {
            // 2. Fetch or create Player record
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.DiscordPlayerId == userContext.DiscordUserId);

            if (player == null)
            {
                player = new Player
                {
                    DiscordPlayerId = userContext.DiscordUserId,
                    DiscordName = userContext.DiscordUsername,
                    Name = userContext.GlobalName ?? userContext.DiscordUsername
                };
                _context.Players.Add(player);
                await _context.SaveChangesAsync();
            }
            //its either created or found
            return player;
        }

        //we will use this to pass user data into and out of the Db
        protected LootSplitUserContext CreateUserContextDtoAsync(IUser user, ulong guildId)
        {
            return new LootSplitUserContext(
                GuildId: guildId,
                DiscordUserId: user.Id,
                DiscordUsername: user.Username,
                GlobalName: user.GlobalName
            );
        }

        //pulled out so we can save participants easier.
        protected async Task SaveParticipantToSplitAsync(int existingSplitId, Player player)
        {
            var participant = new LootsplitParticipant
            {
                LootsplitId = existingSplitId,
                PlayerId = player.Id
            };
            _context.LootsplitParticipants.Add(participant);
            await _context.SaveChangesAsync();
        }

    }
}