using System;

namespace SharpFTP.Server
{
    public sealed class ServerDataContext
    {
        private static volatile ServerDataContext instance;
        private static object threadSafe = new object();
        private static UserDataContext userDataContext;

        public UserDataContext UserDataContextProvider { get { return userDataContext; } }

        public static ServerDataContext Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (threadSafe)
                    {
                        if (instance == null)
                        {
                            if (userDataContext == null)
                                throw new Exception("User data context is nullable value, cannot initialize ServerDataContext");
                            instance = new ServerDataContext();
                        }
                    }
                }
                return instance;
            }
        }

        private ServerDataContext()
        {

        }

        public static void SetDataContext(UserDataContext dataContext)
        {
            userDataContext = dataContext;
        }   
    }
}
