using Xugl.ImmediatelyChat.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugl.ImmediatelyChat.Data.EF.Mapping.Site
{
    public class MMSServerMapping : EntityTypeConfiguration<MMSServer>
    {
        public MMSServerMapping()
        {
            this.HasKey(t => new { t.MMS_IP, t.MMS_Port });

            this.Property(t => t.MMS_Port).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.ArrangeStr).HasMaxLength(255);
            this.Property(t => t.MMS_IP).HasMaxLength(50);
        }
    }


    public class ContactPersonMapping : EntityTypeConfiguration<ContactPerson>
    {
        public ContactPersonMapping()
        {
            this.HasKey(t => t.ObjectID);
            this.Property(t => t.ContactName).HasMaxLength(50);
            this.Property(t => t.ImageSrc).HasMaxLength(255);
            this.Property(t => t.LatestTime).HasMaxLength(50);
            this.Property(t => t.ObjectID).HasMaxLength(50);
            this.Property(t => t.Password).HasMaxLength(50);
            this.Property(t => t.UpdateTime).HasMaxLength(50);
        }
    }
}
