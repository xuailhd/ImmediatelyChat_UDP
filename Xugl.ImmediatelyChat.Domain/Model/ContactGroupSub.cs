using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.Model
{
    public class ContactGroupSub
    {
        public string ContactPersonObjectID { get; set; }

        public string ContactGroupID { get; set; }

        public bool IsDelete { get; set; }

        public string UpdateTime { get; set; }
    }
}
