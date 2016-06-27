using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.Common;
using Xugl.ImmediatelyChat.SocketEngine;

namespace Xugl.ImmediatelyChat.Test
{
    public class TestUPDListener
    {
        private BufferManager m_bufferManager;
        public TestUPDListener()
        {

        }

        public string TestSendUDPToService(string ipaddress,int port)
        {
            Socket socket = new Socket(SocketType.Dgram, ProtocolType.Udp);

            m_bufferManager = BufferManager.CreateBufferManager(100 * 1024, 1024);
            for(int i =0 ;i<=50;i++)
            {
                AsyncClientToken asyncClientToken = new AsyncClientToken();
                //asyncClientToken.HandlerReturnData = handlerReturnData;
                asyncClientToken.Socket = socket;
                //asyncClientToken.MessageID = messageID;
                asyncClientToken.IP = ipaddress;
                asyncClientToken.Port = port;
                asyncClientToken.Buffer = m_bufferManager.TakeBuffer(1024);


                IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(asyncClientToken.IP), asyncClientToken.Port);
                int sendCount = Encoding.UTF8.GetBytes("TestSendUDPToService" + i.ToString(), 0, ("TestSendUDPToService" + i.ToString()).Length, asyncClientToken.Buffer, 0);
                CommonVariables.LogTool.Log(DateTime.Now.ToString(CommonFlag.F_DateTimeFormat) + "   Send:" + asyncClientToken.IP + asyncClientToken.Port.ToString() + "TestSendUDPToService" + i.ToString());
                socket.BeginSendTo(asyncClientToken.Buffer, 0, sendCount, SocketFlags.None, ipe, new AsyncCallback(SendCallback), asyncClientToken);
            }

            return null;
        }


        private void SendCallback(IAsyncResult ar)
        {
            AsyncClientToken token = (AsyncClientToken)ar.AsyncState;
            try
            {
                
                token.Socket.EndSendTo(ar);
                CommonVariables.LogTool.Log(DateTime.Now.ToString(CommonFlag.F_DateTimeFormat) + "   End Send:");
                EndPoint ipe = new IPEndPoint(IPAddress.Parse(token.IP), token.Port);
                token.Socket.BeginReceiveFrom(token.Buffer, 0, 1024, SocketFlags.None,ref ipe, new AsyncCallback(Reviceallback), token);
            }
            catch (Exception ex)
            {
                //token.HandlerReturnData(token.MessageID, true);
                CommonVariables.LogTool.Log(ex.Message + ex.StackTrace);
            }
        }
        private void Reviceallback(IAsyncResult ar)
        {
            AsyncClientToken token = (AsyncClientToken)ar.AsyncState;
            try
            {
                EndPoint ipe = new IPEndPoint(IPAddress.Parse(token.IP), token.Port);
                int receiveCount = token.Socket.EndReceiveFrom(ar, ref ipe);
                if (receiveCount > 0)
                {
                    CommonVariables.LogTool.Log(DateTime.Now.ToString(CommonFlag.F_DateTimeFormat) + "   receive:" + Encoding.UTF8.GetString(token.Buffer, 0, receiveCount));
                }
                else
                {
                    CommonVariables.LogTool.Log(DateTime.Now.ToString(CommonFlag.F_DateTimeFormat) + "   receive:null");
                }
                //token.Socket.Close();
            }
            catch (Exception ex)
            {
                //token.HandlerReturnData(token.MessageID, true);
                CommonVariables.LogTool.Log(ex.Message + ex.StackTrace);
            }
        }
    }

    internal class AsyncClientToken : AsyncUserToken
    {
        public byte[] Buffer { get; set; }

        public int Datasize { get; set; }

        public int IsWhole { get; set; }

        public HandlerReturnData HandlerReturnData { get; set; }

        public string MessageID { get; set; }
    }

}
