using System;
using System.IO;

namespace SharpFTP.Server.FileSystem
{
    public class PathConverter
    {
        public PathConverter()
        {

        }

        public string ConvertToWindowsDirectory(string unixPath,string windowsRootBegin)
        {
            if(string.IsNullOrWhiteSpace(unixPath))
                throw new ArgumentNullException("unixPath is empty or contains only white spaces");
            if (string.IsNullOrWhiteSpace(windowsRootBegin))
                throw new ArgumentNullException("windowsRootBegin is empty or contain only white spaces");

            if (unixPath.Trim() == "/")
                return windowsRootBegin.TrimEnd('\\') + Path.DirectorySeparatorChar;

            string workingPath = unixPath.Trim('/', ' ');
            workingPath = workingPath.Replace('/', '\\');

            string rootBegin = windowsRootBegin.Trim('\\', ' ');
            workingPath = string.Format("{0}\\{1}\\",rootBegin, workingPath);

            return workingPath;
        }

        public string ConvertToUnixDirectory(string windowsPath, string windowsRootSubstring)
        {
            if (string.IsNullOrWhiteSpace(windowsPath))
                throw new ArgumentNullException("windowsPath is empty or contains only white spaces");
            if (string.IsNullOrWhiteSpace(windowsRootSubstring))
                throw new ArgumentNullException("rootSubstring is empty or contains only white spaces");

            string winPath = windowsPath.Trim('\\', ' ');
            string rootPath = windowsPath.Trim('\\', ' ');

            // maybe directory have format: "X:\" where X is Disc name (D,E,C etc.)
            if (winPath.Length == 2 && winPath == rootPath)
                return $"{rootPath}\\";

            //removing windows root substring
            winPath = winPath.Remove(rootPath.Length);
            string unixPath = winPath.Replace('\\', '/');

            unixPath = $"/{unixPath}";

            return unixPath;
        }

        public string ConvertToWindowsFileName(string unixFileName, string windowsRootBegin)
        {
            if (string.IsNullOrWhiteSpace(unixFileName))
                throw new ArgumentNullException("unixFileName is empty or contains only white spaces");
            if (string.IsNullOrWhiteSpace(windowsRootBegin))
                throw new ArgumentNullException("winodwsRootBegin is empty or contains only white spaces");

            string winFileName = unixFileName.Trim(' ', '/');
            winFileName = winFileName.Replace('/', '\\');
            winFileName = string.Format("{0}\\{1}", windowsRootBegin.TrimEnd('\\'), winFileName);

            return winFileName;
        }
    }
}
