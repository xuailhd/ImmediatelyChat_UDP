using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Xugl.ImmediatelyChat.Core;
using Xugl.ImmediatelyChat.SocketEngine;

namespace Xugl.ImmediatelyChat.Site
{
    public class CommonVariables
    {
        public static SyncSocketClientUDP syncSocketClient
        {
            get
            {
                if (Singleton<SyncSocketClientUDP>.Instance == null)
                {
                    Singleton<SyncSocketClientUDP>.Instance = new SyncSocketClientUDP();
                }
                return Singleton<SyncSocketClientUDP>.Instance;
            }
        }

        public static JavaScriptSerializer javaScriptSerializer
        {
            get
            {
                if (Singleton<JavaScriptSerializer>.Instance == null)
                {
                    Singleton<JavaScriptSerializer>.Instance = new JavaScriptSerializer();
                }
                return Singleton<JavaScriptSerializer>.Instance;
            }
        }
    }
}