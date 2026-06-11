using InfoTrack.SolicitorFinder.Domain.Solicitors;
using InfoTrack.SolicitorFinder.Infrastructure.Scraping;

namespace InfoTrack.SolicitorFinder.Tests.Scraping;

public class SolicitorListingParserTests
{
    private readonly SolicitorListingParser _parser = new();
    private readonly Location _london = new("London");

    private IReadOnlyList<Solicitor> ParseLondon() => _parser.Parse(FixtureLoader.Load("london"), _london);

    [Fact]
    public void Parses_every_listing_on_the_page()
    {
        var firms = ParseLondon();

        // The London fixture contains 65 result-item blocks; all carry a name.
        Assert.Equal(65, firms.Count);
        Assert.All(firms, f => Assert.False(string.IsNullOrWhiteSpace(f.Name)));
    }

    [Fact]
    public void Extracts_all_contact_fields_for_a_firm()
    {
        // Some firms are listed more than once on a page; the parser returns each listing
        // verbatim (de-duplication is the handler's job). Assert on the first occurrence.
        var firm = ParseLondon().First(f => f.Name == "EBR Attridge");

        Assert.Equal("020 8808 0774", firm.Contact.Phone);
        Assert.Equal("http://www.ebrattridge.com", firm.Contact.Website);
        Assert.Contains("430-436 High Road", firm.Address.Raw);
        Assert.Equal("London", firm.Address.City);
        Assert.Equal("N17 9JB", firm.Address.Postcode);
        Assert.StartsWith("https://www.solicitors.com/enquiry-form.asp", firm.Contact.Email);
        Assert.Equal(3, firm.Contact.ChannelCount); // phone + website + enquiry
    }

    [Fact]
    public void Reads_a_half_star_rating_and_its_review_count()
    {
        var firm = ParseLondon().First(f => f.Name == "EBR Attridge");

        Assert.NotNull(firm.Rating);
        Assert.Equal(3.5, firm.Rating!.Stars); // three full + one half
        Assert.Equal(73, firm.Rating.ReviewCount);
    }

    [Fact]
    public void Reads_a_whole_star_rating_with_a_thousands_review_count()
    {
        var firm = ParseLondon().First(f => f.Name == "Forbes Solicitors");

        Assert.Equal(4.0, firm.Rating!.Stars);
        Assert.Equal(1523, firm.Rating.ReviewCount);
    }

    [Fact]
    public void Stamps_every_firm_with_the_searched_location()
    {
        Assert.All(ParseLondon(), f => Assert.Equal("London", f.Location.Name));
    }

    [Fact]
    public void Handles_a_listing_with_no_rating_or_contact_details_gracefully()
    {
        const string html =
            """
            <div class="result-item">
              <div class="top-holder"><span class="h2">Acme Law</span></div>
              <a class="link-map"><address>1 Test Street, London, E1 1AA</address></a>
              <p>A minimal listing.</p>
              <ul class="list-item"></ul>
            </div>
            """;

        var firm = _parser.Parse(html, _london).Single();

        Assert.Equal("Acme Law", firm.Name);
        Assert.Equal("E1 1AA", firm.Address.Postcode);
        Assert.Null(firm.Rating);
        Assert.False(firm.Contact.IsReachable); // no phone, email or website
    }

    [Fact]
    public void Returns_empty_for_blank_or_markerless_html()
    {
        Assert.Empty(_parser.Parse(null, _london));
        Assert.Empty(_parser.Parse("<html><body>no results here</body></html>", _london));
    }
}
