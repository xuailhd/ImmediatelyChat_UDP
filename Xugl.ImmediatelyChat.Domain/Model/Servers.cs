using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.Model
{
    public class MMSServer
    {
        public string MMS_IP { get; set; }
        public int MMS_Port { get; set; }
        public string ArrangeStr { get; set; }
    }

    public class MCSServer
    {
        public string MCS_IP { get; set; }
        public int MCS_Port { get; set; }
        public string ArrangeStr { get; set; }
    }

    public class MDSServer
    {
        public string MDS_IP { get; set; }
        public int MDS_Port { get; set; }
        public string ArrangeStr { get; set; }
    }
}
