using SharpFTP.Server.FileSystem.Enums;

namespace SharpFTP.Server
{
    public abstract class UserDataContext
    {
        public struct UserInfo
        {
            string UserName;
            string Password;

            public UserInfo(string userName, string password)
            {
                UserName = name;
                Password = password;
            }
        }

        public UserDataContext()
        {
            
        }

        public abstract bool NeedPassword(UserInfo userInfo);
        public abstract bool IsPasswordCorrect(UserInfo userInfo);
        public abstract bool CanSeePath(UserInfo userInfo, string path);
        public abstract string GetOriginDirectory(UserInfo userInfo);
        public abstract FilePermission GetPathPermission(UserInfo userInfo, string path);
    }
}
