using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Xugl.ImmediatelyChat.Core.DependencyResolution;

namespace Xugl.ImmediatelyChat.Site
{
    public class StructureMapControllerFactory : DefaultControllerFactory
    {
        protected override IController GetControllerInstance(System.Web.Routing.RequestContext requestContext, Type controllerType)
        {
            if (controllerType == null) return null;

            try
            {
                return ObjectContainerFactory.CurrentContainer.Resolver(controllerType) as IController;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
