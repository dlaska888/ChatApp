namespace ChatService.Hubs.Interfaces;

public interface IChatClient
{
    Task ReceiveMessage(string userId, string message);
}