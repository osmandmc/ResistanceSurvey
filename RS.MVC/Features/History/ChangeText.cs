using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Cases;
using static RS.COMMON.Constants.Enums;

namespace RS.MVC.Features.History
{
    /// <summary>One changed field for people: its label and values in Turkish. A list's value has only OldValue (removed) or NewValue (added).</summary>
    public record FieldChangeRow(string Entity, string Field, string Label, string OldValue, string NewValue, bool IsList);

    /// <summary>
    /// Turns recorded changes (R39) into what the screens show: the form's labels, names instead of ids ("Örnek
    /// Tekstil", not 10), Evet/Hayır, dates as 12.09.2026. A value whose record is gone shows as "#id".
    /// </summary>
    public class ChangeText(RSDBContext db)
    {
        private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

        private static readonly HashSet<string> ListFields =
            ["Reasons", "Organizations", "EmploymentTypes", "Articles", "ProtestTypes", "Places", "Interventions", "Locations"];

        /// <summary>The form's labels, by record and field; a field without one shows its name.</summary>
        private static readonly Dictionary<(string Entity, string Field), string> Labels = new()
        {
            [("Resistance", nameof(Resistance.Name))] = "Vaka İsmi",
            [("Resistance", nameof(Resistance.Description))] = "Kısa Açıklama",
            [("Resistance", nameof(Resistance.Note))] = "Kişisel Notlar",
            [("Resistance", nameof(Resistance.CategoryId))] = "Vaka Niteliği",
            [("Resistance", nameof(Resistance.CompanyId))] = "İşveren",
            [("Resistance", nameof(Resistance.EmployerRole))] = "İşveren Durumu",
            [("Resistance", nameof(Resistance.EmployerUnknown))] = "İşveren bilinmiyor",
            [("Resistance", nameof(Resistance.MainCompanyId))] = "Asıl işveren / iş sahibi",
            [("Resistance", nameof(Resistance.WorkplaceId))] = "İşyeri",
            [("Resistance", nameof(Resistance.SupportedResistanceId))] = "Desteklenen Vaka",
            [("Resistance", nameof(Resistance.DevelopRight))] = "Hak Geliştirme/Hak Savunma",
            [("Resistance", nameof(Resistance.EmployeeCountNumber))] = "İş Yerindeki İşçi Sayısı (Tam)",
            [("Resistance", nameof(Resistance.EmployeeCountId))] = "İş Yerindeki İşçi Sayısı",
            [("Resistance", nameof(Resistance.TradeUnionId))] = "Tepki Gösterilen Sendika",
            [("Resistance", nameof(Resistance.TradeUnionAuthorityId))] = "Sendikanın Yetki Durumu",
            [("Resistance", nameof(Resistance.FiredEmployeeCountByProtesto))] = "Mücadele Ettiği için İşten Atılan İşçi Sayısı",
            [("Resistance", nameof(Resistance.ResistanceResult))] = "Sonuç",
            [("Resistance", nameof(Resistance.StartDate))] = "Başlangıç Tarihi",
            [("Resistance", nameof(Resistance.EndDate))] = "Bitiş Tarihi",
            [("Resistance", nameof(Resistance.Deleted))] = "Silindi",
            [("Resistance", "Reasons")] = "Vaka Nedeni",
            [("Resistance", "Organizations")] = "Kurumlar",
            [("Resistance", "EmploymentTypes")] = "İstihdam Türü",
            [("Resistance", "Articles")] = "Haberler",
            [("Protesto", nameof(Protesto.StartDate))] = "Eylem Başlangıç Tarihi",
            [("Protesto", nameof(Protesto.EndDate))] = "Eylem Bitiş Tarihi",
            [("Protesto", nameof(Protesto.EmployeeCountNumber))] = "Eylemdeki İşçi Sayısı (Tam)",
            [("Protesto", nameof(Protesto.ProtestoEmployeeCountId))] = "Eylemdeki İşçi Sayısı",
            [("Protesto", nameof(Protesto.GenderId))] = "Cinsiyet",
            [("Protesto", nameof(Protesto.CustodyCount))] = "Gözaltı Sayısı",
            [("Protesto", nameof(Protesto.StrikeDuration))] = "Eylemin Süresi",
            [("Protesto", nameof(Protesto.SimpleProtestoDescription))] = "İş yerinde Basit Eylem Açıklama",
            [("Protesto", nameof(Protesto.Note))] = "Kontrol Kişisine Notlar",
            [("Protesto", nameof(Protesto.Deleted))] = "Eylem silindi",
            [("Protesto", "ProtestTypes")] = "Eylem Türleri",
            [("Protesto", "Places")] = "Eylem Yerleri",
            [("Protesto", "Interventions")] = "Müdahale Tipi",
            [("Protesto", "Locations")] = "Lokasyonlar",
        };

        /// <summary>Fields whose values are ids, and where their names are.</summary>
        private static readonly Dictionary<string, Func<RSDBContext, IReadOnlyCollection<int>, Task<Dictionary<int, string>>>> Names = new()
        {
            [nameof(Resistance.CategoryId)] = Lookup<Category>,
            [nameof(Resistance.CompanyId)] = Companies,
            [nameof(Resistance.MainCompanyId)] = Companies,
            [nameof(Resistance.WorkplaceId)] = Lookup<Workplace>,
            [nameof(Resistance.TradeUnionId)] = Lookup<Corporation>,
            [nameof(Resistance.TradeUnionAuthorityId)] = Lookup<TradeUnionAuthority>,
            [nameof(Resistance.EmployeeCountId)] = Lookup<EmployeeCount>,
            [nameof(Resistance.SupportedResistanceId)] = (db, ids) =>
                db.Resistance.AsNoTracking().Where(r => ids.Contains(r.Id)).ToDictionaryAsync(r => r.Id, r => r.Name),
            [nameof(Protesto.GenderId)] = Lookup<Gender>,
            [nameof(Protesto.ProtestoEmployeeCountId)] = Lookup<ProtestoEmployeeCount>,
            ["Reasons"] = Lookup<ResistanceReason>,
            ["Organizations"] = Lookup<Corporation>,
            ["EmploymentTypes"] = Lookup<EmploymentType>,
            ["Articles"] = (db, ids) => db.News.AsNoTracking().Where(n => ids.Contains(n.Id)).ToDictionaryAsync(n => n.Id, n => n.Header),
            ["ProtestTypes"] = Lookup<ProtestoType>,
            ["Places"] = Lookup<ProtestoPlace>,
            ["Interventions"] = Lookup<InterventionType>,
        };

        private static Task<Dictionary<int, string>> Lookup<T>(RSDBContext db, IReadOnlyCollection<int> ids) where T : LookupEntity =>
            db.Set<T>().AsNoTracking().Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name);

        private static Task<Dictionary<int, string>> Companies(RSDBContext db, IReadOnlyCollection<int> ids) =>
            db.Company.AsNoTracking().Where(c => ids.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name);

        public async Task<IReadOnlyList<FieldChangeRow>> RowsAsync(IReadOnlyList<FieldChange> changes)
        {
            // One query per kind of name, for all the changes at once.
            var names = new Dictionary<string, Dictionary<int, string>>();
            foreach (var group in changes.Where(c => Names.ContainsKey(c.Field)).GroupBy(c => Names[c.Field]))
            {
                var ids = group.SelectMany(c => new[] { c.OldValue, c.NewValue }).Select(Id).OfType<int>().Distinct().ToList();
                var found = await group.Key(db, ids);
                foreach (var field in group.Select(c => c.Field).Distinct())
                    names[field] = found;
            }
            var locations = changes.Where(c => c.Field == "Locations").SelectMany(c => new[] { c.OldValue, c.NewValue }).OfType<string>()
                .Select(v => v.Split('/', 3)).ToList();
            var cityIds = locations.Select(l => Id(l[0])).OfType<int>().Distinct().ToList();
            var districtIds = locations.Select(l => Id(l[1])).OfType<int>().Distinct().ToList();
            var cities = cityIds.Count == 0 ? new Dictionary<int, string>() : await Lookup<City>(db, cityIds);
            var districts = districtIds.Count == 0 ? new Dictionary<int, string>() : await Lookup<District>(db, districtIds);

            string Value(FieldChange c, string value)
            {
                if (value == null)
                    return null;
                if (c.Field == "Locations")
                {
                    var parts = value.Split('/', 3);
                    return string.Join(" · ", new[]
                    {
                        Name(cities, parts[0]),
                        parts.Length > 1 && parts[1].Length > 0 ? Name(districts, parts[1]) : null,
                        parts.Length > 2 && parts[2].Length > 0 ? parts[2] : null,
                    }.Where(p => p != null));
                }
                if (names.TryGetValue(c.Field, out var found))
                    return Name(found, value);
                return (c.Entity, c.Field) switch
                {
                    (_, nameof(Resistance.EmployerRole)) when Enum.TryParse<EmployerRole>(value, out var role) => EmployerRoles.Name(role),
                    (_, nameof(Resistance.ResistanceResult)) when Enum.TryParse<ResistanceResult>(value, out var result) => Describe(result),
                    ("Protesto", nameof(Protesto.StrikeDuration)) when int.TryParse(value, out var seconds) => Duration(seconds),
                    _ when value is "true" => "Evet",
                    _ when value is "false" => "Hayır",
                    _ when DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day) =>
                        day.ToString("dd.MM.yyyy", Turkish),
                    _ => value,
                };
            }

            return changes.Select(c => new FieldChangeRow(c.Entity, c.Field, Labels.GetValueOrDefault((c.Entity, c.Field), c.Field),
                Value(c, c.OldValue), Value(c, c.NewValue), ListFields.Contains(c.Field))).ToList();
        }

        private static int? Id(string value) => int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var id) ? id : null;

        private static string Name(Dictionary<int, string> names, string value) =>
            Id(value) is { } id && names.TryGetValue(id, out var name) ? name : $"#{value}";

        private static string Describe(Enum value) =>
            value.GetType().GetField(value.ToString())?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? value.ToString();

        /// <summary>A strike's length as the form asks for it: "2 gün 4 saat", "45 dakika"; 0 is none.</summary>
        public static string Duration(int seconds)
        {
            if (seconds <= 0)
                return "Yok";
            var t = TimeSpan.FromSeconds(seconds);
            var parts = new List<string>();
            if (t.Days > 0)
                parts.Add($"{t.Days} gün");
            if (t.Hours > 0)
                parts.Add($"{t.Hours} saat");
            if (t.Minutes > 0)
                parts.Add($"{t.Minutes} dakika");
            return parts.Count == 0 ? $"{seconds} saniye" : string.Join(" ", parts);
        }
    }
}
