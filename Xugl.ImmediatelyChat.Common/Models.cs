using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.Common
{
    public class ClientModel
    {
        public string ObjectID { get; set; }
        public string Client_IP { get; set; }
        public int Client_Port { get; set; }
        public string MCS_IP { get; set; }
        public int MCS_Port { get; set; }
        public string MDS_IP { get; set; }
        public int MDS_Port { get; set; }
        public string LatestTime { get; set; }
        public string UpdateTime { get; set; }
    }

    public class ClientSearchModel
    {
        public string ObjectID { get; set; }
        /// <summary>
        /// 1 person  2 group
        /// </summary>
        public int Type { get; set; }
        public string SearchKey { get; set; }
    }

    public class ClientAddPerson
    {
        public string ObjectID { get; set; }
        public string DestinationObjectID { get; set; }
        public string MCS_IP { get; set; }
        public int MCS_Port { get; set; }

        /// <summary>
        /// 0/1/2  normal/existing/error
        /// </summary>
        public int Status { get; set; }
    }

    public class ClientAddGroup
    {
        public string ObjectID { get; set; }
        public string GroupObjectID { get; set; }
        public string MCS_IP { get; set; }
        public int MCS_Port { get; set; }
        /// <summary>
        /// 0/1/2  normal/existing/error
        /// </summary>
        public int Status { get; set; }
    }

    public class ContactDataWithMCS
    {
        public ContactData ContactData { get; set; }

        public string MCS_IP { get; set; }

        public int MCS_Port { get; set; }
    }

    public class ContactData
    {
        public string ObjectID { get; set; }
        public string Password { get; set; }
        public string ImageSrc { get; set; }
        public string ContactName { get; set; }
        public string LatestTime { get; set; }

        public string ContactPersonName { get; set; }
        public string DestinationObjectID { get; set; }


        public string GroupObjectID { get; set; }
        public string GroupName { get; set; }

        public string ContactPersonObjectID { get; set; }
        public string ContactGroupID { get; set; }


        public bool IsDelete { get; set; }
        public string UpdateTime { get; set; }

        /// <summary>
        /// 0-ContactPerson/1-ContactPersonList/2-ContactGroup/3-ContactGroupSub
        /// </summary>
        public int DataType { get; set; }
        public string ContactDataID { get; set; }

    }



    public class MsgRecordModel
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


        public string MDS_IP { get; set; }
        public int MDS_Port { get; set; }

        /// <summary>
        /// 0:normal  1:success  2:error
        /// </summary>
        //public int Status { get; set; }
    }

    public class GetMsgModel
    {
        public string MessageID { get; set; }
        public string ObjectID { get; set; }

        public string LatestTime { get; set; }

        public string MDS_IP { get; set; }
        public int MDS_Port { get; set; }
        /// <summary>
        /// 0:normal  1:success  2:error
        /// </summary>
        //public int Status { get; set; }
    }

}
