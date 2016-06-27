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
    public class AsyncSocketClientUDP
    {
        //public static ManualResetEvent allDone = new ManualResetEvent(false);
        private int m_maxConnnections;
        private int m_maxSize;
        private SocketAsyncEventArgsPool<AsyncClientToken> m_readWritePool;
        private BufferManager m_bufferManager;

        private Semaphore m_maxNumberAcceptedClients;
        private ICommonLog LogTool;

        public AsyncSocketClientUDP(int _maxSize, int _maxConnnections, ICommonLog _logTool)
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
                asyncClientToken.Socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
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
            m_maxNumberAcceptedClients.WaitOne();
            AsyncClientToken asyncClientToken = m_readWritePool.PopOrNew();
            asyncClientToken.HandlerReturnData = handlerReturnData;
            asyncClientToken.IP = ipaddress;
            asyncClientToken.Port = port;
            asyncClientToken.MessageID = messageID;
            int sendcount = Encoding.UTF8.GetBytes(sendData, 0, sendData.Length, asyncClientToken.Buffer, 0);
            asyncClientToken.Datasize = sendcount;

            
            try
            {
                IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(asyncClientToken.IP), asyncClientToken.Port);
                asyncClientToken.Socket.BeginSendTo(asyncClientToken.Buffer, 0, asyncClientToken.Datasize, SocketFlags.None, ipe, new AsyncCallback(SendCallback), asyncClientToken);
            }
            catch(Exception ex)
            {
                CloseOneInstance(asyncClientToken);
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
                token.Socket.EndSendTo(ar);
                EndPoint ipe = new IPEndPoint(IPAddress.Parse(token.IP), token.Port);
                token.Socket.BeginReceiveFrom(token.Buffer, 0, m_maxSize, SocketFlags.None, ref ipe, new AsyncCallback(ReciveCallback), token);
            }
            catch(Exception ex)
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
                EndPoint ipe = new IPEndPoint(IPAddress.Parse(token.IP), token.Port);
                int recivecount = token.Socket.EndReceiveFrom(ar, ref ipe);
                if (recivecount>0)
                {
                    string sendData = token.HandlerReturnData(Encoding.UTF8.GetString(token.Buffer, 0, recivecount), false);
                    if (!string.IsNullOrEmpty(sendData))
                    {
                        token.Datasize = Encoding.UTF8.GetBytes(sendData, 0, sendData.Length, token.Buffer, 0);
                        token.Socket.BeginSendTo (token.Buffer, 0, token.Datasize, SocketFlags.None,ipe, new AsyncCallback(SendCallback), token);
                    }
                    else
                    {
                        CloseOneInstance(token);
                    }
                    return;
                }
                token.HandlerReturnData(token.MessageID,true);
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

        private void CloseOneInstance(AsyncClientToken token,bool IsError=true)
        {
            if (token!=null)
            {
                if(token.Socket!=null)
                {
                    token.Socket.Close();
                    token.Socket = null;
                }
                m_readWritePool.Push(token);
            }
            m_maxNumberAcceptedClients.Release();
        }
    }

}
