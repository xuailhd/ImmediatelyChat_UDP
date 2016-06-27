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
using Xugl.ImmediatelyChat.AppService.Interfaces;
using Xugl.ImmediatelyChat.Core;

namespace Xugl.ImmediatelyChat.AppService.Sockets
{
    internal class GroupSocketServer
    {
        private Socket serviceSocket;
        private bool IsGoOnRuning;
        Thread thread;
        //private IList<GroupSubSocket> groupSubSockets;

        public GroupSocketServer()
        {
            IsGoOnRuning = true;
        }

        public void Start()
        {
            thread = new Thread(new ThreadStart(StartGroupServer));
            thread.Start();
        }

        public void StartGroupServer()
        {
            IPAddress ip = IPAddress.Parse(CommonVariables.MSGserverHostIP);
            IPEndPoint ipe = new IPEndPoint(ip, CommonVariables.MSGserverHostPort);
            //IPHostEntry hostEntry = null;
            //hostEntry = Dns.GetHostEntry("localhost");

            serviceSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serviceSocket.Bind(ipe);
            serviceSocket.Listen(5);
            CommonVariables.LogTool.Log("begin MSG Server:" + CommonVariables.MSGserverHostIP + ", Port:" + CommonVariables.MSGserverHostPort.ToString());

            try
            {
                //Singleton<ICommonLog>.Instance.Log("begin Server:" + host + ", Port:" + port.ToString());
                while (IsGoOnRuning)
                {
                    Socket clientSocket = serviceSocket.Accept();

                    GroupSubSocket groupSubSocket = new GroupSubSocket(clientSocket, this);
                    //groupSubSockets.Add(groupSubSocket);
                    
                }
                serviceSocket.Close();
                CommonVariables.LogTool.Log("stop MSG Server:" + CommonVariables.MSGserverHostIP + ", Port:" + CommonVariables.MSGserverHostPort.ToString());
            }
            catch(SocketException ex)
            {
                CommonVariables.LogTool.Log("MSG Server Stoped:" + ex.Message);
            }
            catch(ObjectDisposedException ex)
            {
                CommonVariables.LogTool.Log("MSG Server Stoped:" + ex.Message);
            }
            //Singleton<IPortManage>.Instance.ReleasePort(port);
            //Singleton<ICommonLog>.Instance.Log("Stop Server:" + host + ", Port:" + port.ToString());
        }

        public void SendMsgToSub()
        {
            //foreach(GroupSubSocket groupSubSocket in groupSubSockets)
            //{
            //    groupSubSocket
            //}
        }

        public void Stop()
        {
            if (IsGoOnRuning == true)
            {
                IsGoOnRuning = false;
                try
                {
                    IsGoOnRuning = false;

                    byte[] bytesSent;
                    bytesSent = Encoding.UTF8.GetBytes("stop");
                    IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(CommonVariables.MSGserverHostIP), CommonVariables.MSGserverHostPort);
                    Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    tempSocket.Connect(ipe);
                    tempSocket.Send(bytesSent, bytesSent.Length, 0);
                    tempSocket.Close();
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
