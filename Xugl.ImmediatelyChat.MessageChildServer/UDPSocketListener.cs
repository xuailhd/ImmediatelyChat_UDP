using Newtonsoft.Json;
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

    public class MCSListenerUDPToken : AsyncUserToken
    {
        private readonly IContactPersonService _contactPersonService;

        public MCSListenerUDPToken()
        {
            _contactPersonService = ObjectContainerFactory.CurrentContainer.Resolver<IContactPersonService>();
        }

        public MsgRecord Model { get; set; }

        public string UAObjectID { get; set; }

        public string TaskFlag { get; set; }

        public string UAIP { get; set; }

        public int UAPort { get; set; }

        public IContactPersonService ContactPersonService
        {
            get
            {
                return _contactPersonService;
            }
        }
    }

    public class UDPSocketListener : AsyncSocketListenerUDP<MCSListenerUDPToken>
    {
        public UDPSocketListener()
            : base(1024, 100, 20, CommonVariables.LogTool)
        {
        }

        protected override void HandleError(MCSListenerUDPToken token)
        {
            if (token.Model!= null)
            {

            }
            return;
        }

        protected override string HandleRecivedMessage(string inputMessage, MCSListenerUDPToken token)
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

                    if (data.StartsWith(CommonFlag.F_MCSVerifyUAInfo))
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

        private string HandleMCSVerfiyMDSMSG(string data, MCSListenerUDPToken token)
        {
            string tempStr = data.Remove(0, CommonFlag.F_MCSVerfiyMDSMSG.Length);

            MsgRecord tempMsgRecord = JsonConvert.DeserializeObject<MsgRecord>(tempStr);
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

        private string HandlePSCallMCSStart(string data, MCSListenerUDPToken token)
        {
            data = data.Remove(0, CommonFlag.F_PSCallMCSStart.Length);
            IList<MDSServer> mdsServers = JsonConvert.DeserializeObject<IList<MDSServer>>(data.Substring(0, data.IndexOf("&&")));

            if (mdsServers != null && mdsServers.Count > 0)
            {
                data = data.Remove(0, data.IndexOf("&&") + 2);
                CommonVariables.ArrangeStr = JsonConvert.DeserializeObject<MCSServer>(data).ArrangeStr;
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

        private string HandleMCSReceiveUAFBMSG(string data, MCSListenerUDPToken token)
        {
            return string.Empty;
        }

        private string HandleMCSVerifyUA(string data, MCSListenerUDPToken token)
        {
            string tempStr = data.Remove(0, CommonFlag.F_MCSVerifyUA.Length);
            ClientModel clientModel = JsonConvert.DeserializeObject<ClientModel>(tempStr);
            if (clientModel != null)
            {
                if (!string.IsNullOrEmpty(clientModel.ObjectID))
                {
                    ContactPerson contactPerson = token.ContactPersonService.FindContactPerson(clientModel.ObjectID);
                    if (contactPerson != null)
                    {
                        if(!string.IsNullOrEmpty(token.IP))
                        {
                            clientModel.Client_IP = token.IP;
                            clientModel.Client_Port = token.Port;
                            CommonVariables.MessageContorl.AddClientModel(clientModel);
                            CommonVariables.MessageContorl.SendGetMsgToMDS(clientModel);
                            return "ok";
                        }                        
                    }
                }
            }
            return string.Empty;
        }

        private string HandleMCSReceiveMMSUAUpdateTime(string data, MCSListenerUDPToken token)
        {
            ClientModel clientModel = JsonConvert.DeserializeObject<ClientModel>(data.Remove(0, CommonFlag.F_MCSReceiveMMSUAUpdateTime.Length));
            ContactPerson contactPerson = token.ContactPersonService.FindContactPerson(clientModel.ObjectID);

            if (contactPerson == null)
            {
                contactPerson = new ContactPerson();
                contactPerson.ObjectID = clientModel.ObjectID;
                contactPerson.LatestTime = clientModel.LatestTime;
                contactPerson.UpdateTime = CommonFlag.F_MinDatetime;
                token.ContactPersonService.InsertNewPerson(contactPerson);
            }

            if (contactPerson.UpdateTime.CompareTo(clientModel.UpdateTime)<0)
            {
                return CommonFlag.F_MMSVerifyMCSGetUAInfo + JsonConvert.SerializeObject(clientModel);
            }
            return string.Empty;
        }

        private string HandleMCSReceiveUAInfo(string data, MCSListenerUDPToken token)
        {
            ContactData contactData = JsonConvert.DeserializeObject<ContactData>(data.Remove(0, CommonFlag.F_MCSVerifyUAInfo.Length));

            if (string.IsNullOrEmpty(contactData.ContactDataID))
            {
                return string.Empty;
            }

            return CommonFlag.F_MMSVerifyMCSFBGetUAInfo + HandleMMSUAInfo(contactData, token.ContactPersonService);
        }

        private string HandleMCSVerifyUAMSG(string data, MCSListenerUDPToken token)
        {
            string tempStr = data.Remove(0, CommonFlag.F_MCSVerifyUAMSG.Length);
            MsgRecordModel msgModel = JsonConvert.DeserializeObject<MsgRecordModel>(tempStr);

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

        private string HandleMCSVerifyUAGetMSG(string data, MCSListenerUDPToken token)
        {
            string tempStr = data.Remove(0, CommonFlag.F_MCSVerifyUAGetMSG.Length);
            ClientModel clientModel = JsonConvert.DeserializeObject<ClientModel>(tempStr);
            if (clientModel != null)
            {
                if (!string.IsNullOrEmpty(clientModel.ObjectID))
                {
                    clientModel.Client_IP = token.IP;
                    clientModel.Client_Port = token.Port;
                    CommonVariables.MessageContorl.UpdateClientModel(clientModel);
                }
            }
            return string.Empty;
        }

        private string HandleMMSUAInfo(ContactData contactData,IContactPersonService contactPersonService)
        {
            try
            {
                if(contactData.DataType == 3 && String.IsNullOrEmpty(contactData.ObjectID))
                {
                    ContactGroup contactGroup = contactPersonService.FindContactGroup(contactData.ContactGroupID);
                    if (contactGroup != null)
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
                            if (contactGroupSub.UpdateTime.CompareTo(contactData.UpdateTime) < 0)
                            {
                                contactGroupSub.IsDelete = contactData.IsDelete;
                                contactGroupSub.UpdateTime = contactData.UpdateTime;
                                contactPersonService.UpdateContactGroupSub(contactGroupSub);
                            }
                        }
                    }
                    return contactData.ContactDataID;
                }

                ContactPerson contactPerson = contactPersonService.FindContactPerson(contactData.ObjectID);

                if (contactPerson == null)
                {
                    CommonVariables.LogTool.Log("ContactPerson " + contactData.ObjectID + " can not find");
                    return contactData.ContactDataID;
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

                        if (contactPersonList.UpdateTime.CompareTo(contactPerson.UpdateTime) > 0)
                        {
                            contactPerson.UpdateTime = contactPersonList.UpdateTime;
                            contactPersonService.UpdateContactPerson(contactPerson);
                        }
                    }
                    else
                    {
                        if (contactPersonList.UpdateTime.CompareTo(contactData.UpdateTime) < 0)
                        {
                            contactPersonList.IsDelete = contactData.IsDelete;
                            contactPersonList.UpdateTime = contactData.UpdateTime;
                            contactPersonService.UpdateContactPersonList(contactPersonList);

                            if (contactPersonList.UpdateTime.CompareTo(contactPerson.UpdateTime) > 0)
                            {
                                contactPerson.UpdateTime = contactPersonList.UpdateTime;
                                contactPersonService.UpdateContactPerson(contactPerson);
                            }
                        }
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
                        if (contactGroup.UpdateTime.CompareTo(contactPerson.UpdateTime) > 0)
                        {
                            contactPerson.UpdateTime = contactGroup.UpdateTime;
                            contactPersonService.UpdateContactPerson(contactPerson);
                        }
                    }
                    else
                    {
                        if (contactGroup.UpdateTime.CompareTo(contactData.UpdateTime)<0)
                        {
                            contactGroup.GroupName = contactData.GroupName;
                            contactGroup.IsDelete = contactData.IsDelete;
                            contactGroup.UpdateTime = contactData.UpdateTime;
                            contactPersonService.UpdateContactGroup(contactGroup);
                            if (contactGroup.UpdateTime.CompareTo(contactPerson.UpdateTime) > 0)
                            {
                                contactPerson.UpdateTime = contactGroup.UpdateTime;
                                contactPersonService.UpdateContactPerson(contactPerson);
                            }
                        }
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

                        if (contactGroupSub.UpdateTime.CompareTo(contactPerson.UpdateTime) > 0)
                        {
                            contactPerson.UpdateTime = contactGroupSub.UpdateTime;
                            contactPersonService.UpdateContactPerson(contactPerson);
                        }
                    }
                    else
                    {
                        if (contactGroupSub.UpdateTime.CompareTo(contactData.UpdateTime)<0)
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
