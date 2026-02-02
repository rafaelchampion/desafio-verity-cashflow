using MassTransit;
using Microsoft.EntityFrameworkCore;
using Verity.CashFlow.API.Domain;

namespace Verity.CashFlow.API.Infrastructure.Data;

public class CashFlowDbContext : DbContext
{
    public CashFlowDbContext(DbContextOptions<CashFlowDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Description).HasMaxLength(255).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Type).HasConversion<int>();
        });
    }
}
