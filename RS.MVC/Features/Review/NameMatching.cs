using System.Globalization;
using System.Text.RegularExpressions;

namespace RS.MVC.Features.Review
{
    /// <summary>
    /// Compares names typed by researchers with existing ones: ignores case (Turkish rules, so "İ"/"i" and "I"/"ı"
    /// match properly), surrounding spaces and repeated spaces.
    /// </summary>
    public static class NameMatching
    {
        private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

        /// <summary>The name as it is stored: trimmed, single spaces.</summary>
        public static string Clean(string name) => Regex.Replace(name?.Trim() ?? "", @"\s+", " ");

        /// <summary>The comparison key: cleaned and lower-cased the Turkish way.</summary>
        public static string Key(string name) => Clean(name).ToLower(Turkish);
    }
}
