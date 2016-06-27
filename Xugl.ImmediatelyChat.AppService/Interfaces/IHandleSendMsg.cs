using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.Model;

namespace Xugl.ImmediatelyChat.AppService.Interfaces
{
    interface IHandleSendMsg
    {
        void Handler(object msgRecord);
    }
}
