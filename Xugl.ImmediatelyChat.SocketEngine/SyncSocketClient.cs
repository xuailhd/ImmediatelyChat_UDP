using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.Core;

namespace Xugl.ImmediatelyChat.SocketEngine
{
    public delegate bool HandlerSyncReturnData(string returnData, bool isError);

    public class SyncSocketClient
    {
        //public static ManualResetEvent allDone = new ManualResetEvent(false);

        public string SendMsg(string ipaddress, int port, string sendData)
        {
            try
            {
                byte[] bytesSent;
                int bytecount;
                bytesSent = Encoding.UTF8.GetBytes(sendData);
                IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ipaddress), port);
                Socket clientSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(ipe);
                clientSocket.Send(bytesSent, bytesSent.Length, 0);
                bytesSent = new byte[1024];
                bytecount = clientSocket.Receive(bytesSent);
                clientSocket.Close();
                clientSocket = null;
                return Encoding.UTF8.GetString(bytesSent, 0, bytecount);
            }
            catch (Exception ex)
            {
                return ex.Message + ex.StackTrace;
            }
        }
    }
}
