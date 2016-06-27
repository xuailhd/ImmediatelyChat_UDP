using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.Model;
using Xugl.ImmediatelyChat.Model.QueryCondition;

namespace Xugl.ImmediatelyChat.IServices
{
    public interface IWarehouseService
    {
        IList<Warehouse> LoadWarehouse(WarehouseQuery query);
    }
}
