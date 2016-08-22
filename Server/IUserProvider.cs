namespace SharpFTP.Server
{
    public interface IUserProvider
    {
        bool PasswordCorrect(string userName, string password);
        bool RequirePassword(string userName);
        bool UserExist(string userName);
    }
}
