using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        DismissDuplicatesHandler dismissDuplicates) : ControllerBase
    {
        public record DefinitionOptions(IReadOnlyList<Option> CorporationTypes, IReadOnlyList<Option> CompanyTypes, IReadOnlyList<Option> CompanyScales, IReadOnlyList<Option> CompanyWorklines);

        public record UpdateRequest(string Name, int? TypeId, bool AgainstProduction, bool Simple, int? ScaleId, int? WorklineId);

        public record MergeRequest(int IntoId);

        public record MergeGroupRequest(int IntoId, IReadOnlyList<int> FromIds);

        public record DismissRequest(IReadOnlyList<int> Ids);

        /// <summary>GET /api/definitions/options — the choices for the lists' own fields.</summary>
        [HttpGet("options")]
        public async Task<DefinitionOptions> Options() => new(
            await db.CorporationType.OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            await db.CompanyType.OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            await db.CompanyScale.OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            await db.CompanyWorkLine.OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync());

        /// <summary>GET /api/definitions/{kind}?q=…&amp;pending=true&amp;typeId=1&amp;page=1</summary>
        [HttpGet("{kind}")]
        public async Task<ActionResult<DefinitionPage>> List(string kind, string q, bool pending, int? typeId, int page = 1)
        {
            if (!DefinitionKinds.TryParse(kind, out var parsed))
                return UnknownKind();
            return await list.Handle(new ListDefinitionsQuery(parsed, q, pending, typeId, page));
        }

        /// <summary>PUT /api/definitions/{kind}/{id} — name and the kind's own fields. 409 names a value that already has the name.</summary>
        [HttpPut("{kind}/{id:int}")]
        public async Task<IActionResult> Update(string kind, int id, UpdateRequest request)
        {
            if (!DefinitionKinds.TryParse(kind, out var parsed))
                return UnknownKind();
            var result = await update.Handle(new UpdateDefinitionCommand(
                parsed, id, request?.Name, request?.TypeId, request?.AgainstProduction ?? false, request?.Simple ?? false, request?.ScaleId, request?.WorklineId));
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
