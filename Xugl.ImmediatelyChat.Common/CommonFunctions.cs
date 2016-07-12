using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.Model;

namespace Xugl.ImmediatelyChat.Common
{
    public class CommonFunctions : ICommonFunctions
    {
        public MCSServer FindMCSServer(IList<MCSServer> servers, String objectID)
        {
            MCSServer server = null;

            foreach (MCSServer tempserver in servers)
            {
                if (tempserver.ArrangeStr.Contains(objectID.Substring(0, 1)))
                {
                    return tempserver;
                }
            }
            return server;
        }

        public MMSServer FindMMSServer(IList<MMSServer> servers, String objectID)
        {
            MMSServer server = null;

            foreach (MMSServer tempserver in servers)
            {
                if (tempserver.ArrangeStr.Contains(objectID.Substring(0, 1)))
                {
                    return tempserver;
                }
            }
            return server;
        }

        public MDSServer FindMMSServer(IList<MDSServer> servers, String objectID)
        {
            MDSServer server = null;

            foreach (MDSServer tempserver in servers)
            {
                if (tempserver.ArrangeStr.Contains(objectID.Substring(0, 1)))
                {
                    return tempserver;
                }
            }
            return server;
        }
    }
}
