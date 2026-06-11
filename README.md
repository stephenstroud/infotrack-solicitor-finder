# Solicitor Lead Finder

A small full-stack app that automates gathering conveyancing solicitors by location from
[solicitors.com](https://www.solicitors.com), and turns the raw listings into a sales-ready
insight report — built as a **.NET 10 Web API + Angular SPA** pair.

The results page for a location (e.g. `https://www.solicitors.com/london-solicitors.html`) is
fetched and parsed **without any third-party HTML library** — a small, hand-rolled parser does
the extraction so the structure of the logic is on show.

---

## Features

**Core**
- Submit a search across one or more locations and scrape each results page.
- Extract per firm: **name, location, full address (city + postcode parsed out), phone,
  website, enquiry link, description, and star rating + review count**.
- Edit the location list in the UI (the eight default locations are pre-loaded and removable,
  and you can add your own).
- A **standard insight report**: headline totals, contactability breakdown, firms-by-location,
  a national **top-rated** ranking, and the full directory.

**Extras (from the brief's suggestions)**
- **Search history + new-firm alerts** — each run is snapshotted; the next run highlights firms
  that have newly appeared since the previous one.
- **Quality ranking** — firms ranked by star rating *weighted by review volume*, so a lone 5★
  doesn't out-rank a 4.5★ firm with thousands of reviews.

---

## Architecture

Clean, layered (DDD-leaning) so concerns are separated and the pieces are reusable/testable:

```
src/
  InfoTrack.SolicitorFinder.Domain          Pure model — no dependencies
    Solicitors/  Solicitor, Location, Address, ContactDetails, Rating (value objects)
    Search/      SearchSnapshot (history + new-firm diffing)
  InfoTrack.SolicitorFinder.Application      Use cases + ports (interfaces)
    Abstractions/  ISolicitorSource, ISearchSnapshotStore
    Reports/       SearchReport + SearchReportBuilder (the insight projection)
    SearchSolicitors/  SearchSolicitorsHandler (orchestration)
  InfoTrack.SolicitorFinder.Infrastructure   Adapters that implement the ports
    Scraping/      Html (helpers), SolicitorListingParser, HTTP + fixture HTML sources
    Persistence/   EF Core (in-memory) snapshot store
    Fixtures/      Sample results pages embedded for offline runs
  InfoTrack.SolicitorFinder.Api              Thin web layer — controllers, DI, CORS
tests/
  InfoTrack.SolicitorFinder.Tests            xUnit: parser + handler tests
client/                                      Angular 20 + Tailwind SPA
```

Dependencies point inward (Api → Application → Domain; Infrastructure → Application → Domain),
so the domain knows nothing about HTTP, EF, or solicitors.com. Adding *"scrape another site"*
later is just a new `ISolicitorSource` adapter — the orchestration and report don't change.

### The scraper (no third-party libraries)
`SolicitorListingParser` slices each results page into its `result-item` blocks and pulls each
field with small, named helpers in `Html.cs` (`Between`, `HrefContaining`, `Count`, `Clean`…),
using only the BCL (`System.Net.WebUtility`, regex). It's resilient to the site's messy,
class-light markup and degrades gracefully when a field is missing.

---

## Tech stack

| | |
|---|---|
| API | .NET 10, ASP.NET Core Web API |
| Persistence | EF Core **in-memory** (no setup required) |
| SPA | Angular 20 (standalone, signals) + Tailwind CSS |
| Tests | xUnit (backend) · Karma/Jasmine (frontend) |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org) 20.19+ / 22.12+ / 24.x and npm (for the SPA)

That's it — **no database to install**, no connection string to configure (see below).

---

## Running it

Two terminals — API first, then the SPA.

### 1. API

```bash
cd src/InfoTrack.SolicitorFinder.Api
dotnet run
```

The API listens on **http://localhost:5211** (and https://localhost:7117).
Browse the OpenAPI document at `http://localhost:5211/openapi/v1.json`.

### 2. SPA

```bash
cd client
npm install
npm start
```

Open **http://localhost:4200**, adjust the locations, and click **Find solicitors**.

> The SPA calls the API at `http://localhost:5211/api` (configured in
> `client/src/environments/environment.ts`). CORS for `http://localhost:4200` is already
> enabled on the API.

---

## Configuration

All in `src/InfoTrack.SolicitorFinder.Api/appsettings.json`:

| Setting | Purpose |
|---|---|
| `Scraper:Mode` | `"live"` (default) scrapes solicitors.com and falls back to a bundled fixture if a request fails; `"fixture"` runs **fully offline** from the embedded sample pages. |
| `Scraper:BaseUrl` / `ResultsPathTemplate` | Where listings come from and the `{slug}-solicitors.html` URL shape. |
| `DefaultLocations` | The list the UI pre-loads (the brief's eight cities). |
| `Cors:Origins` | Allowed SPA origins (defaults to `http://localhost:4200`). |

Run fully offline (e.g. if the site is unreachable):

```bash
# PowerShell
$env:Scraper__Mode = "fixture"; dotnet run
```

### Database

The app uses an **in-memory EF Core database** (one of the brief's allowed options), so there is
**no connection string, SQL script, or schema to set up** — it just runs. History therefore lives
for the lifetime of the running API process, which is all the new-firm detection needs.

Switching to SQL Server/Postgres would be a one-line change in
`Infrastructure/DependencyInjection.cs` (`UseInMemoryDatabase` → `UseSqlServer`/`UseNpgsql`) plus
a connection string — the rest of the code is provider-agnostic.

---

## Tests

```bash
# Backend (xUnit) — parser extraction against real captured HTML + handler logic
dotnet test

# Frontend (Karma/Jasmine)
cd client && npm test
```

---

## Notes & assumptions

- **Ratings**: the live site exposes star ratings + review counts, so the "rank by rating &
  count" insight is built on real data.
- **Email**: solicitors.com lists an *enquiry form* rather than a raw email address, so the
  "email" channel is captured as that enquiry link (made absolute).
- **Duplicates**: a firm can appear more than once on a page (and across locations); listings are
  de-duplicated by firm identity (name + location) in the handler.
- **Fixtures**: sample results pages for the eight default cities are embedded in the
  Infrastructure assembly purely so the app and tests run with zero configuration and offline.
