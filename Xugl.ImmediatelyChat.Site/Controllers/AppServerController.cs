using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Xugl.ImmediatelyChat.Site.Models;
using Xugl.ImmediatelyChat.Core;
using Xugl.ImmediatelyChat.Model;
using System.Web.Script.Serialization;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Xugl.ImmediatelyChat.Common;
using Xugl.ImmediatelyChat.IServices;
using System.IO;
using Xugl.ImmediatelyChat.SocketEngine;

namespace Xugl.ImmediatelyChat.Site.Controllers
{
    public class AppServerController : Controller
    {
        private readonly ICacheManage cacheManage;
        private readonly IAppServerService appServerService;
        public AppServerController(ICacheManage cacheManage, IAppServerService appServerService)
        {
            this.cacheManage = cacheManage;
            this.appServerService = appServerService;
        }

        public FileStreamResult DownLoadApk()
        {
            string absoluFilePath = Server.MapPath("~/ImmediatelyChat.apk");
            return File(new FileStream(absoluFilePath, FileMode.Open), "application/octet-stream", Server.UrlEncode("ImmediatelyChat.apk"));
        }

        //
        // GET: /AppServer/

        public ActionResult CollectMMS(string ip,int port, string arrangeStr)
        {
            MMSServer mmsServer = new MMSServer();
            mmsServer.ArrangeStr = arrangeStr;
            mmsServer.MMS_IP = ip;
            mmsServer.MMS_Port = port;
            mmsServer.MMS_IP = System.Configuration.ConfigurationManager.AppSettings["LocalIP"].ToString();

            lock (CommonFlag.lockobject)
            {
                if (cacheManage.GetCache<IList<MMSServer>>("MMSServers") == null)
                {
                    IList<MMSServer> mmsServers = new List<MMSServer>();
                    mmsServers.Add(mmsServer);
                    cacheManage.AddCache<IList<MMSServer>>("MMSServers", mmsServers);

                }
                else
                {
                    if (cacheManage.GetCache<IList<MMSServer>>("MMSServers").Count > 0)
                    {
                        for (int i = cacheManage.GetCache<IList<MMSServer>>("MMSServers").Count - 1; i >= 0; i--)
                        {
                            if (cacheManage.GetCache<IList<MMSServer>>("MMSServers")[i].MMS_IP == mmsServer.MMS_IP
                                && cacheManage.GetCache<IList<MMSServer>>("MMSServers")[i].MMS_Port == mmsServer.MMS_Port)
                            {
                                cacheManage.GetCache<IList<MMSServer>>("MMSServers").RemoveAt(i);
                            }
                        }
                    }
                    cacheManage.GetCache<IList<MMSServer>>("MMSServers").Add(mmsServer);
                }
            }

            return Json("ok",JsonRequestBehavior.AllowGet);
        }


        public ActionResult CollectMCS(string ip, int port, string arrangeStr)
        {
            MCSServer mcsServer = new MCSServer();
            mcsServer.ArrangeStr = arrangeStr;
            mcsServer.MCS_IP = ip;
            mcsServer.MCS_Port = port;
            mcsServer.MCS_IP = System.Configuration.ConfigurationManager.AppSettings["LocalIP"].ToString();
            lock (CommonFlag.lockobject)
            {
                if (cacheManage.GetCache<IList<MCSServer>>("MCSServers") == null)
                {
                    IList<MCSServer> mcsServers = new List<MCSServer>();
                    mcsServers.Add(mcsServer);
                    cacheManage.AddCache<IList<MCSServer>>("MCSServers", mcsServers);

                }
                else
                {
                    if (cacheManage.GetCache<IList<MCSServer>>("MCSServers").Count > 0)
                    {
                        for (int i = cacheManage.GetCache<IList<MCSServer>>("MCSServers").Count - 1; i >= 0; i--)
                        {
                            if (cacheManage.GetCache<IList<MCSServer>>("MCSServers")[i].MCS_IP == mcsServer.MCS_IP
                                && cacheManage.GetCache<IList<MCSServer>>("MCSServers")[i].MCS_Port == mcsServer.MCS_Port)
                            {
                                cacheManage.GetCache<IList<MCSServer>>("MCSServers").RemoveAt(i);
                            }
                        }
                    }
                    cacheManage.GetCache<IList<MCSServer>>("MCSServers").Add(mcsServer);
                }
            }
            return Json("ok", JsonRequestBehavior.AllowGet);
        }

        public ActionResult CollectMDS(string ip, int port, string arrangeStr)
        {
            MDSServer mdsServer = new MDSServer();
            mdsServer.ArrangeStr = arrangeStr;
            mdsServer.MDS_IP = ip;
            mdsServer.MDS_Port = port;
            mdsServer.MDS_IP = System.Configuration.ConfigurationManager.AppSettings["LocalIP"].ToString();
            lock (CommonFlag.lockobject)
            {
                if (cacheManage.GetCache<IList<MDSServer>>("MDSServers") == null)
                {
                    IList<MDSServer> mdsServers = new List<MDSServer>();
                    mdsServers.Add(mdsServer);
                    cacheManage.AddCache<IList<MDSServer>>("MDSServers", mdsServers);

                }
                else
                {
                    if (cacheManage.GetCache<IList<MDSServer>>("MDSServers").Count > 0)
                    {
                        for (int i = cacheManage.GetCache<IList<MDSServer>>("MDSServers").Count - 1; i >= 0; i--)
                        {
                            if (cacheManage.GetCache<IList<MDSServer>>("MDSServers")[i].MDS_IP == mdsServer.MDS_IP
                                && cacheManage.GetCache<IList<MDSServer>>("MDSServers")[i].MDS_Port == mdsServer.MDS_Port)
                            {
                                cacheManage.GetCache<IList<MDSServer>>("MDSServers").RemoveAt(i);
                            }
                        }
                    }

                    cacheManage.GetCache<IList<MDSServer>>("MDSServers").Add(mdsServer);
                }
            }
            return Json("ok", JsonRequestBehavior.AllowGet);
        }


        public ActionResult StartServer(string password)
        {
            string returnstr=ArrangeChar();

            if(!string.IsNullOrEmpty(returnstr))
            {
                ViewData["error"] = returnstr;
                return View("StartFailed");
            }
            returnstr = SendStartCommand();
            if(!string.IsNullOrEmpty(returnstr))
            {
                ViewData["error"] = returnstr;
                return View("StartFailed", (object)returnstr);
            }

            ServersModel serversModel = new ServersModel();
            serversModel.MMSServers = cacheManage.GetCache<IList<MMSServer>>("MMSServers");
            serversModel.MCSServers = cacheManage.GetCache<IList<MCSServer>>("MCSServers");
            serversModel.MDSServers = cacheManage.GetCache<IList<MDSServer>>("MDSServers");

            return View("FinishStart", serversModel);
        }


        private string SendStartCommand()
        {
            byte[] bytesSent;
            string tempStr = "";
            Socket tempSocket;

            try
            {
                IList<MMSServer> mmsServers = cacheManage.GetCache<IList<MMSServer>>("MMSServers");
                appServerService.Clean();
                if (appServerService.BatchInsert(mmsServers) > 0)
                {


                    if (Singleton<JavaScriptSerializer>.Instance == null)
                    {
                        Singleton<JavaScriptSerializer>.Instance = new JavaScriptSerializer();
                    }
                    tempStr = Singleton<JavaScriptSerializer>.Instance.Serialize(cacheManage.GetCache<IList<MCSServer>>("MCSServers"));

                    for (int i = 0; i < cacheManage.GetCache<IList<MMSServer>>("MMSServers").Count; i++)
                    {
                        bytesSent = Encoding.UTF8.GetBytes(CommonFlag.F_PSCallMMSStart + tempStr);
                        IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(cacheManage.GetCache<IList<MMSServer>>("MMSServers")[i].MMS_IP), cacheManage.GetCache<IList<MMSServer>>("MMSServers")[i].MMS_Port);
                        tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                        tempSocket.Connect(ipe);
                        tempSocket.Send(bytesSent, bytesSent.Length, 0);
                        tempSocket.Close();
                    }

                    tempStr = Singleton<JavaScriptSerializer>.Instance.Serialize(cacheManage.GetCache<IList<MDSServer>>("MDSServers"));
                    for (int i = 0; i < cacheManage.GetCache<IList<MCSServer>>("MCSServers").Count; i++)
                    {
                        bytesSent = Encoding.UTF8.GetBytes(CommonFlag.F_PSCallMCSStart + tempStr + "&&"
                              + Singleton<JavaScriptSerializer>.Instance.Serialize(cacheManage.GetCache<IList<MCSServer>>("MCSServers")[i]));
                        IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(cacheManage.GetCache<IList<MCSServer>>("MCSServers")[i].MCS_IP), cacheManage.GetCache<IList<MCSServer>>("MCSServers")[i].MCS_Port);
                        tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                        tempSocket.Connect(ipe);
                        tempSocket.Send(bytesSent, bytesSent.Length, 0);
                        tempSocket.Close();
                    }

                    tempStr = Singleton<JavaScriptSerializer>.Instance.Serialize(cacheManage.GetCache<IList<MCSServer>>("MCSServers"));
                    for (int i = 0; i < cacheManage.GetCache<IList<MDSServer>>("MDSServers").Count; i++)
                    {
                        bytesSent = Encoding.UTF8.GetBytes(CommonFlag.F_PSCallMDSStart + tempStr + "&&"
                            + Singleton<JavaScriptSerializer>.Instance.Serialize(cacheManage.GetCache<IList<MDSServer>>("MDSServers")[i]));
                        IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(cacheManage.GetCache<IList<MDSServer>>("MDSServers")[i].MDS_IP), cacheManage.GetCache<IList<MDSServer>>("MDSServers")[i].MDS_Port);
                        tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                        tempSocket.Connect(ipe);
                        tempSocket.Send(bytesSent, bytesSent.Length, 0);
                        tempSocket.Close();
                    }

                    return string.Empty;
                }
                return "save MMSs failed";
            }
            catch(Exception ex)
            {
                return ex.Message + ex.StackTrace;
            }
        }

        //private string SendStartCommandUDP()
        //{
        //    string tempStr = "";
        //    SyncSocketClientUDP syncSocketClientUDP = new SyncSocketClientUDP();
        //    try
        //    {
        //        IList<MMSServer> mmsServers = cacheManage.GetCache<IList<MMSServer>>("MMSServers");
        //        appServerService.Clean();
        //        if (appServerService.BatchInsert(mmsServers) > 0)
        //        {
        //            if (Singleton<JavaScriptSerializer>.Instance == null)
        //            {
        //                Singleton<JavaScriptSerializer>.Instance = new JavaScriptSerializer();
        //            }
        //            tempStr = Singleton<JavaScriptSerializer>.Instance.Serialize(cacheManage.GetCache<IList<MCSServer>>("MCSServers"));

        //            for (int i = 0; i < cacheManage.GetCache<IList<MMSServer>>("MMSServers").Count; i++)
        //            {
        //                syncSocketClientUDP.SendMsg(cacheManage.GetCache<IList<MMSServer>>("MMSServers")[i].MMS_IP,cacheManage.GetCache<IList<MMSServer>>("MMSServers")[i].MMS_Port,
        //                    CommonFlag.F_PSCallMMSStart + tempStr);
        //            }

        //            tempStr = Singleton<JavaScriptSerializer>.Instance.Serialize(cacheManage.GetCache<IList<MDSServer>>("MDSServers"));
        //            for (int i = 0; i < cacheManage.GetCache<IList<MCSServer>>("MCSServers").Count; i++)
        //            {
        //                syncSocketClientUDP.SendMsg(cacheManage.GetCache<IList<MCSServer>>("MCSServers")[i].MCS_IP, cacheManage.GetCache<IList<MCSServer>>("MCSServers")[i].MCS_Port,
        //                    CommonFlag.F_PSCallMCSStart + tempStr + "&&"
        //                      + Singleton<JavaScriptSerializer>.Instance.Serialize(cacheManage.GetCache<IList<MCSServer>>("MCSServers")[i]));
        //            }

        //            tempStr = Singleton<JavaScriptSerializer>.Instance.Serialize(cacheManage.GetCache<IList<MCSServer>>("MCSServers"));
        //            for (int i = 0; i < cacheManage.GetCache<IList<MDSServer>>("MDSServers").Count; i++)
        //            {
        //                syncSocketClientUDP.SendMsg(cacheManage.GetCache<IList<MDSServer>>("MDSServers")[i].MDS_IP, cacheManage.GetCache<IList<MDSServer>>("MDSServers")[i].MDS_Port,
        //                    CommonFlag.F_PSCallMDSStart + tempStr + "&&"
        //                    + Singleton<JavaScriptSerializer>.Instance.Serialize(cacheManage.GetCache<IList<MDSServer>>("MDSServers")[i]));
        //            }

        //            return string.Empty;
        //        }
        //        return "save MMSs failed";
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.Message + ex.StackTrace;
        //    }
        //}

        private string ArrangeChar()
        {
            //String[] chars = { "a", "b", "c", "d", "e", "f", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            try
            {


                int mssCount = cacheManage.GetCache<IList<MMSServer>>("MMSServers").Count;
                int mcsCount = cacheManage.GetCache<IList<MCSServer>>("MCSServers").Count;
                int mdsCount = cacheManage.GetCache<IList<MDSServer>>("MDSServers").Count;
                IList<string> OldChars = new List<string>();
                IList<string> chars = new List<string>();


                //not to handle duplicate chars this time
                //IDictionary<string,char[]> mcsChars=new Dictionary<string,char[]>();
                //IDictionary<string,char> conflictChars=new Dictionary<string,char>();
                for (int i = 0; i < cacheManage.GetCache<IList<MMSServer>>("MMSServers").Count; i++)
                {
                    if (!string.IsNullOrEmpty(cacheManage.GetCache<IList<MMSServer>>("MMSServers")[i].ArrangeStr))
                    {
                        List<string> tempChars = cacheManage.GetCache<IList<MMSServer>>("MMSServers")[i].ArrangeStr.Split(',').ToList<string>();
                        foreach (string tempChar in tempChars)
                        {
                            chars.Add(tempChar);
                        }
                    }
                }


                //arrange MCSs
                for (int i = 0; i < cacheManage.GetCache<IList<MCSServer>>("MCSServers").Count; i++)
                {
                    if (!string.IsNullOrEmpty(cacheManage.GetCache<IList<MCSServer>>("MCSServers")[i].ArrangeStr))
                    {
                        List<string> tempChars = cacheManage.GetCache<IList<MCSServer>>("MCSServers")[i].ArrangeStr.Split(',').ToList<string>();
                        foreach (string tempChar in tempChars)
                        {
                            OldChars.Add(tempChar);
                        }
                    }
                }

                var tempquery = from aa in chars
                                join bb in OldChars on aa equals bb into cc
                                from temp in cc.DefaultIfEmpty()
                                where string.IsNullOrEmpty(temp)
                                select aa;

                //where string.IsNullOrEmpty(temp.ToString())

                IList<string> availableChars = tempquery.ToList();

                int eachServiceCount = chars.Count / mcsCount;
                int count = 0;

                for (int i = 0; i < cacheManage.GetCache<IList<MCSServer>>("MCSServers").Count; i++)
                {
                    count++;
                    if (count < mcsCount)
                    {
                        int tmpi = 0;
                        if (string.IsNullOrEmpty(cacheManage.GetCache<IList<MCSServer>>("MCSServers")[i].ArrangeStr))
                        {
                            tmpi = eachServiceCount;
                        }
                        else
                        {
                            tmpi = eachServiceCount - cacheManage.GetCache<IList<MCSServer>>("MCSServers")[i].ArrangeStr.Split(',').Length;
                        }

                        while (tmpi > 0)
                        {
                            if (string.IsNullOrEmpty(cacheManage.GetCache<IList<MCSServer>>("MCSServers")[i].ArrangeStr))
                            {
                                cacheManage.GetCache<IList<MCSServer>>("MCSServers")[i].ArrangeStr = availableChars[0].ToString();
                            }
                            else
                            {
                                cacheManage.GetCache<IList<MCSServer>>("MCSServers")[i].ArrangeStr = cacheManage.GetCache<IList<MCSServer>>("MCSServers")[i].ArrangeStr + "," + availableChars[0].ToString();
                            }
                            availableChars.Remove(availableChars[0]);
                            tmpi--;
                        }
                    }
                    else
                    {
                        foreach (string availableChar in availableChars)
                        {
                            if (string.IsNullOrEmpty(cacheManage.GetCache<IList<MCSServer>>("MCSServers")[i].ArrangeStr))
                            {
                                cacheManage.GetCache<IList<MCSServer>>("MCSServers")[i].ArrangeStr = availableChar;
                            }
                            else
                            {
                                cacheManage.GetCache<IList<MCSServer>>("MCSServers")[i].ArrangeStr = cacheManage.GetCache<IList<MCSServer>>("MCSServers")[i].ArrangeStr + "," + availableChar;
                            }
                        }
                    }
                }


                //arrange MDSs
                OldChars = new List<string>();
                for (int i = 0; i < cacheManage.GetCache<IList<MDSServer>>("MDSServers").Count; i++)
                {
                    if (!string.IsNullOrEmpty(cacheManage.GetCache<IList<MDSServer>>("MDSServers")[i].ArrangeStr))
                    {
                        List<string> tempChars = cacheManage.GetCache<IList<MDSServer>>("MDSServers")[i].ArrangeStr.Split(',').ToList<string>();
                        foreach (string tempChar in tempChars)
                        {
                            OldChars.Add(tempChar);
                        }
                    }
                }

                tempquery = from aa in chars
                            join bb in OldChars on aa equals bb into cc
                            from temp in cc.DefaultIfEmpty()
                            where string.IsNullOrEmpty(temp)
                            select aa;

                availableChars = tempquery.ToList();

                eachServiceCount = chars.Count / mdsCount;
                count = 0;

                for (int i = 0; i < cacheManage.GetCache<IList<MDSServer>>("MDSServers").Count; i++)
                {
                    count++;
                    if (count < mdsCount)
                    {
                        int tmpi = 0;
                        if (string.IsNullOrEmpty(cacheManage.GetCache<IList<MDSServer>>("MDSServers")[i].ArrangeStr))
                        {
                            tmpi = eachServiceCount;
                        }
                        else
                        {
                            tmpi = eachServiceCount - cacheManage.GetCache<IList<MDSServer>>("MDSServers")[i].ArrangeStr.Split(',').Length;
                        }

                        while (tmpi > 0)
                        {
                            if (string.IsNullOrEmpty(cacheManage.GetCache<IList<MDSServer>>("MDSServers")[i].ArrangeStr))
                            {
                                cacheManage.GetCache<IList<MDSServer>>("MDSServers")[i].ArrangeStr = availableChars[0].ToString();
                            }
                            else
                            {
                                cacheManage.GetCache<IList<MDSServer>>("MDSServers")[i].ArrangeStr = cacheManage.GetCache<IList<MDSServer>>("MDSServers")[i].ArrangeStr + "," + availableChars[0].ToString();
                            }
                            availableChars.Remove(availableChars[0]);
                            tmpi--;
                        }
                    }
                    else
                    {
                        foreach (string availableChar in availableChars)
                        {
                            if (string.IsNullOrEmpty(cacheManage.GetCache<IList<MDSServer>>("MDSServers")[i].ArrangeStr))
                            {
                                cacheManage.GetCache<IList<MDSServer>>("MDSServers")[i].ArrangeStr = availableChar;
                            }
                            else
                            {
                                cacheManage.GetCache<IList<MDSServer>>("MDSServers")[i].ArrangeStr = cacheManage.GetCache<IList<MDSServer>>("MDSServers")[i].ArrangeStr + "," + availableChar;
                            }
                        }
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message + ex.StackTrace;
            }
        }

        //public ActionResult FindMMS()
        //{
        //    MMSModel mMSModel = new MMSModel();

        //    if (cacheManage.GetCache<IList<MMSServer>>("MMSServers") == null || cacheManage.GetCache<IList<MMSServer>>("MMSServers").Count <= 0)
        //    {
        //        IList<MMSServer> mmsServers = mmsServerRepository.Table.ToList();
        //        if (mmsServers != null && mmsServers.Count > 0)
        //        {
        //            cacheManage.AddCache<IList<MMSServer>>("MMSServers", mmsServers);
        //        }
        //    }

        //    return Json(mMSModel, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult Index()
        {
            MMSServer mMSModel = new MMSServer();
            mMSModel.MMS_IP = cacheManage.GetCache<string>("MMSIP");
            mMSModel.MMS_Port = cacheManage.GetCache<int>("MMSPort");
            return View(mMSModel);
        }
    }
}
