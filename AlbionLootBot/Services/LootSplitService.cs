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
        ulong guildId,
        double taxRatePercent,
        ICollection<IUser>? Participants)
        {
            // 1. Guard against an existing open split
            var existingSplit = await _context.Lootsplits
                .FirstOrDefaultAsync(s => s.Status == LootsplitStatus.Open && s.SessionName == sessionName);

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
            LootSplitUserContext initialContext = CreateUserContextDtoAsync(user, guildId);
            Player initialParticipant = await FetchOrCreatePlayerAsync(initialContext);

            // 4. Add creator as initial participant
            newSplit.Participants.Add(new LootsplitParticipant
            {
                Lootsplit = newSplit,
                Player = initialParticipant,
            });

            //THIS MAY BE SUPERCEDED FOR A BATCH METHOD EVENTUALLY
            if (Participants != null)
            {
                foreach (IUser ap in Participants)
                {
                    LootSplitUserContext apContext = CreateUserContextDtoAsync(user, guildId);
                    Player apPlayer = await FetchOrCreatePlayerAsync(apContext);
                    LootsplitParticipant lsap = (new LootsplitParticipant
                    {
                        Lootsplit = newSplit,
                        Player = apPlayer,
                    });
                    newSplit.Participants.Add(lsap);
                }
            }

            _context.Lootsplits.Add(newSplit);

            await _context.SaveChangesAsync();

            return newSplit;
        }

        public async Task MarkSplitCompleted(string splitName)
        {
            //get item first
            //pragmas warning because VSC fails to see the null check below.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Lootsplit split = await _context.Lootsplits
                .FirstOrDefaultAsync(ls => ls.SessionName == splitName);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            //if not found, return
            if (split == null) return;
            //if found mark as split as complete :)
            split.SetLootsplitComplete();
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSplit(string splitName = "", int id = 0)
        {
            if (splitName == "" && id == 0) return;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Lootsplit ls = await _context.Lootsplits.FirstOrDefaultAsync(ls => ls.SessionName == splitName || ls.Id == id);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            if (ls == null) return;

            _context.Lootsplits.Remove(ls);
            await _context.SaveChangesAsync();
        }

        protected async Task AddPlayerToExistingSplitAsync(int existingSplitId, IUser user, ulong guildId)
        {
            //find or get the player
            LootSplitUserContext userContext = CreateUserContextDtoAsync(user, guildId);
            Player playerEntry = await FetchOrCreatePlayerAsync(userContext);

            //find if the participant is already in the split
            bool alreadyJoined = await _context.LootsplitParticipants
        .AnyAsync(p => p.LootsplitId == existingSplitId && p.PlayerId == playerEntry.Id);//

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
            //find exisitng split via ID
            var existingSplit = await _context.Lootsplits
                .Where(ls => ls.Id == existingSplitId)
                .FirstOrDefaultAsync() ?? throw new InvalidOperationException($"Attempting to Save to a lootpslit (id - {existingSplitId}) that does not exist");

            var participant = new LootsplitParticipant
            {
                LootsplitId = existingSplitId,
                PlayerId = player.Id,
                Lootsplit = existingSplit,
                Player = player
            };
            _context.LootsplitParticipants.Add(participant);
            await _context.SaveChangesAsync();
        }


        //THIS SECTION IS FOR ADDTIONAL FEATURES WE DO NOT YET NEED

        //this uses the ID COLUMN to compare against a user to see if they are participating in any splits
        protected async Task<ICollection<Lootsplit>> FindAllSplitsForUser(int PlayerId)
        {
            //get all 
            ICollection<Lootsplit> playerSplits = await _context.LootsplitParticipants
            .Where(lp => lp.Player.Id == PlayerId)
            .Select(lp => lp.Lootsplit)
            .ToListAsync();

            return playerSplits;
        }

        protected async Task<ICollection<Lootsplit>> FindRunningSplitsForUser(int playerId)
        {
            ICollection<Lootsplit> PlayerSplits = await FindAllSplitsForUser(playerId);
            ICollection<Lootsplit> activePlayerSplits = await _context.Lootsplits
            .Where(ls => ls.Status == LootsplitStatus.Completed)
            .ToListAsync();
            return activePlayerSplits;
        }

        protected async Task<ICollection<Lootsplit>> FindCompletedSplitsForUser(int playerId)
        {
            return await _context.LootsplitParticipants
            .Where(lp => lp.PlayerId == playerId && lp.Lootsplit.Status == LootsplitStatus.Completed)
            .Select(lp => lp.Lootsplit)
            .ToListAsync();
        }



    }
}