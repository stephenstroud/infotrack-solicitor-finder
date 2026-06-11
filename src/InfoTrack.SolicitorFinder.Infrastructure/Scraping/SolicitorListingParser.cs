using InfoTrack.SolicitorFinder.Domain.Solicitors;

namespace InfoTrack.SolicitorFinder.Infrastructure.Scraping;

/// <summary>
/// Turns a solicitors.com results page into <see cref="Solicitor"/> entities.
///
/// The page lists each firm in a <c>&lt;div class="result-item"&gt;</c> block. Rather than
/// build a full DOM (and to honour the "no third-party libraries" rule), we slice the page
/// into those blocks and pull each field with the small <see cref="Html"/> helpers. Each
/// block is bounded at its contact-list <c>&lt;/ul&gt;</c> so promo banners sitting between
/// firms are never mistaken for listing data.
/// </summary>
public sealed class SolicitorListingParser(string baseUrl = "https://www.solicitors.com")
{
    private const string RecordMarker = "<div class=\"result-item\">";
    private const string RecordEnd = "</ul>";

    private readonly string _baseUrl = baseUrl.TrimEnd('/');

    public IReadOnlyList<Solicitor> Parse(string? html, Location location)
    {
        var firms = new List<Solicitor>();
        if (string.IsNullOrEmpty(html)) return firms;

        var start = html.IndexOf(RecordMarker, StringComparison.OrdinalIgnoreCase);
        while (start >= 0)
        {
            var next = html.IndexOf(RecordMarker, start + RecordMarker.Length, StringComparison.OrdinalIgnoreCase);

            // End the block at its contact list so banners/adjacent firms are excluded.
            var listEnd = html.IndexOf(RecordEnd, start, StringComparison.OrdinalIgnoreCase);
            var end = listEnd >= 0 ? listEnd + RecordEnd.Length : (next >= 0 ? next : html.Length);
            if (next >= 0 && end > next) end = next;

            var firm = ParseRecord(html[start..end], location);
            if (firm is not null) firms.Add(firm);

            start = next;
        }

        return firms;
    }

    private Solicitor? ParseRecord(string block, Location location)
    {
        var name = Html.Clean(Html.Between(block, "<span class=\"h2\">", "<"));
        if (string.IsNullOrWhiteSpace(name)) return null; // no name = not a real listing; skip

        var address = Html.Clean(Html.Between(block, "<address>", "</address>"));
        var description = Html.Clean(Html.Between(block, "<p>", "</p>"));

        var contact = new ContactDetails(
            phone: ExtractPhone(block),
            email: Absolute(Html.HrefContaining(block, "enquiry-form")),
            website: Absolute(Html.TargetBlankHref(block)));

        return new Solicitor(
            name,
            location,
            Address.FromRaw(address),
            contact,
            description,
            ExtractRating(block));
    }

    /// <summary>Visible text inside the <c>tel:</c> anchor, e.g. "020 7427 5970".</summary>
    private static string? ExtractPhone(string block)
    {
        var anchor = Html.Between(block, "href=\"tel:", "</a>");
        if (anchor is null) return null;

        var textStart = anchor.IndexOf('>');
        return textStart < 0 ? null : Html.Clean(anchor[(textStart + 1)..]);
    }

    /// <summary>
    /// Reads the star rating from the <c>rev-results</c> block: each <c>star-full</c> is one
    /// star and each <c>star-half</c> is a half, with the review count in parentheses. Returns
    /// null when the listing carries no rating at all.
    /// </summary>
    private static Rating? ExtractRating(string block)
    {
        var stars = Html.Between(block, "<span class=\"rev-results\">", "</span>");
        if (stars is null) return null;

        var score = Html.Count(stars, "star-full") + 0.5 * Html.Count(stars, "star-half");
        var reviews = Html.DigitsToInt(Html.Between(stars, "(", ")"));

        return score == 0 && reviews == 0 ? null : new Rating(score, reviews);
    }

    /// <summary>Makes a site-relative href absolute; leaves already-absolute URLs untouched.</summary>
    private string? Absolute(string? href)
    {
        if (string.IsNullOrWhiteSpace(href)) return null;
        if (href.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return href;
        return href.StartsWith('/') ? _baseUrl + href : $"{_baseUrl}/{href}";
    }
}
