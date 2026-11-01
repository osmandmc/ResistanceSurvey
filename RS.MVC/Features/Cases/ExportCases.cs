using RS.COMMON.Entities.LookupEntity;
using RS.COMMON.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RS.COMMON.Entities;
using RS.EF;

namespace RS.MVC.Features.Cases
{
    /// <summary>Vakalar → "Dışa Aktar": the filtered cases as an Excel file, one row per protest.</summary>
    public record ExportCasesQuery(CaseFilter Filter);

    /// <summary>
    /// The export researchers already use (moved here from ResistanceController.Export). Columns and their contents
    /// are kept exactly, quirks included ("Protesto Sayısı" counts the protest's locations; "Direniş Sonucu" is the
    /// enum name), so files stay comparable with earlier exports.
    /// </summary>
    public class ExportCasesHandler(RSDBContext db)
    {
        public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private static readonly string[] NoNames = [];

        /// <summary>Column heading → value for one protest, in the old export's order.</summary>
        public static readonly IReadOnlyList<(string Header, Func<Protesto, object> Value)> Columns =
        [
            ("Direnişin ID", p => p.Resistance.Id),
            ("Açıklama", p => p.Resistance.Description ?? ""),
            ("Kategori Adı", p => p.Resistance.Category?.Name ?? ""),
            ("Şirket Adı", p => p.Resistance.Company?.Name ?? ""),
            // R37: the işkolu, or for a public body the civil servants' hizmet kolu (both were one list before).
            ("Şirket İş Kolu", p => p.Resistance.Company?.CompanyWorkLine?.Name ?? p.Resistance.Company?.ServiceBranch?.Name ?? ""),
            ("Ana Şirket Adı", p => p.Resistance.MainCompany?.Name ?? ""),
            ("Ana Şirket İş Kolu", p => p.Resistance.MainCompany?.CompanyWorkLine?.Name ?? p.Resistance.MainCompany?.ServiceBranch?.Name ?? ""),
            ("Üretim Karşıtı", p => p.ProtestoProtestoTypes.Any(s => s.ProtestoType.AgainstProduction) ? "EVET" : "HAYIR"),
            ("Kalkınma Hakkı", p => p.Resistance.DevelopRight ? "EVET" : "HAYIR"),
            ("Çalışan Sayısı", p => p.Resistance.EmployeeCount?.Name ?? ""),
            ("Çalışan Sayısı (Sayısal)", p => p.Resistance.EmployeeCountNumber),
            ("Sendika Var mı", p => p.Resistance.ResistanceCorporations.Any(s => s.Corporation is TradeUnion or Confederation) ? "EVET" : "HAYIR"),
            ("Sendika Otoritesi", p => p.Resistance.TradeUnionAuthority?.Name ?? ""),
            ("Direniş Nedenleri", p => string.Join("; ", p.Resistance.ResistanceResistanceReasons.Select(x => x.ResistanceReason.Name))),
            ("Kuruluşlar", p => string.Join("; ", p.Resistance.ResistanceCorporations?.Select(x => x.Corporation.Name) ?? NoNames)),
            ("İstihdam Türleri", p => string.Join("; ", p.Resistance.ResistanceEmploymentTypes?.Select(x => x.EmploymentType.Name) ?? NoNames)),
            ("Tepki Gösteren Sendika", p => p.Resistance.TradeUnion?.Name ?? ""),
            ("Cinsiyet", p => p.Gender?.Name ?? ""),
            ("İşten Çıkarılan Çalışan Sayısı", p => p.Resistance.FiredEmployeeCountByProtesto),
            ("Hukuki Müdahale", p => p.Resistance.AnyLegalIntervention.HasValue ? (p.Resistance.AnyLegalIntervention.Value ? "EVET" : "HAYIR") : "BİLİNMİYOR"),
            ("Hukuki Müdahale Açıklaması", p => p.Resistance.LegalInterventionDesc ?? ""),
            ("Protesto ID", p => p.Id),
            ("Protesto Sayısı", p => p.Locations.Count),
            ("Protesto Türleri", p => string.Join("; ", p.ProtestoProtestoTypes?.Select(x => x.ProtestoType.Name) ?? NoNames)),
            ("Protesto Yerleri", p => string.Join("; ", p.ProtestoProtestoPlaces?.Select(x => x.ProtestoPlace.Name) ?? NoNames)),
            ("Başlangıç Tarihi", p => p.StartDate.ToShortDateString()),
            ("Bitiş Tarihi", p => p.EndDate?.ToShortDateString() ?? ""),
            ("Grev Süresi", p => p.StrikeDuration),
            ("Konumlar", p => string.Join("; ", p.Locations.Select(x => x.District != null ? $"{x.City.Name} / {x.District.Name} {x.Place}" : $"{x.City.Name} {x.Place}"))),
            ("Protestoda Çalışan Sayısı", p => p.ProtestoEmployeeCount?.Name ?? ""),
            ("Protestoda Çalışan Sayısı (Sayısal)", p => p.EmployeeCountNumber),
            ("Müdahale Türleri", p => string.Join("; ", p.ProtestoInterventionTypes?.Select(x => x.InterventionType.Name) ?? NoNames)),
            ("Gözaltı Sayısı", p => p.CustodyCount),
            ("Not", p => p.Resistance.Note ?? ""),
            ("Protesto Notu", p => p.Note ?? ""),
            ("Direniş Sonucu", p => p.Resistance.ResistanceResult.ToString()),
            ("Direniş Haberleri", p => string.Join("; ", p.Resistance.ResistanceNews?.Select(x => x.News?.Header ?? "") ?? NoNames)),
            ("Direniş Haberleri Linki", p => string.Join("; ", p.Resistance.ResistanceNews?.Select(x => x.News?.Link ?? "") ?? NoNames)),
            // R31, added at the end so the old columns keep their places: who organized a Genel or Dayanışma case, and the
            // case a Dayanışma case supported.
            ("Örgütleyen Kurumlar", p => string.Join("; ", p.Resistance.ResistanceCorporations?.Where(x => x.Role == OrganizationRole.Organizing).Select(x => x.Corporation.Name) ?? NoNames)),
            ("Desteklenen Vaka", p => p.Resistance.SupportedResistance is { } s ? $"{s.Id}: {s.Name ?? s.Company?.Name}" : ""),
            // R37, also at the end: how the employer works in the case (“Şirket Adı” is the employer, “Ana Şirket Adı” the
            // organization above it: its Asıl işveren, İş sahibi or Varlığın sahibi) and the place, when not the employer itself.
            ("Çalışma Biçimi", p => p.Resistance.CategoryId == KnownListIds.WorkplaceCategory
                ? (p.Resistance.EmployerUnknown ? "İşveren bilinmiyor · " : "") + EmployerRoles.Name(p.Resistance.EmployerRole)
                : ""),
            ("İşyeri (Yer)", p => p.Resistance.Workplace?.Name ?? ""),

        ];

        public async Task<byte[]> Handle(ExportCasesQuery query)
        {
            var protests = await Protests(query.Filter ?? new CaseFilter()).ToListAsync();
            return ToExcel(protests);
        }

        /// <summary>The protests to export, with everything the columns read (public so a test can check the SQL).</summary>
        public IQueryable<Protesto> Protests(CaseFilter filter)
        {
            var caseIds = filter.Apply(db.Resistance).Select(r => r.Id);
            var protests = db.Protesto.Where(p => !p.Deleted && caseIds.Contains(p.ResistanceId));
            // As in the old export, the year and month also pick the protests themselves, not only the cases.
            if (filter.Year is { } year)
                protests = protests.Where(p => p.StartDate.Year == year && (filter.Month == null || p.StartDate.Month == filter.Month));
            else if (filter.Month is { } month)
                protests = protests.Where(p => p.StartDate.Month == month);

            return protests
                .Include(p => p.Resistance).ThenInclude(r => r.EmployeeCount)
                .Include(p => p.Resistance).ThenInclude(r => r.Category)
                .Include(p => p.Resistance).ThenInclude(r => r.Company).ThenInclude(c => c.CompanyWorkLine)
                .Include(p => p.Resistance).ThenInclude(r => r.Company).ThenInclude(c => c.ServiceBranch)
                .Include(p => p.Resistance).ThenInclude(r => r.MainCompany).ThenInclude(c => c.ServiceBranch)
                .Include(p => p.Resistance).ThenInclude(r => r.MainCompany).ThenInclude(c => c.CompanyWorkLine)
                .Include(p => p.Resistance).ThenInclude(r => r.ResistanceEmploymentTypes).ThenInclude(x => x.EmploymentType)
                .Include(p => p.Resistance).ThenInclude(r => r.TradeUnion)
                .Include(p => p.Resistance).ThenInclude(r => r.TradeUnionAuthority)
                .Include(p => p.Resistance).ThenInclude(r => r.ResistanceResistanceReasons).ThenInclude(x => x.ResistanceReason)
                .Include(p => p.Resistance).ThenInclude(r => r.ResistanceCorporations).ThenInclude(x => x.Corporation)
                .Include(p => p.Resistance).ThenInclude(r => r.ResistanceNews).ThenInclude(x => x.News)
                .Include(p => p.Resistance).ThenInclude(r => r.SupportedResistance).ThenInclude(s => s.Company)
                .Include(p => p.Resistance).ThenInclude(r => r.Workplace)
                .Include(p => p.Gender)
                .Include(p => p.ProtestoEmployeeCount)
                .Include(p => p.ProtestoProtestoTypes).ThenInclude(x => x.ProtestoType)
                .Include(p => p.ProtestoProtestoPlaces).ThenInclude(x => x.ProtestoPlace)
                .Include(p => p.ProtestoInterventionTypes).ThenInclude(x => x.InterventionType)
                .Include(p => p.Locations).ThenInclude(l => l.City)
                .Include(p => p.Locations).ThenInclude(l => l.District)
                .AsSplitQuery() // many collections: separate queries instead of one huge join
                .OrderBy(p => p.ResistanceId).ThenBy(p => p.StartDate);
        }

        /// <summary>One sheet "Report": bold grey headings, one row per protest, columns sized to fit.</summary>
        public static byte[] ToExcel(IReadOnlyList<Protesto> protests)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Report");
            for (var c = 0; c < Columns.Count; c++)
            {
                var cell = sheet.Cells[1, c + 1];
                cell.Value = Columns[c].Header;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }
            for (var r = 0; r < protests.Count; r++)
            {
                for (var c = 0; c < Columns.Count; c++)
                {
                    sheet.Cells[r + 2, c + 1].Value = Columns[c].Value(protests[r]);
                }
            }
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            return package.GetAsByteArray();
        }
    }
}
