using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.Common;
using Xugl.ImmediatelyChat.Core;

namespace Xugl.ImmediatelyChat.SocketEngine
{
    public delegate string HandlerReturnData(string returnData,bool isError);

    public class AsyncSocketClient
    {
        //public static ManualResetEvent allDone = new ManualResetEvent(false);
        private int m_maxConnnections;
        private int m_maxSize;
        private SocketAsyncEventArgsPool<AsyncClientToken> m_readWritePool;
        private BufferManager m_bufferManager;

        private Semaphore m_maxNumberAcceptedClients;
        private ICommonLog LogTool;


        public AsyncSocketClient(int _maxSize, int _maxConnnections, ICommonLog _logTool)
        {
            m_readWritePool = new SocketAsyncEventArgsPool<AsyncClientToken>();
            m_maxConnnections = _maxConnnections;
            m_maxSize = _maxSize;
            LogTool = _logTool;
            m_bufferManager = BufferManager.CreateBufferManager(m_maxConnnections * m_maxSize, m_maxSize);

            for (int i = 0; i < _maxConnnections; i++)
            {
                AsyncClientToken asyncClientToken = new AsyncClientToken();
                asyncClientToken.Buffer = m_bufferManager.TakeBuffer(m_maxSize);
                m_readWritePool.Push(asyncClientToken);
            }

            m_maxNumberAcceptedClients = new Semaphore(m_maxConnnections, m_maxConnnections);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipaddress"></param>
        /// <param name="port"></param>
        /// <param name="sendData"></param>
        /// <param name="messageID">if occur error, use this messageID as returndata</param>
        /// <param name="handlerReturnData"></param>
        public void SendMsg(string ipaddress, int port, string sendData, string messageID, HandlerReturnData handlerReturnData)
        {

            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ipaddress), port);
            Socket clientSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            m_maxNumberAcceptedClients.WaitOne();
            AsyncClientToken asyncClientToken = m_readWritePool.PopOrNew();
            asyncClientToken.HandlerReturnData = handlerReturnData;
            asyncClientToken.Socket = clientSocket;
            asyncClientToken.MessageID = messageID;
            int sendcount = Encoding.UTF8.GetBytes(sendData, 0, sendData.Length, asyncClientToken.Buffer, 0);
            asyncClientToken.Datasize = sendcount;


            try
            {
                clientSocket.BeginConnect(ipe, new AsyncCallback(ConnectCallback), asyncClientToken);
            }
            catch (Exception ex)
            {
                CloseOneInstance(asyncClientToken);
                if (LogTool != null)
                {
                    LogTool.Log(ex.Message + ex.StackTrace);
                }
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            AsyncClientToken token = (AsyncClientToken)ar.AsyncState;
            try
            {
                token.Socket.EndConnect(ar);

                token.Socket.BeginSend(token.Buffer, 0, token.Datasize, SocketFlags.None, new AsyncCallback(SendCallback), token);
            }
            catch (Exception ex)
            {
                token.HandlerReturnData(token.MessageID, true);
                CloseOneInstance(token);
                if (LogTool != null)
                {
                    LogTool.Log(ex.Message + ex.StackTrace);
                }
            }
        }


        private void SendCallback(IAsyncResult ar)
        {
            AsyncClientToken token = (AsyncClientToken)ar.AsyncState;
            try
            {
                token.Socket.EndSend(ar);

                token.Socket.BeginReceive(token.Buffer, 0, m_maxSize, SocketFlags.None, new AsyncCallback(ReciveCallback), token);
            }
            catch (Exception ex)
            {
                token.HandlerReturnData(token.MessageID, true);
                CloseOneInstance(token);
                if (LogTool != null)
                {
                    LogTool.Log(ex.Message + ex.StackTrace);
                }
            }
        }

        private void ReciveCallback(IAsyncResult ar)
        {
            AsyncClientToken token = (AsyncClientToken)ar.AsyncState;
            try
            {
                int recivecount = token.Socket.EndReceive(ar);
                if (recivecount > 0)
                {
                    string sendData = token.HandlerReturnData(Encoding.UTF8.GetString(token.Buffer, 0, recivecount), false);
                    if (!string.IsNullOrEmpty(sendData))
                    {
                        token.Datasize = Encoding.UTF8.GetBytes(sendData, 0, sendData.Length, token.Buffer, 0);
                        token.Socket.BeginSend(token.Buffer, 0, token.Datasize, SocketFlags.None, new AsyncCallback(SendCallback), token);
                    }
                    else
                    {
                        CloseOneInstance(token);
                    }
                    return;
                }
                token.HandlerReturnData(token.MessageID, true);
                CloseOneInstance(token);
            }
            catch (Exception ex)
            {
                token.HandlerReturnData(token.MessageID, true);
                CloseOneInstance(token);
                if (LogTool != null)
                {
                    LogTool.Log(ex.Message + ex.StackTrace);
                }
            }
        }

        private void CloseOneInstance(AsyncClientToken token, bool IsError = true)
        {
            if (token != null)
            {
                if (token.Socket != null)
                {
                    token.Socket.Close();
                    token.Socket = null;
                }
                m_readWritePool.Push(token);
            }
            m_maxNumberAcceptedClients.Release();
        }
    }


    internal class AsyncClientToken:AsyncUserToken
    {
        public byte[] Buffer { get; set; }

        public int Datasize { get; set; }

        public int IsWhole { get; set; }

        public HandlerReturnData HandlerReturnData { get; set; }

        public string MessageID { get; set; }
    }
}
