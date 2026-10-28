using AlbionLootBot.Database;
using AlbionLootBot.Models;
using Microsoft.EntityFrameworkCore;
namespace AlbionLootBot.Services
{

    public class AdminService
    {
        public AdminService()
        {

        }
        //add db context
        private readonly IDbContextFactory<LootBotDbContext> _contextFactory;

        //create guild
        public async Task CreateGuild(string AOGuildName, ulong DiscordGuildId)
        {
            await using LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();
            AOGuildInfo newGuildInfo = new AOGuildInfo(AOGuildName, DiscordGuildId, 0, 0, 0);
            _context.AOGuilds.Add(newGuildInfo);
            await _context.SaveChangesAsync();
        }

        //change guild tax rate
        public async Task SetGuildTaxRate(string AOGuildName, GuildTaxType taxType, long newValue)
        {
            //get db item, set vbalue, save
            await using LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();
            AOGuildInfo? existingGuild = _context.AOGuilds.FirstOrDefault(aog => aog.AlbionOnlineGuildName == AOGuildName);
        }

        //VIEW guild tax rate
        public async Task ViewGuildTaxRate(string AOGuildName)
        {
            //get db item, set vbalue, save
            await using LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();
            AOGuildInfo? existingGuild = _context.AOGuilds.FirstOrDefault(aog => aog.AlbionOnlineGuildName == AOGuildName);
        }

        //confirm user and role of guild?



        //report bugs command
        public async Task ReportBug()
        {
            //this should go... somewhere
        }
        //audit lot of a split
        public async Task<List<LootItem>?> AuditSplit(string sessionName)
        {
            await using LootBotDbContext? _context = await _contextFactory.CreateDbContextAsync();
            List<LootItem>? items = await _context.LootItems
                .Where(item => item.Lootsplit.SessionName == sessionName)
                .Include(item => item.Contributor)
                .ToListAsync();
            return items;
        }

    }
}