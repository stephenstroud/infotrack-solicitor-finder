namespace InfoTrack.SolicitorFinder.Application.Reports;

/// <summary>
/// Headline "how usable are these leads" numbers across the whole result set.
/// Percentages are derived so the UI never has to recompute them.
/// </summary>
public sealed record ContactabilityStats(
    int TotalFirms,
    int WithPhone,
    int WithEmail,
    int WithWebsite,
    int Reachable)
{
    public int PhonePercent => Pct(WithPhone);
    public int EmailPercent => Pct(WithEmail);
    public int WebsitePercent => Pct(WithWebsite);
    public int ReachablePercent => Pct(Reachable);

    private int Pct(int part) => TotalFirms == 0 ? 0 : (int)Math.Round(100.0 * part / TotalFirms);
}
