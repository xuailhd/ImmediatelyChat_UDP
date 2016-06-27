using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.AppService.Common
{
    public class InitCommonVariables
    {
        public static void Init()
        {
            CommonVariables.MSGserverHostIP = ConfigurationManager.AppSettings["MSGserverHostIP"].ToString();
            CommonVariables.MSGserverHostPort = Convert.ToInt32(ConfigurationManager.AppSettings["MSGserverHostPort"]);
            CommonVariables.ServerHostIP = ConfigurationManager.AppSettings["ServerHostIP"].ToString();
            CommonVariables.ServerHostPort = Convert.ToInt32(ConfigurationManager.AppSettings["ServerHostPort"]);
            CommonVariables.WebSitePort = Convert.ToInt32(ConfigurationManager.AppSettings["WebSitePort"]);
            CommonVariables.WebSiteIP = ConfigurationManager.AppSettings["WebSiteIP"].ToString();
        }
    }
}
