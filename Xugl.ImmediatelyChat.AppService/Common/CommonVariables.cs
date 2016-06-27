using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.AppService.Interface;
using Xugl.ImmediatelyChat.AppService.Model;
using Xugl.ImmediatelyChat.Core;
using Xugl.ImmediatelyChat.AppService.Container;

namespace Xugl.ImmediatelyChat.AppService.Common
{
    public class CommonVariables
    {

        //private IList<ClientStatusModel> _currentClients;

        //public static Dictionary<string,ClientStatusModel> CurrentClients { 
        //    get 
        //    {
        //        lock (Singleton<Dictionary<string, ClientStatusModel>>.Instance)
        //        {
        //            if (Singleton<Dictionary<string, ClientStatusModel>>.Instance == null)
        //            {
        //                Singleton<Dictionary<string, ClientStatusModel>>.Instance = new Dictionary<string, ClientStatusModel>();
        //            }
        //            return Singleton<Dictionary<string, ClientStatusModel>>.Instance;
        //        }
        //    }
        //}

        public static Dictionary<string, ClientStatusModel> GetCurrentClients
        {
            get
            {
                return Singleton<Dictionary<string, ClientStatusModel>>.Instance;
            }
        }

        public static void AddCurrentClients(ClientStatusModel clientStatusModel)
        {
            if (Singleton<Dictionary<string, ClientStatusModel>>.Instance == null)
            {
                Singleton<Dictionary<string, ClientStatusModel>>.Instance = new Dictionary<string, ClientStatusModel>();
            }

            

            lock (Singleton<Dictionary<string, ClientStatusModel>>.Instance)
            {
                if (Singleton<Dictionary<string, ClientStatusModel>>.Instance.ContainsKey(clientStatusModel.ObjectID))
                {
                    Singleton<Dictionary<string, ClientStatusModel>>.Instance[clientStatusModel.ObjectID] = clientStatusModel;
                }
                else
                {
                    Singleton<Dictionary<string, ClientStatusModel>>.Instance.Add(clientStatusModel.ObjectID, clientStatusModel);
                }
            }
        }

        public static void RemoveCurrentClients(string objectID)
        {
            if (Singleton<Dictionary<string, ClientStatusModel>>.Instance == null)
            {
                return;
            }
            lock (Singleton<Dictionary<string, ClientStatusModel>>.Instance)
            {
                Singleton<Dictionary<string, ClientStatusModel>>.Instance.Remove(objectID);
            }
        }

        public static bool CurrentClientsContainKey(string objectID)
        {
            if (Singleton<Dictionary<string, ClientStatusModel>>.Instance == null)
            {
                Singleton<Dictionary<string, ClientStatusModel>>.Instance = new Dictionary<string, ClientStatusModel>();
            }
            return Singleton<Dictionary<string, ClientStatusModel>>.Instance.ContainsKey(objectID);
        }

        public static ICommonLog LogTool
        {
            get
            {
                if (Singleton<ICommonLog>.Instance == null)
                {
                    Singleton<ICommonLog>.Instance = new CommonLog();
                }
                return Singleton<ICommonLog>.Instance;
            }
        }

        public static string MSGserverHostIP
        {
            get;
            set;
        }

        public static int MSGserverHostPort
        {
            get;
            set;
        }

        public static string ServerHostIP
        {
            get;
            set;
        }

        public static int ServerHostPort
        {
            get;
            set;
        }

        public static string WebSiteIP
        {
            get;
            set;
        }

        public static int WebSitePort
        {
            get;
            set;
        }

        private static int msgContainerCount;

        public static int MsgContainerCount
        {
            get
            {
                if (msgContainerCount == 0)
                {
                    msgContainerCount = Convert.ToInt32(ConfigurationManager.AppSettings["MsgContainerCount"]);
                }
                return msgContainerCount;
            }
        }

        private static int msgContainerFullCount;
        public static int MsgContainerFullCount
        {
            get
            {
                if (msgContainerFullCount == 0)
                {
                    msgContainerFullCount = Convert.ToInt32(ConfigurationManager.AppSettings["MsgContainerFullCount"]);
                }
                return msgContainerFullCount;
            }
        }

        private static int msgOutContainerCount;

        public static int MsgOutContainerCount
        {
            get
            {
                if (msgOutContainerCount == 0)
                {
                    msgOutContainerCount = Convert.ToInt32(ConfigurationManager.AppSettings["MsgOutContainerCount"]);
                }
                return msgOutContainerCount;
            }
        }

        public static MsgContainerProxy MsgRecordContainer
        {
            get
            {
                if(Singleton<MsgContainerProxy>.Instance==null)
                {
                    Singleton<MsgContainerProxy>.Instance = new MsgContainerProxy();
                }
                return Singleton<MsgContainerProxy>.Instance;
            }

        }

         

    }
}
