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
using Xugl.ImmediatelyChat.Core;


namespace Xugl.ImmediatelyChat.AppService.Sockets
{
    class SocketService
    {
        private Socket serviceSocket;
        private bool IsGoOnRuning;
        Thread thread;

        public void Start()
        {
            IsGoOnRuning = true;

            ThreadStart threadStart = new ThreadStart(StartSocketService);
            thread = new Thread(threadStart);
            thread.Start();
        }

        private void StartSocketService()
        {
            //string host = "192.168.1.2";

            //System.Net.IPAddress[] addressList = Dns.GetHostAddresses("localhost");

            IPAddress ip = IPAddress.Parse(CommonVariables.ServerHostIP);
            IPEndPoint ipe = new IPEndPoint(ip, CommonVariables.ServerHostPort);
            //IPHostEntry hostEntry = null;
            //hostEntry = Dns.GetHostEntry("localhost");

            serviceSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serviceSocket.Bind(ipe);
            serviceSocket.Listen(0);
            CommonVariables.LogTool.Log("begin Server:" + CommonVariables.ServerHostIP + ", Port:" + CommonVariables.ServerHostPort.ToString());

            try
            {
                while (IsGoOnRuning)
                {
                    Socket clientSocket = serviceSocket.Accept();
                    SocketThead socketThead = new SocketThead(clientSocket);
                }
                serviceSocket.Close();
                CommonVariables.LogTool.Log("stop Server:" + CommonVariables.ServerHostIP + ", Port:" + CommonVariables.ServerHostPort.ToString());
            }
            catch (SocketException ex)
            {
                CommonVariables.LogTool.Log("Server Stoped:" + ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                CommonVariables.LogTool.Log("Server Stoped:" + ex.Message);
            }
        }

        public void Stop()
        {

            if (IsGoOnRuning == true)
            {
                IsGoOnRuning = false;
                try
                {
                    byte[] bytesSent;
                    bytesSent = Encoding.UTF8.GetBytes("stop");
                    IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(CommonVariables.ServerHostIP), CommonVariables.ServerHostPort);
                    Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    tempSocket.Connect(ipe);
                    tempSocket.Send(bytesSent, bytesSent.Length, 0);
                    tempSocket.Close();
                }
                catch (Exception ex)
                {

                }
                //serviceSocket = null;
            }
        }
    }

    
}
