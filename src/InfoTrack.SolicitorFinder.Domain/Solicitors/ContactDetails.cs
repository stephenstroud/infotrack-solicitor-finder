namespace InfoTrack.SolicitorFinder.Domain.Solicitors;

/// <summary>
/// How a firm can be reached. Value object: immutable, equal by its three channels.
/// Exposes the small amount of behaviour the report needs (which channels exist,
/// and how "contactable" the firm is) so that logic lives with the data, not in the UI.
/// </summary>
public sealed record ContactDetails
{
    public string? Phone { get; }
    public string? Email { get; }
    public string? Website { get; }

    public ContactDetails(string? phone = null, string? email = null, string? website = null)
    {
        Phone = Clean(phone);
        Email = Clean(email);
        Website = Clean(website);
    }

    public static readonly ContactDetails Empty = new();

    public bool HasPhone => Phone is not null;
    public bool HasEmail => Email is not null;
    public bool HasWebsite => Website is not null;

    /// <summary>Number of channels present (0–3). Drives the "contactability" insight.</summary>
    public int ChannelCount => (HasPhone ? 1 : 0) + (HasEmail ? 1 : 0) + (HasWebsite ? 1 : 0);

    /// <summary>True when we have at least one way to reach the firm — i.e. it's a usable lead.</summary>
    public bool IsReachable => ChannelCount > 0;

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
