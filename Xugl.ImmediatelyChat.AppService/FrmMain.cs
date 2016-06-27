using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xugl.ImmediatelyChat.AppService.Common;
using Xugl.ImmediatelyChat.AppService.Interface;
using Xugl.ImmediatelyChat.AppService.Sockets;
using Xugl.ImmediatelyChat.Core;
using Xugl.ImmediatelyChat.Core.DependencyResolution;

namespace Xugl.ImmediatelyChat.AppService
{
    public partial class FrmMain : Form
    {
        private SocketService socketService;
        private GroupSocketServer groupSocketServer;
        public FrmMain()
        {
            InitializeComponent();

            timer1.Interval = 1000;
            timer1.Enabled = true;
        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            InitCommonVariables.Init();

            PostServerInfo();

            socketService = new SocketService();
            socketService.Start();

            groupSocketServer=new GroupSocketServer();
            groupSocketServer.Start();
            
        }

        private void PostServerInfo()
        {

            //HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create("http://" + CommonVariables.WebSiteIP 
            //    + ":" + CommonVariables.WebSitePort + "/AppServer/index?ip=" + CommonVariables.ServerHostIP + "&&port=" + CommonVariables.ServerHostPort);

            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create("http://" + CommonVariables.WebSiteIP
                + ":" + CommonVariables.WebSitePort + "/AppServer/index?ip=183.11.14.151&&port=" + CommonVariables.ServerHostPort);

            myRequest.Method = "Get";

            myRequest.GetResponse();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!CommonVariables.LogTool.IsRecord)
            {
                txt_Log.Text = Singleton<ICommonLog>.Instance.LogMsg;
                Singleton<ICommonLog>.Instance.IsRecord = true;
            }
        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {
            socketService.Stop();
            groupSocketServer.Stop();
        }
    }
}
 