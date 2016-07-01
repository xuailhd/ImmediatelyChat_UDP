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
        private int _maxConnnections = 10;
        private IList<ContactDataWithServer> contactDataBuffer1 = new List<ContactDataWithServer>();
        private IList<ContactDataWithServer> contactDataBuffer2 = new List<ContactDataWithServer>();
        private bool UsingTagForcontactData = false;

        private IList<ContactDataWithServer> exeContactDataBuffer = new List<ContactDataWithServer>();

        private AsyncSocketClientUDP sendContactDataClient;
        private int sendContactDataDelay = 100;

        private IList<ContactDataWithServer> GetUsingContactDataBuffer
        {
            get
            {
                return UsingTagForcontactData ? contactDataBuffer1 : contactDataBuffer2;
            }
        }

        private IList<ContactDataWithServer> GetUnUsingContactDataBuffer
        {
            get
            {
                return UsingTagForcontactData ? contactDataBuffer2 : contactDataBuffer1;
            }
        }

        public bool IsRunning = false;
        public BufferContorl()
        {
            contactPersonService = ObjectContainerFactory.CurrentContainer.Resolver<IContactPersonService>();
        }

        public void StopMainThread()
        {
            IsRunning = false;
        }

        public void AddContactDataIntoBuffer(IList<ContactData> contactDatas,string  serverIP,int port,int serverType)
        {
            ContactDataWithServer contactDataWithServer = new ContactDataWithServer();

            if(string.IsNullOrEmpty(serverIP))
            {
                return;
            }

            if (contactDatas == null || contactDatas.Count <= 0)
            {
                return;
            }

            for(int i =0; i <contactDatas.Count;i++)
            {

            }
        }

        public void SendContactDataThread()
        {
            sendContactDataClient = new AsyncSocketClientUDP(_maxSize, _maxConnnections, CommonVariables.LogTool);
            ContactDataWithServer contactDataWithServer;
            while (IsRunning)
            {
                if(GetUsingContactDataBuffer.Count>0)
                {
                    UsingTagForcontactData = !UsingTagForcontactData;
                    while(GetUnUsingContactDataBuffer.Count>0) 
                    {
                        contactDataWithServer = GetUnUsingContactDataBuffer[0];
                        switch (contactDataWithServer.ServerType)
                        {
                            case 1:
                                sendContactDataClient.SendMsg(contactDataWithServer.ServerIP, contactDataWithServer.ServerPort,
                                    CommonFlag.F_UAVerifyUAInfo + CommonVariables.serializer.Serialize(contactDataWithServer.ContactData),
                                    contactDataWithServer.ContactData.ContactDataID, HandlerSendContactDataReturnData);
                                break;
                            case 2:
                                sendContactDataClient.SendMsg(contactDataWithServer.ServerIP, contactDataWithServer.ServerPort,
                                    CommonFlag.F_MCSVerifyUAInfo + CommonVariables.serializer.Serialize(contactDataWithServer.ContactData),
                                    contactDataWithServer.ContactData.ContactDataID, HandlerSendContactDataReturnData);
                                break;
                            default:
                                continue;
                        }
                        GetUnUsingContactDataBuffer.RemoveAt(0);
                    }
                }
                Thread.Sleep(sendContactDataDelay);
            }
        }


        private string HandlerSendContactDataReturnData(string returnData, bool isError)
        {
            ContactDataWithServer contactDataWithServer = exeContactDataBuffer.Where(t => t.ContactData.ContactDataID == returnData).SingleOrDefault();
            if (contactDataWithServer != null)
            {
                exeContactDataBuffer.Remove(contactDataWithServer);
            }
            if (isError)
            {
                if (contactDataWithServer != null)
                {
                    GetUsingContactDataBuffer.Add(contactDataWithServer);
                }
            }
            return null;
        }
    }

}

