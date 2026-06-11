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
    /// Best-effort parse of an address line into city + postcode. The postcode is found by
    /// scanning the whole line (the site is inconsistent about commas, e.g.
    /// "…High Street, Harlesden, London NW10 4NE"); the city is taken from the trailing
    /// comma-segment with the postcode removed. Degrades gracefully: an unrecognised line
    /// still yields a valid Address holding the raw text, so a miss never loses data or throws.
    /// </summary>
    public static Address FromRaw(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return Empty;

        var trimmed = raw.Trim();
        var parts = trimmed.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var match = PostcodeRegex().Match(trimmed);
        string? postcode = match.Success ? match.Value : null;

        string? city = null;
        if (parts.Length > 0)
        {
            var tail = parts[^1];
            if (postcode is not null && tail.Contains(postcode, StringComparison.OrdinalIgnoreCase))
            {
                var withoutPostcode = tail.Replace(postcode, string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
                city = withoutPostcode.Length > 0 ? withoutPostcode
                     : parts.Length >= 2 ? parts[^2]
                     : null;
            }
            else
            {
                city = tail;
            }
        }

        return new Address(trimmed, city, postcode);
    }

    // UK postcode, found anywhere in the line, tolerant of the optional internal space
    // (e.g. "EC1N 2PZ", "NW10 4NE", "SW1H0HW").
    [GeneratedRegex(@"[A-Za-z]{1,2}\d[A-Za-z\d]?\s*\d[A-Za-z]{2}", RegexOptions.Compiled)]
    private static partial Regex PostcodeRegex();
}
