using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.Common;

namespace Xugl.ImmediatelyChat.SocketEngine
{
    interface IAsyncSocketListener
    {
        void InitSocketListener(int _maxSize, int _maxConnnections, ICommonLog _logTool);
    }
}
