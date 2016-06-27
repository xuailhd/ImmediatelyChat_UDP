using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.Model.QueryCondition
{
    public class WarehouseQuery : OrderPagingQuery<Warehouse>
    {
        public WarehouseQuery()
        {
            this.OrderFieldStore.Add(t => t.WarehouseID);
            this.OrderFieldStore.Add(t => t.WarehouseCode);
        }

        public string WarehouseCode { get; set; }
    }

    public class MsgRecordQuery:OrderPagingQuery<MsgRecord>
    {
        public MsgRecordQuery()
        {
            this.OrderFieldStore.Add(t => t.MsgID);
        }

        public string MsgRecordtime { get; set; }
        public string MsgRecipientObjectID { get; set; }
        public int SendType { get; set; }
        
    }

    public class ContactGroupQuery : OrderPagingQuery<ContactGroup>
    {
        public ContactGroupQuery()
        {
            this.OrderFieldStore.Add(t => t.GroupObjectID);
        }
    }

    public class ContactGroupSubQuery : OrderPagingQuery<ContactGroupSub>
    {
        public ContactGroupSubQuery()
        {
            this.OrderFieldStore.Add(t => t.ContactGroupID);
        }
    }

    public class ContactPersonQuery : OrderPagingQuery<ContactPerson>
    {
        public ContactPersonQuery()
        {
            this.OrderFieldStore.Add(t => t.ObjectID);
        }
    }

    public class ContactPersonListQuery : OrderPagingQuery<ContactPersonList>
    {
        public ContactPersonListQuery()
        {
            this.OrderFieldStore.Add(t => t.ObjectID);
        }

        public string ContactPersonName { get; set; }
    } 
}
