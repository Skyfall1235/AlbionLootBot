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
        //change guild info
        public async Task ChangeGuildInfo()
        {

        }
        //change guild tax rate
        public async Task SetGuildTaxRate(GuildTaxType taxType, long newValue)
        {
            //get db item, set vbalue, save
        }
        //confirm user and role of guild?
        //drop info about bot, like basic commands, and usage
        public async Task DisplayBotInfo()
        {

        }
        //report bugs command
        public async Task ReportBug()
        {

        }

    }
}