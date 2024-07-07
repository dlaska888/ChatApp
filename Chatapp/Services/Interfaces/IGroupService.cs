using MongoDB.Bson;
using WebService.Models.Entities;

namespace WebService.Services.Interfaces;

public interface IGroupService
{
    Task<Group> CreateGroupAsync(Group group);
    Task<Group> GetGroupByIdAsync(ObjectId groupId);
    Task<bool> UpdateGroupAsync(ObjectId groupId, Group updatedGroup);
    Task<bool> DeleteGroupAsync(ObjectId groupId);
    Task<bool> AddUserToGroupAsync(ObjectId userId, ObjectId groupId);
    Task<bool> RemoveUserFromGroupAsync(ObjectId userId, ObjectId groupId);
}