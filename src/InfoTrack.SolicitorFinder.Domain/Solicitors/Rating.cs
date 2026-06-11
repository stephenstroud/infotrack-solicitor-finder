namespace InfoTrack.SolicitorFinder.Domain.Solicitors;

/// <summary>
/// A firm's review standing: an average star score (0–5, half-star resolution) and the
/// number of reviews behind it. Value object — a firm either has a rating or it doesn't.
/// </summary>
public sealed record Rating
{
    public double Stars { get; }
    public int ReviewCount { get; }

    public Rating(double stars, int reviewCount)
    {
        Stars = Math.Clamp(stars, 0, 5);
        ReviewCount = Math.Max(0, reviewCount);
    }

    /// <summary>
    /// Confidence-weighted score for ranking firms nationally. Multiplying the star score by
    /// log(reviews) stops a lone 5★ review from out-ranking a 4.5★ firm with thousands —
    /// rewarding both quality *and* volume, as the brief's "rating and count" hint suggests.
    /// </summary>
    public double Score => Stars * Math.Log10(ReviewCount + 1);
}
