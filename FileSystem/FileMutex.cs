using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpFTP.Server.FileSystem
{
    public class FileMutex
    {
        static List<string> lockedFiles;
        private static volatile object threadSafe = new object();

        static FileMutex()
        {
            lockedFiles = new List<string>();
        }

        /// <summary>
        /// Checks if any file in passed directory is locked.
        /// </summary>
        /// <returns>
        /// If any file is locked returns true
        /// </returns>
        /// <param name="directory"> 
        /// Directory path in Windows format 
        /// </param>
        public static bool DirectoryInUse(string directory)
        {
            lock (threadSafe)
            {
                string path;
                foreach (string filePath in lockedFiles)
                {
                    path = GetFileDirectory(filePath);
                    if (path == directory)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Locking file for future usage
        /// </summary>
        /// <param name="filePath">
        /// File path in Windows format
        /// </param>
        /// <returns>
        /// If file locked successful returns true otherwise false 
        /// </returns>
        public static bool MutexOn(string filePath)
        {
            lock (threadSafe)
            {
                //maybe entire directory is locked
                if (lockedFiles.Contains(filePath) || DirectoryMutex.IsLocked(GetFileDirectory(filePath)))
                    return false;
                else
                {
                    lockedFiles.Add(filePath);
                    return true;
                }
            }   
        }

        /// <summary>
        /// Unlock file 
        /// </summary>
        /// <param name="filePath">
        /// File path in Windows format
        /// </param>
        public static void MutexOff(string filePath)
        {
            lock (threadSafe)
            {
                lockedFiles.Remove(filePath);
            }
        }

        private static string GetFileDirectory(string filePath)
        {
            string[] subDirectories = filePath.Split(Path.DirectorySeparatorChar);

            string directory = string.Join(
                Path.DirectorySeparatorChar.ToString(),
                subDirectories,
                0,
                subDirectories.Length - 1);

            return string.Format("{0}{1}", directory, Path.DirectorySeparatorChar);
        }
    }
}
