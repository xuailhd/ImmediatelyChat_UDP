using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.Model;

namespace Xugl.ImmediatelyChat.Data.EF.Mapping.MCS
{
    public class ContactGroupMapping : EntityTypeConfiguration<ContactGroup>
    {
        public ContactGroupMapping()
        {
            this.HasKey(t => t.GroupObjectID);
        }
    }

    public class ContactGroupSubMapping : EntityTypeConfiguration<ContactGroupSub>
    {
        public ContactGroupSubMapping()
        {
            //this.HasKey(t => t.ContactGroupID);
            this.HasKey(t => new { t.ContactGroupID, t.ContactPersonObjectID });
        }
    }

    public class ContactPersonMapping : EntityTypeConfiguration<ContactPerson>
    {
        public ContactPersonMapping()
        {
            this.HasKey(t => t.ObjectID);
        }
    }

    public class ContactPersonListMapping : EntityTypeConfiguration<ContactPersonList>
    {
        public ContactPersonListMapping()
        {
            this.HasKey(t => new { t.ObjectID, t.DestinationObjectID });
        }
    }
}
