using System;
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
            descriptions.Add(110,@" Restart marker reply.In this case the text is exact and not left to theparticular implementation; it must read:");            descriptions.Add(119,@" Terminal not available, will try mailbox.");            descriptions.Add(120,@" Service ready in nnn minutes");            descriptions.Add(125,@" Data connection already open; transfer starting");            descriptions.Add(150,@" File status okay; about to open data connection.");            descriptions.Add(151,@" User not local; Will forward to < user >@< host >.");            descriptions.Add(152,@" User Unknown; Mail will be forwarded by the operator.");            descriptions.Add(200,@" Command okay");            descriptions.Add(202,@" Command not implemented, superfluous at this site.");            descriptions.Add(211,@" System status, or system help reply");            descriptions.Add(212,@" Directory status");            descriptions.Add(213,@" File status");            descriptions.Add(214,@" Help message");            descriptions.Add(215,@" < scheme > is the preferred scheme.");            descriptions.Add(220,@" Service ready for new user");            descriptions.Add(221,@" Service closing TELNET connection(logged out if appropriate);");            descriptions.Add(225,@" Data connection open; no transfer in progress");            descriptions.Add(226,@" Closing data connection;requested file action successful (for example, file transferor file abort.);");            descriptions.Add(227,@" Entering Passive Mode.h1,h2,h3,h4,p1,p2");            descriptions.Add(230,@" User logged in, proceed");            descriptions.Add(250,@" Requested file action okay, completed.");            descriptions.Add(331,@" User name okay, need password");            descriptions.Add(332,@" Need account for login");            descriptions.Add(350,@" Requested file action pending further information");            descriptions.Add(354,@" Start mail input; end with < CR >< LF >.< CR >< LF >");            descriptions.Add(421,@" Service not available, closing TELNET connection.This may be a reply to any command if the service knows itmust shut down.]");            descriptions.Add(425,@" Can't open data connection");            descriptions.Add(426,@" Connection closed; transfer aborted.");            descriptions.Add(450,@" Requested file action not taken:file unavailable (e.g.file busy);");            descriptions.Add(451,@" Requested action aborted: local error in processing");            descriptions.Add(452,@" Requested action not taken:insufficient storage space in system");            descriptions.Add(500,@" Syntax error, command unrecognized[This may include errors such as command line too long.]");            descriptions.Add(501,@" Syntax error in parameters or arguments");            descriptions.Add(502,@" Command not implemented");            descriptions.Add(503,@" Bad sequence of commands");            descriptions.Add(504,@" Command not implemented for that parameter");            descriptions.Add(530,@" Not logged in");            descriptions.Add(532,@" Need account for storing files");            descriptions.Add(550,@" Requested action not taken:file unavailable(e.g.file not found, no access);");            descriptions.Add(551,@" Requested action aborted: page type unknown");            descriptions.Add(552,@" Requested file action aborted: exceeded storage allocation(for current directory ordataset);");            descriptions.Add(553,@" Requested action not taken: file name not allowed");

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
