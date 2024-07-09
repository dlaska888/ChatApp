using MongoDB.Bson;
using WebService.Models.Entities;

namespace WebService.Services.Interfaces;

public interface IGroupService
{
    Task<Group> CreateGroupAsync(Group group);
    Task<Group> GetGroupByIdAsync(string groupId);
    Task UpdateGroupAsync(string groupId, Group updatedGroup);
    Task DeleteGroupAsync(string groupId);
    Task<bool> AddUserToGroupAsync(string userId, string groupId);
    Task<bool> RemoveUserFromGroupAsync(string userId, string groupId);
}