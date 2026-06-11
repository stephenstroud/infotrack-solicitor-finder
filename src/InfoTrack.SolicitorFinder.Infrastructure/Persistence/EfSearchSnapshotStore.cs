using InfoTrack.SolicitorFinder.Application.Abstractions;
using InfoTrack.SolicitorFinder.Domain.Search;
using InfoTrack.SolicitorFinder.Domain.Solicitors;
using Microsoft.EntityFrameworkCore;

namespace InfoTrack.SolicitorFinder.Infrastructure.Persistence;

/// <summary>
/// EF-backed <see cref="ISearchSnapshotStore"/>. Translates between the rich domain snapshot
/// and the flat persistence records, so neither layer leaks into the other.
/// </summary>
internal sealed class EfSearchSnapshotStore(SolicitorFinderDbContext db) : ISearchSnapshotStore
{
    private const char LocationDelimiter = '|';

    public async Task SaveAsync(SearchSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        db.Snapshots.Add(ToRecord(snapshot));
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<SearchSnapshot?> GetLatestAsync(CancellationToken cancellationToken = default)
    {
        var record = await db.Snapshots
            .AsNoTracking()
            .OrderByDescending(s => s.CapturedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return record is null ? null : ToDomain(record);
    }

    public async Task<IReadOnlyList<SearchSnapshot>> GetHistoryAsync(
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var records = await db.Snapshots
            .AsNoTracking()
            .OrderByDescending(s => s.CapturedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return records.Select(ToDomain).ToList();
    }

    private static SnapshotRecord ToRecord(SearchSnapshot snapshot) => new()
    {
        Id = snapshot.Id,
        CapturedAt = snapshot.CapturedAt,
        Locations = string.Join(LocationDelimiter, snapshot.Locations.Select(l => l.Name)),
        Firms = snapshot.Solicitors.Select(ToFirmRecord).ToList()
    };

    private static SolicitorRecord ToFirmRecord(Solicitor s) => new()
    {
        Name = s.Name,
        LocationName = s.Location.Name,
        AddressRaw = s.Address.Raw,
        City = s.Address.City,
        Postcode = s.Address.Postcode,
        Phone = s.Contact.Phone,
        Email = s.Contact.Email,
        Website = s.Contact.Website,
        Description = s.Description,
        Stars = s.Rating?.Stars,
        ReviewCount = s.Rating?.ReviewCount
    };

    private static SearchSnapshot ToDomain(SnapshotRecord record)
    {
        var locations = record.Locations
            .Split(LocationDelimiter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(name => new Location(name))
            .ToList();

        var firms = record.Firms.Select(ToDomainFirm).ToList();

        return new SearchSnapshot(record.Id, record.CapturedAt, locations, firms);
    }

    private static Solicitor ToDomainFirm(SolicitorRecord r) => new(
        r.Name,
        new Location(r.LocationName),
        new Address(r.AddressRaw, r.City, r.Postcode),
        new ContactDetails(r.Phone, r.Email, r.Website),
        r.Description,
        r.Stars is { } stars ? new Rating(stars, r.ReviewCount ?? 0) : null);
}
