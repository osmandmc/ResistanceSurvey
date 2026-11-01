using System.Linq;
using RS.COMMON.Entities;

namespace RS.MVC.Features.Cases
{
    /// <summary>
    /// The case list's filters, shared by the list and the Excel export. The old filters (company, main company,
    /// category, protest year/month, has notes) plus text, city and "ongoing only". Year and month work on their own
    /// (the old list needed both).
    /// </summary>
    public record CaseFilter(
        string Text = null,
        int? CompanyId = null,
        int? MainCompanyId = null,
        int? CategoryId = null,
        int? Year = null,
        int? Month = null,
        int? CityId = null,
        bool? HasNote = null,
        bool OngoingOnly = false)
    {
        /// <summary>Cases matching every filter that is set; deleted cases are left out.</summary>
        public IQueryable<Resistance> Apply(IQueryable<Resistance> cases)
        {
            cases = cases.Where(r => !r.Deleted);
            var text = Text?.Trim() ?? "";
            if (text != "")
                cases = cases.Where(r => r.Name.Contains(text) || r.Description.Contains(text)
                    || (r.Company != null && r.Company.Name.Contains(text))
                    || (r.MainCompany != null && r.MainCompany.Name.Contains(text))
                    || r.ResistanceCorporations.Any(x => x.Corporation.Name.Contains(text)));
            if (CompanyId is { } companyId)
                cases = cases.Where(r => r.CompanyId == companyId);
            if (MainCompanyId is { } mainId)
                cases = cases.Where(r => r.MainCompanyId == mainId);
            if (CategoryId is { } categoryId)
                cases = cases.Where(r => r.CategoryId == categoryId);
            if (Year is { } year)
                cases = cases.Where(r => r.Protestos.Any(p => !p.Deleted && p.StartDate.Year == year && (Month == null || p.StartDate.Month == Month)));
            else if (Month is { } month)
                cases = cases.Where(r => r.Protestos.Any(p => !p.Deleted && p.StartDate.Month == month));
            if (CityId is { } cityId)
                cases = cases.Where(r => r.Protestos.Any(p => !p.Deleted && p.Locations.Any(l => l.CityId == cityId)));
            // A note on the case itself (“Kişisel Notlar”) or on any of its protests (“Kontrol Kişisine Notlar”).
            if (HasNote is { } hasNote)
                cases = cases.Where(r => ((r.Note != null && r.Note.Trim() != "")
                    || r.Protestos.Any(p => !p.Deleted && p.Note != null && p.Note.Trim() != "")) == hasNote);
            if (OngoingOnly)
                cases = cases.Where(r => r.Protestos.Where(p => !p.Deleted).OrderByDescending(p => p.StartDate).Select(p => p.EndDate == null).FirstOrDefault());
            return cases;
        }
    }
}
