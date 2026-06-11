using InfoTrack.SolicitorFinder.Api.Contracts;
using InfoTrack.SolicitorFinder.Application.Abstractions;
using InfoTrack.SolicitorFinder.Application.Reports;
using InfoTrack.SolicitorFinder.Application.SearchSolicitors;
using Microsoft.AspNetCore.Mvc;

namespace InfoTrack.SolicitorFinder.Api.Controllers;

/// <summary>Runs solicitor searches and exposes the history of past runs.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class SearchController(
    SearchSolicitorsHandler handler,
    ISearchSnapshotStore snapshots) : ControllerBase
{
    /// <summary>Scrapes the given locations and returns the insight report.</summary>
    [HttpPost]
    public async Task<ActionResult<SearchReport>> Search(
        [FromBody] SearchRequest request,
        CancellationToken cancellationToken)
    {
        if (request?.Locations is null || request.Locations.Count == 0)
            return BadRequest("Provide at least one location.");

        var report = await handler.HandleAsync(request.Locations, cancellationToken);
        return Ok(report);
    }

    /// <summary>Recent search runs (newest first), for the history panel.</summary>
    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<SnapshotSummary>>> History(CancellationToken cancellationToken)
    {
        var history = await snapshots.GetHistoryAsync(20, cancellationToken);

        var summaries = history
            .Select(s => new SnapshotSummary(s.Id, s.CapturedAt, s.Locations.Count, s.Solicitors.Count))
            .ToList();

        return Ok(summaries);
    }
}
