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

    //we can expand this outward by logging what everyones payouts are in a new table :)
    // Distribution (Per Person)

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
    public ICollection<LootsplitParticipant> Participations { get; set; } = [];
}

[Table("LootSplit")]
public class Lootsplit
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string SessionName { get; set; }

    public LootsplitStatus Status { get; set; } = LootsplitStatus.Open;

    [Column(TypeName = "decimal(5,4)")]
    public decimal TaxRate { get; set; } = 0.05m;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public ICollection<LootsplitParticipant> Participants { get; set; } = [];
    public ICollection<LootItem> Items { get; set; } = [];

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

    public static CalculationEntity CreateFromLootsplit(this Lootsplit split)
    {
        var totalSilver = split.TotalSilver;
        var taxedSilver = split.TaxedSilver;
        var participantCount = split.Participants?.Count ?? 0;
        decimal perPersonNet = participantCount > 0 ? (decimal)taxedSilver / participantCount : 0;

        // Calculate total tax amount extracted
        long totalTaxAmount = totalSilver - taxedSilver;

        split.SetLootsplitComplete();

        return new CalculationEntity
        {
            LootsplitId = split.Id,
            SilverBagTotal = 0, // Fill this if you separate silver bags from items, otherwise map TotalSilver to ItemTotal
            ItemTotal = totalSilver,
            RecipientCount = participantCount,
            TaxRatePercent = split.TaxRate * 100m, // If you store it as a percentage (e.g., 5.00 for 5%)

            TotalTaxAmount = totalTaxAmount,
            NetTotalAfterTax = taxedSilver,

            PerPersonNetShare = perPersonNet,
            Remainder = taxedSilver - (Math.Floor(perPersonNet) * participantCount), // Tracks any leftover fractional silver decimals
            CreatedAtUtc = DateTime.UtcNow
        };

    }
}


[Table("Participants")]
public class LootsplitParticipant
{
    //autopop by EFC
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Lootsplit))]
    public int LootsplitId { get; set; }

    [ForeignKey(nameof(Player))]
    public int PlayerId { get; set; }

    //actual data we populate

    [Required]
    public Player Player { get; set; } = null!;

    [Required]
    public Lootsplit Lootsplit { get; set; } = null!;
}


[Table("LootEntity")]
public class LootItem
{
    public LootItem(int lootsplitId, TaxableEntity taxableEntity, long valueSilver)
    {
        LootsplitId = lootsplitId;
        TaxableEntity = taxableEntity;
        ValueSilver = valueSilver;
    }

    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Lootsplit))]
    public int LootsplitId { get; set; }

    public Lootsplit Lootsplit { get; set; } = null!;

    public TaxableEntity TaxableEntity { get; set; }

    public long ValueSilver { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}

public class CalculationParticipantPayout
{
    [Key]
    public int Id { get; set; }
    [ForeignKey(nameof(CalculationEntity))]
    public Guid CalculationEntityId { get; set; }
    public CalculationEntity CalculationEntity { get; set; } = null!;
    public required Player playerId { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal GrossShare { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal NetShare { get; set; }
}