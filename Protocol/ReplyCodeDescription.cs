﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpFTP.Server.Protocol
{
    public static class ReplyCodeDescription
    {
        static Dictionary<int, string> replyCodesDescriptions;

        static ReplyCodeDescription()
        {
            replyCodesDescriptions = GetDescriptionsDictionary();
        }

        private static Dictionary<int, string> GetDescriptionsDictionary()
        {
            Dictionary<int, string> descriptions = new Dictionary<int, string>();
            descriptions.Add(110,@" Restart marker reply.In this case the text is exact and not left to theparticular implementation; it must read:");

            return descriptions;
        }

        public static string GetDescription(int replyCode)
        {
            if (replyCodesDescriptions.ContainsKey(replyCode))
            {
                return replyCodesDescriptions[replyCode];
            }
            else
            {
                throw new Exception("cannot find reply code");
            }
        }
    }
}