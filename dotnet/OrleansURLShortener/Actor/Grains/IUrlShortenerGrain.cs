namespace JCystems.MSLearn.OrleansURLShortener.Actor.Grains
{
    public interface IUrlShortenerGrain : IGrainWithStringKey
    {
        Task SetUrl(string fullUrl);

        Task<string> GetUrl();
    }
}
