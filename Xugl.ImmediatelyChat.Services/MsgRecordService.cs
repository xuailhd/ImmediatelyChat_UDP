using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.IServices;
using Xugl.ImmediatelyChat.Model;
using Xugl.ImmediatelyChat.Model.QueryCondition;
using Xugl.ImmediatelyChat.Core;
using Xugl.ImmediatelyChat.Common;

namespace Xugl.ImmediatelyChat.Services
{
    public class MsgRecordService : IMsgRecordService
    {
        private readonly IRepository<MsgRecord> msgRecordRepository;
        private readonly IRepository<ContactGroupSub> contactGroupSubRepository;
        private readonly IDbContext context;

        public MsgRecordService(IRepository<MsgRecord> _msgRecordRepository, IRepository<ContactGroupSub> _contactGroupSubRepository
            , IDbContext _context)
        {
            msgRecordRepository = _msgRecordRepository;
            context = _context;
            contactGroupSubRepository = _contactGroupSubRepository;
        }

        public IList<MsgRecord> LoadMsgRecord(MsgRecordQuery query)
        {
            var tablequery = msgRecordRepository.Table;

            if(query.MsgRecordtime!=null)
            {
                tablequery = tablequery.Where(t => t.SendTime.CompareTo(query.MsgRecordtime)>0);
            }

            if(string.IsNullOrEmpty(query.MsgRecipientObjectID))
            {
                tablequery=tablequery.Where(t=>t.MsgRecipientObjectID==query.MsgRecipientObjectID);
            }

            tablequery = tablequery.OrderBy(t => t.SendTime);
            return tablequery.ToList();
        }

        public int BatchSave(IList<MsgRecord> msgRecords)
        {

            #region prevent duplicate data
            IList<string> msgids=new List<string>();

            for(int i=0;i<msgRecords.Count;i++)
            {
                msgids.Add(msgRecords[i].MsgID);
            }

            var query = from aa in msgRecordRepository.Table
                        join bb in msgids on aa.MsgID equals bb
                        select aa.MsgID;
            IList<string> tempids = query.ToList();
            #endregion


            for (int i = msgRecords.Count-1; i >= 0; i--)
            {
                if(tempids.Contains(msgRecords[i].MsgID))
                {
                    msgRecords.RemoveAt(i);
                }
            }

            return msgRecordRepository.BatchInsert(msgRecords);
        }


        public IList<string> GenerateRecordByGroup(MsgRecordModel msgRecordModel)
        {

            IList<string> recipientObjectIDs = new List<string>();

            IList<ContactGroupSub> contactGroupSubs = contactGroupSubRepository.Table.
                Where(t => t.ContactGroupID == msgRecordModel.MsgRecipientGroupID).ToList();

            foreach(ContactGroupSub contactGroupSub in contactGroupSubs)
            {
                recipientObjectIDs.Add(contactGroupSub.ContactPersonObjectID);
            }
            return recipientObjectIDs;
        }
    }
}
