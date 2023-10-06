namespace JCystems.MSLearn.OrleansURLShortener.Model
{
    [GenerateSerializer]
    public record class UrlDetails
    {
        [Id(0)]
        public string FullUrl { get; set; } = null!;

        [Id(1)]
        public string ShortenedRouteSegment { get; set; } = null!;
    }
}
