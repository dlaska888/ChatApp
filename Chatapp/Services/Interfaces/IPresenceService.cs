namespace WebService.Services.Interfaces;

public interface IPresenceService
{
    Task<IEnumerable<string>> GetUsersToNotify(string userId);
}