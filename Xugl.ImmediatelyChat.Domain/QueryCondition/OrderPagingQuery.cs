using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.Model.QueryCondition
{
    public class OrderPagingQuery<TModel>:PagingQuery where TModel:class
    {
        public OrderFieldStroe<TModel> OrderFieldStore{get;private set;}

        public OrderPagingQuery()
        {
            OrderFieldStore=new OrderFieldStroe<TModel>();
        }


        public OrderPagingQuery(PagingQuery pagingQuery):this()
        {
            if(pagingQuery!=null)
            {
                this.Page = pagingQuery.Page;
                this.PageSize = pagingQuery.PageSize;
                this.IsGetRecordCount = pagingQuery.IsGetRecordCount;
            }
        }

    }
}
