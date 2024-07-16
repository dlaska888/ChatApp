using System.Linq.Expressions;
using MongoDB.Bson;
using Moq;
using WebService.Exceptions;
using WebService.Models.Dtos.Groups;
using WebService.Models.Entities;
using WebService.Repositories.Interfaces;
using WebService.Services;

namespace ChatAppBackend.Tests.Tests;

public class GroupServiceTests
{
    private readonly Mock<IMongoRepository<Group>> _groupRepoMock;
    private readonly GroupService _groupService;

    public GroupServiceTests()
    {
        _groupRepoMock = new Mock<IMongoRepository<Group>>();
        _groupService = new GroupService(_groupRepoMock.Object);
    }

    [Fact]
    public async Task GetAllGroupsAsync_ReturnsGroups_ForGivenUser()
    {
        // Arrange
        var userId = ObjectId.GenerateNewId();
        var groups = new List<Group>
        {
            new Group { Id = ObjectId.GenerateNewId(), UserIds = new List<ObjectId> { userId } },
            new Group { Id = ObjectId.GenerateNewId(), UserIds = new List<ObjectId> { userId } }
        };

        _groupRepoMock.Setup(repo => repo.AsQueryable())
            .Returns(groups.AsQueryable());

        // Act
        var result = await _groupService.GetAllGroupsAsync(userId.ToString());

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetGroupByIdAsync_ReturnsGroup_WhenUserHasAccess()
    {
        // Arrange
        var userId = ObjectId.GenerateNewId();
        var groupId = ObjectId.GenerateNewId().ToString();
        var group = new Group { Id = new ObjectId(groupId), UserIds = new List<ObjectId> { userId } };

        _groupRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<Group, bool>>>()))
            .ReturnsAsync(group);
        _groupRepoMock.Setup(repo => repo.AsQueryable())
            .Returns(new List<Group> { group }.AsQueryable());

        // Act
        var result = await _groupService.GetGroupByIdAsync(userId.ToString(), groupId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(groupId, result.Id);
    }

    [Fact]
    public async Task GetGroupByIdAsync_ThrowsUnauthorizedException_WhenUserHasNoAccess()
    {
        // Arrange
        var userId = ObjectId.GenerateNewId();
        var groupId = ObjectId.GenerateNewId().ToString();
        var group = new Group { Id = new ObjectId(groupId), UserIds = new List<ObjectId>() };

        _groupRepoMock.Setup(repo => repo.AsQueryable())
            .Returns(new List<Group> { group }.AsQueryable());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _groupService.GetGroupByIdAsync(userId.ToString(), groupId));
    }

    [Fact]
    public async Task CreateGroupAsync_CreatesNewGroup()
    {
        // Arrange
        var userId = ObjectId.GenerateNewId();
        var createGroupDto = new CreateGroupDto
        {
            Name = "Test Group",
            Description = "Test Description",
            // UserIds = new List<string> { userId }
        };

        var group = new Group
        {
            Id = ObjectId.GenerateNewId(),
            Name = createGroupDto.Name,
            Description = createGroupDto.Description,
            UserIds = new List<ObjectId> { userId }
        };

        _groupRepoMock.Setup(repo => repo.InsertOneAsync(It.IsAny<Group>()))
            .ReturnsAsync(group);

        // Act
        var result = await _groupService.CreateGroupAsync(userId.ToString(), createGroupDto);

        // Assert
        Assert.Equal(group.Name, result.Name);
        Assert.Equal(group.Description, result.Description);
        Assert.Equal(group.UserIds.Count, result.UserIds.Count);
    }

    [Fact]
    public async Task UpdateGroupAsync_UpdatesGroup_WhenUserHasAccess()
    {
        // Arrange
        var userId = ObjectId.GenerateNewId();
        var updateGroupDto = new UpdateGroupDto
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "Updated Group",
            Description = "Updated Description",
            // UserIds = new List<string> { userId }
        };

        var group = new Group
        {
            Id = new ObjectId(updateGroupDto.Id),
            Name = updateGroupDto.Name,
            Description = updateGroupDto.Description,
            UserIds = new List<ObjectId> { userId }
        };

        _groupRepoMock.Setup(repo => repo.ReplaceOneAsync(It.IsAny<Group>()))
            .ReturnsAsync(group);

        _groupRepoMock.Setup(repo => repo.AsQueryable())
            .Returns(new List<Group> { group }.AsQueryable());

        // Act
        await _groupService.UpdateGroupAsync(userId.ToString(), updateGroupDto);

        // Assert
        _groupRepoMock.Verify(repo => repo.ReplaceOneAsync(It.Is<Group>(g => g.Id == group.Id)), Times.Once);
    }

    [Fact]
    public async Task UpdateGroupAsync_ThrowsUnauthorizedException_WhenUserHasNoAccess()
    {
        // Arrange
        var userId = ObjectId.GenerateNewId();
        var updateGroupDto = new UpdateGroupDto
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "Updated Group",
            Description = "Updated Description",
        };

        var group = new Group
        {
            Id = new ObjectId(updateGroupDto.Id),
            Name = updateGroupDto.Name,
            Description = updateGroupDto.Description,
            // UserIds = new List<ObjectId> { userId }
        };

        _groupRepoMock.Setup(repo => repo.AsQueryable())
            .Returns(new List<Group> { group }.AsQueryable());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _groupService.UpdateGroupAsync(userId.ToString(), updateGroupDto));
    }

    [Fact]
    public async Task DeleteGroupAsync_DeletesGroup_WhenUserHasAccess()
    {
        // Arrange
        var userId = ObjectId.GenerateNewId();
        var groupId = ObjectId.GenerateNewId().ToString();
        var group = new Group { Id = new ObjectId(groupId), UserIds = new List<ObjectId> { userId } };

        _groupRepoMock.Setup(repo => repo.DeleteOneAsync(It.IsAny<Expression<Func<Group, bool>>>()))
            .ReturnsAsync(true);
        _groupRepoMock.Setup(repo => repo.AsQueryable())
            .Returns(new List<Group> { group }.AsQueryable());

        // Act
        await _groupService.DeleteGroupAsync(userId.ToString(), groupId);

        // Assert
        _groupRepoMock.Verify(repo => repo.DeleteOneAsync(It.Is<Expression<Func<Group, bool>>>(filter =>
            filter.Compile()(group))), Times.Once);
    }

    [Fact]
    public async Task DeleteGroupAsync_ThrowsUnauthorizedException_WhenUserHasNoAccess()
    {
        // Arrange
        var userId = ObjectId.GenerateNewId();
        var groupId = ObjectId.GenerateNewId().ToString();
        var group = new Group { Id = new ObjectId(groupId) };

        _groupRepoMock.Setup(repo => repo.AsQueryable())
            .Returns(new List<Group> { group }.AsQueryable());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _groupService.DeleteGroupAsync(userId.ToString(), groupId));
    }

    [Fact]
    public async Task AddUserToGroupAsync_AddsUserToGroup()
    {
        // Arrange
        var userId = ObjectId.GenerateNewId();
        var groupId = ObjectId.GenerateNewId().ToString();
        var group = new Group { Id = new ObjectId(groupId), UserIds = new List<ObjectId>() };

        _groupRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<Group, bool>>>()))
            .ReturnsAsync(group);
        _groupRepoMock.Setup(repo => repo.ReplaceOneAsync(It.IsAny<Group>()))
            .ReturnsAsync(group);

        // Act
        var result = await _groupService.AddUserToGroupAsync(userId.ToString(), groupId);

        // Assert
        Assert.True(result);
        Assert.Contains(userId, group.UserIds);
    }

    [Fact]
    public async Task RemoveUserFromGroupAsync_RemovesUserFromGroup()
    {
        // Arrange
        var userId = ObjectId.GenerateNewId();
        var groupId = ObjectId.GenerateNewId().ToString();
        var group = new Group { Id = new ObjectId(groupId), UserIds = new List<ObjectId> { userId } };

        _groupRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<Group, bool>>>()))
            .ReturnsAsync(group);
        _groupRepoMock.Setup(repo => repo.ReplaceOneAsync(It.IsAny<Group>()))
            .ReturnsAsync(group);

        // Act
        var result = await _groupService.RemoveUserFromGroupAsync(userId.ToString(), groupId);

        // Assert
        Assert.True(result);
        Assert.DoesNotContain(userId, group.UserIds);
    }

    [Fact]
    public async Task UserHasAccessToGroup_ReturnsTrue_WhenUserHasAccess()
    {
        // Arrange
        var userId = ObjectId.GenerateNewId();
        var groupId = ObjectId.GenerateNewId().ToString();
        var group = new Group { Id = new ObjectId(groupId), UserIds = new List<ObjectId> { userId } };

        _groupRepoMock.Setup(repo => repo.AsQueryable())
            .Returns(new List<Group> { group }.AsQueryable());

        // Act
        var result = await _groupService.UserHasAccessToGroup(userId.ToString(), groupId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UserHasAccessToGroup_ReturnsFalse_WhenUserHasNoAccess()
    {
        // Arrange
        var userId = ObjectId.GenerateNewId();
        var groupId = ObjectId.GenerateNewId().ToString();
        var group = new Group { Id = new ObjectId(groupId) };

        _groupRepoMock.Setup(repo => repo.AsQueryable())
            .Returns(new List<Group> { group }.AsQueryable());

        // Act
        var result = await _groupService.UserHasAccessToGroup(userId.ToString(), groupId);

        // Assert
        Assert.False(result);
    }
}