using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WebService.Hubs.Interfaces;
using WebService.Models;
using WebService.Models.Dtos;
using WebService.Models.Hubs;
using WebService.Services.Interfaces;

namespace WebService.Hubs;

[Authorize]
public class ChatHub(IChatService chatService, IGroupService groupService) : Hub<IChatClient>
{
    private static readonly ConcurrentDictionary<string, HubUser> ConnectedUsers = new();

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var userName = GetUserName();
        var userGroups = await groupService.GetAllGroupsAsync(userId);

        await AddConnectedUser(userId, userName);

        foreach (var group in userGroups)
            await Groups.AddToGroupAsync(Context.ConnectionId, group.Id);

        await base.OnConnectedAsync();
    }

    private async Task AddConnectedUser(string userId, string userName)
    {
        var user = ConnectedUsers.GetOrAdd(userId, new HubUser
        {
            Id = userId,
            Name = userName,
            ConnectionIds = []
        });

        user.ConnectionIds.Add(Context.ConnectionId);

        if (user.ConnectionIds.Count == 1)
        {
            await Clients.Others.AlertUserConnected(userId, userName);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        var connectionId = Context.ConnectionId;

        ConnectedUsers.TryGetValue(userId, out var user);

        if (user == null)
        {
            await base.OnDisconnectedAsync(exception);
            return;
        }

        user.ConnectionIds.RemoveWhere(cid => cid.Equals(connectionId));

        if (user.ConnectionIds.Count == 0)
        {
            ConnectedUsers.TryRemove(userId, out _);
            await Clients.Others.AlertUserDisconnected(userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string receiverId, ChatTypeEnum chatTypeEnum, string message)
    {
        var senderId = GetUserId();
        var senderName = GetUserName();

        var newMessage = new CreateMessageDto
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = message
        };

        await chatService.CreateAsync(newMessage);

        switch (chatTypeEnum)
        {
            case ChatTypeEnum.Private:
                await Clients.User(receiverId).ReceiveMessage(senderId, senderName, message);
                break;
            case ChatTypeEnum.Group:
                await Clients.Group(receiverId).ReceiveMessage(senderId, senderName, message);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(chatTypeEnum), chatTypeEnum, null);
        }
    }

    public Task<ICollection<HubUser>> GetConnectedUsers()
    {
        return Task.FromResult(ConnectedUsers.Values);
    }

    private string GetUserId() => Context.UserIdentifier!;
    private string GetUserName() => Context.User!.Identity!.Name!;
}