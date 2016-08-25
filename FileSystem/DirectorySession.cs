using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SharpFTP.Server.FileSystem
{
    class DirectorySession
    {
        ///<summary>
        ///Current working directory in Unix format
        ///</summary>
        public string WorkingUnixDirectory { get; private set; }
        ///<summary>
        ///Current working directory in Windows format
        ///</summary>
        public string WorkingWindowsDirectory { get; private set; }
        /// <summary>
        /// Represents source directory in Windows format which is converted to Unix root path.
        /// </summary>
        public string OriginDirectory { get { return windowsOriginDirectory; } }

        private readonly string windowsOriginDirectory;
        private PathConverter pathConverter;

        public DirectorySession(string windowsOriginDirectory)
        {
            this.windowsOriginDirectory = GetWindowsOriginDirectory(windowsOriginDirectory);
            this.WorkingWindowsDirectory = windowsOriginDirectory;
            this.WorkingUnixDirectory = "/";
            this.pathConverter = new PathConverter();
        }

        private string GetWindowsOriginDirectory(string windowsOriginDirectory)
        {
            //windows dir should have '\' at the end.
            return windowsOriginDirectory.Trim(' ','\\') + Path.DirectorySeparatorChar;
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
            bool result = true;

            WorkingUnixDirectory = unixDirectory;
            try
            {
                WorkingWindowsDirectory = pathConverter.ConvertToWindowsDirectory(unixDirectory, windowsOriginDirectory);
            }
            catch 
            {

                result = false;
            }

#if DEBUG
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\tunix:{0}\n\twin:{1}",WorkingUnixDirectory,WorkingWindowsDirectory);
            Console.ForegroundColor = ConsoleColor.Gray;
#endif

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
                    "-rw-r--r--  1 0 0 currentUser   {0} {1:MMM d HH:mm} {2}",
                    info.Length,
                    info.CreationTime,
                    info.Name);

                unixformatFiles[i] = readyPath;
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
    }
}
