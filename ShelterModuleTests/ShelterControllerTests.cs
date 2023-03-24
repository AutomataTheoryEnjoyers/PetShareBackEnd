using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ShelterModule.Controllers;
using ShelterModule.Models.Shelters;
using ShelterModule.Services.Interfaces.Shelters;
using ShelterModuleTests.Fixtures;
using Xunit;

namespace ShelterModuleTests;

public class ShelterControllerTests
{
    [Fact]
    public async Task GetAll_OnSuccess_Invokes_Query_Once()
    {
        // arrange
        var mockShelterCommand = new Mock<IShelterCommand>();
        var mockShelterQuery = new Mock<IShelterQuery>();
        mockShelterQuery.Setup(service => service.GetAllAsync(CancellationToken.None))
            .ReturnsAsync(ShelterFixture.GetTestShelters());

        var sc = new ShelterController(mockShelterQuery.Object, mockShelterCommand.Object);

        // act
        var result = await sc.GetAll();

        // assert
        mockShelterQuery.Verify(service => service.GetAllAsync(CancellationToken.None), Times.Once());
    }

    [Fact]
    public async Task GetAll_OnSuccess_Returns_List_Of_ShelterResponses()
    {
        // arrange
        var mockShelterCommand = new Mock<IShelterCommand>();
        var mockShelterQuery = new Mock<IShelterQuery>();
        mockShelterQuery.Setup(service => service.GetAllAsync(CancellationToken.None))
            .ReturnsAsync(ShelterFixture.GetTestShelters());

        var sc = new ShelterController(mockShelterQuery.Object, mockShelterCommand.Object);

        // act
        var result = await sc.GetAll();

        // assert
        result.Should().BeOfType<List<ShelterResponse>>();
        // TO DO check status code
    }

    [Fact]
    public async Task Get_OnSuccess_Invokes_Query_Once()
    {
        // arrange
        var mockShelterCommand = new Mock<IShelterCommand>();
        var mockShelterQuery = new Mock<IShelterQuery>();
        mockShelterQuery.Setup(service => service.GetByIdAsync(ShelterFixture.GetTestShelter().Id, CancellationToken.None))
            .ReturnsAsync(ShelterFixture.GetTestShelter());

        var sc = new ShelterController(mockShelterQuery.Object, mockShelterCommand.Object);

        // act
        var result = await sc.Get(ShelterFixture.GetTestShelter().Id);

        // assert
        mockShelterQuery.Verify(service => service.GetByIdAsync(ShelterFixture.GetTestShelter().Id, CancellationToken.None), Times.Once());
    }

    [Fact]
    public async Task Get_OnSuccess_Returns_ShelterResponse()
    {
        // arrange
        var mockShelterCommand = new Mock<IShelterCommand>();
        var mockShelterQuery = new Mock<IShelterQuery>();
        mockShelterQuery.Setup(service => service.GetByIdAsync(ShelterFixture.GetTestShelter().Id, CancellationToken.None))
            .ReturnsAsync(ShelterFixture.GetTestShelter());

        var sc = new ShelterController(mockShelterQuery.Object, mockShelterCommand.Object);

        // act
        var actionResult = await sc.Get(ShelterFixture.GetTestShelter().Id);

        // assert
        actionResult.Should().BeOfType<ActionResult<ShelterResponse>>();

        var result = actionResult.Result as OkObjectResult;
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be(StatusCodes.Status200OK);
        result?.Value.Should().BeEquivalentTo(ShelterFixture.GetTestShelter().ToResponse());
    }

    [Fact]
    public async Task Get_OnFailure_Returns_NotFound()
    {
        // arrange
        var mockShelterCommand = new Mock<IShelterCommand>();
        var mockShelterQuery = new Mock<IShelterQuery>();
        mockShelterQuery.Setup(service => service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None))
            .ReturnsAsync(null as Shelter);

        var sc = new ShelterController(mockShelterQuery.Object, mockShelterCommand.Object);

        // act
        var actionResult = await sc.Get(Guid.NewGuid());

        // assert
        actionResult.Should().BeOfType<ActionResult<ShelterResponse>>();

        var result = actionResult.Result as NotFoundObjectResult;
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}
