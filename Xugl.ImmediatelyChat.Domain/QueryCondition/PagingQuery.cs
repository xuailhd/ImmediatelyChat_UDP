using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Xugl.ImmediatelyChat.Model.QueryCondition
{
    public class PagingQuery
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public bool IsGetRecordCount { get; set; }

        public PagingQuery():this(1,Convert.ToInt32(ConfigurationManager.AppSettings["DefaultPageSize"]),true)
        {

        }

        public PagingQuery(int page,int pagesize,bool isGetRecordCount)
        {
            Page = page;
            PageSize = pagesize;
            IsGetRecordCount = isGetRecordCount;
        }
    }

    public static class PagingQueryExtensions
    {
        public static int GetSkipCount(this PagingQuery pagingQuery)
        {
            return (pagingQuery.Page - 1) * pagingQuery.PageSize;
        }
    }
}
