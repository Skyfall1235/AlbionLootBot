using Discord.Interactions;

namespace AlbionLootBot.Services;
public interface IConfigService
{
    TaxConfig? GetConfigFromContext(SocketInteractionContext context);
    void SaveToFile();
    void UpdateConfig(ulong guildId, string? name = "", ulong? channelId = null);
    void UpdateTaxRate(ulong guildId, int newTaxRate);
}