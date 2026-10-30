using AlbionLootBot.Models;
using Microsoft.EntityFrameworkCore;

namespace AlbionLootBot.Database;
public class LootBotDbContext(DbContextOptions<LootBotDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Lootsplit> LootSplit => Set<Lootsplit>();
    public DbSet<LootsplitParticipant> LootsplitParticipants => Set<LootsplitParticipant>();
    public DbSet<LootItem> LootItems => Set<LootItem>();
    public DbSet<CalculationEntity> Calculations => Set<CalculationEntity>();
    public DbSet<AOGuildInfo> AOGuilds => Set<AOGuildInfo>();
    public DbSet<CalculationParticipantPayout> ParticipantPayouts => Set<CalculationParticipantPayout>();
    public DbSet<GuildTaxEntry> GuildTaxEntries => Set<GuildTaxEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- Player ----
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasIndex(p => p.Name).IsUnique();
            entity.HasIndex(p => p.DiscordPlayerId).IsUnique();
        });

        // ---- Lootsplit ----
        modelBuilder.Entity<Lootsplit>(entity =>
        {
            entity.Property(l => l.Status)
                .HasConversion<string>()   // store enum as text, not int
                .HasMaxLength(20);
        });

        // ---- LootsplitParticipant (join table) ----
        modelBuilder.Entity<LootsplitParticipant>(entity =>
        {
            entity.HasIndex(lp => new { lp.LootsplitId, lp.PlayerId }).IsUnique();

            entity.HasOne(lp => lp.Lootsplit)
                .WithMany(l => l.Participants)
                .HasForeignKey(lp => lp.LootsplitId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(lp => lp.Player)
                .WithMany(p => p.Participations)
                .HasForeignKey(lp => lp.PlayerId)
                .OnDelete(DeleteBehavior.Restrict); // don't wipe player history if split is deleted
        });

        // ---- LootItem ----
        modelBuilder.Entity<LootItem>(entity =>
        {
            entity.Property(li => li.TaxableEntity)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(li => li.Lootsplit)
                .WithMany(l => l.Items)
                .HasForeignKey(li => li.LootsplitId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- CalculationEntity ----
        modelBuilder.Entity<CalculationEntity>(entity =>
        {
            entity.HasIndex(c => c.Id).IsUnique();

            entity.HasOne(c => c.Lootsplit)
                .WithMany() // no back-reference on Lootsplit; add one if you want history navigation
                .HasForeignKey(c => c.LootsplitId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AOGuildInfo>()
        .HasIndex(g => g.DiscordGuildId)
    .IsUnique();

        modelBuilder.Entity<GuildTaxEntry>()
            .HasIndex(e => new { e.AOGuildInfoId, e.PaidAt });

        // deleting a guild must never wipe the ledger
        modelBuilder.Entity<GuildTaxEntry>()
            .HasOne(e => e.AOGuildInfo)
            .WithMany()
            .HasForeignKey(e => e.AOGuildInfoId)
            .OnDelete(DeleteBehavior.Restrict);

        // deleting a payout (via its calculation) removes its tax entries
        modelBuilder.Entity<GuildTaxEntry>()
            .HasOne(e => e.ParticipantPayout)
            .WithMany()
            .HasForeignKey(e => e.ParticipantPayoutId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Player>()
            .HasOne(p => p.AOGuildInfo)
            .WithMany()
            .HasForeignKey(p => p.AOGuildInfoId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CalculationParticipantPayout>()
            .HasOne(p => p.AOGuildInfo)
            .WithMany()
            .HasForeignKey(p => p.AOGuildInfoId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CalculationParticipantPayout>()
            .HasOne(p => p.Player)
            .WithMany()
            .HasForeignKey(p => p.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
