namespace AlbionLootBot.Models;

public record LootSplitUserContext(
    ulong GuildId,
    ulong DiscordUserId,
    string DiscordUsername,
    string? GlobalName
);
