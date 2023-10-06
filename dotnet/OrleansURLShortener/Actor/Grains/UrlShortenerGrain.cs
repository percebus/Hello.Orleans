namespace JCystems.MSLearn.OrleansURLShortener.Actor.Grains
{
    using JCystems.MSLearn.OrleansURLShortener.Model;
    using Orleans.Runtime;

    public sealed class UrlShortenerGrain : Grain, IUrlShortenerGrain
    {
        private IPersistentState<UrlDetails> UrlDetailsPersistentState { get; }

        public UrlShortenerGrain(
            [PersistentState(
            stateName: "url",
            storageName: "urls")]
            IPersistentState<UrlDetails> state) => UrlDetailsPersistentState = state;

        public async Task SetUrl(string fullUrl)
        {
            this.UrlDetailsPersistentState.State = new()
            {
                ShortenedRouteSegment = this.GetPrimaryKeyString(),
                FullUrl = fullUrl
            };

            await this.UrlDetailsPersistentState.WriteStateAsync();
        }

        public Task<string> GetUrl() =>
            Task.FromResult(this.UrlDetailsPersistentState.State.FullUrl);
    }
}
