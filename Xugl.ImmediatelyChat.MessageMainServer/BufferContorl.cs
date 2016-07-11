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
        private IList<ContactDataWithServer> contactDataToUABuffer1 = new List<ContactDataWithServer>();
        private IList<ContactDataWithServer> contactDataToUABuffer2 = new List<ContactDataWithServer>();
        private bool UsingTagForUA = false;


        private IList<ContactDataWithServer> contactDataToMCSBuffer1 = new List<ContactDataWithServer>();
        private IList<ContactDataWithServer> contactDataToMCSBuffer2 = new List<ContactDataWithServer>();
        private bool UsingTagForMCS = false;

        private IList<ContactDataWithServer> exeContactDataToUABuffer = new List<ContactDataWithServer>();
        private IList<ContactDataWithServer> exeContactDataToMCSBuffer = new List<ContactDataWithServer>();
        private int sendContactDataDelay = 100;
        
        #region buffer manager
        private IList<ContactDataWithServer> GetUsingContactDataToUABuffer
        {
            get
            {
                return UsingTagForUA ? contactDataToUABuffer1 : contactDataToUABuffer2;
            }
        }

        private IList<ContactDataWithServer> GetUnUsingContactDataToUABuffer
        {
            get
            {
                return UsingTagForUA ? contactDataToUABuffer2 : contactDataToUABuffer1;
            }
        }

        private IList<ContactDataWithServer> GetUsingContactDataToMCSBuffer
        {
            get
            {
                return UsingTagForMCS ? contactDataToMCSBuffer1 : contactDataToMCSBuffer2;
            }
        }

        private IList<ContactDataWithServer> GetUnUsingContactDataToMCSBuffer
        {
            get
            {
                return UsingTagForMCS ? contactDataToMCSBuffer2 : contactDataToMCSBuffer1;
            }
        }
        #endregion

        public bool IsRunning = false;

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
            if (contactDataWithServer == null)
            {
                return null;
            }
            exeContactDataBuffer.Remove(contactDataWithServer);
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

