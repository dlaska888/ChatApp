namespace WebService.Providers.Interfaces;

public interface IAuthContextProvider
{
    string GetUserId();
    string GetUserEmail();
    string GetUserName();
}