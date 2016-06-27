using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.Core
{
    public interface ICacheManage
    {
        void AddCache<T>(string key, T value);

        T GetCache<T>(string name);
    }
}
