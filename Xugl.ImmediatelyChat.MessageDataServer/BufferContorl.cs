using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Xugl.ImmediatelyChat.Common;
using Xugl.ImmediatelyChat.Core;
using Xugl.ImmediatelyChat.Core.DependencyResolution;
using Xugl.ImmediatelyChat.IServices;
using Xugl.ImmediatelyChat.Model;
using Xugl.ImmediatelyChat.Model.QueryCondition;
using Xugl.ImmediatelyChat.SocketEngine;

namespace Xugl.ImmediatelyChat.MessageDataServer
{
    public class BufferContorl
    {
        private readonly int maxBufferRecordCount;

        private IList<MsgRecord> bufferMsgRecords1 = new List<MsgRecord>();
        private IList<MsgRecord> bufferMsgRecords2 = new List<MsgRecord>();
        private bool UsingTagForMsgRecord = false;

        private IList<GetMsgModel> bufferGetMsgs1 = new List<GetMsgModel>();
        private IList<GetMsgModel> bufferGetMsgs2 = new List<GetMsgModel>();
        private bool UsingTagForGetMsg = false;

        private Thread mainThread = null;

        /// <summary>
        /// which time before or equal this time, will be save into database
        /// </summary>
        private string savedIntoDataBase;

        private readonly IMsgRecordService msgRecordService;
        public bool IsRunning = false;
        //private AsyncSocketClientUDP sendMsgClient;
        private const int _maxSize = 1024;
        private const int _maxSendConnections = 10;

        private AsyncSocketClient asyncSocketClient;

        public BufferContorl()
        {
            msgRecordService = ObjectContainerFactory.CurrentContainer.Resolver<IMsgRecordService>();
            maxBufferRecordCount = 1000;
            asyncSocketClient = new AsyncSocketClient(_maxSize, _maxSendConnections, CommonVariables.LogTool);
        }

        public void SendMsgToMCS(MCSServer mcsServer,MsgRecord msgRecord)
        {
            String strmsg= CommonFlag.F_MCSVerfiyMDSMSG + CommonVariables.serializer.Serialize(msgRecord);
            asyncSocketClient.SendMsg(mcsServer.MCS_IP, mcsServer.MCS_Port, strmsg, msgRecord.MsgID, HandMCSReturnData);
        }

        private string HandMCSReturnData(string returnData, bool isError)
        {
            return string.Empty;
        }

        public MsgRecord AddMSgRecordIntoBuffer(MsgRecordModel msgRecordModel)
        {
            MsgRecord msgRecord = new MsgRecord();
            msgRecord.MsgRecipientGroupID = msgRecordModel.MsgRecipientGroupID;
            msgRecord.MsgContent = msgRecordModel.MsgContent;
            msgRecord.MsgID = msgRecordModel.MsgID;
            msgRecord.MsgRecipientObjectID = msgRecordModel.MsgRecipientObjectID;
            msgRecord.MsgSenderName = msgRecordModel.MsgSenderName;
            msgRecord.MsgSenderObjectID = msgRecordModel.MsgSenderObjectID;
            msgRecord.MsgType = msgRecordModel.MsgType;
            msgRecord.SendTime = msgRecordModel.SendTime;
            GetUsingMsgRecordBuffer.Add(msgRecord);

            return msgRecord;
        }


        private IList<MsgRecord> GetUsingMsgRecordBuffer
        {
            get{
                return UsingTagForMsgRecord ? bufferMsgRecords1 : bufferMsgRecords2;
            }
        }

        private IList<MsgRecord> GetUnUsingMsgRecordBuffer
        {
            get
            {
                return UsingTagForMsgRecord ? bufferMsgRecords2 : bufferMsgRecords1;
            }
        }

        private IList<GetMsgModel> GetUsingGetMsgBuffer
        {
            get
            {
                return UsingTagForMsgRecord ? bufferGetMsgs1 : bufferGetMsgs2;
            }
        }

        private IList<GetMsgModel> GetUnUsingGetMsgBuffer
        {
            get
            {
                return UsingTagForMsgRecord ? bufferGetMsgs2 : bufferGetMsgs1;
            }
        }

        public IList<MsgRecord> GetMSG(IMsgRecordService _msgRecordService, ClientModel clientModel)
        {
            MsgRecordQuery query = new MsgRecordQuery();
            query.MsgRecipientObjectID = clientModel.ObjectID;
            query.MsgRecordtime = clientModel.LatestTime;
            return _msgRecordService.LoadMsgRecord(query);
        }

        public void StartMainThread()
        {
            IsRunning = true;
            ThreadStart threadStart = new ThreadStart(MainSaveRecordThread);
            Thread thread = new Thread(threadStart);
            thread.Start();
        }

        public void StopMainThread()
        {
            IsRunning = false;
            if (GetUsingMsgRecordBuffer.Count > 0)
            {
                msgRecordService.BatchSave(GetUsingMsgRecordBuffer);
                GetUsingMsgRecordBuffer.Clear();
            }

            if (GetUnUsingMsgRecordBuffer.Count > 0)
            {
                msgRecordService.BatchSave(GetUnUsingMsgRecordBuffer);
                GetUnUsingMsgRecordBuffer.Clear();
            }
        }

        private void MainSaveRecordThread()
        {
            CommonVariables.LogTool.Log("begin buffer contorl");
            //savedIntoDataBase = DateTime.Now.ToString(CommonFlag.F_DateTimeFormat);
            try
            {
                while (IsRunning)
                {
                    if (GetUsingMsgRecordBuffer.Count > 0)
                    {
                        UsingTagForMsgRecord = !UsingTagForMsgRecord;

                        msgRecordService.BatchSave(GetUnUsingMsgRecordBuffer);

                        //savedIntoDataBase = GetUnUsingBufferContainer.Select(t => t.SendTime).Max();

                        GetUnUsingMsgRecordBuffer.Clear();
                    }
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                CommonVariables.LogTool.Log(ex.Message + ex.StackTrace);
            }
        }
    }

}

