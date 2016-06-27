using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.IServices;
using Xugl.ImmediatelyChat.Model;
using Xugl.ImmediatelyChat.Model.QueryCondition;
using Xugl.ImmediatelyChat.Model.ViewModel;
using Xugl.ImmediatelyChat.Core;
using Xugl.ImmediatelyChat.Core.DependencyResolution;

namespace Xugl.ImmediatelyChat.Services
{
    public class WarehouseService : IWarehouseService
    {
        public IList<Warehouse> LoadWarehouse(WarehouseQuery query)
        {
            IRepository<Warehouse> _repository=ObjectContainerFactory.CurrentContainer.Resolver<IRepository<Warehouse>>();
            var recordtable = _repository.Table;

            if(!string.IsNullOrEmpty(query.WarehouseCode))
            {
                recordtable = recordtable.Where(t => t.WarehouseCode == query.WarehouseCode);
            }

            return null;
        }
    }
}
