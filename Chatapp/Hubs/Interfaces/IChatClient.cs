namespace WebService.Hubs.Interfaces;

public interface IChatClient
{
    Task ReceiveMessage(string userId, string userName, string message);
    Task AlertUserConnected(string userId, string userName);
    Task AlertUserDisconnected(string userId);
}