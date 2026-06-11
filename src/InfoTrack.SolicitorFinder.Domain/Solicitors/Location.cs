namespace InfoTrack.SolicitorFinder.Domain.Solicitors;

/// <summary>
/// A searchable town or city (e.g. "London", "Milton Keynes").
/// Value object: two locations are equal when their normalised slug matches,
/// so casing and surrounding whitespace never cause duplicates.
/// </summary>
public sealed record Location
{
    public string Name { get; }

    public Location(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Location name is required.", nameof(name));

        Name = name.Trim();
    }

    /// <summary>
    /// URL/identity-friendly form: lower-cased with spaces collapsed to hyphens.
    /// e.g. "London" -> "london", "Milton Keynes" -> "milton-keynes".
    /// The site-specific URL is built from this by the scraper, keeping the
    /// domain free of any knowledge about solicitors.com.
    /// </summary>
    public string Slug => string.Join('-',
        Name.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));

    // Value equality on the slug rather than the raw name.
    public bool Equals(Location? other) => other is not null && Slug == other.Slug;
    public override int GetHashCode() => Slug.GetHashCode();

    public override string ToString() => Name;
}
