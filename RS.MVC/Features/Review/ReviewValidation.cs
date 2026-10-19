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
        public const int NoInterventionTypeId = 7;

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
            if (c.CategoryId is not > 0) errors["case.categoryId"] = "Lütfen vaka niteliğini seçiniz.";
            if (c.TargetType == null) errors["case.targetType"] = "Lütfen kime karşı olduğunu seçiniz.";
            if (c.IsOutsource && c.MainCompanyId is not > 0) errors["case.mainCompanyId"] = "Lütfen ana şirketi seçiniz.";
            if (c.ReasonIds is not { Count: > 0 }) errors["case.reasonIds"] = "Lütfen en az bir vaka nedeni seçiniz.";
            if (c.DevelopRight == null) errors["case.developRight"] = "Lütfen hak geliştirme / hak savunma seçiniz.";
            if (c.CorporationIds is not { Count: > 0 }) errors["case.corporationIds"] = "Lütfen en az bir kurumsallık seçiniz.";
            if (c.EmploymentTypeIds is not { Count: > 0 }) errors["case.employmentTypeIds"] = "Lütfen en az bir istihdam türü seçiniz.";
            if (c.Result == null) errors["case.result"] = "Lütfen sonucu seçiniz.";
        }

        private static void ValidateProtest(ProtestInput p, Dictionary<string, string> errors)
        {
            if (p == null)
            {
                errors["protest"] = "Eylem bilgileri eksik.";
                return;
            }
            if (p.ProtestTypeIds is not { Count: > 0 }) errors["protest.protestTypeIds"] = "Lütfen en az bir eylem türü seçiniz.";
            if (p.StartDate == null) errors["protest.startDate"] = "Lütfen başlangıç tarihini seçiniz.";
            else if (p.EndDate != null && p.StartDate > p.EndDate) errors["protest.startDate"] = "Başlangıç tarihi bitiş tarihinden sonra olamaz.";
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
