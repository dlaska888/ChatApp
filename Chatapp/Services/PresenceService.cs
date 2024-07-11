using MongoDB.Bson;
using WebService.Models.Entities;
using WebService.Repositories.Interfaces;
using WebService.Services.Interfaces;

namespace WebService.Services;

public class PresenceService(IMongoRepository<Message> messageRepo) : IPresenceService
{
    public async Task<IEnumerable<string>> GetUsersToNotify(string userId)
    {
        var userObjectId = new ObjectId(userId);

        return await messageRepo
            .AsQueryable()
            .Where(m => m.SenderId == userObjectId || m.ReceiverId == userObjectId)
            .GroupBy(m => new { m.SenderId, m.ReceiverId })
            .Take(100)
            .Select(g =>
                g.Key.SenderId == userObjectId ? g.Key.ReceiverId.ToString() : g.Key.SenderId.ToString())
            .ToAsyncEnumerable()
            .ToListAsync();
    }
}