using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.MessageMainServer
{
    public class InitCommonVariables
    {
        public InitCommonVariables()
        {
            CommonVariables.PSIP = ConfigurationManager.AppSettings["PSIP"].ToString();
            CommonVariables.PSPort = Convert.ToInt32(ConfigurationManager.AppSettings["PSPort"].ToString());
            CommonVariables.MMSIP = ConfigurationManager.AppSettings["MMSIP"].ToString();
            CommonVariables.MMSPort = Convert.ToInt32(ConfigurationManager.AppSettings["MMSPort"]);
        }
    }
}
