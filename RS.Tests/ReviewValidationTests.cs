using RS.COMMON.Entities;
using RS.MVC.Features.Review;
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.Tests;

public class ReviewValidationTests
{
    /// <summary>A complete new case: every required field filled.</summary>
    internal static ReviewSaveRequest CompleteNewCase() => new()
    {
        Action = ReviewAction.NewCase,
        Case = new CaseInput
        {
            Name = "Örnek Tekstil ücret direnişi",
            Description = "120 işçi ödenmeyen ücretler için iş bıraktı.",
            CategoryId = 1,
            TargetType = ResistanceTargetType.Company,
            CompanyId = 10,
            ReasonIds = [3],
            DevelopRight = false,
            CorporationIds = [4],
            EmploymentTypeIds = [5],
            Result = ResistanceResult.Unknown,
            EmployeeCount = 120,
        },
        Protest = CompleteProtest(),
    };

    internal static ProtestInput CompleteProtest() => new()
    {
        ProtestTypeIds = [1, 2],
        SimpleDescription = "Vardiya sonunda 15 dakika iş bırakma",
        StartDate = new DateTime(2026, 10, 12),
        PlaceIds = [6],
        GenderId = 1,
        InterventionTypeIds = [ReviewValidation.NoInterventionTypeId],
        Locations = [new LocationInput { CityId = 16, DistrictId = 160, Place = "Fabrika önü" }],
    };

    private static Dictionary<string, string> ForNewCase(ReviewSaveRequest request) =>
        ReviewValidation.ForNewCase(request.Case!, request.Protest!);

    [Fact]
    public void ACompleteNewCaseIsValid() => Assert.Empty(ForNewCase(CompleteNewCase()));

    [Fact]
    public void AnEmptyNewCaseNamesEveryRequiredField()
    {
        var errors = ReviewValidation.ForNewCase(new CaseInput(), new ProtestInput());

        Assert.Equal(
            [
                "case.categoryId", "case.corporationIds", "case.description", "case.developRight", "case.employmentTypeIds",
                "case.reasonIds", "case.result", "case.targetType",
                "protest.genderId", "protest.interventionTypeIds", "protest.placeIds", "protest.protestTypeIds", "protest.startDate",
            ],
            errors.Keys.Order());
    }

    [Fact]
    public void StartMustNotBeAfterEnd()
    {
        var request = CompleteNewCase();
        request.Protest.EndDate = new DateTime(2026, 10, 1);

        Assert.Equal(["protest.startDate"], ForNewCase(request).Keys);
    }

    [Theory]
    [InlineData(null, true)]   // intervention without a custody count
    [InlineData(0, false)]     // 0 detentions is a valid answer
    [InlineData(3, false)]
    public void CustodyCountIsRequiredWhenThereWasAnIntervention(int? custody, bool missing)
    {
        var request = CompleteNewCase();
        request.Protest.InterventionTypeIds = [2];
        request.Protest.CustodyCount = custody;

        Assert.Equal(missing, ForNewCase(request).ContainsKey("protest.custodyCount"));
    }

    [Fact]
    public void ALocationNeedsCityAndDistrict()
    {
        var request = CompleteNewCase();
        request.Protest.Locations = [new LocationInput { CityId = 16 }];

        Assert.Equal(["protest.locations"], ForNewCase(request).Keys);
    }

    [Fact]
    public void ASubcontractorNeedsTheMainCompany()
    {
        var request = CompleteNewCase();
        request.Case!.IsOutsource = true;

        Assert.Equal(["case.mainCompanyId"], ForNewCase(request).Keys);
    }

    [Fact]
    public void AddProtestNeedsACaseAndAProtest()
    {
        var errors = ReviewValidation.ForAddProtest(null, null!);

        Assert.Equal(["protest", "resistanceId"], errors.Keys.Order());
    }

    [Fact]
    public void LinkOnlyNeedsOnlyTheCase()
    {
        Assert.Empty(ReviewValidation.ForLinkOnly(412));
        Assert.Equal(["resistanceId"], ReviewValidation.ForLinkOnly(null).Keys);
    }
}
