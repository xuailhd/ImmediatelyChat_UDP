using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.Core;
using Xugl.ImmediatelyChat.IServices;
using Xugl.ImmediatelyChat.Model;

namespace Xugl.ImmediatelyChat.Services
{
    public class AppServerService : IAppServerService
    {
        private readonly IRepository<MMSServer> mmsServerRepository;
        private readonly IDbContext dbContext;

        public AppServerService(IRepository<MMSServer> mmsServerRepository, IDbContext dbContext)
        {
            this.mmsServerRepository = mmsServerRepository;
            this.dbContext = dbContext;
        }

        public int BatchInsert(IList<Model.MMSServer> entitys)
        {
            return mmsServerRepository.BatchInsert(entitys);
        }

        public int Clean()
        {
            return mmsServerRepository.BatchDelete(mmsServerRepository.Table.ToList());
        }


        public IList<MMSServer> FindMMS()
        {
            return mmsServerRepository.Table.ToList();
        }
    }
}
