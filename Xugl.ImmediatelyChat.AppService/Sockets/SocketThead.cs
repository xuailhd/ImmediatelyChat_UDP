using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.AppService.Common;
using Xugl.ImmediatelyChat.AppService.Interface;
using Xugl.ImmediatelyChat.AppService.Model;
using Xugl.ImmediatelyChat.Core;

namespace Xugl.ImmediatelyChat.AppService.Sockets
{
    // Get Client Status
    internal class SocketThead
    {
        private Socket clientSocket;
        private Thread childThread;

        public SocketThead(Socket clientSocket)
        {
            this.clientSocket = clientSocket;
            childThread = new Thread(new ThreadStart(Handle));
            childThread.Start();
        }

        private void Handle()
        {
            ClientStatusModel clientStatusModel;

            byte[] bytmsg=new byte[1024];
            int bytes = clientSocket.Receive(bytmsg);
            string strMsg = Encoding.UTF8.GetString(bytmsg, 0, bytes);

            if (strMsg == "stop")
            {
                return;
            }

            CommonVariables.LogTool.Log(strMsg);

            clientStatusModel = UnboxMsg(strMsg);

            if(string.IsNullOrEmpty(clientStatusModel.ObjectID))
            {
                return;
            }

            clientStatusModel.Recive_IP = ((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString();

            CommonVariables.AddCurrentClients(clientStatusModel);

            CommonVariables.LogTool.Log("server:" + clientStatusModel.Recive_IP + " port:" + clientStatusModel.Recive_Port
                + " ObjectID:" + clientStatusModel.ObjectID + "  is connected");
            
            //strMsg = CommonFlag.F_IP + CommonVariables.MSGserverHostIP + ";" + CommonFlag.F_Port + CommonVariables.MSGserverHostPort.ToString()+ ";";
            strMsg = CommonFlag.F_IP + "183.11.14.151;" + CommonFlag.F_Port + CommonVariables.MSGserverHostPort.ToString() + ";";
            bytmsg = Encoding.UTF8.GetBytes(strMsg);
            clientSocket.Send(bytmsg);

            clientSocket.Close();
        }


        private ClientStatusModel UnboxMsg(string msg)
        {
            ClientStatusModel clientStatusModel = new ClientStatusModel();
            string tempMsg = msg;

            //if (tempMsg.IndexOf(CommonFlag.F_IP) >= 0)
            //{
            //    clientStatusModel.Recive_IP = tempMsg.Substring(tempMsg.IndexOf(CommonFlag.F_IP) + CommonFlag.F_IP.Length, tempMsg.IndexOf(";", tempMsg.IndexOf(CommonFlag.F_IP)) - tempMsg.IndexOf(CommonFlag.F_IP) - CommonFlag.F_IP.Length);
            //    tempMsg = tempMsg.Replace(CommonFlag.F_IP + clientStatusModel.Recive_IP + ";", "");
            //}

            if (tempMsg.IndexOf(CommonFlag.F_Port) >= 0)
            {
                clientStatusModel.Recive_Port = Convert.ToInt32(tempMsg.Substring(tempMsg.IndexOf(CommonFlag.F_Port) + CommonFlag.F_Port.Length, tempMsg.IndexOf(";", tempMsg.IndexOf(CommonFlag.F_Port)) - tempMsg.IndexOf(CommonFlag.F_Port) - CommonFlag.F_Port.Length));
                tempMsg = tempMsg.Replace(CommonFlag.F_Port + clientStatusModel.Recive_Port.ToString() +";", "");
            }

            if (tempMsg.IndexOf(CommonFlag.F_ObjectID) >= 0)
            {
                clientStatusModel.ObjectID = tempMsg.Substring(tempMsg.IndexOf(CommonFlag.F_ObjectID) + CommonFlag.F_ObjectID.Length, tempMsg.IndexOf(";", tempMsg.IndexOf(CommonFlag.F_ObjectID)) - tempMsg.IndexOf(CommonFlag.F_ObjectID) - CommonFlag.F_ObjectID.Length);
                tempMsg = tempMsg.Replace(CommonFlag.F_ObjectID + clientStatusModel.ObjectID.ToString(), "");
            }

            return clientStatusModel;
        }
    }
}
