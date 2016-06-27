using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.AppService.Interfaces;
using Xugl.Immediatelychat.Core.DependencyResolution;
using Xugl.Immediatelychat.AppService.Model;

namespace Xugl.ImmediatelyChat.AppService.Threads
{
    public class FindMSGRecordThread
    {
        private Socket clientsocket;
        private string objectID;

        public FindMSGRecordThread(Socket clientsocket, string objectID)
        {
            this.clientsocket = clientsocket;
            this.objectID = objectID;
        }

        public void Start()
        {
            IFindMsgRecord findMsgRecord = ObjectContainerFactory.CurrentContainer.Resolver<IFindMsgRecord>();
            findMsgRecord.findMsg();

        }
    }
}
