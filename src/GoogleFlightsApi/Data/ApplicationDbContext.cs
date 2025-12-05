using GoogleFlightsApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GoogleFlightsApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ClientInfo> ClientInfos { get; set; }
    public DbSet<SearchHistory> SearchHistories { get; set; }
    public DbSet<FlightResult> FlightResults { get; set; }
    public DbSet<RequestLog> RequestLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ClientInfo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.HasIndex(e => e.IpAddress);
        });

        modelBuilder.Entity<SearchHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Origin).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Destination).IsRequired().HasMaxLength(10);
            entity.Property(e => e.CabinClass).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SearchUrl).HasMaxLength(2000);
            
            entity.HasOne(e => e.ClientInfo)
                .WithMany(c => c.Searches)
                .HasForeignKey(e => e.ClientInfoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SearchedAt);
            entity.HasIndex(e => new { e.Origin, e.Destination });
        });

        modelBuilder.Entity<FlightResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Airline).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FlightNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.DepartureTime).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ArrivalTime).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Duration).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(10);

            entity.HasOne(e => e.SearchHistory)
                .WithMany(s => s.FlightResults)
                .HasForeignKey(e => e.SearchHistoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RequestLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Method).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Path).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);

            entity.HasOne(e => e.ClientInfo)
                .WithMany(c => c.RequestLogs)
                .HasForeignKey(e => e.ClientInfoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.Timestamp);
        });
    }
}
