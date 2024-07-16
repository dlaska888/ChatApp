using WebService.Models.Dtos.Account;

namespace WebService.Services.Interfaces;

public interface IFriendshipService
{
    Task<IEnumerable<GetAccountPublicDto>> GetAcceptedFriendsAsync(string userId);
    Task<IEnumerable<GetAccountPublicDto>> GetFriendRequestsAsync(string userId);
    Task<bool> RequestFriendAsync(string userId, string friendId);
    Task<bool> AcceptFriendAsync(string userId, string friendId);
    Task<bool> RemoveFriendAsync(string userId, string friendId);
}