using Newtonsoft.Json;
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
    public class MDSListenerUDPToken : AsyncUserToken
    {
        private readonly IMsgRecordService _msgRecordService;
        public MDSListenerUDPToken()
        {
            _msgRecordService = ObjectContainerFactory.CurrentContainer.Resolver<IMsgRecordService>();
        }

        public MsgRecord Model { get; set; }

        public string UAObjectID { get; set; }

        public IMsgRecordService MsgRecordService
        {
            get
            {
                return _msgRecordService;
            }
        }
    }

    public class UDPSocketListener : AsyncSocketListenerUDP<MDSListenerUDPToken>
    {
        public UDPSocketListener()
            : base(1024, 50,20, CommonVariables.LogTool)
        {
            
        }

        protected override void HandleError(MDSListenerUDPToken token)
        {
            if (token.Models != null && token.Models.Count > 0)
            {
                token.Models.Clear();
                token.Models = null;
            }
        }

        protected override string HandleRecivedMessage(string inputMessage, MDSListenerUDPToken token)
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

        private string HandlePSCallMDSStart(string data, MDSListenerUDPToken token)
        {
            data = data.Remove(0, CommonFlag.F_PSCallMDSStart.Length);
            IList<MCSServer> mcsServers = JsonConvert.DeserializeObject<IList<MCSServer>>(data.Substring(0, data.IndexOf("&&")));

            if (mcsServers != null && mcsServers.Count > 0)
            {
                data = data.Remove(0, data.IndexOf("&&") + 2);
                CommonVariables.ArrangeStr = JsonConvert.DeserializeObject<MDSServer>(data).ArrangeStr;
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

        private string HandleMDSReciveMCSFBMSG(string data, MDSListenerUDPToken token)
        {
            data = data.Remove(0, CommonFlag.F_MDSReciveMCSFBMSG.Length);
            CommonVariables.MessageContorl.HandleMCSMSGFB(data);
            return string.Empty;
        }

        private string HandleMDSVerifyMCSMSG(string data, MDSListenerUDPToken token)
        {
            string tempStr = data.Remove(0, CommonFlag.F_MDSVerifyMCSMSG.Length);
            MsgRecord msgReocod = JsonConvert.DeserializeObject<MsgRecord>(tempStr);
            if (msgReocod != null)
            {
                if (!string.IsNullOrEmpty(msgReocod.MsgRecipientObjectID))
                {
                    CommonVariables.MessageContorl.AddMSgRecordIntoBuffer(msgReocod);

                    MCSServer server = CommonVariables.CommonFunctions.FindMCSServer(CommonVariables.MCSServers, 
                        msgReocod.MsgRecipientObjectID);

                    CommonVariables.MessageContorl.AddMsgIntoSendBuffer(server, msgReocod);
                }
                return CommonFlag.F_MCSVerfiyFBMDSMSG + msgReocod.MsgID;
            }
            return string.Empty;
        }

        private string HandleMDSVerifyMCSGetMSG(string data, MDSListenerUDPToken token)
        {
            string tempStr = data.Remove(0, CommonFlag.F_MDSVerifyMCSGetMSG.Length);
            ClientModel clientModel = JsonConvert.DeserializeObject<ClientModel>(tempStr);

            if (clientModel != null)
            {
                if (!string.IsNullOrEmpty(clientModel.ObjectID))
                {
                    clientModel.MCS_IP = token.IP;
                    clientModel.MCS_Port = token.Port;
                    CommonVariables.MessageContorl.GetMSG(token.MsgRecordService, clientModel);
                }
            }
            return string.Empty;
        }

        public void BeginService()
        {
            base.BeginService(CommonVariables.MDSIP,CommonVariables.MDSPort);
        }
    }
}
