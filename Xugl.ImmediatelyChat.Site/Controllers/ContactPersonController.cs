using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Xugl.ImmediatelyChat.Core;
using Xugl.ImmediatelyChat.Model;
using Xugl.ImmediatelyChat.Site.Models;
using Xugl.ImmediatelyChat.Core.DependencyResolution;
using Xugl.ImmediatelyChat.Core;
using System.Web.Script.Serialization;
using Xugl.ImmediatelyChat.SocketEngine;
using Xugl.ImmediatelyChat.IServices;

namespace Xugl.ImmediatelyChat.Site.Controllers
{
    public class ContactPersonController : Controller
    {
        private ICacheManage cacheManage;
        private IContactPersonService contactPersonService;
        private IAppServerService appServerService;

        public ContactPersonController(IContactPersonService contactPersonService, IAppServerService appServerService,
            ICacheManage cacheManage)
        {
            this.contactPersonService = contactPersonService;
            this.appServerService = appServerService;
            this.cacheManage = cacheManage;
        }


        public JsonResult Register(string ObjectName, string Password)
        {
            bool finishTag = true;

            ContactPerson contactPerson = new ContactPerson();
            contactPerson.ObjectID = Guid.NewGuid().ToString();
            contactPerson.ContactName = ObjectName;
            contactPerson.Password = Password;
            if (Singleton<SyncSocketClient>.Instance == null)
            {
                Singleton<SyncSocketClient>.Instance = new SyncSocketClient();
            }

            if (Singleton<JavaScriptSerializer>.Instance == null)
            {
                Singleton<JavaScriptSerializer>.Instance = new JavaScriptSerializer();
            }


            for (int i = 0; i < cacheManage.GetCache<IList<MMSServer>>("MMSServers").Count; i++)
            {
                string returnstr = Singleton<SyncSocketClient>.Instance.SendMsg(cacheManage.GetCache<IList<MMSServer>>("MMSServers")[i].MMS_IP,
                    cacheManage.GetCache<IList<MMSServer>>("MMSServers")[i].MMS_Port,
                    Common.CommonFlag.F_PSSendMMSUser + Singleton<JavaScriptSerializer>.Instance.Serialize(contactPerson));

                if (returnstr != contactPerson.ObjectID)
                {
                    finishTag = false;
                }
            }

            if (finishTag)
            {
                if (contactPersonService.InsertNewPerson(contactPerson) > 0)
                {
                    return Json("register success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("register failed", JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoginForAPI(string ObjectName,string Password)
        {
            if (string.IsNullOrEmpty(ObjectName))
            {
                return null;
            }

            LoginReturnContext loginReturnContext = new LoginReturnContext();
            MMSServer mmsServer=null;

            if (cacheManage.GetCache<IList<MMSServer>>("MMSServers") == null || cacheManage.GetCache<IList<MMSServer>>("MMSServers").Count<=0)
            {
                IList<MMSServer> mmsServers = appServerService.FindMMS();
                if (mmsServers != null && mmsServers.Count>0)
                {
                    cacheManage.AddCache<IList<MMSServer>>("MMSServers", mmsServers);
                }
                else
                {
                    loginReturnContext.Status = 2;
                    return Json(loginReturnContext, JsonRequestBehavior.AllowGet);
                }
            }

            ContactPerson contactPerson = contactPersonService.FindContactPerson(t=>t.ContactName==ObjectName);
            if (contactPerson == null)
            {
                loginReturnContext.Status = 3;
            }
            else
            {
                if (contactPerson.Password != Password)
                {
                    loginReturnContext.Status = 1;
                    
                }
                else
                {
                    loginReturnContext.ObjectID = contactPerson.ObjectID;
                    mmsServer = FindMMS(contactPerson.ObjectID);
                    loginReturnContext.Status = 0;
                }
            }

            if (mmsServer!=null)
            {
                loginReturnContext.IP = mmsServer.MMS_IP;
                loginReturnContext.Port = mmsServer.MMS_Port;
                loginReturnContext.Status = 0;
            }

            return Json(loginReturnContext, JsonRequestBehavior.AllowGet);
        }


        private MMSServer FindMMS(string objectID)
        {
            MMSServer mmsServer = null;

            IList<MMSServer> mmsServers = cacheManage.GetCache<IList<MMSServer>>("MMSServers");
            foreach(MMSServer tempMMSServer in mmsServers)
            {
                if(tempMMSServer.ArrangeStr.Contains(objectID.Substring(0, 1)))
                {
                    return tempMMSServer;
                }
            }
            return mmsServer;
        }

    }
}
