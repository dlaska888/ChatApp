using WebService.Models.Dtos.Account;

namespace WebService.Services.Interfaces;

public interface IFriendshipService
{
    Task<IEnumerable<GetAccountPublicDto>> GetAcceptedFriendsAsync(string userId);
    Task<IEnumerable<GetAccountPublicDto>> GetFriendRequestsAsync(string userId);
    Task RequestFriendAsync(string userId, string friendId);
    Task AcceptFriendAsync(string userId, string friendId);
    Task RemoveFriendAsync(string userId, string friendId);
}