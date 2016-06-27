using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.Core
{
    public interface IOperateFile
    {
        void SaveConfig(string fileName, string fieldName, string fieldValue);

        string GetConfig(string fileName, string fieldName);

    }
}
