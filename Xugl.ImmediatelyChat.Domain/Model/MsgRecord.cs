using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.Model
{
    public class MsgRecord
    {
        public string MsgID { get; set; }

        public string MsgSenderObjectID { get; set; }

        public string MsgSenderName { get; set; }

        public string MsgContent { get; set; }

        public string MsgRecipientObjectID { get; set; }

        public string MsgRecipientGroupID { get; set; }

        /// <summary>
        /// 1:text;2:file
        /// </summary>
        public int MsgType { get; set; }

        public string SendTime { get; set; }

        public bool IsSended { get; set; }
    }
}
