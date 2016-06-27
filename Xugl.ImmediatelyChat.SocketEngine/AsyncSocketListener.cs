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
    public abstract class AsyncSocketListener<T> where T : AsyncUserToken, new()
    {
        private int m_maxConnnections;
        private int m_maxSize;
        const int opsToPreAlloc = 2;
        private SocketAsyncEventArgsPool<SocketAsyncEventArgs> m_readWritePool;
        private Socket mainServiceSocket;
        private BufferManager m_bufferManager;

        private Semaphore m_maxNumberAcceptedClients;
        private ICommonLog LogTool;

        public AsyncSocketListener(int _maxSize, int _maxConnnections, ICommonLog _logTool)
        {
            m_readWritePool = new SocketAsyncEventArgsPool<SocketAsyncEventArgs>();
            m_maxConnnections = _maxConnnections;
            m_maxSize = _maxSize;
            LogTool = _logTool;

            //m_bufferManager = BufferManager.CreateBufferManager(m_maxConnnections * m_maxSize * opsToPreAlloc, m_maxSize);
            m_bufferManager = BufferManager.CreateBufferManager(m_maxConnnections * m_maxSize, m_maxSize);

            for (int i = 0; i < m_maxConnnections; i++)
            {
                SocketAsyncEventArgs socketAsyncEventArg = new SocketAsyncEventArgs();
                socketAsyncEventArg.UserToken = new T();
                socketAsyncEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                socketAsyncEventArg.SetBuffer(m_bufferManager.TakeBuffer(m_maxSize), 0, m_maxSize);
                m_readWritePool.Push(socketAsyncEventArg);
            }

            m_maxNumberAcceptedClients = new Semaphore(m_maxConnnections, m_maxConnnections);
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }

        }


        protected void BeginService(string ipaddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipaddress);
            IPEndPoint ipe = new IPEndPoint(ip, port);

            mainServiceSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mainServiceSocket.Bind(ipe);
            mainServiceSocket.Listen(m_maxConnnections);

            StartAccept(null);
        }


        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                acceptEventArg.AcceptSocket = null;
            }
            m_maxNumberAcceptedClients.WaitOne();
            bool willRaiseEvent = mainServiceSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArg);
            }
        }


        private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }


        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            //Interlocked.Increment(ref m_numConnectedSockets);
            //Console.WriteLine("Client connection accepted. There are {0} clients connected to the server",
            //    m_numConnectedSockets);

            // Get the socket for the accepted client connection and put it into the 
            //ReadEventArg object user token
            try
            {
                SocketAsyncEventArgs readEventArgs = m_readWritePool.Pop();
                ((T)readEventArgs.UserToken).Socket = e.AcceptSocket;

                // As soon as the client is connected, post a receive to the connection
                readEventArgs.SetBuffer(0, m_maxSize);
                bool willRaiseEvent = e.AcceptSocket.ReceiveAsync(readEventArgs);
                if (!willRaiseEvent)
                {
                    ProcessReceive(readEventArgs);
                }
                // Accept the next connection request
                StartAccept(e);
            }
            catch (Exception ex)
            {
                LogTool.Log(ex.Message + ex.StackTrace);
                CloseClientSocket(e);
            }
        }

        protected abstract string HandleRecivedMessage(string inputMessage, T token);

        protected abstract void HandleError(T token);

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            T token = (T)e.UserToken;
            try
            {
                // check if the remote host closed the connection
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    //increment the count of the total bytes receive by the server
                    //Interlocked.Add(ref m_totalBytesRead, e.BytesTransferred);
                    //Console.WriteLine("The server has read a total of {0} bytes", m_totalBytesRead);

                    string data = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);

                    string returndata = HandleRecivedMessage(data, token);

                    if (string.IsNullOrEmpty(returndata))
                    {
                        CloseClientSocket(e);
                        return;
                    }

                    int bytecount = Encoding.UTF8.GetBytes(returndata, 0, returndata.Length, e.Buffer, 0);

                    e.SetBuffer(0, bytecount);
                    bool willRaiseEvent = token.Socket.SendAsync(e);
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
                if (ex.InnerException != null)
                {
                    LogTool.Log(ex.InnerException.Message);
                    if (ex.InnerException.InnerException != null)
                    {
                        LogTool.Log(ex.InnerException.InnerException.Message);
                        if (ex.InnerException.InnerException.InnerException != null)
                        {
                            LogTool.Log(ex.InnerException.InnerException.InnerException.Message);
                        }
                    }
                }
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
                    bool willRaiseEvent = token.Socket.ReceiveAsync(e);
                    if (!willRaiseEvent)
                    {
                        ProcessReceive(e);
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


        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = e.UserToken as AsyncUserToken;

            // close the socket associated with the client
            try
            {
                token.Socket.Shutdown(SocketShutdown.Send);
            }
            // throws if client process has already closed
            catch (Exception ex)
            {
                LogTool.Log("close socket error: " + ex.Message);
            }
            token.Socket.Close();

            // decrement the counter keeping track of the total number of clients connected to the server
            //Interlocked.Decrement(ref m_numConnectedSockets);

            //Console.WriteLine("A client has been disconnected from the server. There are {0} clients connected to the server", m_numConnectedSockets);

            // Free the SocketAsyncEventArg so they can be reused by another client
            m_readWritePool.Push(e);
            m_maxNumberAcceptedClients.Release();
        }
    }
}
