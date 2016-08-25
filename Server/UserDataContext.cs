using SharpFTP.Server.FileSystem.Enums;

namespace SharpFTP.Server
{
    public abstract class UserDataContext
    {
        public struct UserInfo
        {
            public string UserName;
            public string Password;

            public UserInfo(string userName, string password)
            {
                UserName = userName;
                Password = password;
            }
        }

        public UserDataContext()
        {
            
        }

        public abstract bool NeedPassword(string userPassword);
        public abstract bool Exist(string userName);
        public abstract bool IsPasswordCorrect(string userName,string password);

        public abstract bool HaveAccess(UserInfo userInfo, string path);
        public abstract string GetOriginDirectory(UserInfo userInfo);
        public abstract FilePermission GetPathPermission(UserInfo userInfo, string path);
    }
}
