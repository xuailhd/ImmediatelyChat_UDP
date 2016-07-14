using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
//using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Xugl.ImmediatelyChat.Common;
using Xugl.ImmediatelyChat.Core;
using Xugl.ImmediatelyChat.Core.DependencyResolution;
using Xugl.ImmediatelyChat.IServices;
using Xugl.ImmediatelyChat.Model;
using Xugl.ImmediatelyChat.SocketEngine;

namespace Xugl.ImmediatelyChat.MessageMainServer
{
    public class BufferContorl
    {

        //private AsyncSocketClientUDP asyncSocketClient;
        private readonly IContactPersonService contactPersonService;

        private int _maxSize = 1024;
        private IList<ContactDataWithServer> contactDataBuffer1 = new List<ContactDataWithServer>();
        private IList<ContactDataWithServer> contactDataBuffer2 = new List<ContactDataWithServer>();
        private bool UsingTag = false;

        private IList<ContactDataWithServer> exeContactDataBuffer = new List<ContactDataWithServer>();
        private IDictionary<string, ClientModel> clientModels = new Dictionary<string, ClientModel>();

        private int sendContactDataDelay = 100;
        public bool IsRunning = false;

        #region buffer manager
        private IList<ContactDataWithServer> GetUsingContactDataBuffer
        {
            get
            {
                return UsingTag ? contactDataBuffer1 : contactDataBuffer2;
            }
        }

        private IList<ContactDataWithServer> GetUnUsingContactDataBuffer
        {
            get
            {
                return UsingTag ? contactDataBuffer2 : contactDataBuffer1;
            }
        }

        private IList<ContactDataWithServer> GetExeContactDataBuffer
        {
            get
            {
                return exeContactDataBuffer;
            }
        }
        #endregion

        public void UpdateClientModel(ClientModel clientModel)
        {
            if(clientModels.ContainsKey(clientModel.ObjectID))
            {
                clientModels[clientModel.ObjectID].Client_IP = clientModel.Client_IP;
                clientModels[clientModel.ObjectID].Client_Port = clientModel.Client_Port;
                if(string.IsNullOrEmpty(clientModels[clientModel.ObjectID].MCS_IP))
                {
                    MCSServer server = CommonVariables.CommonFunctions.FindMCSServer(CommonVariables.MCSServers, clientModel.ObjectID);
                    clientModels[clientModel.ObjectID].MCS_IP = server.MCS_IP;
                    clientModels[clientModel.ObjectID].MCS_Port = server.MCS_Port;
                }
            }
            else
            {
                if(string.IsNullOrEmpty(clientModel.MCS_IP))
                {
                    MCSServer server = CommonVariables.CommonFunctions.FindMCSServer(CommonVariables.MCSServers, clientModel.ObjectID);
                    clientModel.MCS_IP = server.MCS_IP;
                    clientModel.MCS_Port = server.MCS_Port;
                }

                clientModels.Add(clientModel.ObjectID, clientModel);
            }
        }


        public void StopMainThread()
        {
            IsRunning = false;
        }

        public void AddContactDataIntoBuffer(IList<ContactData> contactDatas,string  serverIP,int port,ServerType serverType)
        {
            ContactDataWithServer contactDataWithServer;

            if(string.IsNullOrEmpty(serverIP))
            {
                return;
            }

            if (contactDatas == null || contactDatas.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < contactDatas.Count; i++)
            {
                contactDataWithServer = new ContactDataWithServer();
                contactDataWithServer.ContactData = contactDatas[i];
                contactDataWithServer.ServerIP = serverIP;
                contactDataWithServer.ServerPort = port;
                contactDataWithServer.ServerType = serverType;
                GetUsingContactDataBuffer.Add(contactDataWithServer);
            }
        }

        public void AddContactDataIntoBuffer(ContactData contactData, string serverIP, int port, ServerType serverType)
        {
            ContactDataWithServer contactDataWithServer;

            if (string.IsNullOrEmpty(serverIP))
            {
                return;
            }

            if (contactData == null || String.IsNullOrEmpty(contactData.ContactDataID))
            {
                return;
            }

            contactDataWithServer = new ContactDataWithServer();
            contactDataWithServer.ContactData = contactData;
            contactDataWithServer.ServerIP = serverIP;
            contactDataWithServer.ServerPort = port;
            contactDataWithServer.ServerType = serverType;
            GetUsingContactDataBuffer.Add(contactDataWithServer);
        }

        public void SendContactDataThread()
        {
            ContactDataWithServer contactDataWithServer;
            while (IsRunning)
            {
                if(GetUsingContactDataBuffer.Count>0)
                {
                    UsingTag = !UsingTag;
                    while(GetUnUsingContactDataBuffer.Count>0) 
                    {
                        contactDataWithServer = GetUnUsingContactDataBuffer[0];
                        switch (contactDataWithServer.ServerType)
                        {
                            case ServerType.UA:
                                CommonVariables.Listener.SendMsg(contactDataWithServer.ServerIP, contactDataWithServer.ServerPort,
                                    CommonFlag.F_UAVerifyUAInfo + JsonConvert.SerializeObject(contactDataWithServer.ContactData),
                                    contactDataWithServer.ContactData.ContactDataID);
                                break;
                            case ServerType.MCS:
                                CommonVariables.Listener.SendMsg(contactDataWithServer.ServerIP, contactDataWithServer.ServerPort,
                                    CommonFlag.F_MCSVerifyUAInfo + JsonConvert.SerializeObject(contactDataWithServer.ContactData),
                                    contactDataWithServer.ContactData.ContactDataID);
                                break;
                            case ServerType.UASearchPerson:
                                CommonVariables.Listener.SendMsg(contactDataWithServer.ServerIP, contactDataWithServer.ServerPort,
                                    CommonFlag.F_UAVerifyPersonSearch + JsonConvert.SerializeObject(contactDataWithServer.ContactData),
                                    contactDataWithServer.ContactData.ContactDataID);
                                break;
                            case ServerType.UASearchGroup:
                                CommonVariables.Listener.SendMsg(contactDataWithServer.ServerIP, contactDataWithServer.ServerPort,
                                    CommonFlag.F_UAVerifyGroupSearch + JsonConvert.SerializeObject(contactDataWithServer.ContactData),
                                    contactDataWithServer.ContactData.ContactDataID);
                                break;
                            default:
                                continue;
                        }
                        exeContactDataBuffer.Add(contactDataWithServer);
                        GetUnUsingContactDataBuffer.RemoveAt(0);
                    }
                }
                Thread.Sleep(sendContactDataDelay);
            }
        }

        public void HandlerSendContactDataReturnData(string returnData)
        {
            ContactDataWithServer contactDataWithServer = exeContactDataBuffer.Where(t => t.ContactData.ContactDataID == returnData).SingleOrDefault();
            if (contactDataWithServer == null)
            {
                return;
            }
            exeContactDataBuffer.Remove(contactDataWithServer);
            if (contactDataWithServer != null)
            {
                GetUsingContactDataBuffer.Add(contactDataWithServer);
            }
        }
    }

}

