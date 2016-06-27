using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.AppService.Common;
using Xugl.ImmediatelyChat.AppService.Model;
using Xugl.ImmediatelyChat.Model;
using Xugl.ImmediatelyChat.IServices;
using Xugl.ImmediatelyChat.Core;
using Xugl.ImmediatelyChat.Core.DependencyResolution;
using Xugl.ImmediatelyChat.Model.QueryCondition;

namespace Xugl.ImmediatelyChat.AppService.Container
{
    public class MsgContainerProxy
    {
        private IList<MsgContainer>_InMsgContainers;

        private OutMsgContainer _OutMsgContainer;

        public MsgContainerProxy()
        {
            _InMsgContainers = new List<MsgContainer>();
            for (int i = 0; i < CommonVariables.MsgContainerCount; i++)
            {
                MsgContainer tempMsgContainer = new MsgContainer();
                _InMsgContainers.Add(tempMsgContainer);
            }

            _OutMsgContainer = new OutMsgContainer();
        }

        public void AddMsg(MsgRecord msgRecord)
        {
            int i=0;
            while(i<CommonVariables.MsgContainerCount)
            {
                if (!_InMsgContainers[i].IsFull && !_InMsgContainers[i].Islocked)
                {
                    _InMsgContainers[i].AddMsgRecord(msgRecord);
                }
                i++;
                if(i==CommonVariables.MsgContainerCount)
                {
                    Thread.Sleep(100);
                    i = 0;
                }
            }
        }

        public IList<MsgRecord> GetMsg(GetMsgModel getMsgModel)
        {

            getMsgModel.EndTime = DateTime.Now;

            return _OutMsgContainer.getMsgRecord(getMsgModel);
        }
    }

    internal class OutMsgContainer:MsgContainer
    {
        public OutMsgContainer()
        {
            Thread thread = new Thread();

        }

        private int _MsgRecordCount;
        private DateTime saveTime;

        public IList<MsgRecord> getMsgRecord(GetMsgModel getMsgModel)
        {
            var msgQuery = from msgContainer in base._MsgContainer
                           join groupids in getMsgModel.GroupIDs on msgContainer.MsgRecipientObjectID equals groupids into tempgroupids
                           from groupids in getMsgModel.GroupIDs.DefaultIfEmpty()
                           where (msgContainer.MsgRecipientObjectID == getMsgModel.ObjectID || (groupids != null && msgContainer.SendType == 1))
                           && (DateTime.Compare(msgContainer.SendTime, getMsgModel.EndTime) < 0 && DateTime.Compare(msgContainer.SendTime, getMsgModel.StartTime) > 0)
                           select msgContainer;

            if(DateTime.Compare(getMsgModel.StartTime,saveTime)<0)
            {
                IMsgRecordService msgRecordService=ObjectContainerFactory.CurrentContainer.Resolver<IMsgRecordService>();
                MsgRecordQuery msgRecordQuery=new MsgRecordQuery();
                msgRecordQuery.
                msgRecordService.LoadMsgRecord(MsgRecordQuery)

            }

            return msgQuery.ToList();
        }

        public override void AddMsgRecord(MsgRecord msgRecord)
        {
            base.AddMsgRecord(msgRecord);
            _MsgRecordCount++;
            if(_MsgRecordCount==CommonVariables.MsgOutContainerCount)
            {
                saveTime = msgRecord.SendTime;
            }
        }


        public void SaveRecord()
        {
            var msgQuery = from msgContainer in base._MsgContainer
                           where DateTime.Compare(msgContainer.SendTime,saveTime)<=0
                           select msgContainer;

            IRepository<MsgRecord> msgRecordRepository = ObjectContainerFactory.CurrentContainer.Resolver<IRepository<MsgRecord>>();
            msgRecordRepository.BatchInsert(msgQuery.ToList());

            var msgQuery2 = from msgContainer in base._MsgContainer
                           where DateTime.Compare(msgContainer.SendTime, saveTime) > 0
                           select msgContainer;
            base._MsgContainer = msgQuery2.ToList();


        }
    }

    internal class MsgContainer
    {
        protected IList<MsgRecord> _MsgContainer=new List<MsgRecord>();

        private bool _islocked;

        private bool _isFull;

             
        public void AddMsgRecord(MsgRecord msgRecord)
        {
            lock (_MsgContainer)
            {
                _islocked = true;
                _MsgContainer.Add(msgRecord);

                if (_MsgContainer.Count == CommonVariables.MsgContainerFullCount)
                {
                    _isFull = true;
                }
            }
            _islocked = false;
        }


        public IList<MsgRecord> OutPutRecord()
        {
            IList<MsgRecord> tempMsgContainer = new List<MsgRecord>();

            int tempCount=0;

            lock (_MsgContainer)
            {
                tempCount = _MsgContainer.Count;
            }

            for(int i=0;i<tempCount;i++)
            {
                tempMsgContainer.Add(_MsgContainer[i]);
            }

            return tempMsgContainer;
        }

        public void CleanRecords()
        {
            lock (_MsgContainer)
            {
                _MsgContainer.Clear();
            }
        }

        public bool IsFull
        {
            get{ return _isFull;}
        }

        public bool Islocked
        {
            get { return _islocked; }
        }
    }
}
