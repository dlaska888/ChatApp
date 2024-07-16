using MongoDB.Bson;
using WebService.Exceptions;
using WebService.Models.Dtos.Groups;
using WebService.Models.Entities;
using WebService.Repositories.Interfaces;
using WebService.Services.Interfaces;

namespace WebService.Services;

public class GroupService(IMongoRepository<Group> groupRepo) : IGroupService
{
    public async Task<IEnumerable<GetGroupDto>> GetAllGroupsAsync(string userId)
    {
        return await groupRepo
            .AsQueryable()
            .Where(group => group.UserIds.Contains(new ObjectId(userId)))
            .Select(group => new GetGroupDto
            {
                Id = group.Id.ToString(),
                Name = group.Name,
                Description = group.Description,
                UserIds = group.UserIds.Select(id => id.ToString()).ToList()
            })
            .ToAsyncEnumerable().ToListAsync();
    }

    public async Task<GetGroupDto> GetGroupByIdAsync(string userId, string groupId)
    {
        var groupObjectId = new ObjectId(groupId);

        if (!await UserHasAccessToGroup(userId, groupId))
            throw new UnauthorizedException("User does not have access to this group.");

        var result = await groupRepo.AsQueryable()
            .Where(group => group.Id == groupObjectId)
            .Select(group => new GetGroupDto
            {
                Id = group.Id.ToString(),
                Name = group.Name,
                Description = group.Description,
                UserIds = group.UserIds.Select(id => id.ToString()).ToList()
            }).ToAsyncEnumerable()
            .FirstOrDefaultAsync();

        if (result is null)
            throw new NotFoundException("Group not found.");

        return result;
    }

    public async Task<GetGroupDto> CreateGroupAsync(string userId, CreateGroupDto dto)
    {
        var group = new Group
        {
            Name = dto.Name,
            Description = dto.Description,
            UserIds = new List<ObjectId> { new ObjectId(userId) }
        };

        await groupRepo.InsertOneAsync(group);

        return new GetGroupDto
        {
            Id = group.Id.ToString(),
            Name = group.Name,
            Description = group.Description,
            UserIds = group.UserIds.Select(id => id.ToString()).ToList()
        };
    }

    public async Task UpdateGroupAsync(string userId, UpdateGroupDto dto)
    {
        if (!await UserHasAccessToGroup(userId, dto.Id))
            throw new UnauthorizedException("User does not have access to this group.");

        var updateGroup = new Group
        {
            Id = new ObjectId(dto.Id),
            Name = dto.Name,
            Description = dto.Description
        };

        await groupRepo.ReplaceOneAsync(updateGroup);
    }

    public async Task DeleteGroupAsync(string userId, string groupId)
    {
        var groupObjectId = new ObjectId(groupId);

        if (!await UserHasAccessToGroup(userId, groupId))
            throw new UnauthorizedException("User does not have access to this group.");

        await groupRepo.DeleteOneAsync(group => group.Id == groupObjectId);
    }

    public async Task<bool> AddUserToGroupAsync(string userId, string groupId)
    {
        var userObjectId = new ObjectId(userId);
        var groupObjectId = new ObjectId(groupId);

        var group = await groupRepo.FindOneAsync(group => group.Id == groupObjectId);
        group.UserIds.Add(userObjectId);
        await groupRepo.ReplaceOneAsync(group);

        return true;
    }

    public async Task<bool> RemoveUserFromGroupAsync(string userId, string groupId)
    {
        var userObjectId = new ObjectId(userId);
        var groupObjectId = new ObjectId(groupId);

        var group = await groupRepo.FindOneAsync(group => group.Id == groupObjectId);
        group.UserIds.Remove(userObjectId);
        await groupRepo.ReplaceOneAsync(group);

        return true;
    }

    public async Task<bool> UserHasAccessToGroup(string userId, string groupId)
    {
        var userObjectId = new ObjectId(userId);
        var groupObjectId = new ObjectId(groupId);

        var groupChat = await groupRepo
            .AsQueryable()
            .Where(g => g.Id == groupObjectId)
            .Where(g => g.UserIds.Contains(userObjectId))
            .ToAsyncEnumerable()
            .FirstOrDefaultAsync();

        return groupChat is not null;
    }
}