using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WebService.Hubs.Interfaces;
using WebService.Models;
using WebService.Models.Dtos;
using WebService.Models.Dtos.Messages;
using WebService.Models.Hubs;
using WebService.Services;
using WebService.Services.Interfaces;

namespace WebService.Hubs;

[Authorize]
public class ChatHub(
    IChatService chatService,
    IGroupService groupService,
    IPresenceService presenceService,
    INotificationProducerService notificationProducerService) : Hub<IChatClient>
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
            await Clients.Others.NotifyUserDisconnected(userId);
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
                await ReceivePrivateMessage(senderId, receiverId, senderName, message);
                break;
            case ChatTypeEnum.Group:
                await ReceiveGroupMessage(receiverId, senderId, senderName, message);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(chatTypeEnum), chatTypeEnum, null);
        }
    }

    public Task<ICollection<HubUser>> GetConnectedUsers()
    {
        return Task.FromResult(ConnectedUsers.Values);
    }

    #region Private Methods

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
            var usersToNotify = await presenceService.GetUsersToNotify(userId);
            await Clients.Users(usersToNotify).NotifyUserConnected(userId, userName);
        }
    }

    private async Task ReceivePrivateMessage(string senderId, string receiverId, string senderName, string message)
    {
        if (ConnectedUsers.TryGetValue(receiverId, out _))
        {
            await Clients.Clients(receiverId).ReceiveMessage(senderId, senderName, message);
            return;
        }

        await notificationProducerService.SendMessageNotificationRequestAsync(
            new CreateMessageDto
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = message
            }
        );
    }

    private async Task ReceiveGroupMessage(string receiverId, string senderId, string senderName, string message)
    {
        var group = await groupService.GetGroupByIdAsync(senderId, receiverId);
        var groupUsers = group.UserIds;

        foreach (var userId in groupUsers)
        {
            if (ConnectedUsers.TryGetValue(userId, out _))
                await Clients.Clients(userId).ReceiveMessage(senderId, senderName, message);
            else
                await notificationProducerService.SendMessageNotificationRequestAsync(
                    new CreateMessageDto
                    {
                        SenderId = senderId,
                        ReceiverId = userId,
                        Content = message
                    }
                );
        }
    }

    private string GetUserId() => Context.UserIdentifier!;
    private string GetUserName() => Context.User!.Identity!.Name!;

    #endregion
}