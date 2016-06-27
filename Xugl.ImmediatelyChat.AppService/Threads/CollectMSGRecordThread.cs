using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.AppService.Interfaces;
using Xugl.ImmediatelyChat.Core.DependencyResolution;

namespace Xugl.ImmediatelyChat.AppService
{
    public class CollectRecordThread
    {
        private Socket clientSocket;

        public CollectRecordThread(Socket clientSocket)
        {
            this.clientSocket = clientSocket;
        }

        public void Start()
        {
            byte[] bytmsg = new byte[1024];
            int bytes = clientSocket.Receive(bytmsg);
            string strMsg = Encoding.UTF8.GetString(bytmsg, 0, bytes);

            if(strMsg=="stop")
            {
                return;
            }

            IHandleSendMsg  HandleSendMsg= ObjectContainerFactory.CurrentContainer.Resolver<IHandleSendMsg>();

            ParameterizedThreadStart  threadStart=new ParameterizedThreadStart(HandleSendMsg.Handler);
            Thread thread = new Thread(threadStart);
            thread.Start(strMsg);
        }
    }
}
