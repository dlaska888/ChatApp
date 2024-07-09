using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using WebService.Models.Entities;
using WebService.Models.Entities.Interfaces;
using WebService.Repositories.Interfaces;
using WebService.Services.Interfaces;

namespace WebService.Services
{
    public class GroupService(IMongoRepository<Group> groupRepo) : IGroupService
    {
        public async Task<Group> CreateGroupAsync(Group group)
        {
            await groupRepo.InsertOneAsync(group);
            return group;
        }

        public async Task<Group> GetGroupByIdAsync(string groupId)
        {
            var groupObjectId = new ObjectId(groupId);
            return await groupRepo.FindOneAsync(group => group.Id == groupObjectId);
        }

        public async Task UpdateGroupAsync(string groupId, Group updatedGroup)
        {
            var groupObjectId = new ObjectId(groupId);
            await groupRepo.ReplaceOneAsync(updatedGroup);
        }

        public async Task DeleteGroupAsync(string groupId)
        {
            var groupObjectId = new ObjectId(groupId);
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
    }
}