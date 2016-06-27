using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.Common;
using Xugl.ImmediatelyChat.Model;
using Xugl.ImmediatelyChat.SocketEngine;

namespace Xugl.ImmediatelyChat.MessageMainServer
{
    internal class UDPSocketListener : AsyncSocketListenerUDP<MMSListenerToken>
    {
        public UDPSocketListener()
            : base(1024, 10, CommonVariables.LogTool)
        {
        }

        protected override void HandleError(MMSListenerToken token)
        {
            if (token.Models != null && token.Models.Count > 0)
            {
                token.Models.Clear();
                token.Models = null;
            }
        }

        protected override string HandleRecivedMessage(string inputMessage, MMSListenerToken token)
        {
            if (string.IsNullOrEmpty(inputMessage) || token == null)
            {
                return string.Empty;
            }

            try
            {
                string data = inputMessage;

                if (data.StartsWith(CommonFlag.F_PSSendMMSUser))
                {
                    return HandlePSSendMMSUser(data, token);
                }

                if (data.StartsWith(CommonFlag.F_PSCallMMSStart))
                {
                    return HandlePSCallMMSStart(data);
                }

                if (CommonVariables.IsBeginMessageService)
                {
                    CommonVariables.LogTool.Log("receive UA data:" + data);
                    //UA
                    if (data.StartsWith(CommonFlag.F_MMSVerifyUA))
                    {
                        return HandleMMSVerifyUA(data, token);
                    }

                    if (data.StartsWith(CommonFlag.F_MMSVerifyUAGetUAInfo))
                    {
                        return HandleMMSVerifyUAGetUAInfo(data, token);
                    }

                    if (data.StartsWith(CommonFlag.F_MMSVerifyFBUAGetUAInfo))
                    {
                        return HandleMMSVerifyFBUAGetUAInfo(data, token);
                    }

                    if (data.StartsWith(CommonFlag.F_MMSVerifyUASearch))
                    {
                        return HandleMMSVerifyUASearch(data, token);
                    }

                    if (data.StartsWith(CommonFlag.F_MMSVerifyUAFBSearch))
                    {
                        return HandleMMSVerifyUAFBSearch(data, token);
                    }

                    if (data.StartsWith(CommonFlag.F_MMSVerifyUAAddPerson))
                    {
                        return HandleMMSVerifyUAAddPerson(data, token);
                    }

                    if (data.StartsWith(CommonFlag.F_MMSVerifyUAAddGroup))
                    {
                        return HandleMMSVerifyUAAddGroup(data, token);
                    }
                }
            }
            catch(Exception ex)
            {
                CommonVariables.LogTool.Log(ex.Message + ex.StackTrace);
            }
            return string.Empty;
        }


        private string HandleMMSVerifyUAAddGroup(string data, MMSListenerToken token)
        {
            ClientAddGroup model = CommonVariables.serializer.Deserialize<ClientAddGroup>(data.Remove(0, CommonFlag.F_MMSVerifyUAAddGroup.Length));


            if (model != null && !string.IsNullOrEmpty(model.ObjectID))
            {
                ContactData contactData = new ContactData();
                ContactGroupSub contactGroupSub = token.ContactPersonService.FindContactGroupSub(model.ObjectID, model.GroupObjectID);
                if (contactGroupSub == null)
                {
                    ContactPerson contactPerson = token.ContactPersonService.FindContactPerson(model.ObjectID);
                    if (contactPerson != null)
                    {
                        ContactGroup contactGroup = token.ContactPersonService.FindContactGroup(model.GroupObjectID);
                        if (contactGroup != null)
                        {
                            contactGroupSub = new ContactGroupSub();
                            contactGroupSub.ContactGroupID = contactGroup.GroupObjectID;
                            contactGroupSub.ContactPersonObjectID = model.ObjectID;
                            contactGroupSub.UpdateTime = DateTime.Now.ToString(CommonFlag.F_DateTimeFormat);

                            if (token.ContactPersonService.InsertContactGroupSub(contactGroupSub) == 1)
                            {
                                token.ContactPersonService.UpdateContactUpdateTimeByGroup(contactGroup.GroupObjectID, contactGroupSub.UpdateTime);

                                ClientModel clientStatusModel = new ClientModel();

                                clientStatusModel.MCS_IP = model.MCS_IP;
                                clientStatusModel.MCS_Port = model.MCS_Port;
                                clientStatusModel.ObjectID = model.ObjectID;

                                string mcs_UpdateTime = CommonVariables.SyncSocketClientIntance.SendMsg(clientStatusModel.MCS_IP, clientStatusModel.MCS_Port,
                                    CommonFlag.F_MCSReceiveMMSUAUpdateTime + CommonVariables.serializer.Serialize(clientStatusModel));

                                if (string.IsNullOrEmpty(mcs_UpdateTime))
                                {
                                    return string.Empty;
                                }

                                IList<ContactData> contactDatas = CommonVariables.UAInfoContorl.PreparContactData(clientStatusModel.ObjectID, mcs_UpdateTime);

                                foreach (ContactData _contactData in contactDatas)
                                {
                                    CommonVariables.SyncSocketClientIntance.SendMsg(clientStatusModel.MCS_IP, clientStatusModel.MCS_Port,
                                        CommonFlag.F_MCSReceiveUAInfo + CommonVariables.serializer.Serialize(_contactData));
                                }

                                contactGroup = token.ContactPersonService.FindContactGroup(model.GroupObjectID);
                                contactData.GroupName = contactGroup.GroupName;
                                contactData.GroupObjectID = contactGroup.GroupObjectID;
                                contactData.IsDelete = contactGroup.IsDelete;
                                contactData.UpdateTime = contactGroup.UpdateTime;
                                contactData.DataType = 2;
                            }
                            else
                            {
                                return string.Empty;
                            }
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    ContactGroup contactGroup = token.ContactPersonService.FindContactGroup(contactGroupSub.ContactGroupID);
                    contactData.GroupName = contactGroup.GroupName;
                    contactData.GroupObjectID = contactGroup.GroupObjectID;
                    contactData.IsDelete = contactGroup.IsDelete;
                    contactData.DataType = 2;
                }
                return CommonVariables.serializer.Serialize(contactData);
            }

            return string.Empty;
        }

        private string HandleMMSVerifyUAAddPerson(string data, MMSListenerToken token)
        {
            ClientAddPerson model = CommonVariables.serializer.Deserialize<ClientAddPerson>(data.Remove(0, CommonFlag.F_MMSVerifyUAAddPerson.Length));
            if (model != null && !string.IsNullOrEmpty(model.ObjectID))
            {
                ContactData contactData = new ContactData();
                ContactPersonList contactPersonList = token.ContactPersonService.FindContactPersonList(model.ObjectID, model.DestinationObjectID);
                if (contactPersonList == null)
                {
                    ContactPerson contactPerson = token.ContactPersonService.FindContactPerson(model.ObjectID);
                    if (contactPerson != null)
                    {
                        ContactPerson contactPerson2 = token.ContactPersonService.FindContactPerson(model.DestinationObjectID);
                        if (contactPerson2 != null)
                        {
                            contactPersonList = new ContactPersonList();
                            contactPersonList.ContactPersonName = contactPerson2.ContactName;
                            contactPersonList.DestinationObjectID = contactPerson2.ObjectID;
                            contactPersonList.ObjectID = contactPerson.ObjectID;
                            contactPersonList.UpdateTime = DateTime.Now.ToString(CommonFlag.F_DateTimeFormat);

                            if (token.ContactPersonService.InsertContactPersonList(contactPersonList) == 1)
                            {
                                contactPerson.UpdateTime = contactPersonList.UpdateTime;
                                token.ContactPersonService.UpdateContactPerson(contactPerson);
                                //contactPerson2.UpdateTime = contactPersonList.UpdateTime;
                                //token.ContactPersonService.UpdateContactPerson(contactPerson2);

                                contactData.ContactPersonName = contactPersonList.ContactPersonName;
                                contactData.DestinationObjectID = contactPersonList.DestinationObjectID;
                                contactData.IsDelete = contactPersonList.IsDelete;
                                contactData.UpdateTime = contactPersonList.UpdateTime;
                                contactData.ObjectID = contactPersonList.ObjectID;
                                contactData.DataType = 1;

                                CommonVariables.SyncSocketClientIntance.SendMsg(model.MCS_IP, model.MCS_Port,
                                CommonFlag.F_MCSReceiveUAInfo + CommonVariables.serializer.Serialize(contactData));
                            }

                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    contactData.ContactPersonName = contactPersonList.ContactPersonName;
                    contactData.DestinationObjectID = contactPersonList.DestinationObjectID;
                    contactData.IsDelete = contactPersonList.IsDelete;
                    contactData.UpdateTime = contactPersonList.UpdateTime;
                    contactData.ObjectID = contactPersonList.ObjectID;
                    contactData.DataType = 1;
                }

                return CommonVariables.serializer.Serialize(contactData);
            }
            return string.Empty;
        }

        private string HandlePSSendMMSUser(string data, MMSListenerToken token)
        {
            ContactPerson contactPerson = CommonVariables.serializer.Deserialize<ContactPerson>(data.Remove(0, CommonFlag.F_PSSendMMSUser.Length));
            if (contactPerson != null && !string.IsNullOrEmpty(contactPerson.ObjectID))
            {
                if (token.ContactPersonService.InsertNewPerson(contactPerson) > 0)
                {
                    //if (token.ContactPersonService.InsertDefaultGroup(contactPerson.ObjectID) > 0)
                    //{
                    return contactPerson.ObjectID;
                    //}
                }
            }
            return "failed";
        }

        private string HandleMMSVerifyUAFBSearch(string data, MMSListenerToken token)
        {
            string contactDataID = data.Remove(0, CommonFlag.F_MMSVerifyUAFBSearch.Length);
            CommonVariables.LogTool.Log("UA F_MMSVerifyUAFBSearch:" + contactDataID);
            if (token.Models[0].ContactDataID != contactDataID)
            {
                CommonVariables.LogTool.Log("Search data transfer error " + token.Models[0].ContactDataID + "  vs " + contactDataID);
            }
            token.Models.RemoveAt(0);
            if (token.Models.Count <= 0)
            {
                return string.Empty;
            }
            return CommonVariables.serializer.Serialize(token.Models[0]);
        }

        private string HandleMMSVerifyUASearch(string data, MMSListenerToken token)
        {
            ClientSearchModel clientSearchModel = CommonVariables.serializer.Deserialize<ClientSearchModel>(data.Remove(0, CommonFlag.F_MMSVerifyUASearch.Length));
            if (clientSearchModel != null && !string.IsNullOrEmpty(clientSearchModel.ObjectID))
            {
                CommonVariables.LogTool.Log("UA:" + clientSearchModel.ObjectID + "Type " + clientSearchModel.Type + " Search request  " + clientSearchModel.SearchKey);
                if (clientSearchModel.Type == 1)
                {
                    token.Models = ContactPersonToContacData(token.ContactPersonService.SearchPerson(clientSearchModel.ObjectID, clientSearchModel.SearchKey));
                }
                else if (clientSearchModel.Type == 2)
                {
                    token.Models = ContactGroupToContacData(token.ContactPersonService.SearchGroup(clientSearchModel.ObjectID, clientSearchModel.SearchKey));
                }

                if (token.Models != null && token.Models.Count > 0)
                {
                    return CommonVariables.serializer.Serialize(token.Models[0]);
                }
                else
                {
                    CommonVariables.LogTool.Log("Search request Result null ");
                }
            }

            return string.Empty;
        }

        private string HandleMMSVerifyFBUAGetUAInfo(string data, MMSListenerToken token)
        {
            string contactDataID = data.Remove(0, CommonFlag.F_MMSVerifyFBUAGetUAInfo.Length);
            if (token.Models[0].ContactDataID != contactDataID)
            {
                CommonVariables.LogTool.Log("data transfer error " + token.Models[0].ContactDataID + "  vs " + contactDataID);
            }
            token.Models.RemoveAt(0);
            if (token.Models.Count <= 0)
            {
                return string.Empty;
            }
            return CommonVariables.serializer.Serialize(token.Models[0]);
        }

        private string HandleMMSVerifyUAGetUAInfo(string data, MMSListenerToken token)
        {
            ClientModel clientStatusModel = CommonVariables.serializer.Deserialize<ClientModel>(data.Remove(0, CommonFlag.F_MMSVerifyUAGetUAInfo.Length));

            if (clientStatusModel == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(clientStatusModel.ObjectID) || string.IsNullOrEmpty(clientStatusModel.UpdateTime))
            {
                return string.Empty;
            }

            token.Models = CommonVariables.UAInfoContorl.PreparContactData(clientStatusModel.ObjectID, clientStatusModel.UpdateTime);
            if (token.Models != null && token.Models.Count > 0)
            {
                return CommonVariables.serializer.Serialize(token.Models[0]);
            }
            return string.Empty;
        }

        private string HandleMMSVerifyUA(string data, MMSListenerToken token)
        {
            ContactPerson tempContactPerson = null;
            ClientModel clientStatusModel = CommonVariables.serializer.Deserialize<ClientModel>(data.Remove(0, CommonFlag.F_MMSVerifyUA.Length));

            CommonVariables.LogTool.Log("UA:" + clientStatusModel.ObjectID + " connected  " + clientStatusModel.LatestTime);
            //Find MCS
            for (int i = 0; i < CommonVariables.MCSServers.Count; i++)
            {
                if (CommonVariables.MCSServers[i].ArrangeStr.Contains(clientStatusModel.ObjectID.Substring(0, 1)))
                {
                    clientStatusModel.MCS_IP = CommonVariables.MCSServers[i].MCS_IP;
                    clientStatusModel.MCS_Port = CommonVariables.MCSServers[i].MCS_Port;
;

                    tempContactPerson = token.ContactPersonService.FindContactPersonNoTracking(clientStatusModel.ObjectID);
                    if (tempContactPerson == null)
                    {
                        return string.Empty;
                    }

                    if (clientStatusModel.LatestTime.CompareTo(tempContactPerson.LatestTime) <= 0)
                    {
                        clientStatusModel.LatestTime = tempContactPerson.LatestTime;
                    }
                    else
                    {
                        tempContactPerson.LatestTime = clientStatusModel.LatestTime;
                        token.ContactPersonService.UpdateContactPerson(tempContactPerson);
                    }

                    clientStatusModel.UpdateTime = tempContactPerson.UpdateTime;

                    string mcs_UpdateTime = CommonVariables.SyncSocketClientIntance.SendMsg(CommonVariables.MCSServers[i].MCS_IP, CommonVariables.MCSServers[i].MCS_Port,
                        CommonFlag.F_MCSReceiveMMSUAUpdateTime + CommonVariables.serializer.Serialize(clientStatusModel));

                    //CommonVariables.LogTool.Log("mcs_UpdateTime:" + mcs_UpdateTime);

                    if (string.IsNullOrEmpty(mcs_UpdateTime))
                    {
                        return string.Empty;
                    }

                    IList<ContactData> contactDatas = CommonVariables.UAInfoContorl.PreparContactData(clientStatusModel.ObjectID, mcs_UpdateTime);

                    foreach (ContactData contactData in contactDatas)
                    {
                        CommonVariables.SyncSocketClientIntance.SendMsg(CommonVariables.MCSServers[i].MCS_IP, CommonVariables.MCSServers[i].MCS_Port,
                            CommonFlag.F_MCSReceiveUAInfo + CommonVariables.serializer.Serialize(contactData));
                    }

                    //CommonVariables.UAInfoContorl.AddUAModelIntoBuffer(clientStatusModel);
                    break;
                }
            }

            //Send MCS
            return CommonVariables.serializer.Serialize(clientStatusModel);
        }

        private string HandlePSCallMMSStart(string data)
        {
            IList<MCSServer> mcsServers = CommonVariables.serializer.Deserialize<IList<MCSServer>>(data.Remove(0, CommonFlag.F_PSCallMMSStart.Length));
            CommonVariables.LogTool.Log("MCS count:" + mcsServers.Count);
            foreach (MCSServer mcsServer in mcsServers)
            {
                CommonVariables.MCSServers.Add(mcsServer);
                CommonVariables.LogTool.Log("IP:" + mcsServer.MCS_IP + " Port:" + mcsServer.MCS_Port + "  ArrangeStr:" + mcsServer.ArrangeStr);
            }

            CommonVariables.LogTool.Log("Start Message Main Server:" + CommonVariables.MMSIP + ", Port:" + CommonVariables.MMSPort.ToString());
            CommonVariables.IsBeginMessageService = true;
            return string.Empty;
        }

        private IList<ContactData> ContactPersonToContacData(IList<ContactPerson> entitys)
        {
            if (entitys != null && entitys.Count > 0)
            {
                ContactData contactData;
                IList<ContactData> contactDatas = new List<ContactData>();

                foreach (ContactPerson entity in entitys)
                {
                    contactData = new ContactData();
                    contactData.ContactDataID = Guid.NewGuid().ToString();
                    contactData.ContactName = entity.ContactName;
                    contactData.ObjectID = entity.ObjectID;
                    contactData.ImageSrc = entity.ImageSrc;
                    contactDatas.Add(contactData);
                }

                return contactDatas;
            }

            return null;
        }

        private IList<ContactData> ContactGroupToContacData(IList<ContactGroup> entitys)
        {
            if (entitys != null && entitys.Count > 0)
            {
                ContactData contactData;
                IList<ContactData> contactDatas = new List<ContactData>();

                foreach (ContactGroup entity in entitys)
                {
                    contactData = new ContactData();
                    contactData.ContactDataID = Guid.NewGuid().ToString();
                    contactData.GroupObjectID = entity.GroupObjectID;
                    contactData.GroupName = entity.GroupName;
                    contactDatas.Add(contactData);
                }

                return contactDatas;
            }

            return null;
        }


        public void BeginService()
        {
            //CommonVariables.UAInfoContorl.StartMainThread();
            base.BeginService(CommonVariables.MMSIP, CommonVariables.MMSPort);
        }
    }
}
