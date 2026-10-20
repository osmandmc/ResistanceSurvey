using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RS.MVC.Features.Review
{
    /// <summary>
    /// Finds known companies in an article's text. A company's name is reduced to its core: Turkish lower case,
    /// punctuation removed, legal-form words dropped ("Örnek Tekstil San. ve Tic. A.Ş." → "örnek tekstil"). The core
    /// must appear as whole words, so "metro" does not match "metropol", while "Örnek Tekstil'de" still matches.
    /// Cores that are too short or too common to mean a specific company are skipped.
    /// </summary>
    public static class CompanyNameMatcher
    {
        private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

        /// <summary>Legal-form and filler words that say nothing about which company it is.</summary>
        private static readonly HashSet<string> LegalWords =
        [
            "a", "ş", "aş", "as", "anonim", "şirketi", "şti", "sti", "ltd", "limited", "san", "sanayi", "sanayii",
            "tic", "ticaret", "ve", "vb", "inc", "co", "corp", "llc", "gmbh", "ag", "tas", "taş", "koop", "kooperatifi",
            "ith", "ihr", "ithalat", "ihracat", "paz", "pazarlama", "end", "endüstri", "endüstrisi",
        ];

        /// <summary>A one-word core must be at least this long ("Metro", "Bosch"); shorter ones match too often.</summary>
        public const int MinSingleWordLength = 5;

        /// <summary>The text in comparable form: Turkish lower case, words separated by single spaces, padded.</summary>
        public static string Normalize(string text)
        {
            var lower = (text ?? "").ToLower(Turkish);
            var builder = new StringBuilder(lower.Length + 2).Append(' ');
            var lastWasSpace = true;
            foreach (var c in lower)
            {
                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(c);
                    lastWasSpace = false;
                }
                else if (!lastWasSpace)
                {
                    builder.Append(' ');
                    lastWasSpace = true;
                }
            }
            if (!lastWasSpace)
                builder.Append(' ');
            return builder.ToString();
        }

        /// <summary>The words that identify the company, or null when the name is too short or generic to match on.</summary>
        public static string Core(string companyName)
        {
            var words = Normalize(companyName).Split(' ', System.StringSplitOptions.RemoveEmptyEntries)
                .Where(w => !LegalWords.Contains(w))
                .ToList();
            if (words.Count == 0)
                return null;
            if (words.Count == 1 && words[0].Length < MinSingleWordLength)
                return null;
            return string.Join(' ', words);
        }

        public record Match(int CompanyId, string CompanyName, bool InTitle);

        /// <summary>Companies whose core appears in the title or the body, title matches first.</summary>
        public static IReadOnlyList<Match> Find(IEnumerable<(int Id, string Name)> companies, string title, string body)
        {
            var normalizedTitle = Normalize(title);
            var normalizedBody = Normalize(body);
            var matches = new List<Match>();
            foreach (var (id, name) in companies)
            {
                var core = Core(name);
                if (core == null)
                    continue;
                var needle = $" {core} ";
                var inTitle = normalizedTitle.Contains(needle);
                if (inTitle || normalizedBody.Contains(needle))
                    matches.Add(new Match(id, name, inTitle));
            }
            return matches.OrderByDescending(m => m.InTitle).ToList();
        }
    }
}
