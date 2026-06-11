namespace InfoTrack.SolicitorFinder.Api.Contracts;

/// <summary>The locations the CEO wants solicitors gathered for.</summary>
public sealed record SearchRequest(IReadOnlyList<string> Locations);
