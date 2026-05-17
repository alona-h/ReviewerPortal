using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using ReviewerPortal.API.Application.Exceptions;
using ReviewerPortal.API.Application.Interfaces;
using ReviewerPortal.API.Application.Services;
using ReviewerPortal.API.Domain.Entities;
using ReviewerPortal.API.Infrastructure.Persistence;

namespace ReviewerPortal.Tests;

public class UserServiceTests
{
    private readonly Mock<IUniversityService> _universityServiceMock = new();

    private static AppDbContext CreateContext() => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private UserService CreateService(AppDbContext context) =>
        new(context, _universityServiceMock.Object);

    [Fact]
    public async Task RegisterUserAsync_ReturnsResponseDto_WhenInputIsValid()
    {
        // Arrange
        var university = new University { UniversityId = 1, UniversityName = "University of Oxford", Score = 110 };
        _universityServiceMock.Setup(s => s.GetOrCreateUniversityAsync("Oxford")).ReturnsAsync(university);

        using var context = CreateContext();

        // Act
        var result = await CreateService(context).RegisterUserAsync("alice", "Oxford", 5);

        // Assert
        result.UserId.Should().BeGreaterThan(0);
        result.UserName.Should().Be("alice");
        result.NumberOfPublications.Should().Be(5);
        result.University.UniversityName.Should().Be("University of Oxford");
        result.University.Score.Should().Be(110);
    }

    [Fact]
    public async Task RegisterUserAsync_ThrowsBadRequestException_WhenUserNameAlreadyExists()
    {
        // Arrange
        using var context = CreateContext();
        context.Users.Add(new User { UserId = 1, UserName = "alice", UniversityId = 0, NumberOfPublications = 0 });
        await context.SaveChangesAsync();

        // Act
        var act = async () => await CreateService(context).RegisterUserAsync("alice", "Oxford", 5);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*alice*");
    }

    [Fact]
    public async Task RegisterUserAsync_PropagatesBadRequestException_WhenUniversityServiceFails()
    {
        // Arrange
        _universityServiceMock.Setup(s => s.GetOrCreateUniversityAsync(It.IsAny<string>()))
            .ThrowsAsync(new BadRequestException("University API is unreachable: Connection refused"));

        using var context = CreateContext();

        // Act
        var act = async () => await CreateService(context).RegisterUserAsync("alice", "Oxford", 5);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*University API is unreachable*");
    }

    [Fact]
    public async Task InviteReviewerAsync_ReturnsSuccess_WhenUserMeetsBothCriteria()
    {
        // Arrange
        using var context = CreateContext();
        var university = new University { UniversityId = 1, UniversityName = "MIT", Score = 60 };
        context.Universities.Add(university);
        context.Users.Add(new User { UserId = 1, UserName = "alice", UniversityId = 1, NumberOfPublications = 4 });
        await context.SaveChangesAsync();

        // Act
        var result = await CreateService(context).InviteReviewerAsync(1);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Invitation was successful");
    }

    [Fact]
    public async Task InviteReviewerAsync_ReturnsFailure_WhenUserDoesNotMeetCriteria()
    {
        // Arrange
        using var context = CreateContext();
        var university = new University { UniversityId = 1, UniversityName = "MIT", Score = 59 };
        context.Universities.Add(university);
        context.Users.Add(new User { UserId = 1, UserName = "alice", UniversityId = 1, NumberOfPublications = 3 });
        await context.SaveChangesAsync();

        // Act
        var result = await CreateService(context).InviteReviewerAsync(1);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invitation could not be sent");
    }

    [Fact]
    public async Task InviteReviewerAsync_ThrowsNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var act = async () => await CreateService(context).InviteReviewerAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*99*");
    }

    [Fact]
    public async Task InviteReviewerAsync_ReturnsSuccess_WhenPublicationsAndScoreExceedThresholds()
    {
        // Arrange
        using var context = CreateContext();
        var university = new University { UniversityId = 1, UniversityName = "MIT", Score = 95m };
        context.Universities.Add(university);
        context.Users.Add(new User { UserId = 1, UserName = "alice", UniversityId = 1, NumberOfPublications = 10 });
        await context.SaveChangesAsync();

        // Act
        var result = await CreateService(context).InviteReviewerAsync(1);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Invitation was successful");
    }

    [Fact]
    public async Task InviteReviewerAsync_ReturnsFailure_WhenPublicationsExactlyThree()
    {
        // Arrange
        using var context = CreateContext();
        var university = new University { UniversityId = 1, UniversityName = "MIT", Score = 90m };
        context.Universities.Add(university);
        context.Users.Add(new User { UserId = 1, UserName = "alice", UniversityId = 1, NumberOfPublications = 3 });
        await context.SaveChangesAsync();

        // Act
        var result = await CreateService(context).InviteReviewerAsync(1);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task InviteReviewerAsync_ReturnsFailure_WhenScoreExactlyFiftyNine()
    {
        // Arrange
        using var context = CreateContext();
        var university = new University { UniversityId = 1, UniversityName = "MIT", Score = 59m };
        context.Universities.Add(university);
        context.Users.Add(new User { UserId = 1, UserName = "alice", UniversityId = 1, NumberOfPublications = 10 });
        await context.SaveChangesAsync();

        // Act
        var result = await CreateService(context).InviteReviewerAsync(1);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterUserAsync_NeverCallsUniversityService_WhenUserNameAlreadyExists()
    {
        // Arrange
        using var context = CreateContext();
        context.Users.Add(new User { UserId = 1, UserName = "alice", UniversityId = 0, NumberOfPublications = 0 });
        await context.SaveChangesAsync();

        // Act
        var act = async () => await CreateService(context).RegisterUserAsync("alice", "Oxford", 5);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        _universityServiceMock.Verify(s => s.GetOrCreateUniversityAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterUserAsync_CallsGetOrCreateUniversityAsync_WithExactUniversityNameFromRequest()
    {
        // Arrange
        var university = new University { UniversityId = 1, UniversityName = "University of Oxford", Score = 100 };
        _universityServiceMock.Setup(s => s.GetOrCreateUniversityAsync("Oxford")).ReturnsAsync(university);

        using var context = CreateContext();

        // Act
        await CreateService(context).RegisterUserAsync("alice", "Oxford", 5);

        // Assert
        _universityServiceMock.Verify(s => s.GetOrCreateUniversityAsync("Oxford"), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_PersistsUserWithUniversityIdMatchingReturnedUniversity()
    {
        // Arrange
        var university = new University { UniversityId = 42, UniversityName = "Oxford", Score = 100 };
        _universityServiceMock.Setup(s => s.GetOrCreateUniversityAsync("Oxford")).ReturnsAsync(university);

        using var context = CreateContext();

        // Act
        await CreateService(context).RegisterUserAsync("alice", "Oxford", 5);

        // Assert
        context.Users.Single().UniversityId.Should().Be(42);
    }
}
