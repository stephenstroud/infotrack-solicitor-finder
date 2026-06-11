using Microsoft.EntityFrameworkCore;

namespace InfoTrack.SolicitorFinder.Infrastructure.Persistence;

/// <summary>
/// EF Core context holding search history. Configured for the in-memory provider (see
/// Infrastructure DI), so it needs no connection string or migrations to run — swapping to
/// SQL Server/Postgres is just a different <c>UseXxx</c> call.
/// </summary>
internal sealed class SolicitorFinderDbContext(DbContextOptions<SolicitorFinderDbContext> options)
    : DbContext(options)
{
    public DbSet<SnapshotRecord> Snapshots => Set<SnapshotRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SnapshotRecord>(snapshot =>
        {
            snapshot.HasKey(s => s.Id);
            // Firms have no identity of their own — they belong to the snapshot.
            snapshot.OwnsMany(s => s.Firms);
        });
    }
}
