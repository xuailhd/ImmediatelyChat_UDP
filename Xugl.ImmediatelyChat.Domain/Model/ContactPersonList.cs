using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.Model
{
    public class ContactPersonList
    {
        public string ContactPersonName { get; set; }

        public string ObjectID { get; set; }

        public string DestinationObjectID { get; set; }

        public bool IsDelete { get; set; }

        public string UpdateTime { get; set; }
    }
}
