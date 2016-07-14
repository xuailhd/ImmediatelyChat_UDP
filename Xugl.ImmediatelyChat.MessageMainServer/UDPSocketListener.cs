using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.Common;
using Xugl.ImmediatelyChat.Core.DependencyResolution;
using Xugl.ImmediatelyChat.IServices;
using Xugl.ImmediatelyChat.Model;
using Xugl.ImmediatelyChat.SocketEngine;

namespace Xugl.ImmediatelyChat.MessageMainServer
{
    public class MMSListenerUDPToken : AsyncUserToken
    {
        private readonly IContactPersonService _contactPersonService;

        public MMSListenerUDPToken()
        {
            _contactPersonService = ObjectContainerFactory.CurrentContainer.Resolver<IContactPersonService>();
        }

        public ContactData Model { get; set; }

        public string UAObjectID { get; set; }

        public IContactPersonService ContactPersonService
        {
            get
            {
                return _contactPersonService;
            }
        }
    }


    internal class UDPSocketListener : AsyncSocketListenerUDP<MMSListenerUDPToken>
    {
        public UDPSocketListener()
            : base(1024,100, 20, CommonVariables.LogTool)
        {
        }

        protected override void HandleError(MMSListenerUDPToken token)
        {

        }

        protected override string HandleRecivedMessage(string inputMessage, MMSListenerUDPToken token)
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

                    if(data.StartsWith(CommonFlag.F_MMSVerifyMCSGetUAInfo))
                    {
                        return HandleMMSVerifyMCSGetUAInfo(data, token);
                    }

                    if (data.StartsWith(CommonFlag.F_MMSVerifyMCSFBGetUAInfo))
                    {
                        return HandleMMSVerifyMCSFBGetUAInfo(data, token);
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


        private string HandleMMSVerifyUAAddGroup(string data, MMSListenerUDPToken token)
        {
            ClientAddGroup model = JsonConvert.DeserializeObject<ClientAddGroup>(data.Remove(0, CommonFlag.F_MMSVerifyUAAddGroup.Length));


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


                                base.SendMsg(clientStatusModel.MCS_IP, clientStatusModel.MCS_Port,
                                    CommonFlag.F_MCSReceiveMMSUAUpdateTime + JsonConvert.SerializeObject(clientStatusModel), clientStatusModel.ObjectID);

                                IList<ContactData> contactDatas = PreparContactData(clientStatusModel.ObjectID, mcs_UpdateTime, token.ContactPersonService);

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
                return JsonConvert.SerializeObject(contactData);
            }

            return string.Empty;
        }


        private IList<ContactData> PreparContactData(string objectID, string updateTime, IContactPersonService contactPersonService)
        {
            IList<ContactData> tempContactDatas = null;
            ContactData tempContactData;

            ContactPerson contactPerson = contactPersonService.FindContactPerson(objectID);

            if (contactPerson != null)
            {
                tempContactDatas = new List<ContactData>();
                tempContactData = new ContactData();

                tempContactData.ContactName = contactPerson.ContactName;
                tempContactData.ImageSrc = contactPerson.ImageSrc;
                tempContactData.LatestTime = contactPerson.LatestTime;
                tempContactData.ObjectID = contactPerson.ObjectID;
                tempContactData.UpdateTime = contactPerson.UpdateTime;
                tempContactData.ContactDataID = Guid.NewGuid().ToString();
                tempContactData.DataType = 0;
                tempContactDatas.Add(tempContactData);

                IList<ContactPersonList> contactPersonLists = contactPersonService.GetLastestContactPersonList(objectID, updateTime);
                IList<ContactGroup> contactGroups = contactPersonService.GetLastestContactGroup(objectID, updateTime);
                IList<ContactGroupSub> contactGroupSubs = contactPersonService.GetLastestContactGroupSub(objectID, updateTime);
                if (contactPersonLists != null && contactPersonLists.Count > 0)
                {
                    foreach (ContactPersonList contactPersonList in contactPersonLists)
                    {
                        tempContactData = new ContactData();

                        tempContactData.DestinationObjectID = contactPersonList.DestinationObjectID;
                        tempContactData.ContactPersonName = contactPersonList.ContactPersonName;
                        tempContactData.ObjectID = contactPersonList.ObjectID;
                        tempContactData.IsDelete = contactPersonList.IsDelete;
                        tempContactData.UpdateTime = contactPersonList.UpdateTime;
                        tempContactData.ContactDataID = Guid.NewGuid().ToString();
                        tempContactData.DataType = 1;
                        tempContactDatas.Add(tempContactData);
                    }
                }
                if (contactGroups != null && contactGroups.Count > 0)
                {
                    foreach (ContactGroup contactGroup in contactGroups)
                    {
                        tempContactData = new ContactData();
                        tempContactData.GroupObjectID = contactGroup.GroupObjectID;
                        tempContactData.GroupName = contactGroup.GroupName;
                        tempContactData.IsDelete = contactGroup.IsDelete;
                        tempContactData.UpdateTime = contactGroup.UpdateTime;
                        tempContactData.ContactDataID = Guid.NewGuid().ToString();
                        tempContactData.DataType = 2;
                        tempContactData.ObjectID = objectID;
                        tempContactDatas.Add(tempContactData);
                    }
                }
                if (contactGroupSubs != null && contactGroupSubs.Count > 0)
                {
                    foreach (ContactGroupSub contactGroupSub in contactGroupSubs)
                    {
                        tempContactData = new ContactData();
                        tempContactData.ContactGroupID = contactGroupSub.ContactGroupID;
                        tempContactData.ContactPersonObjectID = contactGroupSub.ContactPersonObjectID;
                        tempContactData.IsDelete = contactGroupSub.IsDelete;
                        tempContactData.UpdateTime = contactGroupSub.UpdateTime;
                        tempContactData.ContactDataID = Guid.NewGuid().ToString();
                        tempContactData.DataType = 3;
                        tempContactData.ObjectID = objectID;
                        tempContactDatas.Add(tempContactData);
                    }
                }
                return tempContactDatas;
            }
            return null;
        }


        private string HandleMMSVerifyUAAddPerson(string data, MMSListenerUDPToken token)
        {
            ClientAddPerson model = JsonConvert.DeserializeObject<ClientAddPerson>(data.Remove(0, CommonFlag.F_MMSVerifyUAAddPerson.Length));
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

                                CommonVariables.UAInfoContorl.AddContactDataIntoBuffer(contactData, model.MCS_IP, model.MCS_Port, ServerType.MCS);
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

                return JsonConvert.SerializeObject(contactData);
            }
            return string.Empty;
        }

        private string HandlePSSendMMSUser(string data, MMSListenerUDPToken token)
        {
            ContactPerson contactPerson = JsonConvert.DeserializeObject<ContactPerson>(data.Remove(0, CommonFlag.F_PSSendMMSUser.Length));
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

        private string HandleMMSVerifyUAFBSearch(string data, MMSListenerUDPToken token)
        {
            string contactDataID = data.Remove(0, CommonFlag.F_MMSVerifyUAFBSearch.Length);
            CommonVariables.UAInfoContorl.HandlerSendContactDataReturnData(contactDataID);

            return string.Empty;
        }

        private string HandleMMSVerifyUASearch(string data, MMSListenerUDPToken token)
        {
            ClientSearchModel clientSearchModel = JsonConvert.DeserializeObject<ClientSearchModel>(data.Remove(0, CommonFlag.F_MMSVerifyUASearch.Length));
            if (clientSearchModel != null && !string.IsNullOrEmpty(clientSearchModel.ObjectID))
            {
                ClientModel clientModel = TransformModel(clientSearchModel);
                clientModel.Client_IP = token.IP;
                clientModel.Client_Port = token.Port;
                IList<ContactData> contactdatas = null;
                //CommonVariables.LogTool.Log("UA:" + clientSearchModel.ObjectID + "Type " + clientSearchModel.Type + " Search request  " + clientSearchModel.SearchKey);
                if (clientSearchModel.Type == 1)
                {
                    CommonVariables.UAInfoContorl.UpdateClientModel(clientModel);
                    contactdatas = ContactPersonToContacData(token.ContactPersonService.SearchPerson(clientSearchModel.ObjectID, clientSearchModel.SearchKey));
                    CommonVariables.UAInfoContorl.AddContactDataIntoBuffer(contactdatas,
                        clientModel.Client_IP, clientModel.Client_Port, ServerType.UASearchPerson);
                }
                else if (clientSearchModel.Type == 2)
                {
                    CommonVariables.UAInfoContorl.UpdateClientModel(clientModel);
                    contactdatas = ContactGroupToContacData(token.ContactPersonService.SearchGroup(clientSearchModel.ObjectID, clientSearchModel.SearchKey));
                    CommonVariables.UAInfoContorl.AddContactDataIntoBuffer(ContactGroupToContacData(token.ContactPersonService.SearchGroup(clientSearchModel.ObjectID, clientSearchModel.SearchKey)),
                        clientModel.Client_IP, clientModel.Client_Port, ServerType.UASearchGroup);
                }

                if(contactdatas!=null && contactdatas.Count>0)
                {
                    return contactdatas.Count.ToString();
                }
            }

            return string.Empty;
        }

        private string HandleMMSVerifyFBUAGetUAInfo(string data, MMSListenerUDPToken token)
        {
            string contactDataID = data.Remove(0, CommonFlag.F_MMSVerifyFBUAGetUAInfo.Length);

            CommonVariables.UAInfoContorl.HandlerSendContactDataReturnData(contactDataID);

            return string.Empty;
        }

        private string HandleMMSVerifyMCSGetUAInfo(string data, MMSListenerUDPToken token)
        {
            ClientModel clientStatusModel = JsonConvert.DeserializeObject<ClientModel>(data.Remove(0, CommonFlag.F_MMSVerifyMCSGetUAInfo.Length));

            if (clientStatusModel == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(clientStatusModel.ObjectID) || string.IsNullOrEmpty(clientStatusModel.UpdateTime))
            {
                return string.Empty;
            }

            IList<ContactData> contactDatas = PreparContactData(clientStatusModel.ObjectID, clientStatusModel.UpdateTime, token.ContactPersonService);

            if (contactDatas != null && contactDatas.Count > 0)
            {
                CommonVariables.UAInfoContorl.AddContactDataIntoBuffer(contactDatas, token.IP, token.Port, ServerType.MCS);

                return contactDatas.Count.ToString();
            }
            return string.Empty;
        }

        private string HandleMMSVerifyMCSFBGetUAInfo(string data, MMSListenerUDPToken token)
        {
            string contactDataID = data.Remove(0, CommonFlag.F_MMSVerifyFBUAGetUAInfo.Length);

            CommonVariables.UAInfoContorl.HandlerSendContactDataReturnData(contactDataID);

            return string.Empty;
        }

        private string HandleMMSVerifyUAGetUAInfo(string data, MMSListenerUDPToken token)
        {
            ClientModel clientStatusModel = JsonConvert.DeserializeObject<ClientModel>(data.Remove(0, CommonFlag.F_MMSVerifyUAGetUAInfo.Length));

            if (clientStatusModel == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(clientStatusModel.ObjectID) || string.IsNullOrEmpty(clientStatusModel.UpdateTime))
            {
                return string.Empty;
            }


            IList<ContactData> contactDatas = PreparContactData(clientStatusModel.ObjectID, clientStatusModel.UpdateTime, token.ContactPersonService);

            if (contactDatas != null && contactDatas.Count > 0)
            {
                clientStatusModel.Client_IP = token.IP;
                clientStatusModel.Client_Port = token.Port;
                CommonVariables.UAInfoContorl.UpdateClientModel(clientStatusModel);
                CommonVariables.UAInfoContorl.AddContactDataIntoBuffer(contactDatas, clientStatusModel.Client_IP, clientStatusModel.Client_Port, ServerType.MCS);

                return contactDatas.Count.ToString();
            }
            return string.Empty;
        }

        private string HandleMMSVerifyUA(string data, MMSListenerUDPToken token)
        {
            ContactPerson tempContactPerson = null;
            ClientModel clientStatusModel = JsonConvert.DeserializeObject<ClientModel>(data.Remove(0, CommonFlag.F_MMSVerifyUA.Length));

            CommonVariables.LogTool.Log("UA:" + clientStatusModel.ObjectID + " connected  " + clientStatusModel.LatestTime);
            //Find MCS
            MCSServer server = CommonVariables.CommonFunctions.FindMCSServer(CommonVariables.MCSServers,clientStatusModel.ObjectID);
            if(server == null)
            {
                return "MMS:can not find MCS server";
            }

            clientStatusModel.MCS_IP = server.MCS_IP;
            clientStatusModel.MCS_Port = server.MCS_Port;
            tempContactPerson = token.ContactPersonService.FindContactPerson(clientStatusModel.ObjectID);
            if (tempContactPerson == null)
            {
                return "MMS:did not exist this user";
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

            base.SendMsg(server.MCS_IP,server.MCS_Port,
                CommonFlag.F_MCSReceiveMMSUAUpdateTime + JsonConvert.SerializeObject(clientStatusModel),clientStatusModel.ObjectID);

            //Send MCS
            return JsonConvert.SerializeObject(clientStatusModel);
        }

        private string HandlePSCallMMSStart(string data)
        {
            IList<MCSServer> mcsServers = JsonConvert.DeserializeObject<IList<MCSServer>>(data.Remove(0, CommonFlag.F_PSCallMMSStart.Length));
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

        private ClientModel TransformModel(ClientSearchModel model)
        {
            ClientModel clientModel;
            if(model!=null && String.IsNullOrEmpty(model.ObjectID))
            {

            }

        }


        public void BeginService()
        {
            //CommonVariables.UAInfoContorl.StartMainThread();
            base.BeginService(CommonVariables.MMSIP, CommonVariables.MMSPort);
        }
    }
}
