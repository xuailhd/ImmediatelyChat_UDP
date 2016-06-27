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
using Xugl.ImmediatelyChat.Model;

namespace Xugl.ImmediatelyChat.SocketEngine
{
    public abstract class AsyncSocketListenerUDP<T> where T : AsyncUserToken, new()
    {
        private int m_maxConnnections;
        private int m_maxSize;
        const int opsToPreAlloc = 2;
        private SocketAsyncEventArgsPool<SocketAsyncEventArgs> m_readWritePool;
        private Socket mainServiceSocket;
        private BufferManager m_bufferManager;

        private Semaphore m_maxNumberAcceptedClients;
        private ICommonLog LogTool;

        public AsyncSocketListenerUDP(int _maxSize, int _maxConnnections, ICommonLog _logTool)
        {
            m_readWritePool = new SocketAsyncEventArgsPool<SocketAsyncEventArgs>();
            m_maxConnnections = _maxConnnections;
            m_maxSize = _maxSize;
            LogTool = _logTool;

            m_maxNumberAcceptedClients = new Semaphore(m_maxConnnections, m_maxConnnections);
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.ReceiveFrom:
                    ProcessReceive(e);
                    //StartReceive();
                    break;
                case SocketAsyncOperation.SendTo:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
            
        }


        protected void BeginService(string ipaddress,int port)
        {
            IPAddress ip = IPAddress.Parse(ipaddress);
            IPEndPoint ipe = new IPEndPoint(ip, port);

            mainServiceSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            mainServiceSocket.Bind(ipe);
            //mainServiceSocket.Listen(m_maxConnnections);

            //m_bufferManager = BufferManager.CreateBufferManager(m_maxConnnections * m_maxSize * opsToPreAlloc, m_maxSize);
            m_bufferManager = BufferManager.CreateBufferManager(m_maxConnnections * m_maxSize, m_maxSize);

            for (int i = 0; i < m_maxConnnections; i++)
            {
                SocketAsyncEventArgs socketAsyncEventArg = new SocketAsyncEventArgs();
                socketAsyncEventArg.UserToken = new T();
                socketAsyncEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                socketAsyncEventArg.RemoteEndPoint = ipe;
                socketAsyncEventArg.SetBuffer(m_bufferManager.TakeBuffer(m_maxSize), 0, m_maxSize);
                m_readWritePool.Push(socketAsyncEventArg);
            }
            
            StartReceive();
        }


        private void StartReceive()
        {
            m_maxNumberAcceptedClients.WaitOne();
            SocketAsyncEventArgs e = m_readWritePool.Pop();
            bool willRaiseEvent = mainServiceSocket.ReceiveFromAsync(e);
            if (!willRaiseEvent)
            {
                ProcessReceive(e);
            }
            StartReceive();
        }

        protected abstract string HandleRecivedMessage(string inputMessage, T token);

        protected abstract void HandleError(T token);

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            T token = (T)e.UserToken;
            try
            {
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    LogTool.Log("Ip:" + ((IPEndPoint)e.RemoteEndPoint).Address + " port:" + ((IPEndPoint)e.RemoteEndPoint).Port);

                    string data = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                    string returndata = HandleRecivedMessage(data, token);
                    if (string.IsNullOrEmpty(returndata))
                    {
                        CloseClientSocket(e);
                        return;
                    }
                    
                    int bytecount = Encoding.UTF8.GetBytes(returndata, 0, returndata.Length, e.Buffer,0);

                    e.SetBuffer(0,bytecount);
                    bool willRaiseEvent = mainServiceSocket.SendToAsync(e);
                    if (!willRaiseEvent)
                    {
                        ProcessSend(e);
                    }
                }
                else
                {
                    HandleError(token);
                    CloseClientSocket(e);
                }
            }
            catch (Exception ex)
            {
                LogTool.Log(ex.Message + ex.StackTrace);
                HandleError(token);
                CloseClientSocket(e);
            }
        }


        // This method is invoked when an asynchronous send operation completes.  
        // The method issues another receive on the socket to read any additional 
        // data sent from the client
        //
        // <param name="e"></param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            T token = (T)e.UserToken;
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    e.SetBuffer(0, m_maxSize);
                    bool willRaiseEvent = mainServiceSocket.ReceiveFromAsync(e);
                    if (!willRaiseEvent)
                    {
                        ProcessReceive(e);
                    }
                    //CloseClientSocket(e);
                }
                else
                {
                    HandleError(token);
                    CloseClientSocket(e);
                }
            }
            catch (Exception ex)
            {
                LogTool.Log(ex.Message + ex.StackTrace);
                HandleError(token);
                CloseClientSocket(e);
            }
        }

        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            m_readWritePool.Push(e);
            m_maxNumberAcceptedClients.Release();
        }

        public void CloseListener()
        {
            mainServiceSocket.Close();
            mainServiceSocket = null;
        }
    }
}
