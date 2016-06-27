using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Xugl.ImmediatelyChat.Model;
using Xugl.ImmediatelyChat.Core.DependencyResolution;
using Xugl.ImmediatelyChat.Core;
using Xugl.ImmediatelyChat.Data.EF;
using Xugl.ImmediatelyChat.IServices;
using Xugl.ImmediatelyChat.Model.QueryCondition;
using IBM.Woox.MvcTest.Filters;

namespace Xugl.ImmediatelyChat.Site.Controllers
{
    public class WarehouseController : Controller
    {
        //
        // GET: /Warehouse/
        [InitializeSimpleMembership]
        public ActionResult Index()
        {
            IWarehouseService warehouseService = ObjectContainerFactory.CurrentContainer.Resolver<IWarehouseService>();
            //repository.Delete(repository.Find(2));
            WarehouseQuery query = new WarehouseQuery();
            query.WarehouseCode = "001";
            return View(warehouseService.LoadWarehouse(query)[0]);
        }

    }
}
