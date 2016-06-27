using StructureMap.Configuration.DSL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.Core;

namespace Xugl.ImmediatelyChat.Data.EF
{
    public class DbRegistry : Registry
    {
        public DbRegistry()
        {
            this.For<IDbContext>().Use<DefaultDBContext>().Transient();
            this.For(typeof(IRepository<>)).Use(typeof(Repository<>)).Transient();
            //this.For<IDbContext>().Use<DefaultDBContext>();
            //this.For(typeof(IRepository<>)).Use(typeof(Repository<>));
        }
    }
}
