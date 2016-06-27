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
    //public delegate bool HandlerSyncReturnData(string returnData,bool isError);

    public class SyncSocketClientUDP
    {
        //public static ManualResetEvent allDone = new ManualResetEvent(false);

        public void SendMsg(string ipaddress, int port, string sendData)
        {
            try
            {
                byte[] bytesSent;
                bytesSent = Encoding.UTF8.GetBytes(sendData);
                IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ipaddress), port);
                Socket clientSocket = new Socket(SocketType.Dgram, ProtocolType.Udp);

                clientSocket.SendTo(bytesSent, bytesSent.Length, SocketFlags.None, ipe);

                clientSocket.Close();
                clientSocket = null;
            }
            catch (Exception ex)
            {
            }
        }

        public string SendMsgWithReceive(string ipaddress, int port, string sendData)
        {
            try
            {
                byte[] bytesSent;
                byte[] buffer = new byte[1024];
                bytesSent = Encoding.UTF8.GetBytes(sendData);
                EndPoint ipe = new IPEndPoint(IPAddress.Parse(ipaddress), port);
                Socket clientSocket = new Socket(SocketType.Dgram, ProtocolType.Udp);

                clientSocket.SendTo(bytesSent, bytesSent.Length, SocketFlags.None, ipe);

                int receiveCount = clientSocket.ReceiveFrom(buffer, SocketFlags.None, ref ipe);

                string data = Encoding.UTF8.GetString(buffer, 0, receiveCount);

                clientSocket.Close();
                clientSocket = null;

                return data;
            }
            catch (Exception ex)
            {
            }
            return null;
        }
    }
}
