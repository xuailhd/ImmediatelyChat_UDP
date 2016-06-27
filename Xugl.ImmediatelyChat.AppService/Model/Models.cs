using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.AppService.Model
{
    public class ClientStatusModel
    {
        public string Recive_IP { get; set; }

        public int Recive_Port { get; set; }

        public string ObjectID { get; set; }
    }

    public class MsgRecordModel
    {
        public string ObjectID { get; set; }

        public string ObjectName { get; set; }

        public string Content { get; set; }

        public string RecivedObjectID { get; set; }

        public string RecivedObjectID2 { get; set; }

        /// <summary>
        /// 0:text;1:file
        /// </summary>
        public int MsgType { get; set; }

        /// <summary>
        /// 0:single;1:group
        /// </summary>
        public int SendType { get; set; }

        public bool IsGetMessage { get; set; }

    }

    public class GetMsgModel
    {
        public string ObjectID { get; set; }

        public IList<string> GroupIDs { get; set; }

        public DateTime EndTime { get; set; }

        public DateTime StartTime { get; set; }
    }

}
