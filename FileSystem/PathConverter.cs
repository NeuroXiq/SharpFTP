using System;

namespace SharpFTP.Server.FileSystem
{
    public class PathConverter
    {
        public PathConverter()
        {

        }

        public string ConvertToWindowsPath(string unixPath,string windowsRootBegin)
        {
            if(string.IsNullOrWhiteSpace(unixPath))
                throw new ArgumentNullException("unixPath is empty or contains only white spaces");
            if (string.IsNullOrWhiteSpace(windowsRootBegin))
                throw new ArgumentNullException("windowsRootBegin is empty or contain only white spaces");

            string workingPath = unixPath.Trim('/', ' ');
            workingPath = workingPath.Replace('/', '\\');

            string rootBegin = windowsRootBegin.Trim('\\', ' ');
            workingPath = string.Format("{0}\\{1}",rootBegin, workingPath);

            return workingPath;
        }

        public string ConvertToUnixPath(string windowsPath, string windowsRootSubstring)
        {
            if (string.IsNullOrWhiteSpace(windowsPath))
                throw new ArgumentNullException("windowsPath is empty or contains only white spaces");
            if (string.IsNullOrWhiteSpace(windowsRootSubstring))
                throw new ArgumentNullException("rootSubstring is empty or contains only white spaces");

            string winPath = windowsPath.Trim('\\', ' ');
            string rootPath = windowsPath.Trim('\\', ' ');

            // maybe directory now have format: "X:\" where X is Disc name (D,E,C etc.)
            if (winPath.Length == 2 && winPath == rootPath)
                return $"{rootPath}\\";


            winPath = winPath.Remove(rootPath.Length);
            string unixPath = winPath.Replace('\\', '/');

            unixPath = $"/{unixPath}";

            return unixPath;
        }
    }
}
