using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.Model;

namespace Xugl.ImmediatelyChat.Data.EF.Mapping.MDS
{
    public class MsgRecordMapping : EntityTypeConfiguration<MsgRecord>
    {
        public MsgRecordMapping()
        {
            this.HasKey(t => t.MsgID);
        }
    }

}
