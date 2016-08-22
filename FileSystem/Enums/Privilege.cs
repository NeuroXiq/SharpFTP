using System;

namespace SharpFTP.Server.FileSystem.Enums
{
    [Flags]
    public enum FilePermission
    {
        Read,
        Write,
        ACCESS_DENIED
    }
}