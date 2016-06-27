using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.AppService.Interfaces;

namespace Xugl.ImmediatelyChat.AppService.Implements
{
    public class PortManage:IPortManage
    {
        private Dictionary<int, bool> _ports = new Dictionary<int, bool>();

        public PortManage()
        {
            for(int i=0;i<1000;i++)
            {
                _ports.Add(i + 30000, false);
            }
        }

        public int GetNewPort()
        {
            int tempPort=0;
            foreach(var index in _ports.Keys)
            {
                if (!_ports[index])
                {
                    tempPort = index;
                    break;
                }
            }
            _ports[tempPort] = true;
            return tempPort;
        }

        public void ReleasePort(int port)
        {
            _ports[port] = false;
        }
    }
}
