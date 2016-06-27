using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.Common
{
    public interface ICommonLog
    {
        void Log(string msg);
        string GetLogMsg { get; }
    }
}
