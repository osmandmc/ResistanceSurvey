using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RS.COMMON.DTO;
using RS.MVC.Applications;
using RS.MVC.Controllers;
using RS.MVC.Models;
using Xunit;

namespace RS.Tests.Api.Controllers;

public class ResistanceApiControllerTests
{
    [Fact]
    public void List_ReturnsJsonResult_WithPagedResult()
    {
        // Arrange
        var mockApp = new Mock<IResistanceApplication>();
        var paged = new PagedResult<ResistanceIndexDto, ResistanceFilterModel>
        {
            Items = new List<ResistanceIndexDto>(),
            Total = 0,
            Page = 1,
            PageSize = 10,
            Filter = new ResistanceFilterModel()
        };
        mockApp.Setup(m => m.GetPaged(It.IsAny<ResistanceFilterModel>())).Returns(paged);

        // The controller requires an RSDBContext parameter; pass null for this unit test since List does not use the DB directly.
        var controller = new ResistanceApiController(mockApp.Object, null);

        // Act
        var result = controller.List(new ResistanceFilterModel());

        // Assert
        var json = Assert.IsType<JsonResult>(result);
        Assert.Same(paged, json.Value);
        mockApp.Verify(m => m.GetPaged(It.IsAny<ResistanceFilterModel>()), Times.Once);
    }
}
