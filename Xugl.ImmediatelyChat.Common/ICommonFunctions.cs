using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.Model;

namespace Xugl.ImmediatelyChat.Common
{
    public interface ICommonFunctions
    {
        MCSServer FindMCSServer(IList<MCSServer> servers, String objectID);

        MMSServer FindMMSServer(IList<MMSServer> servers, String objectID);

        MDSServer FindMMSServer(IList<MDSServer> servers, String objectID);
    }
}
