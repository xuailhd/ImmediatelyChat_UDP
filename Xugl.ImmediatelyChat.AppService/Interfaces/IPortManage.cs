using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.AppService.Interfaces
{
    interface IPortManage
    {
        int GetNewPort();

        void ReleasePort(int port);
    }
}
