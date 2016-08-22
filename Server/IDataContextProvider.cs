namespace SharpFTP.Server
{
    public interface IDataContextProvider
    {
        IUserProvider UserProvider { get; }
        IDirectoryProvider DirectoryProvider { get; }
    }
}
