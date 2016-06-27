using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.Common;
using Xugl.ImmediatelyChat.Core;
using Xugl.ImmediatelyChat.Core.DependencyResolution;
using Xugl.ImmediatelyChat.IServices;
using Xugl.ImmediatelyChat.Model;
using Xugl.ImmediatelyChat.SocketEngine;

namespace Xugl.ImmediatelyChat.MessageChildServer
{
    internal class UDPSocketListener : AsyncSocketListenerUDP<MCSListenerToken>
    {
        public UDPSocketListener()
            : base(1024, 100, CommonVariables.LogTool)
        {
        }

        protected override void HandleError(MCSListenerToken token)
        {
            if (token.Models != null && token.Models.Count > 0)
            {
                token.Models.Clear();
                token.Models = null;
            }
            return;
        }

        protected override string HandleRecivedMessage(string inputMessage, MCSListenerToken token)
        {
            if (string.IsNullOrEmpty(inputMessage) || token == null)
            {
                return string.Empty;
            }

            try
            {
                string data = inputMessage;

                if (data.StartsWith(CommonFlag.F_PSCallMCSStart))
                {
                    return HandlePSCallMCSStart(data, token);
                }

                if (CommonVariables.IsBeginMessageService)
                {
                    //handle UA feedback
                    if (data.StartsWith(CommonFlag.F_MCSReceiveUAFBMSG))
                    {
                        return HandleMCSReceiveUAFBMSG(data, token);
                    }

                    if (data.StartsWith(CommonFlag.F_MCSVerifyUA))
                    {
                        return HandleMCSVerifyUA(data, token);
                    }

                    if (data.StartsWith(CommonFlag.F_MCSReceiveMMSUAUpdateTime))
                    {
                        return HandleMCSReceiveMMSUAUpdateTime(data, token);
                    }

                    if (data.StartsWith(CommonFlag.F_MCSReceiveUAInfo))
                    {
                        return HandleMCSReceiveUAInfo(data, token);
                    }

                    if (data.StartsWith(CommonFlag.F_MCSVerifyUAMSG))
                    {
                        return HandleMCSVerifyUAMSG(data, token);
                    }

                    if (data.StartsWith(CommonFlag.F_MCSVerifyUAGetMSG))
                    {
                        return HandleMCSVerifyUAGetMSG(data, token);
                    }

                    if (data.StartsWith(CommonFlag.F_MCSVerfiyMDSMSG))
                    {
                        return HandleMCSVerfiyMDSMSG(data, token);
                    }
                }
            }
            catch (Exception ex)
            {
                CommonVariables.LogTool.Log(ex.Message + ex.StackTrace);
            }
            return string.Empty;
        }

        private string HandleMCSVerfiyMDSMSG(string data,MCSListenerToken token)
        {
            string tempStr = data.Remove(0, CommonFlag.F_MCSVerfiyMDSMSG.Length);

            MsgRecord tempMsgRecord = CommonVariables.serializer.Deserialize<MsgRecord>(tempStr);
            if (tempMsgRecord != null)
            {
                if (!string.IsNullOrEmpty(tempMsgRecord.MsgID))
                {
                    CommonVariables.MessageContorl.AddMsgIntoOutBuffer(tempMsgRecord);
                    return CommonFlag.F_MDSReciveMCSFBMSG + tempMsgRecord.MsgID;
                }
            }
            return string.Empty;
        }

        private string HandlePSCallMCSStart(string data,MCSListenerToken token)
        {
            data = data.Remove(0, CommonFlag.F_PSCallMCSStart.Length);
            IList<MDSServer> mdsServers = CommonVariables.serializer.Deserialize<IList<MDSServer>>(data.Substring(0, data.IndexOf("&&")));

            if (mdsServers != null && mdsServers.Count > 0)
            {
                data = data.Remove(0, data.IndexOf("&&") + 2);
                CommonVariables.ArrangeStr = CommonVariables.serializer.Deserialize<MCSServer>(data).ArrangeStr;
                CommonVariables.OperateFile.SaveConfig(CommonVariables.ConfigFilePath, CommonFlag.F_ArrangeChars, CommonVariables.ArrangeStr);
                CommonVariables.LogTool.Log("ArrangeStr:" + CommonVariables.ArrangeStr);
                CommonVariables.LogTool.Log("MDS count:" + mdsServers.Count);
                foreach (MDSServer mdsServer in mdsServers)
                {
                    CommonVariables.MDSServers.Add(mdsServer);
                    CommonVariables.LogTool.Log("IP:" + mdsServer.MDS_IP + " Port:" + mdsServer.MDS_Port + "  ArrangeStr:" + mdsServer.ArrangeStr);
                }
                CommonVariables.LogTool.Log("Start MCS service:" + CommonVariables.MCSIP + ", Port:" + CommonVariables.MCSPort.ToString());
                CommonVariables.IsBeginMessageService = true;
            }
            return string.Empty;
        }

        private string HandleMCSReceiveUAFBMSG(string data,MCSListenerToken token)
        {
            string tempStr = data.Remove(0, CommonFlag.F_MCSReceiveUAFBMSG.Length);
            if (token.Models != null && token.Models.Count > 0)
            {
                if (token.Models[0].MsgID == tempStr)
                {
                    token.Models.RemoveAt(0);
                }
                else
                {
                    for (int i = 1; i < token.Models.Count; i++)
                    {
                        if (token.Models[i].MsgID == tempStr)
                        {
                            token.Models.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            if (token.Models != null && token.Models.Count > 0)
            {
                return CommonVariables.serializer.Serialize(token.Models[0]);
            }
            else
            {
                return string.Empty;
            }

        }

        private string HandleMCSVerifyUA(string data,MCSListenerToken token)
        {
            string tempStr = data.Remove(0, CommonFlag.F_MCSVerifyUA.Length);
            ClientModel clientModel = CommonVariables.serializer.Deserialize<ClientModel>(tempStr);
            if (clientModel != null)
            {
                if (!string.IsNullOrEmpty(clientModel.ObjectID))
                {
                    ContactPerson contactPerson = token.ContactPersonService.FindContactPerson(clientModel.ObjectID);
                    if (contactPerson != null)
                    {
                        //CommonVariables.LogTool.Log(contactPerson.UpdateTime + " VS" + clientModel.UpdateTime);
                        if (contactPerson.UpdateTime.CompareTo(clientModel.UpdateTime) == 0)
                        {
                            CommonVariables.MessageContorl.AddClientModel(clientModel);

                            //CommonVariables.LogTool.Log(clientModel.ObjectID + " VS" + clientModel.LatestTime + "VS" + clientModel.MDS_IP);

                            CommonVariables.MessageContorl.SendGetMsgToMDS(clientModel);
                            return "ok";
                        }

                    }
                    return "wait";
                }
            }
            return string.Empty;
        }

        private string HandleMCSReceiveMMSUAUpdateTime(string data,MCSListenerToken token)
        {
            ClientModel clientModel = CommonVariables.serializer.Deserialize<ClientModel>(data.Remove(0, CommonFlag.F_MCSReceiveMMSUAUpdateTime.Length));
            ContactPerson contactPerson = token.ContactPersonService.FindContactPerson(clientModel.ObjectID);

            if (contactPerson == null)
            {
                contactPerson = new ContactPerson();
                contactPerson.ObjectID = clientModel.ObjectID;
                contactPerson.LatestTime = clientModel.LatestTime;
                contactPerson.UpdateTime = CommonFlag.F_MinDatetime;
                token.ContactPersonService.InsertNewPerson(contactPerson);
            }
            //clientModel.UpdateTime = contactPerson.UpdateTime;
            return contactPerson.UpdateTime;
        }

        private string HandleMCSReceiveUAInfo(string data,MCSListenerToken token)
        {
            ContactData contactData = CommonVariables.serializer.Deserialize<ContactData>(data.Remove(0, CommonFlag.F_MCSReceiveUAInfo.Length));

            if (string.IsNullOrEmpty(contactData.ContactDataID))
            {
                return string.Empty;
            }

            return HandleMMSUAInfo(contactData, token.ContactPersonService);
        }

        private string HandleMCSVerifyUAMSG(string data,MCSListenerToken token)
        {
            string tempStr = data.Remove(0, CommonFlag.F_MCSVerifyUAMSG.Length);
            MsgRecordModel msgModel = CommonVariables.serializer.Deserialize<MsgRecordModel>(tempStr);

            if (msgModel != null)
            {
                if (!string.IsNullOrEmpty(msgModel.MsgSenderObjectID))
                {
                    CommonVariables.MessageContorl.AddMsgRecordIntoBuffer(msgModel);
                    return msgModel.MsgID;
                }
            }
            return string.Empty;
        }

        private string HandleMCSVerifyUAGetMSG(string data,MCSListenerToken token)
        {
            string tempStr = data.Remove(0, CommonFlag.F_MCSVerifyUAGetMSG.Length);
            ClientModel getMsgModel = CommonVariables.serializer.Deserialize<ClientModel>(tempStr);
            if (getMsgModel != null)
            {
                if (!string.IsNullOrEmpty(getMsgModel.ObjectID))
                {
                    CommonVariables.MessageContorl.UpdateClientModel(getMsgModel);
                    token.Models = CommonVariables.MessageContorl.GetMSG(getMsgModel);
                    if (token.Models != null && token.Models.Count > 0)
                    {
                        token.UAObjectID = getMsgModel.ObjectID;
                        return CommonVariables.serializer.Serialize(token.Models[0]);
                    }
                }
            }
            return string.Empty;
        }

        private string HandleMMSUAInfo(ContactData contactData,IContactPersonService contactPersonService)
        {
            try
            {
                int temp = 0;
                ContactPerson contactPerson = contactPersonService.FindContactPerson(contactData.ObjectID);

                if (contactPerson == null)
                {
                    CommonVariables.LogTool.Log("ContactPerson " + contactData.ObjectID + " can not find");
                    return string.Empty;
                }

                if(contactData.DataType==0)
                {
                    contactPerson.ContactName = contactData.ContactName;
                    contactPerson.ImageSrc = contactData.ImageSrc;
                    contactPerson.LatestTime = contactData.LatestTime;
                    if (contactData.UpdateTime.CompareTo(contactPerson.UpdateTime) > 0)
                    {
                        contactPerson.UpdateTime = contactData.UpdateTime;
                    }
                    contactPersonService.UpdateContactPerson(contactPerson);
                }


                if (contactData.DataType == 1)
                {
                    ContactPersonList contactPersonList = contactPersonService.FindContactPersonList(contactData.ObjectID, contactData.DestinationObjectID);
                    if (contactPersonList == null)
                    {
                        contactPersonList = new ContactPersonList();
                        contactPersonList.DestinationObjectID = contactData.DestinationObjectID;
                        contactPersonList.IsDelete = contactData.IsDelete;
                        contactPersonList.ObjectID = contactData.ObjectID;
                        contactPersonList.UpdateTime = contactData.UpdateTime;
                        contactPersonService.InsertContactPersonList(contactPersonList);
                    }
                    else
                    {
                        contactPersonList.IsDelete = contactData.IsDelete;
                        contactPersonList.UpdateTime = contactData.UpdateTime;
                        contactPersonService.UpdateContactPersonList(contactPersonList);
                    }

                    if (contactPersonList.UpdateTime.CompareTo(contactPerson.UpdateTime) > 0)
                    {
                        contactPerson.UpdateTime = contactPersonList.UpdateTime;
                        contactPersonService.UpdateContactPerson(contactPerson);
                    }

                }
                else if (contactData.DataType == 2)
                {
                    ContactGroup contactGroup = contactPersonService.FindContactGroup(contactData.GroupObjectID);
                    if (contactGroup == null)
                    {
                        contactGroup = new ContactGroup();
                        contactGroup.GroupName = contactData.GroupName;
                        contactGroup.GroupObjectID = contactData.GroupObjectID;
                        contactGroup.IsDelete = contactData.IsDelete;
                        contactGroup.UpdateTime = contactData.UpdateTime;
                        contactPersonService.InsertNewGroup(contactGroup);
                    }
                    else
                    {
                        contactGroup.GroupName = contactData.GroupName;
                        contactGroup.IsDelete = contactData.IsDelete;
                        contactGroup.UpdateTime = contactData.UpdateTime;
                        contactPersonService.UpdateContactGroup(contactGroup);
                    }

                    if (contactGroup.UpdateTime.CompareTo(contactPerson.UpdateTime) > 0)
                    {
                        contactPerson.UpdateTime = contactGroup.UpdateTime;
                        contactPersonService.UpdateContactPerson(contactPerson);
                    }
                }
                else if (contactData.DataType == 3)
                {
                    ContactGroupSub contactGroupSub = contactPersonService.FindContactGroupSub(contactData.ContactGroupID, contactData.ContactPersonObjectID);
                    if (contactGroupSub == null)
                    {
                        contactGroupSub = new ContactGroupSub();
                        contactGroupSub.ContactGroupID = contactData.ContactGroupID;
                        contactGroupSub.ContactPersonObjectID = contactData.ContactPersonObjectID;
                        contactGroupSub.IsDelete = contactData.IsDelete;
                        contactGroupSub.UpdateTime = contactData.UpdateTime;
                        contactPersonService.InsertContactGroupSub(contactGroupSub);
                    }
                    else
                    {
                        contactGroupSub.IsDelete = contactData.IsDelete;
                        contactGroupSub.UpdateTime = contactData.UpdateTime;
                        contactPersonService.UpdateContactGroupSub(contactGroupSub);
                    }

                    if (contactGroupSub.UpdateTime.CompareTo(contactPerson.UpdateTime) > 0)
                    {
                        contactPerson.UpdateTime = contactGroupSub.UpdateTime;
                        contactPersonService.UpdateContactPerson(contactPerson);
                    }
                }
                return contactData.ContactDataID;
            }
            catch (Exception ex)
            {
                CommonVariables.LogTool.Log("get UAInfo " + ex.Message + ex.StackTrace);
                return string.Empty;
            }
        }

        public void BeginService()
        {
            CommonVariables.MessageContorl.StartMainThread();
            base.BeginService(CommonVariables.MCSIP,CommonVariables.MCSPort);
        }
    }

}
