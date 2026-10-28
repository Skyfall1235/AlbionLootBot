using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlbionLootBot.Models;

public enum GuildTaxType { Silver, ItemValue, Resource }

//for tracking payouts
[Table("ParticipantPayout")]
public class CalculationParticipantPayout
{
    //calculations
    [Key]
    public int Id { get; set; }
    [ForeignKey(nameof(CalculationEntity))]
    public Guid CalculationEntityId { get; set; }
    public CalculationEntity CalculationEntity { get; set; } = null!;
    //AO guild info
    public int? AOGuildInfoId { get; set; }
    public AOGuildInfo? AOGuildInfo { get; set; }
    //player
    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    //payout
    public long GrossShare { get; set; }
    public long NetShare { get; set; }
}

//for keeping better track of guilds
[Table("AOGuildInfo")]
public class AOGuildInfo(string albionOnlineGuildName, ulong discordGuildId, decimal silverTaxRate, decimal itemValueTaxRate, decimal resourceTaxRate)
{
    [Key]
    public int Id { get; set; }
    //albion online guild name
    public string AlbionOnlineGuildName { get; set; } = albionOnlineGuildName;
    //personal discord server id?
    public ulong DiscordGuildId { get; set; } = discordGuildId;
    //silver tax rate, resource tax rate, loot tax rate
    [Column(TypeName = "decimal(5,2)")]
    public decimal SilverTaxRate { get; set; } = silverTaxRate;
    [Column(TypeName = "decimal(5,2)")]
    public decimal ItemValueTaxRate { get; set; } = itemValueTaxRate;
    [Column(TypeName = "decimal(5,2)")]
    public decimal ResourceTaxRate { get; set; } = resourceTaxRate;
    //role id for admin? maybe list of roles?

}

[Table("GuildTaxEntry")]
public class GuildTaxEntry
{
    [Key]
    public int Id { get; set; }

    public int ParticipantPayoutId { get; set; }
    public CalculationParticipantPayout ParticipantPayout { get; set; } = null!;

    public int AOGuildInfoId { get; set; }
    public AOGuildInfo AOGuildInfo { get; set; } = null!;

    public GuildTaxType TaxType { get; set; }
    public long TaxableAmountSilver { get; set; }
    public decimal RateApplied { get; set; }
    public long TaxAmountSilver { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
}


[Table("Players")]
public class Player
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";
    [MaxLength(100)]
    public string DiscordName { get; set; } = "";
    public ulong DiscordPlayerId { get; set; }

    public int? AOGuildInfoId { get; set; }   // null = guildless / unknown
    public AOGuildInfo? AOGuildInfo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<LootsplitParticipant> Participations { get; set; } = [];
}
//role 