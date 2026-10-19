using AlbionLootBot.Models;

namespace AlbionLootBot.Dtos;

public record LootSplitUserContextDTO(
    ulong GuildId,
    ulong DiscordUserId,
    string DiscordUsername,
    string? GlobalName
);
public record LootItemDTO(
    TaxableEntity TaxableEntity,
    long ValueSilver
);
