using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.Common;

namespace Xugl.ImmediatelyChat.SocketEngine
{
    public abstract class ServerInstance : AsyncSocketListenerUDP<AsyncUserToken>
    {
        public ServerInstance(int _maxSize, int _maxReciveCount, int _maxSendCount, ICommonLog _logTool) : base(_maxSize, _maxReciveCount, _maxSendCount, _logTool)
        {
        }

        private IList<DataWithServer> contactDataBuffer1 = new List<DataWithServer>();
        private IList<DataWithServer> contactDataBuffer2 = new List<DataWithServer>();
        private bool UsingTag = false;


        private IList<DataWithServer> exeContactDataBuffer = new List<DataWithServer>();
        private IList<string> exeContactDataBufferKeys = new List<string>();

        //读取大文件
        private IList<DataSortModel> redContactDataBuffer = new List<DataSortModel>();
        private IList<string> redContactDataBufferKeys = new List<string>();
        private IDictionary<string, bool> redContactDataBufferconfic1 = new Dictionary<string, bool>();
        private IDictionary<string, bool> redContactDataBufferconfic2 = new Dictionary<string, bool>();

        private int sendContactDataDelay = 100;

        private int reSendSec = 1;
        private int reSendCount = 3;

        private int reciveWait = 5;

        private int eachSendCount = 6000;

        private string ipaddress;
        private int port;

        private int tempi;
        private int tempj;

        #region buffer manager
        private IList<DataWithServer> GetUsingContactDataBuffer
        {
            get
            {
                return UsingTag ? contactDataBuffer1 : contactDataBuffer2;
            }
        }

        private IList<DataWithServer> GetUnUsingContactDataBuffer
        {
            get
            {
                return UsingTag ? contactDataBuffer2 : contactDataBuffer1;
            }
        }
        #endregion

        public void StopMainThread()
        {
            IsRunning = false;
            base.CloseListener();
        }

        public void StartMainThread(string _ipaddress, int _port)
        {
            if (!IsRunning)
            {
                IsRunning = true;

                ipaddress = _ipaddress;
                port = _port;

                ThreadStart st = new ThreadStart(SendDataThread);
                Thread thread = new Thread(st);
                thread.Start();

                st = new ThreadStart(HandlerReadBuffer);
                thread = new Thread(st);
                thread.Start();

                st = new ThreadStart(HandlerReSend);
                thread = new Thread(st);
                thread.Start();

                st = new ThreadStart(MainThread);
                thread = new Thread(st);
                thread.Start();
            }
        }

        private void MainThread()
        {
            base.BeginService(ipaddress, port);
        }

        protected abstract byte[] HandleRecivedMessage(byte[] data);

        protected override byte[] HandleRecivedMessage(byte[] data, AsyncUserToken token)
        {
            string SendID = Encoding.UTF8.GetString(data, 0, 32);

            //传输出去的数据 回传响应
            if (exeContactDataBufferKeys.Contains(SendID))
            {
                exeContactDataBufferKeys.Remove(SendID);
                //tempj++;
                //base.LogTool.Log("接受回传响应数据包:" + SendID + "    " + tempj.ToString());
                int i = exeContactDataBuffer.Count - 1;
                while (i >= 0)
                {
                    DataWithServer tempdata = exeContactDataBuffer[i];
                    if (tempdata.SendID == SendID)
                    {
                        tempdata.IsDelete = true;
                        break;
                    }
                    i--;
                }
                
                return null;
            }
            else
            {
                //空数据包
                if (data.Length <= 66)
                {
                    return null;
                }

                //新数据
                //if (data.Length < 64)
                //{
                //    base.LogTool.Log("异常数据包:" + SendID);
                //    return null;
                //}

                byte[] returnbyte;
                if (data[64] > 0)
                {
                    returnbyte = HandleReadBuffer(data.Skip(64).ToArray(), Encoding.UTF8.GetString(data, 32, 32));
                }

                //else
                //{
                //    returnbyte = HandleRecivedMessage(data.Skip(66).ToArray());
                //}
                //if (returnbyte == null || returnbyte.Length <= 0)
                //{
                //    return data.Take(32).ToArray();
                //}

                DataWithServer contactDataWithServer = new DataWithServer();
                contactDataWithServer = new DataWithServer();
                contactDataWithServer.ServerIP = token.IP;
                contactDataWithServer.ServerPort = token.Port;
                contactDataWithServer.SendID = SendID;
                contactDataWithServer.MsgID = Encoding.UTF8.GetString(data, 32, 32);
                contactDataWithServer.Sort = data[65];
                contactDataWithServer.AllCount = 0;
                contactDataWithServer.ContactData = null;
                GetUsingContactDataBuffer.Add(contactDataWithServer);

                return null;
            }
        }

        private byte[] HandleReadBuffer(byte[] data, string MsgID)
        {
            //base.LogTool.Log("接受组合数据包:" + MsgID + "  " + data[0].ToString() + "  " + data.Length);
            if (data[0] > 0 && !string.IsNullOrEmpty(MsgID))
            {
                if (redContactDataBufferKeys.Contains(MsgID))
                {
                    UpdateDataSortModel(data, MsgID);
                }
                else
                {
                    try
                    {
                        DataSortModel model = new DataSortModel();
                        model.MsgID = MsgID;
                        model.AllCount = data[0];
                        model.UpdateTime = DateTime.Now.ToString(CommonFlag.F_DateTimeFormat);
                        model.DataWithServers = new Dictionary<byte, byte[]>();
                        model.DataWithServersKeys = new List<byte>();
                        model.AsyncFlag = new Stack<bool>();
                        model.AsyncFlag.Push(true);
                        model.DataWithServersKeys.Add(data[1]);
                        model.DataWithServers.Add(data[1],data.Skip(2).ToArray());

                        //双保险
                        redContactDataBufferconfic1.Add(MsgID, false);
                        redContactDataBufferconfic2.Add(MsgID, false);
                        if(!redContactDataBufferKeys.Contains(MsgID))
                        {
                            redContactDataBuffer.Add(model);
                            redContactDataBufferKeys.Add(MsgID);
                            redContactDataBufferconfic1.Remove(MsgID);
                            redContactDataBufferconfic2.Remove(MsgID);
                        }
                        else
                        {
                            UpdateDataSortModel(data, MsgID);
                        }
                    }
                    catch(ArgumentException ex)
                    {
                        UpdateDataSortModel(data, MsgID);
                    }
                }
            }
            return null;
        }

        private void UpdateDataSortModel(byte[] data, string MsgID)
        {
            int retrycount = 0;
            int index = redContactDataBufferKeys.IndexOf(MsgID);
            while (index < 0 && retrycount <= 100)
            {
                index = redContactDataBufferKeys.IndexOf(MsgID);
                retrycount++;
            }

            retrycount = 0;
            DataSortModel model = null;
            DataSortModel tempModel = null;
            while (retrycount<= 100 && model==null)
            {
                try
                {
                    tempModel = redContactDataBuffer[index];
                    if (MsgID == tempModel.MsgID)
                    {
                        model = tempModel;
                    }
                    else
                    {
                        index = 0;
                        while (index < redContactDataBuffer.Count)
                        {
                            tempModel = redContactDataBuffer[index];
                            if (MsgID == tempModel.MsgID)
                            {
                                model = tempModel;
                                break;
                            }
                        }
                    }
                    break;
                }
                catch(Exception ex)
                {
                    //index--;  //已被其它进程删除 往下找
                    retrycount++;
                }
            }

            if (model != null)
            {
                model.UpdateTime = DateTime.Now.ToString(CommonFlag.F_DateTimeFormat);
                if (!model.DataWithServersKeys.Contains(data[1]))
                {
                    try
                    {
                        model.DataWithServers.Add(data[1], data.Skip(2).ToArray());
                        model.DataWithServersKeys.Add(data[1]);
                        //base.LogTool.Log("当前数量:" + MsgID + "  " + model.DataWithServers.Count);
                        if (model.DataWithServers.Count == model.AllCount)
                        {
                            if (model.AsyncFlag.Pop())
                            {
                                byte[] tempData = null;
                                for (byte i = 1; i < model.AllCount; i++)
                                {
                                    tempData = tempData.Combine(model.DataWithServers[i]);
                                }
                                redContactDataBufferKeys.Remove(model.MsgID);
                                model.IsDelete = true;
                                HandleRecivedMessage(tempData);
                            }
                        }
                    }
                    catch(ArgumentException ex)
                    {

                    }
                    
                }
            }
            //else
            //{
            //    base.LogTool.Log("我擦，还是没有!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            //}

        }


        protected abstract void HandleError(string msgID);
        protected override void HandleError(AsyncUserToken token)
        {
            if (!string.IsNullOrWhiteSpace(token.MsgID))
            {
                HandleError(token.MsgID);
            }
        }

        public void SendMsg(string MsgID, byte[] data, string serverIP, int port)
        {
            DataWithServer contactDataWithServer;

            if (string.IsNullOrEmpty(serverIP))
            {
                return;
            }

            if (data == null || data.Length <= 0)
            {
                return;
            }

            if (data.Length > eachSendCount)
            {
                //大文件 分开发
                byte AllCount = (byte)Math.Ceiling(((double)data.Length) / ((double)eachSendCount));
                for (int i = 1; i <= 255 && i <= AllCount; i++)
                {
                    contactDataWithServer = new DataWithServer();
                    contactDataWithServer.ServerIP = serverIP;
                    contactDataWithServer.ServerPort = port;
                    contactDataWithServer.Sort = (byte)i;
                    contactDataWithServer.SendID = Guid.NewGuid().ToString("N");
                    contactDataWithServer.MsgID = MsgID;
                    contactDataWithServer.AllCount = AllCount;
                    contactDataWithServer.ContactData = data.Skip((i-1) * eachSendCount).Take(eachSendCount).ToArray();
                    GetUsingContactDataBuffer.Add(contactDataWithServer);
                }

            }
            else
            {
                contactDataWithServer = new DataWithServer();
                contactDataWithServer.ServerIP = serverIP;
                contactDataWithServer.ServerPort = port;
                contactDataWithServer.SendID = Guid.NewGuid().ToString("N");
                contactDataWithServer.MsgID = MsgID;
                contactDataWithServer.Sort = 0;
                contactDataWithServer.AllCount = 0;
                contactDataWithServer.ContactData = data;
                GetUsingContactDataBuffer.Add(contactDataWithServer);
            }
        }

        private void SendDataThread()
        {

            DataWithServer contactDataWithServer;

            while (IsRunning)
            {
                if (GetUsingContactDataBuffer.Count > 0)
                {
                    UsingTag = !UsingTag;
                    while (GetUnUsingContactDataBuffer.Count > 0)
                    {
                        contactDataWithServer = GetUnUsingContactDataBuffer[0];
                        if (contactDataWithServer != null)
                        {
                            byte[] Sort = { contactDataWithServer.AllCount, contactDataWithServer.Sort };
                            if(contactDataWithServer.ContactData != null)
                            {
                                contactDataWithServer.SendTime = DateTime.Now.ToString(CommonFlag.F_DateTimeFormat);
                                contactDataWithServer.ReCount = contactDataWithServer.ReCount + 1;
                                exeContactDataBuffer.Add(contactDataWithServer);
                                exeContactDataBufferKeys.Add(contactDataWithServer.SendID);
                            }
                            base.SendMsg(contactDataWithServer.ServerIP, contactDataWithServer.ServerPort,
                                        Encoding.UTF8.GetBytes(contactDataWithServer.SendID).Combine(Encoding.UTF8.GetBytes(contactDataWithServer.MsgID)).Combine(Sort).Combine(contactDataWithServer.ContactData).ToArray(),
                                        contactDataWithServer.MsgID);
                            //tempi++;
                            //base.LogTool.Log("发送数据包" + tempi.ToString() + "  port:" + contactDataWithServer.ServerPort.ToString());
                        }
                        GetUnUsingContactDataBuffer.RemoveAt(0);
                    }
                }
                Thread.Sleep(sendContactDataDelay);
            }

        }

        private void HandlerReSend()
        {
            while (IsRunning)
            {

                if (exeContactDataBufferKeys.Count > 0)
                {
                    int i = exeContactDataBuffer.Count - 1;
                    while (i >= 0)
                    {
                        DataWithServer item = exeContactDataBuffer[i];
                        if (item != null)
                        {
                            if(item.IsDelete)
                            {
                                exeContactDataBuffer.Remove(item);
                                i--;
                                continue;
                            }

                            if (item.SendTime.CompareTo(DateTime.Now.AddSeconds(-reSendSec).ToString(CommonFlag.F_DateTimeFormat)) < 0)
                            {
                                exeContactDataBufferKeys.Remove(item.SendID);
                                exeContactDataBuffer.Remove(item);
                                if (item.ReCount < reSendCount)
                                {
                                    GetUsingContactDataBuffer.Add(item);
                                }
                                else
                                {
                                    //base.LogTool.Log("未发送成功的数据包 item.MsgID:" + item.MsgID + " item.SendID:" + item.SendID + " item.Sort:" + item.Sort.ToString() + "item.ContactData" + item.ContactData.Length);
                                    HandleError(item.MsgID);
                                }
                            }
                        }
                        //else
                        //{
                        //    base.LogTool.Log("错误的空数据");
                        //}

                        i--;
                    }
                }
                Thread.Sleep(sendContactDataDelay);
            }
        }

        private void HandlerReadBuffer()
        {

            while (IsRunning)
            {
                if (redContactDataBufferKeys.Count > 0)
                {
                    int i = redContactDataBuffer.Count - 1;

                    while (i >= 0)
                    {
                        DataSortModel item = redContactDataBuffer[i];


                        if(item.IsDelete)
                        {
                            redContactDataBuffer.Remove(item);
                            i--;
                            continue;
                        }

                        string now = DateTime.Now.AddSeconds(-reciveWait).ToString(CommonFlag.F_DateTimeFormat);
                        if (item.UpdateTime.CompareTo(now) < 0)
                        {
                            string tmpstr = "";
                            for (int j= 0; j < item.DataWithServersKeys.Count;j++)
                            {
                                tmpstr = tmpstr + item.DataWithServersKeys[j].ToString() + ",";
                            }

                            //base.LogTool.Log("未接受成功的数据 item.MsgID:" + item.MsgID + " item.AllCount:" + item.AllCount + " item.DataWithServersKeys.Count:" + item.DataWithServersKeys.Count
                            //    + " item.DataWithServers.Count:" + item.DataWithServers.Count + " tmpstr:" + tmpstr);
                            redContactDataBuffer.Remove(item);
                            redContactDataBufferKeys.Remove(item.MsgID);
                        }
                        i--;
                    }
                }
                Thread.Sleep(sendContactDataDelay);
            }

        }

    }
}
    