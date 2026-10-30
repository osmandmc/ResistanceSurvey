using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Constants;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Activity;
using RS.MVC.Features.Review;

namespace RS.MVC.Features.Definitions
{
    /// <summary>
    /// Unions and confederations (R34): which class an organization is follows its type. "Sendika" is a TradeUnion,
    /// "Konfederasyon" a Confederation, anything else a plain organization.
    /// </summary>
    public static class Organizations
    {
        public static OrganizationKind KindOf(int? typeId) => typeId switch
        {
            KnownListIds.TradeUnionCorporationType => OrganizationKind.TradeUnion,
            KnownListIds.ConfederationCorporationType => OrganizationKind.Confederation,
            _ => OrganizationKind.Organization,
        };

        public static OrganizationKind KindOf(Corporation c) => c switch
        {
            Confederation => OrganizationKind.Confederation,
            TradeUnion => OrganizationKind.TradeUnion,
            _ => OrganizationKind.Organization,
        };

        /// <summary>A new organization of the class its type calls for.</summary>
        public static Corporation New(string name, int? typeId, int? confederationId, bool approved) => KindOf(typeId) switch
        {
            OrganizationKind.TradeUnion => new TradeUnion { Name = name, CorporationTypeId = typeId, ConfederationId = confederationId, Approved = approved },
            OrganizationKind.Confederation => new Confederation { Name = name, CorporationTypeId = typeId, Approved = approved },
            _ => new Corporation { Name = name, CorporationTypeId = typeId, Approved = approved },
        };

        /// <summary>
        /// After its type changed to or from Sendika / Konfederasyon, the row changes class. EF cannot change the class of a
        /// loaded entity, so this writes the Kind column (and the union's confederation) directly. Relational databases
        /// only; the in-memory test database keeps the class an entity was created with.
        /// </summary>
        public static async Task SetKindAsync(RSDBContext db, int id, OrganizationKind kind, int? confederationId)
        {
            if (!db.Database.IsRelational())
                return;
            var union = kind == OrganizationKind.TradeUnion ? confederationId : null;
            // A union of a confederation that stops being one keeps no confederation.
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE [Corporation] SET [Kind] = {(int)kind}, [ConfederationId] = {union} WHERE [Id] = {id}");
            if (kind != OrganizationKind.Confederation)
                await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE [Corporation] SET [ConfederationId] = NULL WHERE [ConfederationId] = {id}");
        }

        /// <summary>The id if it names a confederation, else null.</summary>
        public static async Task<int?> ConfederationAsync(RSDBContext db, int? id) =>
            id is { } c && await db.Set<Confederation>().AnyAsync(x => x.Id == c) ? c : null;
    }

    /// <summary>"Kurum Ekle" (R34): name, type and, for a union, its confederation. Approved: added in Tanımlar (not from a case).</summary>
    public record CreateCorporationCommand(string Name, int? TypeId, int? ConfederationId, bool Approved);

    /// <summary>Existed: an organization of that name (case, punctuation and Turkish letters ignored) is returned instead of a duplicate.</summary>
    public record CreateCorporationResult(CorporationOption Corporation, bool Existed, string Error = null);

    public class CreateCorporationHandler(RSDBContext db)
    {
        public async Task<CreateCorporationResult> Handle(CreateCorporationCommand command)
        {
            var name = NameMatching.Clean(command.Name);
            if (name == "")
                return new CreateCorporationResult(null, false, "Lütfen kurumun adını giriniz.");
            if (command.TypeId is { } typeId && !await db.CorporationType.AnyAsync(t => t.Id == typeId))
                return new CreateCorporationResult(null, false, "Bilinmeyen kurum türü.");

            var key = DuplicateKey.For(DefinitionKind.Corporations, name);
            var existing = (await db.Corporation.Select(c => new { c.Id, c.Name }).ToListAsync())
                .FirstOrDefault(c => DuplicateKey.For(DefinitionKind.Corporations, c.Name ?? "") == key);
            if (existing != null)
            {
                var isUnion = await db.Corporation.Where(c => c.Id == existing.Id).Select(c => c is TradeUnion || c is Confederation).FirstAsync();
                return new CreateCorporationResult(new CorporationOption(existing.Id, existing.Name, isUnion), Existed: true);
            }

            var corporation = Organizations.New(name, command.TypeId, await Organizations.ConfederationAsync(db, command.ConfederationId), command.Approved);
            db.Corporation.Add(corporation);
            await db.SaveChangesAsync();
            db.LogActivity(Actions.CorporationCreate, $"Kurum {ActivityLog.Quoted(name)}", subject: "corporations", subjectId: corporation.Id);
            await db.SaveChangesAsync();
            return new CreateCorporationResult(
                new CorporationOption(corporation.Id, corporation.Name, corporation is TradeUnion or Confederation), Existed: false);
        }
    }
}
