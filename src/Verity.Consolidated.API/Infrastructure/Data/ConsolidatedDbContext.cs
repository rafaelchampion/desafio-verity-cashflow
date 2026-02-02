using MassTransit;
using Microsoft.EntityFrameworkCore;
using Verity.Consolidated.API.Domain;

namespace Verity.Consolidated.API.Infrastructure.Data;

public class ConsolidatedDbContext : DbContext
{
    public ConsolidatedDbContext(DbContextOptions<ConsolidatedDbContext> options) : base(options) { }

    public DbSet<DailyBalance> DailyBalances { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.Entity<DailyBalance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Date).IsUnique();
            entity.Property(e => e.TotalCredit).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalDebit).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ClosingBalance).HasColumnType("decimal(18,2)");
            entity.UseXminAsConcurrencyToken();
        });
    }
}
