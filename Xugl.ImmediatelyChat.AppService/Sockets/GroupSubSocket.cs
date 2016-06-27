using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.AppService.Common;
using Xugl.ImmediatelyChat.AppService.Interface;
using Xugl.ImmediatelyChat.AppService.Model;
using Xugl.ImmediatelyChat.Core;
using Xugl.ImmediatelyChat.Model;
using Xugl.ImmediatelyChat.IServices;
using Xugl.ImmediatelyChat.Core.DependencyResolution;
using Xugl.ImmediatelyChat.AppService.Interfaces;
using Xugl.ImmediatelyChat.AppService.Implements;
using System.Net;
using System.Json;
using Xugl.ImmediatelyChat.AppService.Interfaces;

namespace Xugl.ImmediatelyChat.AppService.Sockets
{
    internal class GroupSubSocket
    {
        private Socket clientSocket;
        private GroupSocketServer groupSocketServer;
        public bool IsMsgSocket;
        
        public GroupSubSocket(Socket clientSocket, GroupSocketServer groupSocketServer)
        {
            this.clientSocket = clientSocket;
        }

        public void Handler()
        {
            try
            {
                byte[] bytmsg = new byte[1024];
                int bytes = clientSocket.Receive(bytmsg);
                string strMsg = Encoding.UTF8.GetString(bytmsg, 0, bytes);

                if(strMsg=="stop")
                {
                    return;
                }

                MsgRecordModel msgModel = UnboxMsg(strMsg);

                if(string.IsNullOrEmpty(msgModel.ObjectID))
                {
                    return;
                }

                if (string.IsNullOrEmpty(msgModel.Content))
                {
                    IFindMsgRecord findMsgRecord=ObjectContainerFactory.CurrentContainer.Resolver<IFindMsgRecord>();
                    ParameterizedThreadStart threadStart=new ParameterizedThreadStart(findMsgRecord.findMsg);
                    Thread thread = new Thread(threadStart);
                    thread.Start(msgModel.ObjectID);
                }
                else if (!string.IsNullOrEmpty(msgModel.Content))
                {
                    IHandleSendMsg handleSendMsg = ObjectContainerFactory.CurrentContainer.Resolver<IHandleSendMsg>();
                    ParameterizedThreadStart threadStart = new ParameterizedThreadStart(handleSendMsg.Handler);
                    Thread thread = new Thread(threadStart);
                    thread.Start(msgModel);
                }               
            }
            catch(Exception ex)
            {

            }
        }

        private MsgRecordModel UnboxMsg(string msg)
        {
            string tempMsg = msg;

            MsgRecordModel msgModel = new MsgRecordModel();

            if (tempMsg.IndexOf(CommonFlag.F_ObjectID) >= 0 && tempMsg.IndexOf(CommonFlag.F_ObjectID) < tempMsg.IndexOf(CommonFlag.F_Content))
            {
                msgModel.ObjectID = tempMsg.Substring(tempMsg.IndexOf(CommonFlag.F_ObjectID) + CommonFlag.F_ObjectID.Length,
                    tempMsg.IndexOf(";", tempMsg.IndexOf(CommonFlag.F_ObjectID)) - tempMsg.IndexOf(CommonFlag.F_ObjectID) - CommonFlag.F_ObjectID.Length);
                tempMsg = tempMsg.Replace(CommonFlag.F_ObjectID + msgModel.ObjectID + ";", "");
            }

            if (tempMsg.IndexOf(CommonFlag.F_MsgType) >= 0 && tempMsg.IndexOf(CommonFlag.F_MsgType) < tempMsg.IndexOf(CommonFlag.F_Content))
            {
                msgModel.MsgType = Convert.ToInt32(tempMsg.Substring(tempMsg.IndexOf(CommonFlag.F_MsgType) + CommonFlag.F_MsgType.Length,
                    tempMsg.IndexOf(";", tempMsg.IndexOf(CommonFlag.F_MsgType)) - tempMsg.IndexOf(CommonFlag.F_MsgType) - CommonFlag.F_MsgType.Length));
                tempMsg = tempMsg.Replace(CommonFlag.F_MsgType + msgModel.MsgType + ";", "");
            }

            if (tempMsg.IndexOf(CommonFlag.F_RecivedObjectID) >= 0 && tempMsg.IndexOf(CommonFlag.F_RecivedObjectID) < tempMsg.IndexOf(CommonFlag.F_Content))
            {
                msgModel.RecivedObjectID = tempMsg.Substring(tempMsg.IndexOf(CommonFlag.F_RecivedObjectID) + CommonFlag.F_RecivedObjectID.Length,
                    tempMsg.IndexOf(";", tempMsg.IndexOf(CommonFlag.F_RecivedObjectID)) - tempMsg.IndexOf(CommonFlag.F_RecivedObjectID) - CommonFlag.F_RecivedObjectID.Length);
                tempMsg = tempMsg.Replace(CommonFlag.F_RecivedObjectID + msgModel.RecivedObjectID + ";", "");
            }

            if (tempMsg.IndexOf(CommonFlag.F_RecivedObjectID2) >= 0 && tempMsg.IndexOf(CommonFlag.F_RecivedObjectID2) < tempMsg.IndexOf(CommonFlag.F_Content))
            {
                msgModel.RecivedObjectID2 = tempMsg.Substring(tempMsg.IndexOf(CommonFlag.F_RecivedObjectID2) + CommonFlag.F_RecivedObjectID2.Length,
                    tempMsg.IndexOf(";", tempMsg.IndexOf(CommonFlag.F_RecivedObjectID2)) - tempMsg.IndexOf(CommonFlag.F_RecivedObjectID2) - CommonFlag.F_RecivedObjectID2.Length);
                tempMsg = tempMsg.Replace(CommonFlag.F_RecivedObjectID2 + msgModel.RecivedObjectID2 + ";", "");
            }

            if (tempMsg.IndexOf(CommonFlag.F_SendType) >= 0 && tempMsg.IndexOf(CommonFlag.F_SendType) < tempMsg.IndexOf(CommonFlag.F_Content))
            {
                msgModel.SendType = Convert.ToInt32(tempMsg.Substring(tempMsg.IndexOf(CommonFlag.F_SendType) + CommonFlag.F_SendType.Length,
                    tempMsg.IndexOf(";", tempMsg.IndexOf(CommonFlag.F_SendType)) - tempMsg.IndexOf(CommonFlag.F_SendType) - CommonFlag.F_SendType.Length));
                tempMsg = tempMsg.Replace(CommonFlag.F_SendType + msgModel.SendType + ";", "");
            }

            if (tempMsg.IndexOf(CommonFlag.F_Content) >= 0)
            {
                msgModel.Content = tempMsg.Replace(CommonFlag.F_Content, "");
                //tempMsg = tempMsg.Replace(CommonFlag.F_SendType + msgModel.SendType + ";", "");
            }

            return msgModel;
        }
    }
}
