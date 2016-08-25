using System;
using System.Collections.Generic;
using System.IO;

namespace SharpFTP.Server.FileSystem
{
    public class DirectoryMutex
    {
        private static List<string> lockedDirectories;
        private static volatile object threadSafe = new object();

        static DirectoryMutex()
        {
            lockedDirectories = new List<string>();
        }

        /// <summary>
        /// Lock entire directory for future use.
        /// </summary>
        /// <param name="directory">
        /// Directory path in Windows format.
        /// </param>
        /// <returns>
        /// If operation succeeded return true otherwise return false.
        /// </returns>
        /// <seealso cref="MutexOff(string)"/>
        public static bool MutexOn(string directory)
        {
            string path = string.Format("{0}{1}", directory.TrimEnd('\\', ' '), Path.DirectorySeparatorChar);

            lock (threadSafe)
            {
                if (lockedDirectories.Contains(path))
                    return false;
                else
                {
                    //maybe some file in this diretory is editing right now 
                    if (FileMutex.DirectoryInUse(path))
                        return false;
                    else return true;
                }
            }
        }

        /// <summary>
        /// Check if directory is locked right now
        /// </summary>
        /// <returns>
        /// If directory is locked return true otherwise returns false
        /// </returns>
        public static bool IsLocked(string directory)
        {
            lock (threadSafe)
            {
                return lockedDirectories.Contains(directory);
            }
        }

        /// <summary>
        /// Unlock directory.
        /// </summary>
        /// <param name="directory">
        /// Directory path in Windows format.
        /// </param>
        /// <seealso cref="MutexOn(string)"/>
        public static void MutexOff(string directory)
        {
            string path = string.Format("{0}{1}", directory.TrimEnd('\\', ' '), Path.DirectorySeparatorChar);
            lock (lockedDirectories)
            {
                lockedDirectories.Remove(path);
            }
        }
    }
}
