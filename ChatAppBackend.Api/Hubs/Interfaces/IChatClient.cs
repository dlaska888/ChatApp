namespace WebService.Hubs.Interfaces;

public interface IChatClient
{
    Task ReceiveMessage(string userId, string userName, string message);
    Task NotifyUserConnected(string userId, string userName);
    Task NotifyUserDisconnected(string userId);
}