using AlbionLootBot.Models;
using Microsoft.EntityFrameworkCore;

namespace AlbionLootBot.Database;
public class LootBotDbContext(DbContextOptions<LootBotDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players { get; set; }
    public DbSet<Lootsplit> Lootsplits { get; set; }
    public DbSet<LootsplitParticipant> LootsplitParticipants { get; set; }
    public DbSet<LootItem> LootItems { get; set; }
    public DbSet<CalculationEntity> Calculations { get; set; }

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
            entity.Property(li => li.taxableEntity)
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
            entity.HasIndex(c => c.CalculationId).IsUnique();

            entity.HasOne(c => c.Lootsplit)
                .WithMany() // no back-reference on Lootsplit; add one if you want history navigation
                .HasForeignKey(c => c.LootsplitId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
