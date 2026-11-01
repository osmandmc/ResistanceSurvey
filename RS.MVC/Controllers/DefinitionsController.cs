using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.COMMON.Entities;
using Microsoft.EntityFrameworkCore;
using RS.EF;
using RS.MVC.Features.Definitions;
using RS.MVC.Features.Review;

namespace RS.MVC.Controllers
{
    /// <summary>
    /// The new app's Tanımlar (/app/definitions/{kind}): the lists researchers keep clean. Kind: companies, corporations,
    /// reasons, protest-types, protest-places.
    /// </summary>
    [ApiController]
    [Route("api/definitions")]
    [Authorize]
    public class DefinitionsController(
        RSDBContext db,
        ListDefinitionsHandler list,
        UpdateDefinitionHandler update,
        ApproveDefinitionHandler approve,
        DeleteDefinitionHandler delete,
        MergeDefinitionsHandler merge,
        FindDuplicatesHandler findDuplicates,
        MergeGroupHandler mergeGroup,
        DismissDuplicatesHandler dismissDuplicates,
        GetCompanyHandler getCompany,
        SaveCompanyRelationHandler saveRelation,
        RemoveCompanyRelationHandler removeRelation,
        CreateCorporationHandler createCorporation,
        CreateWorkplaceHandler createWorkplace) : ControllerBase
    {
        /// <summary>CompanyWorklines: the workers' işkolları; ServiceBranches: the civil servants' hizmet kolları (R37).</summary>
        public record DefinitionOptions(IReadOnlyList<Option> CorporationTypes, IReadOnlyList<Option> CompanyTypes, IReadOnlyList<Option> CompanyScales,
            IReadOnlyList<Option> CompanyWorklines, IReadOnlyList<Option> Confederations, IReadOnlyList<Option> ServiceBranches = null);

        public record CreateCorporationRequest(string Name, int? TypeId, int? ConfederationId);

        public record UpdateRequest(string Name, int? TypeId, bool AgainstProduction, bool Simple, int? ScaleId, int? WorklineId, int? ConfederationId = null,
            InstitutionKind? InstitutionKind = null, int? ServiceBranchId = null, int? CityId = null, int? DistrictId = null, int? OwnerCompanyId = null);

        public record CreateWorkplaceRequest(string Name, int? CityId, int? DistrictId, int? OwnerCompanyId);

        public record MergeRequest(int IntoId);

        public record MergeGroupRequest(int IntoId, IReadOnlyList<int> FromIds);

        public record DismissRequest(IReadOnlyList<int> Ids);

        /// <summary>A fact with another company (R37, part B); ThisIsFirst: “this company → the other”.</summary>
        public record RelationRequest(CompanyRelationKind Kind, int OtherCompanyId, bool ThisIsFirst, decimal? SharePercent, DateTime? StartDate,
            DateTime? EndDate, string SourceUrl, string Note);

        /// <summary>GET /api/definitions/options — the choices for the lists' own fields.</summary>
        [HttpGet("options")]
        public async Task<DefinitionOptions> Options() => new(
            await db.CorporationType.OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            await db.CompanyType.OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            await db.CompanyScale.OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            await db.CompanyWorkLine.Where(e => !e.IsCivilService).OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            await db.Set<COMMON.Entities.LookupEntity.Confederation>().OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            await db.CompanyWorkLine.Where(e => e.IsCivilService).OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync());

        /// <summary>POST /api/definitions/workplaces — a place added in Tanımlar (approved; R37); the same name in the same province is returned instead.</summary>
        [HttpPost("workplaces")]
        public async Task<ActionResult<CreateWorkplaceResult>> CreateWorkplace(CreateWorkplaceRequest request)
        {
            var result = await createWorkplace.Handle(new CreateWorkplaceCommand(request?.Name, request?.CityId, request?.DistrictId, request?.OwnerCompanyId, Approved: true));
            return result.Error == null ? result : BadRequest(new { error = result.Error });
        }

        /// <summary>POST /api/definitions/corporations — a new organization added in Tanımlar (approved); an existing one of the same name is returned instead.</summary>
        [HttpPost("corporations")]
        public async Task<ActionResult<CreateCorporationResult>> CreateCorporation(CreateCorporationRequest request)
        {
            var result = await createCorporation.Handle(new CreateCorporationCommand(request?.Name, request?.TypeId, request?.ConfederationId, Approved: true));
            return result.Error == null ? result : BadRequest(new { error = result.Error });
        }

        /// <summary>GET /api/definitions/{kind}?q=…&amp;pending=true&amp;typeId=1&amp;unused=true&amp;page=1 (unused: Şirketler only)</summary>
        [HttpGet("{kind}")]
        public async Task<ActionResult<DefinitionPage>> List(string kind, string q, bool pending, int? typeId, bool unused = false, int page = 1)
        {
            if (!DefinitionKinds.TryParse(kind, out var parsed))
                return UnknownKind();
            return await list.Handle(new ListDefinitionsQuery(parsed, q, pending, typeId, page, unused));
        }

        /// <summary>PUT /api/definitions/{kind}/{id} — name and the kind's own fields. 409 names a value that already has the name.</summary>
        [HttpPut("{kind}/{id:int}")]
        public async Task<IActionResult> Update(string kind, int id, UpdateRequest request)
        {
            if (!DefinitionKinds.TryParse(kind, out var parsed))
                return UnknownKind();
            var result = await update.Handle(new UpdateDefinitionCommand(
                parsed, id, request?.Name, request?.TypeId, request?.AgainstProduction ?? false, request?.Simple ?? false, request?.ScaleId, request?.WorklineId, request?.ConfederationId,
                request?.InstitutionKind, request?.ServiceBranchId, request?.CityId, request?.DistrictId, request?.OwnerCompanyId));
            return Answer(result);
        }

        /// <summary>POST /api/definitions/{kind}/{id}/approve</summary>
        [HttpPost("{kind}/{id:int}/approve")]
        public async Task<IActionResult> Approve(string kind, int id)
        {
            if (!DefinitionKinds.TryParse(kind, out var parsed))
                return UnknownKind();
            return Answer(await approve.Handle(new ApproveDefinitionCommand(parsed, id)));
        }

        /// <summary>DELETE /api/definitions/{kind}/{id} — only unused values; 409 otherwise (merge instead).</summary>
        [HttpDelete("{kind}/{id:int}")]
        public async Task<IActionResult> Delete(string kind, int id)
        {
            if (!DefinitionKinds.TryParse(kind, out var parsed))
                return UnknownKind();
            return Answer(await delete.Handle(new DeleteDefinitionCommand(parsed, id)));
        }

        /// <summary>GET /api/definitions/{kind}/{id}/merge-preview — how many live cases (or protests) a merge would touch.</summary>
        [HttpGet("{kind}/{id:int}/merge-preview")]
        public async Task<ActionResult<object>> MergePreview(string kind, int id)
        {
            if (!DefinitionKinds.TryParse(kind, out var parsed))
                return UnknownKind();
            return new { affected = await merge.PreviewAsync(parsed, id) };
        }

        /// <summary>POST /api/definitions/{kind}/{id}/merge { intoId } — everything that used {id} uses intoId; {id} goes.</summary>
        [HttpPost("{kind}/{id:int}/merge")]
        public async Task<ActionResult<object>> Merge(string kind, int id, MergeRequest request)
        {
            if (!DefinitionKinds.TryParse(kind, out var parsed))
                return UnknownKind();
            var result = await merge.Handle(new MergeDefinitionsCommand(parsed, id, request?.IntoId ?? 0));
            return result.Status switch
            {
                MergeStatus.Ok => new { affected = result.Affected },
                MergeStatus.SameValue => BadRequest(new { error = "Bir değer kendisiyle birleştirilemez." }),
                _ => NotFound(new { error = "Değer bulunamadı." }),
            };
        }

        /// <summary>GET /api/definitions/{kind}/duplicates — groups of likely duplicates, biggest first.</summary>
        [HttpGet("{kind}/duplicates")]
        public async Task<ActionResult<IReadOnlyList<DuplicateGroup>>> Duplicates(string kind)
        {
            if (!DefinitionKinds.TryParse(kind, out var parsed))
                return UnknownKind();
            return Ok(await findDuplicates.Handle(parsed));
        }

        /// <summary>POST /api/definitions/{kind}/duplicates/merge { intoId, fromIds } — all in one go, or nothing.</summary>
        [HttpPost("{kind}/duplicates/merge")]
        public async Task<ActionResult<object>> MergeGroup(string kind, MergeGroupRequest request)
        {
            if (!DefinitionKinds.TryParse(kind, out var parsed))
                return UnknownKind();
            var result = await mergeGroup.Handle(new MergeGroupCommand(parsed, request?.IntoId ?? 0, request?.FromIds ?? []));
            return result.Status switch
            {
                MergeStatus.Ok => new { affected = result.Affected },
                MergeStatus.SameValue => BadRequest(new { error = "Birleştirilecek başka değer seçin." }),
                _ => NotFound(new { error = "Değerlerden biri bulunamadı; liste yenilendi mi?" }),
            };
        }

        /// <summary>POST /api/definitions/{kind}/duplicates/dismiss { ids } — "aynı değil".</summary>
        [HttpPost("{kind}/duplicates/dismiss")]
        public async Task<IActionResult> Dismiss(string kind, DismissRequest request)
        {
            if (!DefinitionKinds.TryParse(kind, out var parsed))
                return UnknownKind();
            await dismissDuplicates.Handle(new DismissDuplicatesCommand(parsed, request?.Ids ?? []));
            return NoContent();
        }

        /// <summary>GET /api/definitions/companies/{id} — the company with its cases, its facts with other companies and its links from cases.</summary>
        [HttpGet("companies/{id:int}")]
        public async Task<ActionResult<CompanyDetail>> Company(int id) =>
            await getCompany.Handle(id) is { } company ? company : NotFound(new { error = "Şirket bulunamadı." });

        /// <summary>POST /api/definitions/companies/{id}/relations — a fact with another company (R37, part B).</summary>
        [HttpPost("companies/{id:int}/relations")]
        public async Task<IActionResult> AddRelation(int id, RelationRequest request) =>
            RelationAnswer(await saveRelation.Handle(new SaveCompanyRelationCommand(id, null, request.Kind, request.OtherCompanyId, request.ThisIsFirst,
                request.SharePercent, request.StartDate, request.EndDate, request.SourceUrl, request.Note, User.Identity?.Name)));

        /// <summary>PUT /api/definitions/companies/{id}/relations/{relationId} — its share, dates, source and note.</summary>
        [HttpPut("companies/{id:int}/relations/{relationId:int}")]
        public async Task<IActionResult> ChangeRelation(int id, int relationId, RelationRequest request) =>
            RelationAnswer(await saveRelation.Handle(new SaveCompanyRelationCommand(id, relationId, request.Kind, request.OtherCompanyId, request.ThisIsFirst,
                request.SharePercent, request.StartDate, request.EndDate, request.SourceUrl, request.Note, User.Identity?.Name)));

        [HttpDelete("companies/{id:int}/relations/{relationId:int}")]
        public async Task<IActionResult> RemoveRelation(int id, int relationId) =>
            RelationAnswer(await removeRelation.Handle(new RemoveCompanyRelationCommand(id, relationId)));

        private IActionResult RelationAnswer(RelationResult result) => result.Status switch
        {
            RelationStatus.Ok => result.Id is { } created ? Ok(new { id = created }) : NoContent(),
            RelationStatus.Invalid => BadRequest(new { error = result.Error }),
            _ => NotFound(new { error = "Şirket ya da ilişki bulunamadı." }),
        };

        private IActionResult Answer(ChangeResult result) => result.Status switch
        {
            ChangeStatus.Ok => NoContent(),
            ChangeStatus.Invalid => BadRequest(new { error = "Ad boş olamaz.", fields = new Dictionary<string, string> { ["name"] = "Ad boş olamaz." } }),
            ChangeStatus.Duplicate => Conflict(new
            {
                error = $"“{result.ExistingName}” zaten var. İkisini birleştirebilirsiniz.",
                existingId = result.ExistingId,
                existingName = result.ExistingName,
            }),
            ChangeStatus.InUse => Conflict(new
            {
                error = result.Usage > 0
                    ? $"Bu değer {result.Usage} kayıtta kullanılıyor; silmek yerine başka bir değerle birleştirin."
                    : "Bu değer yalnızca silinmiş kayıtlarda kullanılıyor; silmek yerine başka bir değerle birleştirin.",
                usage = result.Usage,
            }),
            _ => NotFound(new { error = "Değer bulunamadı." }),
        };

        private BadRequestObjectResult UnknownKind() =>
            BadRequest(new { error = "Liste: companies, corporations, reasons, protest-types, protest-places" });
    }
}
