using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebService.Providers.Interfaces;
using WebService.Services.Interfaces;

namespace WebService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class FriendshipController(
            IFriendshipService friendshipService,
            IAuthContextProvider contextProvider)
        : ControllerBase
    {
        [HttpGet("accepted")]
        public async Task<IActionResult> GetAcceptedFriends()
        {
            var userId = contextProvider.GetUserId();
            return Ok(await friendshipService.GetAcceptedFriendsAsync(userId));
        }

        [HttpGet("requests")]
        public async Task<IActionResult> GetFriendRequests()
        {
            var userId = contextProvider.GetUserId();
            return Ok(await friendshipService.GetFriendRequestsAsync(userId));
        }

        [HttpPost("request")]
        public async Task<IActionResult> RequestFriend([FromBody] string friendId)
        {
            var userId = contextProvider.GetUserId();
            return Ok(await friendshipService.RequestFriendAsync(userId, friendId));
        }

        [HttpPost("accept/{friendId}")]
        public async Task<IActionResult> AcceptFriend(string friendId)
        {
            var userId = contextProvider.GetUserId();
            return Ok(await friendshipService.AcceptFriendAsync(userId, friendId));
        }

        [HttpDelete("remove/{friendId}")]
        public async Task<IActionResult> RemoveFriend(string friendId)
        {
            var userId = contextProvider.GetUserId();
            return Ok(await friendshipService.RemoveFriendAsync(userId, friendId));
        }
    }
}