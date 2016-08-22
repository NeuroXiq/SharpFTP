using SharpFTP.Server.FileSystem.Enums;
using System.IO;

namespace SharpFTP.Server
{
    public interface IDirectoryProvider
    {
        bool CanDelete(string winFormatPath, string userName);
        bool DeletePath(string winFormat);
        bool CanChangeDirectory(string winFormatDirectory, string userName);
        bool CanCreateDirectory(string winFormatDirectory, string userName);
        bool CanStorefile(string winFilePath, string userName);
        bool CreateDirectory(string directory);
        bool CanRename(string winPath, string userName);
        bool RenamePath(string source, string renameTo);

        uint GetFileSize(string fileName);
        uint GetDirectorySize(string directory);
        FilePermission GetFilePermission(string fileName,string userName);
        string GetOriginDirectory(string userName);
        Stream GetFileStream(string winFilePath);
    }
}
