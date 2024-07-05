using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Models;
using WebService.Models.Entities;
using WebService.Services.Interfaces;

namespace WebService.Services
{
    public class GroupService : IGroupService
    {
        private readonly IMongoCollection<Group> _groups;

        public GroupService(
            IOptions<ChatAppDbOptions> chatAppDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                chatAppDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                chatAppDatabaseSettings.Value.DatabaseName);

            _groups = mongoDatabase.GetCollection<Group>("groups");
        }

        public async Task<Group> CreateGroupAsync(Group group)
        {
            await _groups.InsertOneAsync(group);
            return group;
        }

        public async Task<Group> GetGroupByIdAsync(ObjectId groupId)
        {
            return await _groups.Find(group => group.Id == groupId).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateGroupAsync(ObjectId groupId, Group updatedGroup)
        {
            var result = await _groups.ReplaceOneAsync(group => group.Id == groupId, updatedGroup);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteGroupAsync(ObjectId groupId)
        {
            var result = await _groups.DeleteOneAsync(group => group.Id == groupId);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<bool> AddUserToGroupAsync(ObjectId userId, ObjectId groupId)
        {
            var update = Builders<Group>.Update.AddToSet(group => group.UserIds, userId);
            var result = await _groups.UpdateOneAsync(group => group.Id == groupId, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> RemoveUserFromGroupAsync(ObjectId userId, ObjectId groupId)
        {
            var update = Builders<Group>.Update.Pull(group => group.UserIds, userId);
            var result = await _groups.UpdateOneAsync(group => group.Id == groupId, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
    }
}