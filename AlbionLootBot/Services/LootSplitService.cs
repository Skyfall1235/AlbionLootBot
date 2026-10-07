using AlbionLootBot.Database;
using AlbionLootBot.Models;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

namespace AlbionLootBot.Services
{
    public class LootSplitService(InteractionService interactionService, AdminService adminService, IDbContextFactory<LootBotDbContext> _contextFactory, ConfigService configService)
    {
        private readonly InteractionService _interactionService = interactionService;
        private readonly AdminService adminService = adminService;
        private readonly ConfigService _configService = configService;
        private readonly IDbContextFactory<LootBotDbContext> _contextFactory = _contextFactory;

        public async Task<Lootsplit> CreateSplitAsync(
        IUser user,
        string sessionName,
        ulong guildId,
        double taxRatePercent,
        ICollection<IUser>? Participants)
        {
            // 1. Guard against an existing open split
            LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();

            var existingSplit = await _context.LootSplit
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

            Player initialParticipant = await FindOrGetPlayer(_context, user, guildId);

            // 4. Add creator as initial participant
            newSplit.Participants.Add(new LootsplitParticipant
            {
                Lootsplit = newSplit,
                Player = initialParticipant,
            });

            //THIS MAY BE SUPERCEDED FOR A BATCH METHOD EVENTUALLY
            if (Participants != null)
            {
                //remove the user if they add themselves
                Participants.Remove(user);

                foreach (IUser ap in Participants)
                {
                    Player apPlayer = await FindOrGetPlayer(_context, ap, guildId);
                    LootsplitParticipant lsap = (new LootsplitParticipant
                    {
                        Lootsplit = newSplit,
                        Player = apPlayer,
                    });
                    newSplit.Participants.Add(lsap);
                }
            }

            _context.LootSplit.Add(newSplit);

            await _context.SaveChangesAsync();

            return newSplit;
        }
        public async Task MarkSplitCompleted(string splitName)
        {
            //get item first
            LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();
            //pragmas warning because VSC fails to see the null check below.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Lootsplit split = await _context.LootSplit
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

            LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Lootsplit ls = await _context.LootSplit.FirstOrDefaultAsync(ls => ls.SessionName == splitName || ls.Id == id);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            if (ls == null) return;

            _context.LootSplit.Remove(ls);
            await _context.SaveChangesAsync();
        }
        public async Task AddPlayerToExistingSplitAsync(int existingSplitId, IUser user, ulong guildId)
        {
            LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();
            Player playerEntry = await FindOrGetPlayer(_context, user, guildId);

            //find if the participant is already in the split
            bool alreadyJoined = await _context.LootsplitParticipants
        .AnyAsync(p => p.LootsplitId == existingSplitId && p.PlayerId == playerEntry.Id);//

            if (alreadyJoined)
            {
                throw new InvalidOperationException($"User is already inside of the Lootsplit id : {existingSplitId}");
            }

            //finally, save
            await SaveParticipantToSplitAsyncDB(existingSplitId, playerEntry);
        }
        public async Task RemovePlayerFromExistingSplitAsync(int existingSplitId, IUser user, ulong guildId)
        {
            LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();
            Player playerEntry = await FindOrGetPlayer(_context, user, guildId);

            //find if the participant is already in the split
            bool alreadyJoined = await _context.LootsplitParticipants
        .AnyAsync(p => p.LootsplitId == existingSplitId && p.PlayerId == playerEntry.Id);//

            if (alreadyJoined)
            {
                throw new InvalidOperationException($"User is already inside of the Lootsplit id : {existingSplitId}");
            }

            //finally, save
            await RemoveParticipantFromSplitAsyncDB(existingSplitId, playerEntry);
        }

        //Db interaction
        protected async Task SaveParticipantToSplitAsyncDB(int existingSplitId, Player player)
        {
            LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();

            var existingSplit = await _context.LootSplit
                .FirstOrDefaultAsync(ls => ls.Id == existingSplitId)
                ?? throw new InvalidOperationException($"Attempting to save to a lootsplit (id - {existingSplitId}) that does not exist");

            bool alreadyExists = await _context.LootsplitParticipants
                .AnyAsync(lp => lp.LootsplitId == existingSplitId && lp.PlayerId == player.Id);

            if (alreadyExists)
            {
                throw new InvalidOperationException($"Player {player.Id} is already a participant in lootsplit {existingSplitId}");
            }

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
        protected async Task RemoveParticipantFromSplitAsyncDB(int existingSplitId, Player player)
        {
            LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();

            var lsp = await _context.LootsplitParticipants
                .FirstOrDefaultAsync(p => p.LootsplitId == existingSplitId && p.PlayerId == player.Id)
                ?? throw new InvalidOperationException($"Player {player.Id} is not a participant in lootsplit {existingSplitId}");

            _context.LootsplitParticipants.Remove(lsp);
            await _context.SaveChangesAsync();
        }
        //name should be self explanatory, sometimes we know or dont know if they exist.
        public async Task<Player> FetchOrCreatePlayerAsync(LootBotDbContext context, LootSplitUserContext userContext)
        {
            // Check if the player is already saved inside the database disk table rows
            var player = await context.Players
                .FirstOrDefaultAsync(p => p.DiscordPlayerId == userContext.DiscordUserId);

            if (player != null)
            {
                return player;
            }

            // 💡 CRUCIAL FOR BATCH LOOPS: Check if the player was already instantiated earlier in this exact command session.
            // This stops parallel loop iterations from attempting duplicate insertions.
            player = context.Players.Local
                .FirstOrDefault(p => p.DiscordPlayerId == userContext.DiscordUserId);

            if (player != null)
            {
                return player;
            }

            // If they do not exist anywhere, initialize them cleanly inside the active state tracker
            player = new Player
            {
                DiscordPlayerId = userContext.DiscordUserId,
                DiscordName = userContext.DiscordUsername,
                Name = userContext.GlobalName ?? userContext.DiscordUsername,
                CreatedAt = DateTime.UtcNow // Placed here if your schema tracks profile creation timelines
            };

            context.Players.Add(player);

            // Note: Do NOT call SaveChangesAsync() here! Let the main caller save everything together at the end.
            return player;
        }

        //supporting methods
        public async Task<int> GetSplitIdFromSessionNameAsync(string sessionName)
        {
            LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();

            Lootsplit? split = await _context.LootSplit.FirstOrDefaultAsync(ls => ls.SessionName == sessionName);
            if (split == null) throw new InvalidDataException();
            return split.Id;
        }

        protected LootSplitUserContext CreateUserContextDto(IUser user, ulong guildId)
        {
            return new LootSplitUserContext(
                GuildId: guildId,
                DiscordUserId: user.Id,
                DiscordUsername: user.Username,
                GlobalName: user.GlobalName
            );
        }

        public async Task<Player> FindOrGetPlayer(LootBotDbContext context, IUser user, ulong guildId)
        {
            LootSplitUserContext userContext = CreateUserContextDto(user, guildId);
            Player playerEntry = await FetchOrCreatePlayerAsync(context, userContext);
            return playerEntry;
        }

        //THIS SECTION IS FOR ADDTIONAL FEATURES WE DO NOT YET NEED (might be moved to metrics :3)

        //this uses the ID COLUMN to compare against a user to see if they are participating in any splits
        protected async Task<ICollection<Lootsplit>> FindAllSplitsForUser(int PlayerId)
        {
            LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();

            //get all 
            ICollection<Lootsplit> playerSplits = await _context.LootsplitParticipants
            .Where(lp => lp.Player.Id == PlayerId)
            .Select(lp => lp.Lootsplit)
            .ToListAsync();

            return playerSplits;
        }
        protected async Task<ICollection<Lootsplit>> FindRunningSplitsForUser(int playerId)
        {
            LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();

            ICollection<Lootsplit> PlayerSplits = await FindAllSplitsForUser(playerId);
            ICollection<Lootsplit> activePlayerSplits = await _context.LootSplit
            .Where(ls => ls.Status == LootsplitStatus.Completed)
            .ToListAsync();
            return activePlayerSplits;
        }
        protected async Task<ICollection<Lootsplit>> FindCompletedSplitsForUser(int playerId)
        {
            LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();

            return await _context.LootsplitParticipants
            .Where(lp => lp.PlayerId == playerId && lp.Lootsplit.Status == LootsplitStatus.Completed)
            .Select(lp => lp.Lootsplit)
            .ToListAsync();
        }
    }
}