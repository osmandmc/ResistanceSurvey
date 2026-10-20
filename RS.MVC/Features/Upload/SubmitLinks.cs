using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RS.MVC.Applications;
using RS.MVC.Models;

namespace RS.MVC.Features.Upload
{
    /// <summary>
    /// "Bağlantılar": a list of article links pasted by a researcher - one per line, or separated by spaces, commas
    /// or semicolons. They go to the news pipeline, which downloads and reads each page (any month; the cut-over does
    /// not apply, since only the pipeline can fetch pages).
    /// </summary>
    public record SubmitLinksCommand(string Text);

    /// <summary>Status: "Accepted", "Duplicate" or "Rejected" from the pipeline, or "Invalid" for text that is not a link.</summary>
    public record LinkResult(string Url, string Status, string Reason = null);

    public record SubmitLinksResult(IReadOnlyList<LinkResult> Links, string Error = null);

    public class SubmitLinksHandler(IPipelineClient pipeline)
    {
        public const int MaxLinks = 200;

        private static readonly Regex Separators = new(@"[\s,;]+", RegexOptions.Compiled);

        /// <summary>The distinct pieces of the text, in order (brackets, quotes and a final full stop from pasted sentences removed).</summary>
        public static IReadOnlyList<string> Split(string text) =>
            Separators.Split(text ?? "")
                .Select(piece => piece.Trim().TrimStart('(', '[', '<', '"', '\'').TrimEnd('.', ')', ']', '>', '"', '\''))
                .Where(piece => piece != "")
                .Distinct()
                .ToList();

        public static bool IsLink(string piece) =>
            Uri.TryCreate(piece, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        public async Task<SubmitLinksResult> Handle(SubmitLinksCommand command)
        {
            var pieces = Split(command.Text);
            if (pieces.Count == 0)
                return new SubmitLinksResult([], "Lütfen en az bir bağlantı girin.");
            if (pieces.Count > MaxLinks)
                return new SubmitLinksResult([], $"Bir seferde en çok {MaxLinks} bağlantı gönderilebilir ({pieces.Count} girildi).");

            var links = pieces.Where(IsLink).ToList();
            var answers = links.Count == 0
                ? []
                : (await pipeline.SubmitAsync(links.Select(url => new PipelineArticle(url, null, null, null)).ToList(), "Manual"))
                    .ToDictionary(r => r.Url, r => r);

            // In the order they were entered.
            var results = pieces.Select(piece =>
                !IsLink(piece) ? new LinkResult(piece, "Invalid", "Geçerli bir bağlantı değil")
                : answers.TryGetValue(piece, out var answer) ? new LinkResult(piece, answer.Status, answer.Reason)
                : new LinkResult(piece, "Rejected", "İşleme hattından yanıt gelmedi")).ToList();
            return new SubmitLinksResult(results);
        }
    }
}
