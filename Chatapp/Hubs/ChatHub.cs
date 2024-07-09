using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WebService.Hubs.Interfaces;
using WebService.Models;
using WebService.Models.Dtos;
using WebService.Services.Interfaces;

namespace WebService.Hubs;

[Authorize]
public class ChatHub(ILogger<ChatHub> logger, IChatService chatService) : Hub<IChatClient>
{
    private static readonly ICollection<string> ConnectedUsers = new HashSet<string>();

    public override async Task OnConnectedAsync()
    {
        if (Context.User?.Identity is null || !Context.User.Identity.IsAuthenticated)
        {
            logger.LogWarning("Unauthorized access attempt.");
            Context.Abort();
        }
        else
        {
            ConnectedUsers.Add(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await Clients.All.UpdateConnectedUsers(ConnectedUsers);
            await base.OnConnectedAsync();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        ConnectedUsers.Remove(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await Clients.All.UpdateConnectedUsers(ConnectedUsers);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string receiverId, ChatTypeEnum chatTypeEnum, string message)
    {
        var senderId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        var newMessage = new CreateMessageDto
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = message
        };

        await chatService.CreateAsync(newMessage);
        await Clients.User(receiverId).ReceiveMessage(senderId, message);
    }

    public Task<ICollection<string>> GetConnectedUsers()
    {
        return Task.FromResult(ConnectedUsers);
    }
}