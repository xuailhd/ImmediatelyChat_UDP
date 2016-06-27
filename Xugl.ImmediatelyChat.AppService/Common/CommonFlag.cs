using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.AppService.Common
{
    public class CommonFlag
    {
        private static string f_IP="IP";
        private static string f_Port = "Port";
        private static string f_ObjectID = "ObjectID";
        private static string f_ObjectName = "ObjectName";

        public static string F_IP
        {
            get { return f_IP; }
        }

        public static string F_Port
        {
            get { return f_Port; }
        }

        public static string F_ObjectID
        {
            get { return f_ObjectID; }
        }

        public static string F_ObjectName
        {
            get { return f_ObjectName; }
        }

        private static string f_Content = "Content";

        private static string f_MsgType="MsgType";

        private static string f_SendType="SendType";

        private static string f_RecivedObjectID = "RecivedObjectID";

        private static string f_RecivedObjectID2 = "RecivedObjectID2";

        public static string F_Content { get { return f_Content; } }

        public static string F_MsgType { get { return f_MsgType; } }

        public static string F_SendType { get { return f_SendType; } }

        public static string F_RecivedObjectID { get { return f_RecivedObjectID; } }

        public static string F_RecivedObjectID2 { get { return f_RecivedObjectID2; } }
    }
}
