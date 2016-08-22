using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SharpFTP.Server.FileSystem
{
    class DirectorySession
    {
        public string WorkingUnixDirectory { get; private set; }
        public string WorkingWindowsDirectory { get; private set; }

        private readonly string windowsOriginDirectory;
        private PathConverter pathConverter;

        public DirectorySession(string windowsOriginDirectory)
        {
            this.windowsOriginDirectory = GetWindowsOriginDirectory(windowsOriginDirectory);
            WorkingWindowsDirectory = windowsOriginDirectory;
            this.WorkingUnixDirectory = "/";
            this.pathConverter = new PathConverter();
        }

        private string GetWindowsOriginDirectory(string windowsOriginDirectory)
        {
            //windows dir should have '\' at the end.
            string winDir = windowsOriginDirectory.Trim(' ','\\') + "\\";
            return winDir;
        }

        ///<summary>
        /// Changing working directory in current session object.
        ///</summary>
        ///<param name="unixDirectory">
        /// New directory path or folder in Unix path format.
        ///</param>
        ///<returns>
        /// If directory changed successful returns true else return false.
        ///</returns>
        public bool ChangeWorkingDirectory(string unixDirectory)
        {
            bool result = false;

            WorkingUnixDirectory = unixDirectory;
            WorkingWindowsDirectory = pathConverter.ConvertToWindowsPath(unixDirectory, windowsOriginDirectory);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\tUnix => {0}\n\tWin => {1}",WorkingUnixDirectory,WorkingWindowsDirectory);
            Console.ForegroundColor = ConsoleColor.Gray;

            return result;
        }
        
        public string[] GetFilesNamesInUnixFormat()
        {
            string[] files = Directory.GetFiles(WorkingWindowsDirectory);
            string[] unixformatFiles = new string[files.Length];
            FileInfo info;

            for (int i=0;i<files.Length;i++)
            {
                info = new FileInfo(files[i]);

                string readyPath = string.Format(
                    DateTimeFormatInfo.InvariantInfo,
                    "-rw-r--r--  1 0 0 someuserExample   {0} {1:MMM d HH:mm} {2}",
                    info.Length,
                    info.CreationTime,
                    info.Name);

                unixformatFiles[i] = readyPath;//"-rw-r--r-- 1 0 0 123213 Feb 19 2016 1000GB.zip";//readyPath;
            }

            return unixformatFiles;
        }

        public string[] GetDirectoriesInUnixFormat()
        {
            DirectoryInfo info;
            string[] directories = Directory.GetDirectories(WorkingWindowsDirectory);
            string[] unixformatDirectories = new string[directories.Length];

            for (int i = 0; i < directories.Length; i++)
            {
                info = new DirectoryInfo(directories[i]);
                string readyDir = string.Format(
                    DateTimeFormatInfo.InvariantInfo,
                    "drw-------  1 user   1 {0:MMM d HH:mm} {1}",
                    info.CreationTime,
                    info.Name);

                unixformatDirectories[i] = readyDir;
            }

            return unixformatDirectories;
        }

        /*if (unixDir == "/" )
            {
                WorkingUnixDirectory = "/";
                WorkingWindowsDirectory = windowsOriginDirectory;
                result = true;
            }
            else
            {
                string folder = unixDirectory.Trim('/', ' ');
                string winfolder = unixDir
                    .Trim('/', '\\') // deleting / from start position /a/b/c/d/ => a/b/c/d
                    .Replace('/', '\\'); // replace / to \ => a\b\c\d

                winfolder = $"{windowsOriginDirectory}{winfolder}\\";
                
                WorkingWindowsDirectory = winfolder;
                WorkingUnixDirectory = $"/{folder}";
                result = true;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\tWinDir => {0}\n\tUnixDir => {1}",WorkingWindowsDirectory,WorkingUnixDirectory);
            Console.ForegroundColor = ConsoleColor.Gray;*/
    }
}
