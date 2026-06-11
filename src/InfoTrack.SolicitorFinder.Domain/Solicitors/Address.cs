using System.Text.RegularExpressions;

namespace InfoTrack.SolicitorFinder.Domain.Solicitors;

/// <summary>
/// A firm's postal address. Value object that keeps the original line verbatim but
/// also exposes the city and postcode it could parse out, so the report can group
/// firms geographically without re-parsing strings everywhere.
/// </summary>
public sealed partial record Address
{
    public string Raw { get; }
    public string? City { get; }
    public string? Postcode { get; }

    public Address(string raw, string? city = null, string? postcode = null)
    {
        Raw = (raw ?? string.Empty).Trim();
        City = string.IsNullOrWhiteSpace(city) ? null : city.Trim();
        Postcode = string.IsNullOrWhiteSpace(postcode) ? null : postcode.Trim().ToUpperInvariant();
    }

    public static readonly Address Empty = new(string.Empty);

    public bool HasValue => Raw.Length > 0;

    /// <summary>
    /// The postcode "area" — the leading letters of the outward code
    /// (e.g. "SW1H 0HW" -> "SW"). Useful for clustering firms by district.
    /// </summary>
    public string? PostcodeArea
    {
        get
        {
            if (Postcode is null) return null;
            var letters = new string(Postcode.TakeWhile(char.IsLetter).ToArray());
            return letters.Length > 0 ? letters : null;
        }
    }

    /// <summary>
    /// Best-effort parse of a comma-delimited address line into city + postcode.
    /// Degrades gracefully: an unrecognised line still yields a valid Address holding
    /// the raw text, so a parsing miss never loses data or throws.
    /// </summary>
    public static Address FromRaw(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return Empty;

        var parts = raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return new Address(raw);

        var last = parts[^1];
        if (PostcodeRegex().IsMatch(last))
        {
            var city = parts.Length >= 2 ? parts[^2] : null;
            return new Address(raw, city, last);
        }

        return new Address(raw, last);
    }

    // UK postcode, tolerant of the optional internal space (e.g. "EC1N 2PZ", "SW1H0HW").
    [GeneratedRegex(@"^[A-Za-z]{1,2}\d[A-Za-z\d]?\s*\d[A-Za-z]{2}$", RegexOptions.Compiled)]
    private static partial Regex PostcodeRegex();
}
