using InfoTrack.SolicitorFinder.Domain.Solicitors;

namespace InfoTrack.SolicitorFinder.Application.Reports;

/// <summary>
/// Flat, serialisation-friendly projection of a <see cref="Solicitor"/> for the API/UI.
/// Keeps the domain entity from leaking transport concerns.
/// </summary>
public sealed record SolicitorView(
    string Name,
    string Location,
    string Address,
    string? City,
    string? Postcode,
    string? Phone,
    string? Email,
    string? Website,
    string? Description,
    int ChannelCount,
    bool IsReachable)
{
    public static SolicitorView From(Solicitor s) => new(
        s.Name,
        s.Location.Name,
        s.Address.Raw,
        s.Address.City,
        s.Address.Postcode,
        s.Contact.Phone,
        s.Contact.Email,
        s.Contact.Website,
        s.Description,
        s.Contact.ChannelCount,
        s.Contact.IsReachable);
}
