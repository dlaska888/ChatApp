using System.Security.Claims;
using ChatService.Hubs.Interfaces;
using ChatService.Models.Entities;
using ChatService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using Shared.Models;

namespace ChatService.Hubs;

[Authorize]
public class ChatHub(ILogger<ChatHub> logger, IMessageService messageService) : Hub<IChatClient>
{
    public override async Task OnConnectedAsync()
    {
        if (Context.User?.Identity is null || !Context.User.Identity.IsAuthenticated)
        {
            logger.LogWarning("Unauthorized access attempt.");
            Context.Abort();
        }
        else
        {
            await base.OnConnectedAsync();
        }
    }

    public async Task SendMessage(string receiverId, ChatTypeEnum chatTypeEnum, string message)
    {
        var senderId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        
        var newMessage = new Message
        {
            SenderId = new ObjectId(senderId),
            ReceiverId = new ObjectId(receiverId),
            Content = message
        };
        
        await messageService.CreateAsync(newMessage);
        
        await Clients.User(senderId).ReceiveMessage(senderId, "ACK");
        await Clients.All.ReceiveMessage(senderId, message);
    }
}