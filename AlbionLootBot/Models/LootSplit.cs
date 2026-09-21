using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlbionLootBot.Models;

public enum LootsplitStatus
{
    Open,
    Completed
}
public enum TaxableEntity
{
    SilverBag, Item
}

[Table("Calculations")]
public class CalculationEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    //link back to original lootsplit

    [ForeignKey(nameof(Lootsplit))]
    public int LootsplitId { get; set; }
    public Lootsplit Lootsplit { get; set; } = null!;

    [Required]
    [MaxLength(64)]
    public string CalculationId { get; set; } = string.Empty;

    // Input Parameters
    [Column(TypeName = "decimal(18,2)")]
    public decimal SilverBagTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ItemTotal { get; set; }
    public int RecipientCount { get; set; }
    [Column(TypeName = "decimal(5,2)")]
    public decimal TaxRatePercent { get; set; }

    // Tax Summary
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalTaxAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal NetTotalAfterTax { get; set; }

    // Distribution (Per Person)
    [Column(TypeName = "decimal(18,2)")]
    public decimal PerPersonGrossShare { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal PerPersonNetShare { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Remainder { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<LootsplitParticipant> Participations { get; set; } = new List<LootsplitParticipant>();
}

[Table("LootSplit")]
public class Lootsplit
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string SessionName { get; set; }

    public LootsplitStatus Status { get; set; } = LootsplitStatus.Open;

    [Column(TypeName = "decimal(5,4)")]
    public decimal TaxRate { get; set; } = 0.05m;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public ICollection<LootsplitParticipant> Participants { get; set; } = new List<LootsplitParticipant>();
    public ICollection<LootItem> Items { get; set; } = new List<LootItem>();

    // Derived / convenience — not mapped to DB
    [NotMapped]
    public long TotalSilver => Items?.Sum(i => i.ValueSilver) ?? 0;

    [NotMapped]
    public long TaxedSilver => (long)(TotalSilver * (1 - TaxRate));

    [NotMapped]
    public decimal PayoutPerPerson =>
        Participants != null && Participants.Count > 0
            ? TaxedSilver / (decimal)Participants.Count
            : 0;
}

public static class LootsplitMethodExtensions
{
    public static void SetLootsplitComplete(this Lootsplit ls)
    {
        ls.CompletedAt = DateTime.UtcNow;
        ls.Status = LootsplitStatus.Completed;
    }
}


[Table("Participants")]
public class LootsplitParticipant
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Lootsplit))]
    public int LootsplitId { get; set; }
    public Lootsplit Lootsplit { get; set; }

    [ForeignKey(nameof(Player))]
    public int PlayerId { get; set; }
    public Player Player { get; set; }
}

[Table("LootEntity")]
public class LootItem
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Lootsplit))]
    public int LootsplitId { get; set; }
    public Lootsplit Lootsplit { get; set; }

    [Required]
    public TaxableEntity taxableEntity { get; set; } = TaxableEntity.SilverBag;

    [Required]
    public long ValueSilver { get; set; } = 0;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}