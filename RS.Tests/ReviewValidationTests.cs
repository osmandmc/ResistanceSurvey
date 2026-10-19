using RS.COMMON.Entities;
using RS.MVC.Applications;
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

    [Fact]
    public void ACompleteNewCaseIsValid() => Assert.Empty(ReviewValidation.Validate(CompleteNewCase()));

    [Fact]
    public void AnEmptyNewCaseNamesEveryRequiredField()
    {
        var errors = ReviewValidation.Validate(new ReviewSaveRequest { Action = ReviewAction.NewCase, Case = new(), Protest = new() });

        Assert.Equal(
            [
                "case.categoryId", "case.corporationIds", "case.description", "case.developRight", "case.employmentTypeIds",
                "case.reasonIds", "case.result", "case.targetType",
                "protest.genderId", "protest.interventionTypeIds", "protest.placeIds", "protest.protestTypeIds", "protest.startDate",
            ],
            errors.Keys.Order());
    }

    [Fact]
    public void AnUnknownActionIsRejected() =>
        Assert.Contains("action", ReviewValidation.Validate(new ReviewSaveRequest { Action = "delete" }).Keys);

    [Fact]
    public void StartMustNotBeAfterEnd()
    {
        var request = CompleteNewCase();
        request.Protest.EndDate = new DateTime(2026, 10, 1);

        Assert.Equal(["protest.startDate"], ReviewValidation.Validate(request).Keys);
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

        Assert.Equal(missing, ReviewValidation.Validate(request).ContainsKey("protest.custodyCount"));
    }

    [Fact]
    public void ALocationNeedsCityAndDistrict()
    {
        var request = CompleteNewCase();
        request.Protest.Locations = [new LocationInput { CityId = 16 }];

        Assert.Equal(["protest.locations"], ReviewValidation.Validate(request).Keys);
    }

    [Fact]
    public void ASubcontractorNeedsTheMainCompany()
    {
        var request = CompleteNewCase();
        request.Case!.IsOutsource = true;

        Assert.Equal(["case.mainCompanyId"], ReviewValidation.Validate(request).Keys);
    }

    [Fact]
    public void AddProtestNeedsACaseAndAProtest()
    {
        var errors = ReviewValidation.Validate(new ReviewSaveRequest { Action = ReviewAction.AddProtest });

        Assert.Equal(["protest", "resistanceId"], errors.Keys.Order());
    }

    [Fact]
    public void LinkOnlyNeedsOnlyTheCase()
    {
        Assert.Empty(ReviewValidation.Validate(new ReviewSaveRequest { Action = ReviewAction.LinkOnly, ResistanceId = 412 }));
        Assert.Equal(["resistanceId"], ReviewValidation.Validate(new ReviewSaveRequest { Action = ReviewAction.LinkOnly }).Keys);
    }
}
