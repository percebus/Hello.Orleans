using JCystems.MSLearn.OrleansURLShortener.Actor.Grains;

WebApplicationBuilder oWebApplicationBuilder = WebApplication.CreateBuilder(args);
oWebApplicationBuilder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();
    siloBuilder.AddMemoryGrainStorage("urls");
});

oWebApplicationBuilder.Services.AddEndpointsApiExplorer();
oWebApplicationBuilder.Services.AddSwaggerGen();

// using?!
using WebApplication oWebApplication = oWebApplicationBuilder.Build();

oWebApplication.UseHttpsRedirection();

oWebApplication.MapGet("/", () => "Hello World!");

oWebApplication.MapGet("/shorten",
    async (IGrainFactory grains, HttpRequest request, string url) =>
    {
        var host = $"{request.Scheme}://{request.Host.Value}";

        // Validate the URL query string.
        if (string.IsNullOrWhiteSpace(url) &&
            Uri.IsWellFormedUriString(url, UriKind.Absolute) is false)
        {
            return Results.BadRequest($"""
                The URL query string is required and needs to be well formed.
                Consider, ${host}/shorten?url=https://www.microsoft.com.
                """);
        }

        // Create a unique, short ID
        var shortenedRouteSegment = Guid.NewGuid()
                .GetHashCode()
                .ToString("X");

        // Create and persist a grain with the shortened ID and full URL
        var shortenerGrain = grains.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);
        await shortenerGrain.SetUrl(url);

        // Return the shortened URL for later use
        var resultBuilder = new UriBuilder(host)
        {
            Path = $"/go/{shortenedRouteSegment}"
        };

        return Results.Ok(resultBuilder.Uri);
    });

oWebApplication.MapGet(
    "/go/{shortenedRouteSegment:required}",
    async (IGrainFactory grains, string shortenedRouteSegment) =>
    {
        // Retrieve the grain using the shortened ID and url to the original URL        
        var shortenerGrain = grains.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);
        var url = await shortenerGrain.GetUrl();

        return Results.Redirect(url);
    });

if (oWebApplication.Environment.IsDevelopment())
{
    oWebApplication.UseSwagger();
    oWebApplication.UseSwaggerUI();
}

oWebApplication.Run();
