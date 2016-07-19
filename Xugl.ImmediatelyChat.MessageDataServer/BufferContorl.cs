using Newtonsoft.Json;
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

        private IDictionary<string, MsgRecord> bufferMsgRecords1 = new Dictionary<string, MsgRecord>();
        private IDictionary<string, MsgRecord> bufferMsgRecords2 = new Dictionary<string, MsgRecord>();
        private bool UsingTagForMsgRecord = false;

        private IDictionary<string, MsgRecordModel> exeSendMsgRecords1Buffer = new Dictionary<string,MsgRecordModel>();

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

        private const int _saveDataDelay = 100;
        private const int _sendMsgDelay = 50;
        private const int _retryCount = 2;

        public BufferContorl()
        {
            msgRecordService = ObjectContainerFactory.CurrentContainer.Resolver<IMsgRecordService>();
        }

        public void AddMsgIntoSendBuffer(MCSServer mcsServer, MsgRecord msgRecord)
        {
            if (!exeSendMsgRecords1Buffer.ContainsKey(msgRecord.MsgID))
            {
                MsgRecordModel msgRecordmodel = new MsgRecordModel();
                msgRecordmodel.IsSended = msgRecord.IsSended;
                msgRecordmodel.MCS_IP = mcsServer.MCS_IP;
                msgRecordmodel.MCS_Port = mcsServer.MCS_Port;
                msgRecordmodel.MDS_IP = CommonVariables.MDSIP;
                msgRecordmodel.MDS_Port = CommonVariables.MDSPort;
                msgRecordmodel.MsgContent = msgRecord.MsgContent;
                msgRecordmodel.MsgID = msgRecord.MsgID;
                msgRecordmodel.MsgRecipientGroupID = msgRecord.MsgRecipientGroupID;
                msgRecordmodel.MsgRecipientObjectID = msgRecord.MsgRecipientObjectID;
                msgRecordmodel.MsgSenderName = msgRecord.MsgSenderName;
                msgRecordmodel.MsgSenderObjectID = msgRecord.MsgSenderObjectID;
                msgRecordmodel.MsgType = msgRecord.MsgType;
                msgRecordmodel.reTryCount = 1;
                msgRecordmodel.SendTime = msgRecord.SendTime;
                msgRecordmodel.ExeSendTime = DateTime.Now.ToString(CommonFlag.F_DateTimeFormat);

                CommonVariables.Listener.SendMsg(msgRecordmodel.MCS_IP, msgRecordmodel.MCS_Port,
                    CommonFlag.F_MCSVerfiyMDSMSG + JsonConvert.SerializeObject(msgRecord), msgRecordmodel.MsgID);
                exeSendMsgRecords1Buffer.Add(msgRecordmodel.MsgID, msgRecordmodel);
            }
        }

        public void SendMsgToMCS(MsgRecordModel msgRecordmodel)
        {
            CommonVariables.Listener.SendMsg(msgRecordmodel.MCS_IP, msgRecordmodel.MCS_Port,
                CommonFlag.F_MCSVerfiyMDSMSG + JsonConvert.SerializeObject(ModelTransfor(msgRecordmodel)), msgRecordmodel.MsgID);
        }

        private MsgRecord ModelTransfor(MsgRecordModel msgRecordModel)
        {
            MsgRecord msgRecord = new MsgRecord();
            msgRecord.IsSended = msgRecordModel.IsSended;
            msgRecord.MsgContent = msgRecordModel.MsgContent;
            msgRecord.MsgID = msgRecordModel.MsgContent;
            msgRecord.MsgRecipientGroupID = msgRecordModel.MsgRecipientGroupID;
            msgRecord.MsgRecipientObjectID = msgRecordModel.MsgRecipientObjectID;
            msgRecord.MsgSenderName = msgRecordModel.MsgSenderName;
            msgRecord.MsgSenderObjectID = msgRecordModel.MsgSenderObjectID;
            msgRecord.MsgType = msgRecordModel.MsgType;
            msgRecord.SendTime = msgRecordModel.SendTime;

            return msgRecord;
        }

        private void HandMCSMsgReturnData(string returnData)
        {
            if (!string.IsNullOrEmpty(returnData))
            {
                if (exeSendMsgRecords1Buffer.ContainsKey(returnData))
                {
                    exeSendMsgRecords1Buffer.Remove(returnData);
                }
            }
        }

        public void AddMSgRecordIntoBuffer(MsgRecord msgRecord)
        {
            if (!GetUsingMsgRecordBuffer.ContainsKey(msgRecord.MsgID))
            {
                GetUsingMsgRecordBuffer.Add(msgRecord.MsgID, msgRecord);
            }
        }


        #region buffer manager

        private IDictionary<string, MsgRecord> GetUsingMsgRecordBuffer
        {
            get{
                return UsingTagForMsgRecord ? bufferMsgRecords1 : bufferMsgRecords2;
            }
        }

        private IDictionary<string, MsgRecord> GetUnUsingMsgRecordBuffer
        {
            get
            {
                return UsingTagForMsgRecord ? bufferMsgRecords2 : bufferMsgRecords1;
            }
        }

        //private IList<MsgRecordModel> GetUsingSendMsgRecordBuffer
        //{
        //    get
        //    {
        //        return UsingTagForSendMsgRecord ? bufferSendMsgRecords1 : bufferSendMsgRecords2;
        //    }
        //}

        //private IList<MsgRecordModel> GetUnUsingSendMsgRecordBuffer
        //{
        //    get
        //    {
        //        return UsingTagForSendMsgRecord ? bufferSendMsgRecords2 : bufferSendMsgRecords1;
        //    }
        //}

        #endregion


        public void GetMSG(IMsgRecordService _msgRecordService, ClientModel clientModel)
        {
            MsgRecordModel msgRecordmodel = null;
            MsgRecordQuery query = new MsgRecordQuery();
            query.MsgRecipientObjectID = clientModel.ObjectID;
            query.MsgRecordtime = clientModel.LatestTime;
            IList<MsgRecord> msgRecords = _msgRecordService.LoadMsgRecord(query);

            foreach(MsgRecord msgRecord in msgRecords)
            {
                msgRecordmodel = new MsgRecordModel();
                msgRecordmodel.IsSended = msgRecord.IsSended;
                msgRecordmodel.MCS_IP = clientModel.MCS_IP;
                msgRecordmodel.MCS_Port = clientModel.MCS_Port;
                msgRecordmodel.MDS_IP = CommonVariables.MDSIP;
                msgRecordmodel.MDS_Port = CommonVariables.MDSPort;
                msgRecordmodel.MsgContent = msgRecord.MsgContent;
                msgRecordmodel.MsgID = msgRecord.MsgID;
                msgRecordmodel.MsgRecipientGroupID = msgRecord.MsgRecipientGroupID;
                msgRecordmodel.MsgRecipientObjectID = msgRecord.MsgRecipientObjectID;
                msgRecordmodel.MsgSenderName = msgRecord.MsgSenderName;
                msgRecordmodel.MsgSenderObjectID = msgRecord.MsgSenderObjectID;
                msgRecordmodel.MsgType = msgRecord.MsgType;
                msgRecordmodel.reTryCount = 1;
                msgRecordmodel.SendTime = msgRecord.SendTime;
                msgRecordmodel.ExeSendTime = DateTime.Now.ToString(CommonFlag.F_DateTimeFormat);

                CommonVariables.Listener.SendMsg(msgRecordmodel.MCS_IP, msgRecordmodel.MCS_Port,
                    CommonFlag.F_MCSVerfiyMDSMSG + JsonConvert.SerializeObject(msgRecord), msgRecordmodel.MsgID);
                exeSendMsgRecords1Buffer.Add(msgRecordmodel.MsgID, msgRecordmodel);
            }
        }

        public void StartMainThread()
        {
            IsRunning = true;
            ThreadStart threadStart = new ThreadStart(MainSaveRecordThread);
            Thread thread = new Thread(threadStart);
            thread.Start();

            threadStart = new ThreadStart(MainHandErrorRecordThread);
            thread = new Thread(threadStart);
            thread.Start();
        }

        public void StopMainThread()
        {
            IsRunning = false;
            if (GetUsingMsgRecordBuffer.Count > 0)
            {
                msgRecordService.BatchSave(GetUsingMsgRecordBuffer.Values.ToList());
                GetUsingMsgRecordBuffer.Clear();
            }

            if (GetUnUsingMsgRecordBuffer.Count > 0)
            {
                msgRecordService.BatchSave(GetUsingMsgRecordBuffer.Values.ToList());
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

                        msgRecordService.BatchSave(GetUnUsingMsgRecordBuffer.Values.ToList());

                        GetUnUsingMsgRecordBuffer.Clear();
                    }
                    Thread.Sleep(_saveDataDelay);
                }
            }
            catch (Exception ex)
            {
                CommonVariables.LogTool.Log(ex.Message + ex.StackTrace);
            }
        }

        private void MainHandErrorRecordThread()
        {
            try
            {
                while (IsRunning)
                {
                    if (exeSendMsgRecords1Buffer.Count > 0)
                    {
                        IList<string> tempMsgIDs = (from aa in exeSendMsgRecords1Buffer.Values
                                                    where aa.ExeSendTime.CompareTo(DateTime.Now.AddMilliseconds(-2).ToString(CommonFlag.F_DateTimeFormat)) <= 0
                                                    select aa.MsgID).ToList();

                        if (tempMsgIDs != null && tempMsgIDs.Count > 0)
                        {
                            foreach (string tempMsgID in tempMsgIDs)
                            {
                                if (exeSendMsgRecords1Buffer[tempMsgID].reTryCount < _retryCount)
                                {
                                    exeSendMsgRecords1Buffer[tempMsgID].reTryCount++;
                                    exeSendMsgRecords1Buffer[tempMsgID].ExeSendTime = DateTime.Now.ToString(CommonFlag.F_DateTimeFormat);
                                    SendMsgToMCS(exeSendMsgRecords1Buffer[tempMsgID]);
                                }
                                else
                                {
                                    exeSendMsgRecords1Buffer.Remove(tempMsgID);
                                }
                            }
                        }
                    }
                    Thread.Sleep(_sendMsgDelay);
                }
            }
            catch (Exception ex)
            {
                CommonVariables.LogTool.Log(ex.Message + ex.StackTrace);
            }
        }

        public void HandleMCSMSGFB(string returnData)
        {
            if (!string.IsNullOrEmpty(returnData))
            {
                if(exeSendMsgRecords1Buffer.ContainsKey(returnData))
                {
                    MsgRecordModel tempmodel = exeSendMsgRecords1Buffer[returnData];

                    if (tempmodel != null)
                    {
                        exeSendMsgRecords1Buffer.Remove(tempmodel.MsgID);
                    }
                }
            }
        }
    }

}

