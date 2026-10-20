using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RS.MVC.Models;

namespace RS.MVC.Applications
{
    public interface IPipelineClient
    {
        Task<IReadOnlyList<PipelineSubmitResult>> SubmitAsync(IReadOnlyList<PipelineArticle> articles, string origin);

        Task<PipelineArticleList> ListArticlesAsync(PipelineArticleQuery query);

        /// <summary>Null when the pipeline has no such article.</summary>
        Task<PipelineArticleDetail> GetArticleAsync(string articleId);

        /// <summary>Safe to repeat; the latest decision wins. Throws HttpRequestException when the pipeline cannot be reached.</summary>
        Task RecordOutcomeAsync(string articleId, PipelineOutcomeRequest outcome);
    }

    /// <summary>Calls the news pipeline API. BaseAddress and the function key header are set at registration.</summary>
    public class PipelineClient : IPipelineClient
    {
        /// <summary>The pipeline accepts at most this many articles per request.</summary>
        public const int MaxBatchSize = 100;

        private readonly HttpClient _http;
        private readonly ILogger<PipelineClient> _logger;

        public PipelineClient(HttpClient http, ILogger<PipelineClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<IReadOnlyList<PipelineSubmitResult>> SubmitAsync(IReadOnlyList<PipelineArticle> articles, string origin)
        {
            var results = new List<PipelineSubmitResult>();
            foreach (var batch in articles.Chunk(MaxBatchSize))
            {
                var response = await _http.PostAsJsonAsync("api/articles", new PipelineSubmitRequest(batch, origin));
                await EnsureSuccessAsync(response);
                var body = await response.Content.ReadFromJsonAsync<PipelineSubmitResponse>();
                results.AddRange(body.Results);
            }
            return results;
        }

        public async Task<PipelineArticleList> ListArticlesAsync(PipelineArticleQuery query)
        {
            var parameters = new List<string>
            {
                "status=" + Uri.EscapeDataString(query.Status),
                "outcome=" + Uri.EscapeDataString(query.Outcome),
                "pageSize=" + query.PageSize,
            };
            if (query.From is { } from) parameters.Add("from=" + from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            if (query.To is { } to) parameters.Add("to=" + to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(query.SourceId)) parameters.Add("source=" + Uri.EscapeDataString(query.SourceId));
            if (!string.IsNullOrEmpty(query.Next)) parameters.Add("next=" + Uri.EscapeDataString(query.Next));
            if (!string.IsNullOrWhiteSpace(query.Search)) parameters.Add("q=" + Uri.EscapeDataString(query.Search.Trim()));

            var response = await _http.GetAsync("api/articles?" + string.Join("&", parameters));
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<PipelineArticleList>();
        }

        public async Task<PipelineArticleDetail> GetArticleAsync(string articleId)
        {
            var response = await _http.GetAsync("api/articles/" + Uri.EscapeDataString(articleId));
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<PipelineArticleDetail>();
        }

        public async Task RecordOutcomeAsync(string articleId, PipelineOutcomeRequest outcome)
        {
            var response = await _http.PostAsJsonAsync("api/articles/" + Uri.EscapeDataString(articleId) + "/outcome", outcome);
            await EnsureSuccessAsync(response);
        }

        // Logs what the pipeline answered (method, path, status, the start of its error text) before failing, so a
        // failure shows its cause. The function key travels in a header and is never logged.
        private async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;
            var body = await response.Content.ReadAsStringAsync();
            if (body.Length > 300)
                body = body[..300];
            _logger.LogWarning("Pipeline {Method} {Path} answered {Status}: {Body}",
                response.RequestMessage?.Method, response.RequestMessage?.RequestUri?.AbsolutePath, (int)response.StatusCode, body);
            throw new HttpRequestException($"The pipeline answered HTTP {(int)response.StatusCode}.", null, response.StatusCode);
        }
    }
}
