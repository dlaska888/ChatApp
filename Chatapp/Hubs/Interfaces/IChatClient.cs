namespace WebService.Hubs.Interfaces;

public interface IChatClient
{
    Task ReceiveMessage(string userId, string message);
    Task UpdateConnectedUsers(ICollection<string> users);
}