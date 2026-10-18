using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using RS.MVC.Models;

namespace RS.MVC.Applications
{
    public interface IPipelineClient
    {
        Task<IReadOnlyList<PipelineSubmitResult>> SubmitAsync(IReadOnlyList<PipelineArticle> articles, string origin);
    }

    /// <summary>Calls the news pipeline API. BaseAddress and the function key header are set at registration.</summary>
    public class PipelineClient : IPipelineClient
    {
        /// <summary>The pipeline accepts at most this many articles per request.</summary>
        public const int MaxBatchSize = 100;

        private readonly HttpClient _http;

        public PipelineClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<IReadOnlyList<PipelineSubmitResult>> SubmitAsync(IReadOnlyList<PipelineArticle> articles, string origin)
        {
            var results = new List<PipelineSubmitResult>();
            foreach (var batch in articles.Chunk(MaxBatchSize))
            {
                var response = await _http.PostAsJsonAsync("api/articles", new PipelineSubmitRequest(batch, origin));
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadFromJsonAsync<PipelineSubmitResponse>();
                results.AddRange(body.Results);
            }
            return results;
        }
    }
}
