using AlbionLootBot.Database;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

namespace AlbionLootBot.Services;
public interface IConfigService
{
    TaxConfig? GetConfigFromContext(SocketInteractionContext context);
    void SaveToFile();
    void UpdateConfig(ulong guildId, string? name = "", ulong? channelId = null);
    void UpdateTaxRate(ulong guildId, int newTaxRate);
}

public class NewConfigService
{
    private readonly IDbContextFactory<LootBotDbContext> _contextFactory;

    //TaxConfig? GetConfigFromContext(SocketInteractionContext interactionContext, LootBotDbContext context)
    //{

    //}

    void UpdateTaxRate(ulong guildId, int newTaxRate)
    {

    }
}
