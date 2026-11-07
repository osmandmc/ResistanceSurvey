using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;

namespace RS.DatabaseTests;

/// <summary>
/// A few made-up cases, companies and protests (R41). Never real data: both repositories are public. The names carry
/// Turkish capitals on purpose (İ, I, Ş, Ç), so searches with small letters show whether the collation works.
/// </summary>
public sealed class MadeUpData
{
    public required Category Wages { get; init; }
    public required City Izmir { get; init; }
    public required Company Metal { get; init; }
    public required Company Holding { get; init; }
    public required Company Isik { get; init; }
    public required Resistance MetalCase { get; init; }
    public required Resistance IsikCase { get; init; }
    public required Resistance OldCase { get; init; }

    public static async Task<MadeUpData> AddAsync(RSDBContext db)
    {
        var wages = new Category { Name = "Ücret" };
        var izmir = new City { Name = "İzmir" };
        var holding = new Company { Name = "ÖRNEK HOLDİNG" };
        var metal = new Company { Name = "İŞÇİ METAL SANAYİ" };
        var isik = new Company { Name = "IŞIK TEKSTİL" };
        var union = new Corporation { Name = "Örnek Metal İşçileri Sendikası", Approved = true };

        Resistance Case(string name, Company company, DateTime start, DateTime? end = null, Company? main = null) => new()
        {
            Name = name,
            Description = "Uydurma bir vaka: testler için.",
            Category = wages,
            Company = company,
            MainCompany = main,
            Protestos = [new Protesto { StartDate = start, EndDate = end, Locations = [new ProtestoLocation { City = izmir }] }],
            // The union organizes only the metal company's cases.
            ResistanceCorporations = company == metal ? [new ResistanceCorporation { Corporation = union, Role = OrganizationRole.Organizing }] : [],
            ResistanceNews = [new ResistanceNews { News = new News { Header = $"{name} sürüyor", Link = $"https://example.org/{Guid.NewGuid():N}", Date = start } }],
        };

        var data = new MadeUpData
        {
            Wages = wages,
            Izmir = izmir,
            Metal = metal,
            Holding = holding,
            Isik = isik,
            MetalCase = Case("İŞÇİLER ÜCRET İÇİN DİRENİYOR", metal, new DateTime(2026, 9, 1), main: holding),
            IsikCase = Case("Işık Tekstil grevi", isik, new DateTime(2026, 10, 1)),
            OldCase = Case("Eski direniş", metal, new DateTime(2025, 3, 1), new DateTime(2025, 3, 2)),
        };
        db.Resistance.AddRange(data.MetalCase, data.IsikCase, data.OldCase);
        await db.SaveChangesAsync();
        return data;
    }
}
