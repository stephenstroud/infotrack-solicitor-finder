using InfoTrack.SolicitorFinder.Application;
using InfoTrack.SolicitorFinder.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

const string SpaCorsPolicy = "spa";

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Application use cases + Infrastructure adapters (scraper, EF in-memory store).
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// CORS for the Angular SPA. Origins are configurable; default to the Angular dev server.
var allowedOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
    ?? ["http://localhost:4200"];
builder.Services.AddCors(options => options.AddPolicy(SpaCorsPolicy, policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    // Only force HTTPS outside development, so the SPA's CORS preflight isn't redirected in dev.
    app.UseHttpsRedirection();
}

app.UseCors(SpaCorsPolicy);
app.MapControllers();

app.Run();

// Exposed so the WebApplicationFactory-based integration tests can reference the entry point.
public partial class Program;
