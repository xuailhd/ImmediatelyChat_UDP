using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.Model.ViewModel
{
    public class PagingModel
    {
        public int PageSize { get; set; }

        public int PageIndex { get; set; }

        public int RecordCount { get; set; }

        public PagingModel(int pageIndex,int pageSize,int recordCount=0)
        {
            PageSize = pageSize;
            PageIndex = pageIndex;
            RecordCount = recordCount;
        }
    }
}
