namespace JCystems.MSLearn.OrleansURLShortener.Actor.Grains
{
    using JCystems.MSLearn.OrleansURLShortener.Model;
    using Orleans.Runtime;

    public sealed class UrlShortenerGrain : Grain, IUrlShortenerGrain
    {
        private IPersistentState<UrlDetails> State { get; }

        public UrlShortenerGrain(
            [PersistentState(
            stateName: "url",
            storageName: "urls")]
            IPersistentState<UrlDetails> state) => State = state;

        public async Task SetUrl(string fullUrl)
        {
            this.State.State = new()
            {
                ShortenedRouteSegment = this.GetPrimaryKeyString(),
                FullUrl = fullUrl
            };

            await this.State.WriteStateAsync();
        }

        public Task<string> GetUrl() =>
            Task.FromResult(this.State.State.FullUrl);
    }
}
