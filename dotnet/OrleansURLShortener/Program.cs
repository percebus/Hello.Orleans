using JCystems.MSLearn.OrleansURLShortener.Actor.Grains;

WebApplicationBuilder oWebApplicationBuilder = WebApplication.CreateBuilder(args);
oWebApplicationBuilder.Host.UseOrleans((hostBuilderContext, siloBuilder) =>
{
    if (hostBuilderContext.HostingEnvironment.IsDevelopment())
    {
        siloBuilder.UseLocalhostClustering();
        siloBuilder.AddMemoryGrainStorage("urls");
    }
    else
    {
        // siloBuilder.useKubernetesHosting // TODO
    }
});

oWebApplicationBuilder.Services.AddEndpointsApiExplorer();
oWebApplicationBuilder.Services.AddSwaggerGen();

// using?!
using WebApplication oWebApplication = oWebApplicationBuilder.Build();

oWebApplication.UseHttpsRedirection();

oWebApplication.MapGet("/", () => "Hello World!");

oWebApplication.MapGet("/shorten",
    async (IGrainFactory grainFactory, HttpRequest httpRequest, string url) =>
    {
        var host = $"{httpRequest.Scheme}://{httpRequest.Host.Value}";

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
        var oUrlShortenerGrain = grainFactory.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);
        await oUrlShortenerGrain.SetUrl(url);

        // Return the shortened URL for later use
        var uriBuilder = new UriBuilder(host)
        {
            Path = $"/go/{shortenedRouteSegment}"
        };

        return Results.Ok(uriBuilder.Uri);
    });

oWebApplication.MapGet(
    "/go/{shortenedRouteSegment:required}",
    async (IGrainFactory grainFactory, string shortenedRouteSegment) =>
    {
        // Retrieve the grain using the shortened ID and url to the original URL        
        var oUrlShortenerGrain = grainFactory.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);
        var url = await oUrlShortenerGrain.GetUrl();

        return Results.Redirect(url);
    });

if (oWebApplication.Environment.IsDevelopment())
{
    oWebApplication.UseSwagger();
    oWebApplication.UseSwaggerUI();
}

oWebApplication.Run();
