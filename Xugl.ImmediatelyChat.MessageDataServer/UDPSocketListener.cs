using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.Common;
using Xugl.ImmediatelyChat.Core;
using Xugl.ImmediatelyChat.Core.DependencyResolution;
using Xugl.ImmediatelyChat.IServices;
using Xugl.ImmediatelyChat.Model;
using Xugl.ImmediatelyChat.SocketEngine;

namespace Xugl.ImmediatelyChat.MessageDataServer
{

    internal class UDPSocketListener : AsyncSocketListenerUDP<MDSListenerToken>
    {
        public UDPSocketListener()
            : base(1024, 50, CommonVariables.LogTool)
        {
            
        }

        protected override void HandleError(MDSListenerToken token)
        {
            if (token.Models != null && token.Models.Count > 0)
            {
                token.Models.Clear();
                token.Models = null;
            }
        }

        protected override string HandleRecivedMessage(string inputMessage, MDSListenerToken token)
        {
            if (string.IsNullOrEmpty(inputMessage))
            {
                return string.Empty;
            }

            string data = inputMessage;

            if (token == null)
            {
                return string.Empty;
            }

            try
            {
                if (data.StartsWith(CommonFlag.F_PSCallMDSStart))
                {
                    return HandlePSCallMDSStart(data, token);
                }

                if (CommonVariables.IsBeginMessageService)
                {
                    //handle UA feedback
                    if (data.StartsWith(CommonFlag.F_MDSReciveMCSFBMSG))
                    {
                        return HandleMDSReciveMCSFBMSG(data, token);
                    }

                    if (data.StartsWith(CommonFlag.F_MDSVerifyMCSMSG))
                    {
                        return HandleMDSVerifyMCSMSG(data, token);
                    }

                    if (data.StartsWith(CommonFlag.F_MDSVerifyMCSGetMSG))
                    {
                        return HandleMDSVerifyMCSGetMSG(data, token);
                    }
                }
            }
            catch (Exception ex)
            {
                CommonVariables.LogTool.Log(ex.Message + ex.StackTrace);
            }
            return string.Empty;
        }

        private string HandlePSCallMDSStart(string data,MDSListenerToken token)
        {
            data = data.Remove(0, CommonFlag.F_PSCallMDSStart.Length);
            IList<MCSServer> mcsServers = CommonVariables.serializer.Deserialize<IList<MCSServer>>(data.Substring(0, data.IndexOf("&&")));

            if (mcsServers != null && mcsServers.Count > 0)
            {
                data = data.Remove(0, data.IndexOf("&&") + 2);
                CommonVariables.ArrangeStr = CommonVariables.serializer.Deserialize<MDSServer>(data).ArrangeStr;
                CommonVariables.OperateFile.SaveConfig(CommonVariables.ConfigFilePath, CommonFlag.F_ArrangeChars, CommonVariables.ArrangeStr);
                CommonVariables.LogTool.Log("ArrangeStr:" + CommonVariables.ArrangeStr);
                CommonVariables.LogTool.Log("MCS count:" + mcsServers.Count);
                foreach (MCSServer mcsServer in mcsServers)
                {
                    CommonVariables.MCSServers.Add(mcsServer);
                    CommonVariables.LogTool.Log("IP:" + mcsServer.MCS_IP + " Port:" + mcsServer.MCS_Port + "  ArrangeStr:" + mcsServer.ArrangeStr);
                }
                CommonVariables.LogTool.Log("Start MDS service:" + CommonVariables.MDSIP + ", Port:" + CommonVariables.MDSPort.ToString());
                CommonVariables.IsBeginMessageService = true;
            }
            return string.Empty;
        }

        private string HandleMDSReciveMCSFBMSG(string data,MDSListenerToken token)
        {
            string tempStr = data.Remove(0, CommonFlag.F_MDSReciveMCSFBMSG.Length);

            if (token.Models != null && token.Models.Count > 0)
            {
                if (token.Models[0].MsgID == tempStr)
                {
                    token.Models.RemoveAt(0);
                }
                else
                {
                    for (int i = 1; i < token.Models.Count; i++)
                    {
                        if (token.Models[i].MsgID == tempStr)
                        {
                            token.Models.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            if (token.Models != null && token.Models.Count > 0)
            {
                return CommonFlag.F_MCSVerfiyMDSMSG + CommonVariables.serializer.Serialize(token.Models[0]);
            }
            else
            {
                return string.Empty;
            }

        }

        private string HandleMDSVerifyMCSMSG(string data,MDSListenerToken token)
        {
            string tempStr = data.Remove(0, CommonFlag.F_MDSVerifyMCSMSG.Length);
            MsgRecordModel msgModel = CommonVariables.serializer.Deserialize<MsgRecordModel>(tempStr);
            if (msgModel != null)
            {
                if (!string.IsNullOrEmpty(msgModel.MsgSenderObjectID))
                {
                    MsgRecord msgReocod = CommonVariables.MessageContorl.AddMSgRecordIntoBuffer(msgModel);

                    foreach(MCSServer mcsServer in CommonVariables.MCSServers)
                    {
                        if (mcsServer.ArrangeStr.Contains(msgReocod.MsgRecipientObjectID.Substring(0, 1)))
                        {
                            CommonVariables.MessageContorl.SendMsgToMCS(mcsServer, msgReocod);
                            break;
                        }
                    }

                    return msgModel.MsgID;
                }
            }
            return string.Empty;
        }

        private string HandleMDSVerifyMCSGetMSG(string data,MDSListenerToken token)
        {
            string tempStr = data.Remove(0, CommonFlag.F_MDSVerifyMCSGetMSG.Length);
            ClientModel clientModel = CommonVariables.serializer.Deserialize<ClientModel>(tempStr);

            if (clientModel != null)
            {
                if (!string.IsNullOrEmpty(clientModel.ObjectID))
                {

                    token.Models = CommonVariables.MessageContorl.GetMSG(token.MsgRecordService, clientModel);
                    //CommonVariables.LogTool.Log("recive mcs get msg:" + getMsgModel.LatestTime + "/" + getMsgModel.ObjectID);
                    if (token.Models != null && token.Models.Count > 0)
                    {
                        //CommonVariables.LogTool.Log("token.Models.Count:" + token.Models.Count.ToString());
                        return CommonFlag.F_MCSVerfiyMDSMSG + CommonVariables.serializer.Serialize(token.Models[0]);
                    }
                }
            }
            return string.Empty;
        }


        public void BeginService()
        {
            CommonVariables.MessageContorl.StartMainThread();
            base.BeginService(CommonVariables.MDSIP,CommonVariables.MDSPort);
        }
    }
}
