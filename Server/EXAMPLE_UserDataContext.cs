using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFTP.Server.FileSystem.Enums;
using System.IO;

namespace SharpFTP.Server.Server
{
    /* Example implementaion of UserDataContext abstract class.
     * Context:
     * Example contains two users with different privileges:
     * 
     * - anonymous (default ftp user, no case sensitive)
     *   * password not require
     *   * have access to 'D:\ftpdata\anonymous\' and all subdirectories 
     *      - for this dirs read/write privileges
     *   * have access to 'D:\ftpdata\importantTextFiles\'
     *     - for this dir only read privileges
     *      
     * - admin (case sensitive)
     *   * password require (adminPass123)
     *   * have access to all directories and for all read/write privileges
     */

    class EXAMPLE_UserDataContext : UserDataContext
    {
        private readonly string anonymousOriginDir;
        private readonly string anonymousReadOnlyDir;
        private const string adminOrigin = "C:\\";

        public EXAMPLE_UserDataContext()
        {
            anonymousOriginDir = "D:\\ftpdata\\anonymous\\";
            anonymousReadOnlyDir = @"D:\ftpdata\importantTextFiles\";
        }

        public override bool Exist(string userName)
        {
            return userName.ToLower() == "anonymous" || userName == "admin";
        }

        public override string GetOriginDirectory(UserInfo userInfo)
        {
            if (userInfo.UserName == "admin")
                return adminOrigin;
            else if (userInfo.UserName.ToLower() == "anonymous")
                return anonymousOriginDir;
            else throw new ArgumentException($"cannot find origin directory for {userInfo.UserName}");
        }

        public override FilePermission GetPathPermission(UserInfo userInfo, string path)
        {
            if (userInfo.UserName == "admin")
                return FilePermission.Read & FilePermission.Write;
            else if (userInfo.UserName.ToLower() == "anonymous")
            {
                if (path.Contains(anonymousOriginDir))
                {
                    return FilePermission.Read & FilePermission.Write;
                }
                else if (path.Contains(anonymousReadOnlyDir))
                {
                    return FilePermission.Read;
                }
                else throw new ArgumentException($"specified path is incorrect for {userInfo.UserName} user");
            }
            else throw new ArgumentException("user cannot be found");
        }

        public override bool HaveAccess(UserInfo userInfo, string path)
        {
            if (userInfo.UserName == "admin")
                return Directory.Exists(path) || File.Exists(path);
            else if (userInfo.UserName.ToLower() == "anonymous")
            {
                return (Directory.Exists(path) || File.Exists(path)) &&
                    (path.Contains(anonymousOriginDir) || path.Contains(anonymousReadOnlyDir));

            }
            else throw new ArgumentException("cannot find specify user");
        }

        public override bool IsPasswordCorrect(string userName, string password)
        {
            if (userName == "admin")
                return password == "adminPass123";
            else if (userName == "anonymous")
                return true;
            else return false;
        }

        public override bool NeedPassword(string user)
        {
            if (user == "anonymous")
                return false;
            else return true;
        }

        
    }
}
