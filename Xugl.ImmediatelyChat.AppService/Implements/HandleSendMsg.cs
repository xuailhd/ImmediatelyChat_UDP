using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.AppService.Common;
using Xugl.ImmediatelyChat.AppService.Interfaces;
using Xugl.ImmediatelyChat.Model;
using Xugl.ImmediatelyChat.Core;
using Xugl.ImmediatelyChat.IServices;
using Xugl.ImmediatelyChat.Core.DependencyResolution;
using System.Net.Sockets;
using System.Net;
using System.Web.Script.Serialization;
using System.Json;
using Xugl.ImmediatelyChat.AppService.Model;

namespace Xugl.ImmediatelyChat.AppService.Implements
{
    public class HandleSendMsg : IHandleSendMsg
    {

        private readonly IContactPersonService contactGroupService;
        private readonly IRepository<ContactPerson> contactPersonRepository;

        public HandleSendMsg(IContactPersonService contactGroupService,IRepository<ContactPerson> contactPersonRepository)
        {
            this.contactGroupService = contactGroupService;
            this.contactPersonRepository = contactPersonRepository;
        }

        public void Handler(object msgstr)
        {
            MsgRecord msgRecord=UnboxMsg(msgstr.ToString());


            if (!CheckMsg(msgRecord))
            {
                return;
            }

            CommonVariables.MsgRecordContainer.AddMsg(msgRecord);
        }


        private MsgRecord UnboxMsg(string msg)
        {
            string tempMsg = msg;

            MsgRecord msgModel = new MsgRecord();

            if (tempMsg.IndexOf(CommonFlag.F_ObjectID) >= 0 && tempMsg.IndexOf(CommonFlag.F_ObjectID) < tempMsg.IndexOf(CommonFlag.F_Content))
            {
                msgModel.MsgSenderObjectID = tempMsg.Substring(tempMsg.IndexOf(CommonFlag.F_ObjectID) + CommonFlag.F_ObjectID.Length,
                    tempMsg.IndexOf(";", tempMsg.IndexOf(CommonFlag.F_ObjectID)) - tempMsg.IndexOf(CommonFlag.F_ObjectID) - CommonFlag.F_ObjectID.Length);
                tempMsg = tempMsg.Replace(CommonFlag.F_ObjectID + msgModel.MsgSenderObjectID + ";", "");
            }

            if (tempMsg.IndexOf(CommonFlag.F_MsgType) >= 0 && tempMsg.IndexOf(CommonFlag.F_MsgType) < tempMsg.IndexOf(CommonFlag.F_Content))
            {
                msgModel.MsgType = Convert.ToInt32(tempMsg.Substring(tempMsg.IndexOf(CommonFlag.F_MsgType) + CommonFlag.F_MsgType.Length,
                    tempMsg.IndexOf(";", tempMsg.IndexOf(CommonFlag.F_MsgType)) - tempMsg.IndexOf(CommonFlag.F_MsgType) - CommonFlag.F_MsgType.Length));
                tempMsg = tempMsg.Replace(CommonFlag.F_MsgType + msgModel.MsgType + ";", "");
            }

            if (tempMsg.IndexOf(CommonFlag.F_RecivedObjectID) >= 0 && tempMsg.IndexOf(CommonFlag.F_RecivedObjectID) < tempMsg.IndexOf(CommonFlag.F_Content))
            {
                msgModel.MsgRecipientObjectID = tempMsg.Substring(tempMsg.IndexOf(CommonFlag.F_RecivedObjectID) + CommonFlag.F_RecivedObjectID.Length,
                    tempMsg.IndexOf(";", tempMsg.IndexOf(CommonFlag.F_RecivedObjectID)) - tempMsg.IndexOf(CommonFlag.F_RecivedObjectID) - CommonFlag.F_RecivedObjectID.Length);
                tempMsg = tempMsg.Replace(CommonFlag.F_RecivedObjectID + msgModel.MsgRecipientObjectID + ";", "");
            }

            if (tempMsg.IndexOf(CommonFlag.F_RecivedObjectID2) >= 0 && tempMsg.IndexOf(CommonFlag.F_RecivedObjectID2) < tempMsg.IndexOf(CommonFlag.F_Content))
            {
                msgModel.MsgRecipientObjectID2 = tempMsg.Substring(tempMsg.IndexOf(CommonFlag.F_RecivedObjectID2) + CommonFlag.F_RecivedObjectID2.Length,
                    tempMsg.IndexOf(";", tempMsg.IndexOf(CommonFlag.F_RecivedObjectID2)) - tempMsg.IndexOf(CommonFlag.F_RecivedObjectID2) - CommonFlag.F_RecivedObjectID2.Length);
                tempMsg = tempMsg.Replace(CommonFlag.F_RecivedObjectID2 + msgModel.MsgRecipientObjectID2 + ";", "");
            }

            if (tempMsg.IndexOf(CommonFlag.F_SendType) >= 0 && tempMsg.IndexOf(CommonFlag.F_SendType) < tempMsg.IndexOf(CommonFlag.F_Content))
            {
                msgModel.SendType = Convert.ToInt32(tempMsg.Substring(tempMsg.IndexOf(CommonFlag.F_SendType) + CommonFlag.F_SendType.Length,
                    tempMsg.IndexOf(";", tempMsg.IndexOf(CommonFlag.F_SendType)) - tempMsg.IndexOf(CommonFlag.F_SendType) - CommonFlag.F_SendType.Length));
                tempMsg = tempMsg.Replace(CommonFlag.F_SendType + msgModel.SendType + ";", "");
            }

            if (tempMsg.IndexOf(CommonFlag.F_Content) >= 0)
            {
                msgModel.MsgContent = tempMsg.Replace(CommonFlag.F_Content, "");
                //tempMsg = tempMsg.Replace(CommonFlag.F_SendType + msgModel.SendType + ";", "");
            }

            msgModel.SendTime = DateTime.Now;

            return msgModel;
        }


        public void SendMsg(object msgRecord)
        {

            if(!CheckMsg((MsgRecordModel)msgRecord))
            {
                return;
            }

            if (((MsgRecordModel)msgRecord).SendType == 1)
            {
                SendGroupMsg((MsgRecordModel)msgRecord);
            }

        }

        private void SendGroupMsg(MsgRecordModel msgRecord)
        {

            IList<ContactPerson> contactPersons = contactGroupService.GetContactPersonIDListByGroupID(msgRecord.RecivedObjectID);
            ContactPerson curcontactPerson = contactPersonRepository.Find(t => t.ObjectID == msgRecord.ObjectID);
            msgRecord.ObjectName = curcontactPerson.ContactName;

            foreach (ContactPerson contactPerson in contactPersons)
            {
                if (CommonVariables.CurrentClientsContainKey(contactPerson.ObjectID))
                {
                    SendMsg(CommonVariables.GetCurrentClients[contactPerson.ObjectID].Recive_IP, CommonVariables.GetCurrentClients[contactPerson.ObjectID].Recive_Port,contactPerson.ObjectID, msgRecord);

                    //msgRecord.IsSended = true;
                }
                else
                {

                }
            }
        }

        private bool CheckMsg(MsgRecordModel msgRecord)
        {
            bool IsWholeTrue = true;
            if (!CommonVariables.CurrentClientsContainKey(msgRecord.ObjectID))
            {
                IsWholeTrue = false;
            }

            return IsWholeTrue;
        }

        private bool CheckMsg(MsgRecord msgRecord)
        {
            bool IsWholeTrue = true;
            if (string.IsNullOrEmpty(msgRecord.MsgSenderObjectID))
            {
                IsWholeTrue = false;
            }

            return IsWholeTrue;
        }

        private void SendMsg(string recive_IP, int recive_Port,string objectID, MsgRecordModel msgRecord)
        {
            byte[] bytesSent;

            JavaScriptSerializer js = new JavaScriptSerializer();

            string msg= js.Serialize(msgRecord);

            bytesSent = Encoding.UTF8.GetBytes(msg);

            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(recive_IP), recive_Port);
            Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            { 
                tempSocket.Connect(ipe);

                tempSocket.Send(bytesSent, bytesSent.Length, 0);

                tempSocket.Close();
            }
            catch(SocketException ex)
            {
                CommonVariables.RemoveCurrentClients(objectID);
            }
            catch(Exception ex)
            {
                CommonVariables.RemoveCurrentClients(objectID);
            }
        }
    }
}
