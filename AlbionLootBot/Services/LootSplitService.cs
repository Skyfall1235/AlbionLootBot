using AlbionLootBot.Database;
using AlbionLootBot.Dtos;
using AlbionLootBot.Models;
using Discord;
using Microsoft.EntityFrameworkCore;

namespace AlbionLootBot.Services
{
    public class LootSplitService(IDbContextFactory<LootBotDbContext> _contextFactory)
    {
        private readonly IDbContextFactory<LootBotDbContext> _contextFactory = _contextFactory;

        #region Create, modify, and Delete
        public async Task<Lootsplit> CreateSplitAsync(
        IUser user,
        string sessionName,
        ulong guildId,
        double taxRatePercent,
        ICollection<IUser>? Participants)
        {
            // 1. Guard against an existing open split
            await using LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();

            var existingSplit = await _context.LootSplit
                .FirstOrDefaultAsync(s => s.Status == LootsplitStatus.Open && s.SessionName == sessionName);

            if (existingSplit != null)
            {
                throw new InvalidOperationException($"An active loot split '(**{existingSplit.SessionName}**', ID: `{existingSplit.Id}`) is already open!");
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

            Player initialParticipant = await FindOrGetPlayerAsyncDB(_context, user, guildId);

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
                    Player apPlayer = await FindOrGetPlayerAsyncDB(_context, ap, guildId);
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
            await using LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();

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

            await using LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Lootsplit ls = await _context.LootSplit.FirstOrDefaultAsync(ls => ls.SessionName == splitName || ls.Id == id);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            if (ls == null) return;

            _context.LootSplit.Remove(ls);
            await _context.SaveChangesAsync();
        }
        public async Task AddPlayerToExistingSplitAsync(int existingSplitId, IUser user, ulong guildId)
        {
            await using LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();
            Player playerEntry = await FindOrGetPlayerAsyncDB(_context, user, guildId);

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
            await using LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();
            Player playerEntry = await FindOrGetPlayerAsyncDB(_context, user, guildId);

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

        public async Task<Lootsplit> ViewActiveSplitContents(string existingSplitName)
        {
            await using LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();
            int existingId = await GetSplitIdFromSessionNameAsyncDB(existingSplitName);
            Lootsplit split = await RetrieveSingleLootsplitAsyncDB(_context, existingId) ?? throw new InvalidDataException($"Session name '{existingSplitName}' not associated with any split");
            return split;
        }
        public async Task AddItemToLootsplitAsyncDB(int existingSplitId, LootItemDTO dto)
        {
            await using LootBotDbContext? context = await _contextFactory.CreateDbContextAsync();
            Lootsplit? existingSplit = await RetrieveSingleLootsplitAsyncDB(context, existingSplitId);
            if (existingSplit == null) throw new ArgumentNullException(nameof(existingSplit));
            if (existingSplit.Status == LootsplitStatus.Completed) throw new InaccessableActionException("Cannot modify a completed split. Please make a new split");
            LootItem newItem = new(existingSplitId, dto.TaxableEntity, dto.ValueSilver);
            await context.LootItems.AddAsync(newItem);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// removes a given item from an existing lootplit
        /// </summary>
        /// <param name="existingSplitId"></param>
        /// <param name="context"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task RemoveLootItemFromLootsplitAsyncDB(int existingSplitId, LootBotDbContext context, int itemId)
        {
            Lootsplit? existingSplit = await RetrieveSingleLootsplitAsyncDB(context, existingSplitId);
            if (existingSplit == null) throw new ArgumentNullException(nameof(existingSplit));
            if (existingSplit.Status == LootsplitStatus.Completed) throw new InaccessableActionException("Cannot modify a completed split");
            await context.LootItems.Where(li => li.LootsplitId == existingSplit.Id && li.Id == itemId).ExecuteDeleteAsync();
        }

        /// <summary>
        /// in the event that we want to make things simple, we can just clear existing items, input the new total combined value, and have that as ONE entry
        /// </summary>
        /// <param name="existingSplitId"></param>
        /// <param name="context"></param>
        /// <returns> complete if successful, else throws an error</returns>
        public static async Task SetLootsplitItemValueAsyncDB(int existingSplitId, LootBotDbContext context, LootItem combinedValue)
        {
            Lootsplit? existingSplit = await RetrieveSingleLootsplitAsyncDB(context, existingSplitId);
            if (existingSplit == null) throw new ArgumentNullException(nameof(existingSplit));
            if (existingSplit.Status == LootsplitStatus.Completed) throw new InaccessableActionException("Cannot modify a completed split. Please make a new split.");
            await context.LootItems.Where(li => li.LootsplitId == existingSplit.Id).ExecuteDeleteAsync();
            //combine and set cause these may not be correct
            combinedValue.LootsplitId = existingSplit.Id;
            combinedValue.Lootsplit = existingSplit;

            context.LootItems.Add(combinedValue);
        }

        #endregion

        #region Calculations
        public async Task<CalculationEntity> CompleteSplitAndCalculatePayout(int existingSplitId, IUser user, ulong guildId)
        {
            //we will use the user to condfirm they are in the split, if not then follow up with a not allowed action excerption i think
            await using LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();
            Player playerEntry = await FindOrGetPlayerAsyncDB(_context, user, guildId);

            //confirm user is in split first
            bool exists = await DoesPlayerExistInLootSplitAsyncDB(_context, existingSplitId, playerEntry);
            if (!exists) throw new UserNotFoundException("User does not exist in the lootsplit entered");

            //retrieve, convert and seal, the return
            Lootsplit? split = await RetrieveSingleLootsplitAsyncDB(_context, existingSplitId) ?? throw new Exception("lootsplit does not exist. if this error is called, please notify developer");
            CalculationEntity entity = split.CreateFromLootsplit();
            _context.Calculations.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        #endregion

        #region Db interaction
        protected static async Task<bool> DoesPlayerExistInLootSplitAsyncDB(LootBotDbContext context, int existingSplitId, Player playerEntry)
        {
            return await context.LootsplitParticipants.AnyAsync(p => p.LootsplitId == existingSplitId && p.PlayerId == playerEntry.Id);
        }

        //can return null!!!
        protected static async Task<Lootsplit?> RetrieveSingleLootsplitAsyncDB(LootBotDbContext context, int existingSplitId)
        {
            return await context.LootSplit
                .Where(ls => ls.Status == LootsplitStatus.Open)//only get open ones
                .Include(ls => ls.Participants)
                .Include(ls => ls.Items)
                .FirstOrDefaultAsync(ls => ls.Id == existingSplitId);
        }

        protected async Task SaveParticipantToSplitAsyncDB(int existingSplitId, Player player)
        {
            await using LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();

            bool splitExists = await _context.LootSplit.AnyAsync(ls => ls.Id == existingSplitId);
            if (!splitExists)
                throw new InvalidOperationException($"Attempting to save to a lootsplit (id - {existingSplitId}) that does not exist");

            if (await DoesPlayerExistInLootSplitAsyncDB(_context, existingSplitId, player))
                throw new InvalidOperationException($"Player {player.Id} is already a participant in lootsplit {existingSplitId}");


            var participant = new LootsplitParticipant
            {
                LootsplitId = existingSplitId,
                PlayerId = player.Id,
            };
            _context.LootsplitParticipants.Add(participant);
            await _context.SaveChangesAsync();
        }
        protected async Task RemoveParticipantFromSplitAsyncDB(int existingSplitId, Player player)
        {
            await using LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();

            var lsp = await _context.LootsplitParticipants
                .FirstOrDefaultAsync(p => p.LootsplitId == existingSplitId && p.PlayerId == player.Id)
                ?? throw new InvalidOperationException($"Player {player.Id} is not a participant in lootsplit {existingSplitId}");

            _context.LootsplitParticipants.Remove(lsp);

            await _context.SaveChangesAsync();
        }
        //name should be self explanatory, sometimes we know or dont know if they exist.
        public async Task<Player> FetchOrCreatePlayerAsyncDB(LootBotDbContext context, LootSplitUserContextDTO userContext)
        {
            //check if the player is already saved inside the database disk table rows
            var player = await context.Players
                .FirstOrDefaultAsync(p => p.DiscordPlayerId == userContext.DiscordUserId);

            if (player != null)
            {
                return player;
            }

            //if they do not exist anywhere, initialize
            player = new Player
            {
                DiscordPlayerId = userContext.DiscordUserId,
                DiscordName = userContext.DiscordUsername,
                Name = userContext.GlobalName ?? userContext.DiscordUsername,
                CreatedAt = DateTime.UtcNow
            };

            context.Players.Add(player);
            return player;
        }

        public async Task<int> GetSplitIdFromSessionNameAsyncDB(string sessionName)
        {
            await using LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();

            Lootsplit? split = await _context.LootSplit.FirstOrDefaultAsync(ls => ls.SessionName == sessionName) ?? throw new InvalidDataException("Session name not associated with any split");
            return split.Id;
        }

        public async Task<Player> FindOrGetPlayerAsyncDB(LootBotDbContext context, IUser user, ulong guildId)
        {
            LootSplitUserContextDTO userContext = CreateUserContextDto(user, guildId);
            Player playerEntry = await FetchOrCreatePlayerAsyncDB(context, userContext);
            await context.SaveChangesAsync();
            return playerEntry;
        }

        //can return null!!!
        protected static async Task<CalculationEntity?> RetrieveAndCompleteSplitAsyncDB(LootBotDbContext context, int splitId)
        {
            //get the lootsplit
            Lootsplit? split = await context.LootSplit.FindAsync(splitId);
            //save
            if (split == null) return null;
            return split.CreateFromLootsplit();
        }

        protected async Task<ICollection<Lootsplit>> FindAllSplitsForUserAsyncDB(int PlayerId, LootBotDbContext _context)
        {
            //get all 
            ICollection<Lootsplit> playerSplits = await _context.LootsplitParticipants
            .Where(lp => lp.Player.Id == PlayerId)
            .Select(lp => lp.Lootsplit)
            .ToListAsync();

            return playerSplits;
        }

        public async Task<ICollection<Lootsplit>> FindRunningSplitsForUserAsyncDB(int playerId)
        {
            await using LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();

            ICollection<Lootsplit> PlayerSplits = await FindAllSplitsForUserAsyncDB(playerId, _context);
            ICollection<Lootsplit> activePlayerSplits = PlayerSplits.Where(ls => ls.Status == LootsplitStatus.Open).ToList();
            return activePlayerSplits;
        }

        public async Task<ICollection<Lootsplit>> FindCompletedSplitsForUserAsyncBD(int playerId)
        {
            await using LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();

            return await _context.LootsplitParticipants
            .Where(lp => lp.PlayerId == playerId && lp.Lootsplit.Status == LootsplitStatus.Completed)
            .Select(lp => lp.Lootsplit)
            .ToListAsync();
        }

        //can return null!!!
        public async Task<Player?> GetPlayerDBInfo(IUser user)
        {
            await using LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();
            Player? player = await _context.Players.Where(p => p.DiscordPlayerId == user.Id).FirstOrDefaultAsync();
            return player;//zero is gonna be undefined i think
        }
        #endregion

        //supporting methods
        //this can have the context factory pulled out
        public LootSplitUserContextDTO CreateUserContextDto(IUser user, ulong guildId)
        {
            return new LootSplitUserContextDTO(
                GuildId: guildId,
                DiscordUserId: user.Id,
                DiscordUsername: user.Username,
                GlobalName: user.GlobalName
            );
        }
    }
}