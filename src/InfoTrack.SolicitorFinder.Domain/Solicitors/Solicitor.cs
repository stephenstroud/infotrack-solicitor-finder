namespace InfoTrack.SolicitorFinder.Domain.Solicitors;

/// <summary>
/// A solicitor firm found for a given location. This is an *entity*, not a value
/// object: two firms with the same details are still distinguished by identity
/// (<see cref="Key"/>), which is what lets us de-duplicate within a run and detect
/// newly-appearing firms between runs.
/// </summary>
public sealed class Solicitor
{
    public string Name { get; }
    public Location Location { get; }
    public Address Address { get; }
    public ContactDetails Contact { get; }
    public string? Description { get; }

    /// <summary>The firm's review standing, when the listing carries one; otherwise null.</summary>
    public Rating? Rating { get; }

    public Solicitor(
        string name,
        Location location,
        Address? address = null,
        ContactDetails? contact = null,
        string? description = null,
        Rating? rating = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Solicitor name is required.", nameof(name));

        Name = name.Trim();
        Location = location ?? throw new ArgumentNullException(nameof(location));
        Address = address ?? Address.Empty;
        Contact = contact ?? ContactDetails.Empty;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Rating = rating;
    }

    /// <summary>
    /// Stable identity for de-duplication and new-firm detection across scrape runs:
    /// a firm is "the same" when its name and location match, ignoring case and spacing.
    /// </summary>
    public string Key => $"{Canonical(Name)}|{Location.Slug}";

    private static string Canonical(string value) =>
        string.Join(' ', value.ToLowerInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
