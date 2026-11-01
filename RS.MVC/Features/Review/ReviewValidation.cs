using RS.COMMON.Constants;
using System.Collections.Generic;
using System.Linq;
using RS.COMMON.Entities;
using RS.MVC.Models;

namespace RS.MVC.Features.Review
{
    /// <summary>
    /// Required fields of the case form, checked on the server: the same rules the old form checked in the browser,
    /// plus what saving needs (a location needs city and district; AddProtest/LinkOnly need a resistance).
    /// Keys are field paths ("case.categoryId"); messages are shown to users, so they are Turkish.
    /// </summary>
    public static class ReviewValidation
    {
        /// <summary>The intervention type meaning "no intervention": with it, there is no custody count.</summary>
        public const int NoInterventionTypeId = KnownListIds.NoInterventionType;

        public static Dictionary<string, string> ForNewCase(CaseInput caseInput, ProtestInput protest)
        {
            var errors = new Dictionary<string, string>();
            ValidateCase(caseInput, errors);
            ValidateProtest(protest, errors);
            return errors;
        }

        public static Dictionary<string, string> ForAddProtest(int? resistanceId, ProtestInput protest)
        {
            var errors = new Dictionary<string, string>();
            RequireResistance(resistanceId, errors);
            ValidateProtest(protest, errors);
            return errors;
        }

        /// <summary>The case part alone (editing a case).</summary>
        public static Dictionary<string, string> ForCase(CaseInput caseInput)
        {
            var errors = new Dictionary<string, string>();
            ValidateCase(caseInput, errors);
            return errors;
        }

        /// <summary>One protest alone (adding or editing a protest on the case page).</summary>
        public static Dictionary<string, string> ForProtest(ProtestInput protest)
        {
            var errors = new Dictionary<string, string>();
            ValidateProtest(protest, errors);
            return errors;
        }

        public static Dictionary<string, string> ForLinkOnly(int? resistanceId)
        {
            var errors = new Dictionary<string, string>();
            RequireResistance(resistanceId, errors);
            return errors;
        }

        private static void RequireResistance(int? resistanceId, Dictionary<string, string> errors)
        {
            if (resistanceId is not > 0)
                errors["resistanceId"] = "Lütfen bir vaka seçiniz.";
        }

        private static void ValidateCase(CaseInput c, Dictionary<string, string> errors)
        {
            if (c == null)
            {
                errors["case"] = "Vaka bilgileri eksik.";
                return;
            }
            if (string.IsNullOrWhiteSpace(c.Description)) errors["case.description"] = "Lütfen kısa açıklama giriniz.";
            if (c.CategoryId is not (KnownListIds.WorkplaceCategory or KnownListIds.SolidarityCategory or KnownListIds.GeneralCategory))
                errors["case.categoryId"] = "Lütfen vaka niteliğini seçiniz.";
            if (!HasAny(c.ReasonIds, c.NewReasonNames)) errors["case.reasonIds"] = "Lütfen en az bir vaka nedeni seçiniz.";
            if (c.DevelopRight == null) errors["case.developRight"] = "Lütfen hak geliştirme / hak savunma seçiniz.";

            // R31: the workplace's fields only for İşletme; Genel and Dayanışma name their organizers (optional: kurumsuz).
            if (c.CategoryId != KnownListIds.WorkplaceCategory)
                return;
            if (c.CompanyId is not > 0) errors["case.companyId"] = "Lütfen işyerini seçiniz.";
            if (c.IsOutsource && c.MainCompanyId is not > 0) errors["case.mainCompanyId"] = "Lütfen ana şirketi seçiniz.";
            if (!HasAny(c.CorporationIds, c.NewCorporationNames)) errors["case.corporationIds"] = "Lütfen en az bir kurumsallık seçiniz.";
            if (c.EmploymentTypeIds is not { Count: > 0 }) errors["case.employmentTypeIds"] = "Lütfen en az bir istihdam türü seçiniz.";
            if (c.Result == null) errors["case.result"] = "Lütfen sonucu seçiniz.";
        }

        // Picked from the list, or typed as a new value.
        private static bool HasAny(List<int> ids, List<string> newNames) =>
            ids is { Count: > 0 } || (newNames != null && newNames.Exists(n => !string.IsNullOrWhiteSpace(n)));

        private static void ValidateProtest(ProtestInput p, Dictionary<string, string> errors)
        {
            if (p == null)
            {
                errors["protest"] = "Eylem bilgileri eksik.";
                return;
            }
            if (p.ProtestTypeIds is not { Count: > 0 }) errors["protest.protestTypeIds"] = "Lütfen en az bir eylem türü seçiniz.";
            if (p.StartDate == null) errors["protest.startDate"] = "Lütfen başlangıç tarihini seçiniz.";
            else if (!p.Ongoing && p.EndDate != null && p.StartDate > p.EndDate) errors["protest.startDate"] = "Başlangıç tarihi bitiş tarihinden sonra olamaz.";
            // An empty end date only on purpose: "Eylem sürüyor" (2026-11-01); before, it could mean ongoing or forgotten.
            if (!p.Ongoing && p.EndDate == null) errors["protest.endDate"] = "Bitiş tarihini girin ya da “Eylem sürüyor”u işaretleyin.";
            if (p.PlaceIds is not { Count: > 0 }) errors["protest.placeIds"] = "Lütfen en az bir eylem yeri seçiniz.";
            if (p.GenderId is not > 0) errors["protest.genderId"] = "Lütfen cinsiyeti seçiniz.";
            if (p.InterventionTypeIds is not { Count: > 0 }) errors["protest.interventionTypeIds"] = "Lütfen en az bir müdahale tipi seçiniz.";
            else if (!p.InterventionTypeIds.Contains(NoInterventionTypeId) && p.CustodyCount == null)
                errors["protest.custodyCount"] = "Lütfen gözaltı sayısını giriniz (yoksa 0).";
            if (p.Locations != null && p.Locations.Any(l => l.CityId is not > 0 || l.DistrictId is not > 0))
                errors["protest.locations"] = "Her lokasyon için il ve ilçe seçiniz.";
        }
    }
}
