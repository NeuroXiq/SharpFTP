using System;
using SharpFTP.Server.Protocol;

namespace SharpFTP.Server
{
    public sealed class ServerDataContext
    {
        private static volatile ServerDataContext instance;
        private static object threadSafe = new object();
        private static IDataContextProvider dataContext;

        public IDataContextProvider DataContextInstance { get { return dataContext; } }

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
                            if (dataContext == null)
                                throw new Exception("Data context is not set in current instance!");
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

        public static void SetDataContext(IDataContextProvider contextProvider)
        {
            if (dataContext == null)
            {
                dataContext = contextProvider;
            }
            else
            {
                throw new Exception("Cannot set second data context provider");
            }
        }
    }
}
