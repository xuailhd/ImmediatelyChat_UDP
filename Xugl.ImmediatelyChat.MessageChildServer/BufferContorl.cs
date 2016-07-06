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
using Newtonsoft.Json;

namespace Xugl.ImmediatelyChat.MessageChildServer
{
    /// <summary>
    /// 通过UDP p2p发送的信息 不在这里的处理范围
    /// </summary>
    public class BufferContorl
    {
        /// <summary>
        /// new message Buffer, from UA
        /// </summary>
        private IList<MsgRecordModel> MsgRecordBufferToMDS1 = new List<MsgRecordModel>();
        private IList<MsgRecordModel> MsgRecordBufferToMDS2 = new List<MsgRecordModel>();
        private bool UsingTagForMDS = false;


        /// <summary>
        /// new message Buffer, from MDS
        /// </summary>
        private IList<MsgRecordModel> MsgRecordBufferToUA1 = new List<MsgRecordModel>();
        private IList<MsgRecordModel> MsgRecordBufferToUA2 = new List<MsgRecordModel>();
        private bool UsingTagForUA = false;

        /// <summary>
        /// for prevent async data error
        /// </summary>
        private IList<MsgRecordModel> ExeingMsgRecordForMDS = new List<MsgRecordModel>();
        private IList<MsgRecordModel> ExeingMsgRecordForUA = new List<MsgRecordModel>();

        /// <summary>
        /// message Buffer, use to send to UA
        /// </summary>
        private static IDictionary<string, IList<MsgRecord>> OutMsgRecords = new Dictionary<string, IList<MsgRecord>>();
        //private IDictionary<string, IList<MsgRecord>> OutMsgRecords2 = new Dictionary<string, IList<MsgRecord>>();
        ////for loop and delete
        //private IList<string> OutMsgRecordKeys1 = new List<string>();
        //private IList<string> OutMsgRecordKeys2 = new List<string>();
        //private bool UsingTagForOutMsg = false;

        private IDictionary<string, ClientModel> clientModels = new Dictionary<string, ClientModel>();

        private const int _maxSize = 1024;
        private const int _maxSendConnections = 10;
        private const int _maxGetConnections = 10;
        private const int _sendDelay = 200;
        private const int _getDelay = 5000;

        public bool IsRunning = false;

        private object lockObject = new object();

        public void AddMsgRecordIntoBuffer(MsgRecordModel _msgRecordModel)
        {
            IList<MsgRecordModel> msgRecordModels = GenerateMsgRecordModel(_msgRecordModel);

            foreach (MsgRecordModel msgRecordModel in msgRecordModels)
            {
                GetUsingMsgRecordBufferToMDS.Add(msgRecordModel);
            }
        }

        public void AddClientModel(ClientModel clientModel)
        {
            if (!clientModels.ContainsKey(clientModel.ObjectID))
            {
                for (int i = 0; i < CommonVariables.MDSServers.Count; i++)
                {
                    if (CommonVariables.MDSServers[i].ArrangeStr.Contains(clientModel.ObjectID.Substring(0, 1)))
                    {
                        clientModel.MDS_IP = CommonVariables.MDSServers[i].MDS_IP;
                        clientModel.MDS_Port = CommonVariables.MDSServers[i].MDS_Port;
                        if (String.IsNullOrEmpty(clientModel.LatestTime))
                        {
                            clientModel.LatestTime = DateTime.Now.ToString(CommonFlag.F_DateTimeFormat);
                        }
                        break;
                    }
                }
                clientModels.Add(clientModel.ObjectID, clientModel);
            }
        }

        #region using unusing buffer manage

        private IList<MsgRecordModel> GetUsingMsgRecordBufferToMDS
        {
            get { 
                if(UsingTagForMDS)
                {
                    return MsgRecordBufferToMDS1;
                }
                else
                {
                    return MsgRecordBufferToMDS2;
                }
            }
        }

        private IList<MsgRecordModel> GetUnUsingMsgRecordBufferToMDS
        {
            get
            {
                if (!UsingTagForMDS)
                {
                    return MsgRecordBufferToMDS1;
                }
                else
                {
                    return MsgRecordBufferToMDS2;
                }
            }
        }

        private IList<MsgRecordModel> GetUsingMsgRecordBufferToUA
        {
            get
            {
                if (UsingTagForUA)
                {
                    return MsgRecordBufferToUA1;
                }
                else
                {
                    return MsgRecordBufferToUA2;
                }
            }
        }

        private IList<MsgRecordModel> GetUnUsingMsgRecordBufferToUA
        {
            get
            {
                if (!UsingTagForUA)
                {
                    return MsgRecordBufferToUA1;
                }
                else
                {
                    return MsgRecordBufferToUA2;
                }
            }
        }
        #endregion

        private IList<MsgRecordModel> GenerateMsgRecordModel(MsgRecordModel msgRecordModel)
        {
            IList<MsgRecordModel> msgRecords = new List<MsgRecordModel>();
            if (!string.IsNullOrEmpty(msgRecordModel.MsgRecipientGroupID))
            {
                IContactPersonService contactGroupService = ObjectContainerFactory.CurrentContainer.Resolver<IContactPersonService>();
                IList<String> ContactPersonIDs = contactGroupService.GetContactPersonIDListByGroupID(msgRecordModel.MsgSenderObjectID,msgRecordModel.MsgRecipientGroupID);
                foreach (String objectID in ContactPersonIDs)
                {
                    MsgRecordModel _msgRecordModel = new MsgRecordModel();
                    _msgRecordModel.MsgContent = msgRecordModel.MsgContent;
                    _msgRecordModel.MsgType = msgRecordModel.MsgType;
                    _msgRecordModel.MsgSenderObjectID = msgRecordModel.MsgSenderObjectID;
                    _msgRecordModel.MsgSenderName = msgRecordModel.MsgSenderName;
                    _msgRecordModel.MsgRecipientGroupID = msgRecordModel.MsgRecipientGroupID;
                    _msgRecordModel.IsSended = msgRecordModel.IsSended;
                    _msgRecordModel.MsgRecipientObjectID = objectID;
                    _msgRecordModel.SendTime = msgRecordModel.SendTime;
                    _msgRecordModel.MsgID = Guid.NewGuid().ToString();
                    for (int i = 0; i < CommonVariables.MDSServers.Count;i++ )
                    {
                        if (CommonVariables.MDSServers[i].ArrangeStr.Contains(_msgRecordModel.MsgRecipientObjectID.Substring(0, 1)))
                        {
                            _msgRecordModel.MDS_IP = CommonVariables.MDSServers[i].MDS_IP;
                            _msgRecordModel.MDS_Port = CommonVariables.MDSServers[i].MDS_Port;
                            //_msgRecordModel.MDS_ID = CommonVariables.MDSServers[i].MDS_ID;
                            break;
                        }
                    }

                    msgRecords.Add(_msgRecordModel);
                }
            }
            else if (string.IsNullOrEmpty(msgRecordModel.MsgRecipientGroupID) && !string.IsNullOrEmpty(msgRecordModel.MsgRecipientObjectID))
            {
                for (int i = 0; i < CommonVariables.MDSServers.Count; i++)
                {
                    if (CommonVariables.MDSServers[i].ArrangeStr.Contains(msgRecordModel.MsgRecipientObjectID.Substring(0, 1)))
                    {
                        msgRecordModel.MDS_IP = CommonVariables.MDSServers[i].MDS_IP;
                        msgRecordModel.MDS_Port = CommonVariables.MDSServers[i].MDS_Port;
                        if (string.IsNullOrEmpty(msgRecordModel.MsgID))
                        {
                            msgRecordModel.MsgID = Guid.NewGuid().ToString();
                        }
                        break;
                    }
                }
                msgRecords.Add(msgRecordModel);
            }
            return msgRecords;
        }

        public void UpdateClientModel(ClientModel clientModel)
        {
            if (clientModels.ContainsKey(clientModel.ObjectID))
            {
                clientModels[clientModel.ObjectID].LatestTime = DateTime.Now.ToString(CommonFlag.F_DateTimeFormat);
            }
            else
            {
                AddClientModel(clientModel);
            }
        }

        public void AddMsgIntoOutBuffer(MsgRecord msgRecord)
        {
            if (clientModels.ContainsKey(msgRecord.MsgRecipientObjectID))
            {
                if (clientModels[msgRecord.MsgRecipientObjectID].LatestTime.CompareTo(msgRecord.SendTime) < 0)
                {
                    clientModels[msgRecord.MsgRecipientObjectID].LatestTime = msgRecord.SendTime;
                }
            }

            if (OutMsgRecords.ContainsKey(msgRecord.MsgRecipientObjectID))
            {
                if (!(OutMsgRecords[msgRecord.MsgRecipientObjectID].Where(t => t.MsgID == msgRecord.MsgID).Count() > 0))
                {
                    OutMsgRecords[msgRecord.MsgRecipientObjectID].Add(msgRecord);
                }
            }
            else
            {
                OutMsgRecords.Add(msgRecord.MsgRecipientObjectID, new List<MsgRecord>());
                OutMsgRecords[msgRecord.MsgRecipientObjectID].Add(msgRecord);
            }
        }

        public IList<MsgRecord> GetMSG(ClientModel clientModel)
        {
            if (OutMsgRecords.ContainsKey(clientModel.ObjectID))
            {
                IList<MsgRecord> msgRecords = OutMsgRecords[clientModel.ObjectID].Where(t => t.SendTime.CompareTo(clientModel.LatestTime) > 0).ToList();

                if(msgRecords!=null && msgRecords.Count>0)
                {
                    OutMsgRecords[clientModel.ObjectID].Clear();
                    return msgRecords;
                }
            }
            return null;
        }

        public void StartMainThread()
        {
            IsRunning = true;
            ThreadStart threadStart = new ThreadStart(MainSendMSGToMDSThread);
            Thread thread = new Thread(threadStart);
            thread.Start();

            //threadStart = new ThreadStart(MainGetMSGThread);
            //thread = new Thread(threadStart);
            //thread.Start();
        }


        public void StopMainThread()
        {
            IsRunning = false;
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

        private MsgRecordModel ModelTransfor(MsgRecord msgRecord)
        {
            MsgRecordModel msgRecordModel = new MsgRecordModel();
            msgRecordModel.IsSended = msgRecord.IsSended;
            msgRecordModel.MsgContent = msgRecord.MsgContent;
            msgRecordModel.MsgID = msgRecord.MsgContent;
            msgRecordModel.MsgRecipientGroupID = msgRecord.MsgRecipientGroupID;
            msgRecordModel.MsgRecipientObjectID = msgRecord.MsgRecipientObjectID;
            msgRecordModel.MsgSenderName = msgRecord.MsgSenderName;
            msgRecordModel.MsgSenderObjectID = msgRecord.MsgSenderObjectID;
            msgRecordModel.MsgType = msgRecord.MsgType;
            msgRecordModel.SendTime = msgRecord.SendTime;
            return msgRecordModel;
        }

        private void MainSendMSGToMDSThread()
        {
            try
            {
                while (IsRunning)
                {
                    if (GetUsingMsgRecordBufferToMDS.Count > 0)
                    {
                        UsingTagForMDS = !UsingTagForMDS;

                        while (GetUnUsingMsgRecordBufferToMDS.Count > 0)
                        {
                            MsgRecordModel msgRecordModel = GetUnUsingMsgRecordBufferToMDS[0];
                            try
                            {
                                string messageStr = CommonFlag.F_MDSVerifyMCSMSG + JsonConvert.SerializeObject(ModelTransfor(msgRecordModel));
                                //CommonVariables.LogTool.Log("begin send mds " + msgRecordModel.MDS_IP + " port:" + msgRecordModel.MDS_Port + messageStr);
                                if( CommonVariables.Listener.SendMsg(msgRecordModel.MDS_IP, msgRecordModel.MDS_Port, messageStr, msgRecordModel.MsgID))
                                {
                                    msgRecordModel.ExeSendTime = DateTime.Now.ToString(CommonFlag.F_DateTimeFormat);
                                    msgRecordModel.reTryCount = msgRecordModel.reTryCount + 1;
                                    ExeingMsgRecordForMDS.Add(msgRecordModel);
                                }
                            }
                            catch (Exception ex)
                            {
                                CommonVariables.LogTool.Log(msgRecordModel.MsgID + ex.Message + ex.StackTrace);
                            }
                            GetUnUsingMsgRecordBufferToMDS.RemoveAt(0);
                        }
                    }
                    Thread.Sleep(_sendDelay);
                }
            }
            catch (Exception ex)
            {
                CommonVariables.LogTool.Log(ex.Message + ex.StackTrace);
            }
        }

        private void MainSendMSGToUAThread()
        {
            try
            {
                while (IsRunning)
                {
                    if (GetUsingMsgRecordBufferToUA.Count > 0)
                    {
                        UsingTagForUA = !UsingTagForUA;

                        while (GetUnUsingMsgRecordBufferToUA.Count > 0)
                        {
                            MsgRecordModel msgRecordModel = GetUnUsingMsgRecordBufferToUA[0];
                            try
                            {
                                string messageStr = CommonFlag.F_UAVerifyMCSMSG + JsonConvert.SerializeObject(ModelTransfor(msgRecordModel));
                                //CommonVariables.LogTool.Log("begin send mds " + msgRecordModel.MDS_IP + " port:" + msgRecordModel.MDS_Port + messageStr);
                                if (CommonVariables.Listener.SendMsg(msgRecordModel.Client_IP, msgRecordModel.Client_Port, messageStr, msgRecordModel.MsgID))
                                {
                                    msgRecordModel.ExeSendTime = DateTime.Now.ToString(CommonFlag.F_DateTimeFormat);
                                    msgRecordModel.reTryCount = msgRecordModel.reTryCount + 1;
                                    ExeingMsgRecordForUA.Add(msgRecordModel);
                                }
                            }
                            catch (Exception ex)
                            {
                                CommonVariables.LogTool.Log(msgRecordModel.MsgID + ex.Message + ex.StackTrace);
                            }
                            GetUnUsingMsgRecordBufferToUA.RemoveAt(0);
                        }
                    }
                    Thread.Sleep(_sendDelay);
                }
            }
            catch (Exception ex)
            {
                CommonVariables.LogTool.Log(ex.Message + ex.StackTrace);
            }
        }

        private void MainHandErrorMsgThread()
        {
            try
            {
                while (IsRunning)
                {
                    if (ExeingMsgRecordForUA.Count > 0)
                    {
                        IList<MsgRecordModel> needdelete1 = ExeingMsgRecordForUA.Where(t => t.reTryCount > 3).ToList();

                        if(needdelete1!=null && needdelete1.Count>0)
                        {
                            foreach(MsgRecordModel tempmodel in needdelete1)
                            {
                                ExeingMsgRecordForUA.Remove(tempmodel);
                            }
                        }

                        IList<MsgRecordModel> tempmodels = ExeingMsgRecordForUA.Where(t => t.ExeSendTime.CompareTo(DateTime.Now.AddSeconds(-3)
                            .ToString(CommonFlag.F_DateTimeFormat)) < 0 && t.reTryCount <= 3).ToList();

                        IList<ClientModel> tempclientModels = clientModels.Values.Where(t => t.LatestTime.CompareTo(DateTime.Now.AddSeconds(-10)
                            .ToString(CommonFlag.F_DateTimeFormat)) < 0).ToList();

                        if(tempclientModels!=null && tempclientModels.Count>0)
                        {
                            foreach (ClientModel clientModel in tempclientModels)
                            {
                                clientModels.Remove(clientModel.ObjectID);
                            }
                        }

                        IList<MsgRecordModel> needdelete2 = (from aa in tempmodels
                                                             join bb in tempclientModels on aa.MsgRecipientObjectID equals bb.ObjectID 
                                                             select aa).ToList();

                

                        if (needdelete2 != null && needdelete2.Count > 0)
                        {
                            for (int i = 0; i < needdelete2.Count; i++)
                            {
                                foreach (MsgRecordModel tempmodel in needdelete2)
                                {
                                    ExeingMsgRecordForUA.Remove(tempmodel);
                                }
                            }
                        }

                        needdelete2 = (from aa in tempmodels
                                       join bb in tempclientModels on aa.MsgRecipientObjectID equals bb.ObjectID into cc
                                       from temp in cc.DefaultIfEmpty()
                                       where temp == null
                                       select aa).ToList();

                        if (needdelete2 != null && needdelete2.Count > 0)
                        {
                            foreach (MsgRecordModel tempmodel in needdelete2)
                            {
                                ExeingMsgRecordForUA.Remove(tempmodel);
                                if (tempmodel.reTryCount < 3)
                                {
                                    if (clientModels.ContainsKey(tempmodel.MsgRecipientObjectID))
                                    {
                                        tempmodel.Client_IP = clientModels[tempmodel.MsgRecipientObjectID].Client_IP;
                                        tempmodel.Client_Port = clientModels[tempmodel.MsgRecipientObjectID].Client_Port;
                                    }
                                    GetUsingMsgRecordBufferToUA.Add(tempmodel);
                                }
                            }
                        }


                    }
                    Thread.Sleep(_sendDelay);
                }
            }
            catch (Exception ex)
            {
                CommonVariables.LogTool.Log(ex.Message + ex.StackTrace);
            }
        }


        public void SendGetMsgToMDS(ClientModel clientModel)
        {
            string messageStr = CommonFlag.F_MDSVerifyMCSGetMSG + JsonConvert.SerializeObject(clientModel);
            CommonVariables.Listener.SendMsg(clientModel.MDS_IP, clientModel.MDS_Port, messageStr, clientModel.ObjectID);
        }

        public void HandlerMDSmsgReturnData(string returnData, bool IsError)
        {
            if (!string.IsNullOrEmpty(returnData))
            {
                MsgRecordModel tempmodel = ExeingMsgRecordForMDS.Single(t => t.MsgID == returnData);

                if (tempmodel != null)
                {
                    if (IsError)
                    {
                        GetUsingMsgRecordBufferToMDS.Add(tempmodel);
                    }
                    ExeingMsgRecordForMDS.Remove(tempmodel);
                }
            }
        }


        public void HandlerUAMsgReturnData(string returnData,bool IsError)
        {
            if (!string.IsNullOrEmpty(returnData))
            {
                MsgRecordModel tempmodel= ExeingMsgRecordForUA.Single(t => t.MsgID == returnData);

                if (tempmodel != null)
                {
                    if (IsError)
                    {
                        GetUsingMsgRecordBufferToUA.Add(tempmodel);
                    }
                    ExeingMsgRecordForUA.Remove(tempmodel);
                }
            }
        }
    }

}

