using AlbionLootBot.Database;
using AlbionLootBot.Dtos;
using AlbionLootBot.Models;
using Discord;
using Microsoft.EntityFrameworkCore;

namespace AlbionLootBot.Services
{
    public class PlayerManagementService(IDbContextFactory<LootBotDbContext> _contextFactory)
    {
        private readonly IDbContextFactory<LootBotDbContext> _contextFactory = _contextFactory;

        //can return null!!!
        public async Task<Player?> GetPlayerDBInfo(IUser user)
        {
            await using LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();
            Player? player = await _context.Players.Where(p => p.DiscordPlayerId == user.Id).FirstOrDefaultAsync();
            return player;//zero is gonna be undefined i think
        }

        public async Task<Player> FindOrGetPlayerAsyncDB(LootBotDbContext context, IUser user, ulong guildId)
        {
            LootSplitUserContextDTO userContext = CreateUserContextDto(user, guildId);
            Player playerEntry = await FetchOrCreatePlayerAsyncDB(context, userContext);
            await context.SaveChangesAsync();
            return playerEntry;
        }

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

        public LootSplitUserContextDTO CreateUserContextDto(IUser user, ulong guildId)
        {
            return new LootSplitUserContextDTO(
                GuildId: guildId,
                DiscordUserId: user.Id,
                DiscordUsername: user.Username,
                GlobalName: user.GlobalName
            );
        }

        public static async Task<bool> DoesPlayerExistInLootSplitAsyncDB(LootBotDbContext context, int existingSplitId, Player playerEntry)
        {
            return await context.LootsplitParticipants.AnyAsync(p => p.LootsplitId == existingSplitId && p.PlayerId == playerEntry.Id);
        }

    }

}