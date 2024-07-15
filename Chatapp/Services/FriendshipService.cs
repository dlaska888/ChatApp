using MongoDB.Bson;
using WebService.Exceptions;
using WebService.Models.Dtos.Account;
using WebService.Models.Entities;
using WebService.Repositories.Interfaces;
using WebService.Services.Interfaces;

namespace WebService.Services;

public class FriendshipService(
    IMongoRepository<Friendship> friendshipRepo,
    IMongoRepository<ChatUser> userRepo) : IFriendshipService
{
    public async Task<IEnumerable<GetAccountPublicDto>> GetAcceptedFriendsAsync(string userId)
    {
        return await GetFriendsAsync(userId, true);
    }

    public Task<IEnumerable<GetAccountPublicDto>> GetFriendRequestsAsync(string userId)
    {
        return GetFriendsAsync(userId, false);
    }

    private async Task<IEnumerable<GetAccountPublicDto>> GetFriendsAsync(string userId, bool accepted)
    {
        var userObjectId = new ObjectId(userId);

        var friendships = await friendshipRepo
            .AsQueryable()
            .Where(f => f.User1Id == userObjectId || f.User2Id == userObjectId)
            .Where(f => f.IsAccepted == accepted)
            .ToAsyncEnumerable()
            .ToListAsync();

        var friendIds = friendships.Select(f => f.User1Id == userObjectId ? f.User2Id : f.User1Id);

        var friends = await userRepo
            .AsQueryable()
            .Where(u => friendIds.Contains(u.Id))
            .Select(u => new GetAccountPublicDto
            {
                Id = u.Id.ToString()!,
                UserName = u.UserName!,
                Email = u.Email!
            })
            .ToAsyncEnumerable()
            .ToListAsync();

        return friends;
    }

    public async Task<bool> RequestFriendAsync(string userId, string friendId)
    {
        var userObjectId = new ObjectId(userId);
        var friendObjectId = new ObjectId(friendId);

        var friendship = await FindFriendshipAsync(userId, friendId);
        if (friendship is not null)
            throw new BadRequestException("Friendship already exists");

        var newFriendship = new Friendship
        {
            User1Id = userObjectId,
            User2Id = friendObjectId,
            IsAccepted = false
        };

        await friendshipRepo.InsertOneAsync(newFriendship);

        return true;
    }

    public async Task<bool> AcceptFriendAsync(string userId, string friendId)
    {
        var friendship = await FindFriendshipAsync(userId, friendId);
        if (friendship is null)
            throw new Exception("Friendship not found");

        friendship.IsAccepted = true;

        await friendshipRepo.ReplaceOneAsync(friendship);
        
        return true;
    }

    public async Task<bool> RemoveFriendAsync(string userId, string friendId)
    {
        var friendship = await FindFriendshipAsync(userId, friendId);
        if (friendship is null)
            throw new Exception("Friendship not found");

        await friendshipRepo.DeleteOneAsync(f => f.Id == friendship.Id);
        
        return true;
    }

    private async Task<Friendship?> FindFriendshipAsync(string userId, string friendId)
    {
        var userObjectId = new ObjectId(userId);
        var friendObjectId = new ObjectId(friendId);

        var friendship = await friendshipRepo
            .AsQueryable()
            .Where(f => (f.User1Id == userObjectId && f.User2Id == friendObjectId) ||
                        (f.User1Id == friendObjectId && f.User2Id == userObjectId))
            .ToAsyncEnumerable()
            .FirstOrDefaultAsync();

        return friendship;
    }
}