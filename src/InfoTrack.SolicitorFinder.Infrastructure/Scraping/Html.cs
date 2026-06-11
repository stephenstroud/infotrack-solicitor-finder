using System.Net;
using System.Text.RegularExpressions;

namespace InfoTrack.SolicitorFinder.Infrastructure.Scraping;

/// <summary>
/// Tiny, dependency-free helpers for pulling values out of raw HTML. Deliberately *not* a
/// general HTML/DOM parser — the brief asks us not to use third-party libraries and to show
/// how we structure the logic, so these are small, named, testable string operations that
/// the listing parser composes. Uses only the BCL (<see cref="WebUtility"/>, regex).
/// </summary>
internal static partial class Html
{
    /// <summary>
    /// The substring between the first <paramref name="start"/> at or after <paramref name="from"/>
    /// and the next <paramref name="end"/> following it. Null if either marker is missing.
    /// Comparison is case-insensitive so tag/attribute casing doesn't matter.
    /// </summary>
    public static string? Between(string source, string start, string end, int from = 0)
    {
        var open = source.IndexOf(start, from, StringComparison.OrdinalIgnoreCase);
        if (open < 0) return null;

        open += start.Length;
        var close = source.IndexOf(end, open, StringComparison.OrdinalIgnoreCase);
        return close < 0 ? null : source[open..close];
    }

    /// <summary>
    /// The value of the first <c>href="…"</c> whose URL contains <paramref name="needle"/>
    /// (e.g. "enquiry-form"). Returns the raw href, or null if not found.
    /// </summary>
    public static string? HrefContaining(string source, string needle)
    {
        var at = source.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
        if (at < 0) return null;

        var hrefStart = source.LastIndexOf("href=\"", at, StringComparison.OrdinalIgnoreCase);
        if (hrefStart < 0) return null;

        hrefStart += "href=\"".Length;
        var hrefEnd = source.IndexOf('"', hrefStart);
        return hrefEnd < 0 ? null : source[hrefStart..hrefEnd];
    }

    /// <summary>The href of the first anchor carrying <c>target="_blank"</c> — the outbound website link.</summary>
    public static string? TargetBlankHref(string source)
    {
        var blank = source.IndexOf("target=\"_blank\"", StringComparison.OrdinalIgnoreCase);
        if (blank < 0) return null;

        var hrefStart = source.IndexOf("href=\"", blank, StringComparison.OrdinalIgnoreCase);
        if (hrefStart < 0) return null;

        hrefStart += "href=\"".Length;
        var hrefEnd = source.IndexOf('"', hrefStart);
        return hrefEnd < 0 ? null : source[hrefStart..hrefEnd];
    }

    /// <summary>Number of non-overlapping occurrences of <paramref name="token"/>.</summary>
    public static int Count(string source, string token)
    {
        var count = 0;
        var i = source.IndexOf(token, StringComparison.OrdinalIgnoreCase);
        while (i >= 0)
        {
            count++;
            i = source.IndexOf(token, i + token.Length, StringComparison.OrdinalIgnoreCase);
        }
        return count;
    }

    /// <summary>Strips tags, decodes HTML entities, and collapses runs of whitespace to single spaces.</summary>
    public static string Clean(string? html)
    {
        if (string.IsNullOrEmpty(html)) return string.Empty;

        var withoutTags = TagRegex().Replace(html, " ");
        var decoded = WebUtility.HtmlDecode(withoutTags);
        return WhitespaceRegex().Replace(decoded, " ").Trim();
    }

    /// <summary>The leading run of digits in <paramref name="text"/> as an int (ignoring commas/spaces); 0 if none.</summary>
    public static int DigitsToInt(string? text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        var digits = new string(text.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out var value) ? value : 0;
    }

    [GeneratedRegex("<[^>]+>", RegexOptions.Compiled)]
    private static partial Regex TagRegex();

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();
}
