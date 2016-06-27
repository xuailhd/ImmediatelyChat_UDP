using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.Core;
using Xugl.ImmediatelyChat.Model;
using Xugl.ImmediatelyChat.Model.QueryCondition;

namespace Xugl.ImmediatelyChat.IServices
{
    public interface IMsgRecordService
    {
        IList<MsgRecord> LoadMsgRecord(MsgRecordQuery query);

        int BatchSave(IList<MsgRecord> msgRecords);

    }
}
