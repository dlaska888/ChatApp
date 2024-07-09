using MongoDB.Bson;
using WebService.Models.Dtos.Groups;
using WebService.Models.Entities;

namespace WebService.Services.Interfaces;

public interface IGroupService
{
    Task<IEnumerable<GetGroupDto>> GetAllGroupsAsync(string userId);
    Task<GetGroupDto> GetGroupByIdAsync(string userId, string groupId);
    Task<GetGroupDto> CreateGroupAsync(string userId, CreateGroupDto group);
    Task UpdateGroupAsync(string userId, UpdateGroupDto dto);
    Task DeleteGroupAsync(string userId, string groupId);
    Task<bool> AddUserToGroupAsync(string userId, string groupId);
    Task<bool> RemoveUserFromGroupAsync(string userId, string groupId);
    
    Task<bool> UserHasAccessToGroup(string userId, string groupId);
}