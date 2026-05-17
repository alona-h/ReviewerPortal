using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using ReviewerPortal.API.Application.Exceptions;
using ReviewerPortal.API.Application.Interfaces;
using ReviewerPortal.API.Application.Models;
using ReviewerPortal.API.Application.Services;
using ReviewerPortal.API.Domain.Entities;
using ReviewerPortal.API.Infrastructure.Persistence;

namespace ReviewerPortal.Tests;

public class UniversityServiceTests
{
    private readonly Mock<IUniversityApiClient> _apiClientMock = new();

    private static AppDbContext CreateContext() => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private UniversityService CreateService(AppDbContext context) =>
        new(context, _apiClientMock.Object);

    [Fact]
    public async Task GetOrCreateUniversityAsync_ReturnsExistingUniversity_WhenUniversityNameFoundInDb()
    {
        // Arrange
        using var context = CreateContext();
        context.Universities.Add(new University { UniversityId = 1, UniversityName = "Oxford", Score = 100 });
        await context.SaveChangesAsync();

        // Act
        var result = await CreateService(context).GetOrCreateUniversityAsync("Oxford");

        // Assert
        result.UniversityName.Should().Be("Oxford");
        result.Score.Should().Be(100);
        _apiClientMock.Verify(c => c.FindAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateUniversityAsync_ReturnsCachedUniversity_WhenOrganizationNameFoundInDbAfterApiCall()
    {
        // Arrange
        using var context = CreateContext();
        context.Universities.Add(new University { UniversityId = 2, UniversityName = "University of Oxford", Score = 110.01m });
        await context.SaveChangesAsync();

        _apiClientMock.Setup(c => c.FindAsync("Oxford"))
            .ReturnsAsync(new UniversityApiResult("University of Oxford", 110.01m));

        // Act
        var result = await CreateService(context).GetOrCreateUniversityAsync("Oxford");

        // Assert
        result.UniversityName.Should().Be("University of Oxford");
        _apiClientMock.Verify(c => c.FindAsync("Oxford"), Times.Once);
        context.Universities.Count().Should().Be(1);
    }

    [Fact]
    public async Task GetOrCreateUniversityAsync_CreatesAndReturnsNewUniversity_WhenNeitherNameFoundInDb()
    {
        // Arrange
        using var context = CreateContext();
        _apiClientMock.Setup(c => c.FindAsync("Oxford"))
            .ReturnsAsync(new UniversityApiResult("University of Oxford", 110.01m));

        // Act
        var result = await CreateService(context).GetOrCreateUniversityAsync("Oxford");

        // Assert
        result.UniversityName.Should().Be("University of Oxford");
        result.Score.Should().Be(110.01m);
        context.Universities.Should().ContainSingle(u =>
            u.UniversityName == "University of Oxford" && u.Score == 110.01m);
    }

    [Fact]
    public async Task GetOrCreateUniversityAsync_ThrowsBadRequestException_WhenApiReturnsNull()
    {
        // Arrange
        using var context = CreateContext();
        _apiClientMock.Setup(c => c.FindAsync(It.IsAny<string>()))
            .ReturnsAsync((UniversityApiResult?)null);

        // Act
        var act = async () => await CreateService(context).GetOrCreateUniversityAsync("Oxford");

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*No university found*");
    }

    [Fact]
    public async Task GetOrCreateUniversityAsync_ThrowsBadRequestException_WhenApiClientThrows()
    {
        // Arrange
        using var context = CreateContext();
        _apiClientMock.Setup(c => c.FindAsync(It.IsAny<string>()))
            .ThrowsAsync(new BadRequestException("University API is unreachable: Connection refused"));

        // Act
        var act = async () => await CreateService(context).GetOrCreateUniversityAsync("Oxford");

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*unreachable*");
    }

    [Fact]
    public async Task GetOrCreateUniversityAsync_CallsApiClient_WhenUniversityNameNotFoundInDb()
    {
        // Arrange
        using var context = CreateContext();
        _apiClientMock.Setup(c => c.FindAsync("Oxford"))
            .ReturnsAsync(new UniversityApiResult("University of Oxford", 110.01m));

        // Act
        await CreateService(context).GetOrCreateUniversityAsync("Oxford");

        // Assert
        _apiClientMock.Verify(c => c.FindAsync("Oxford"), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateUniversityAsync_NeverCallsApiClient_WhenUniversityNameFoundInDb()
    {
        // Arrange
        using var context = CreateContext();
        context.Universities.Add(new University { UniversityId = 1, UniversityName = "Oxford", Score = 100 });
        await context.SaveChangesAsync();

        // Act
        await CreateService(context).GetOrCreateUniversityAsync("Oxford");

        // Assert
        _apiClientMock.Verify(c => c.FindAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateUniversityAsync_DoesNotCreateNewUniversity_WhenOrganizationNameFoundInDb()
    {
        // Arrange
        using var context = CreateContext();
        context.Universities.Add(new University { UniversityId = 2, UniversityName = "University of Oxford", Score = 110.01m });
        await context.SaveChangesAsync();

        _apiClientMock.Setup(c => c.FindAsync("Oxford"))
            .ReturnsAsync(new UniversityApiResult("University of Oxford", 110.01m));

        // Act
        await CreateService(context).GetOrCreateUniversityAsync("Oxford");

        // Assert
        context.Universities.Count().Should().Be(1);
    }

    [Fact]
    public async Task GetOrCreateUniversityAsync_PersistsUniversityWithApiNameAndScore_WhenCreatingNew()
    {
        // Arrange
        using var context = CreateContext();
        _apiClientMock.Setup(c => c.FindAsync("Oxford"))
            .ReturnsAsync(new UniversityApiResult("University of Oxford", 110.01m));

        // Act
        await CreateService(context).GetOrCreateUniversityAsync("Oxford");

        // Assert
        context.Universities.Should().ContainSingle(u =>
            u.UniversityName == "University of Oxford" && u.Score == 110.01m);
    }
}
