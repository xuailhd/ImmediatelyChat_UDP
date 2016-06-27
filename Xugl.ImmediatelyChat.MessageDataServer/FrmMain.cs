using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xugl.ImmediatelyChat.Core;

namespace Xugl.ImmediatelyChat.MessageDataServer
{
    public partial class FrmMain : Form
    {
        private SocketService sockectService;
        private int logLength = 0;

        public FrmMain()
        {
            InitializeComponent();
            sockectService = new SocketService();
            InitCommonVariables initCommonVariables = new InitCommonVariables();

            timer1.Interval = 100;
            timer1.Enabled = true;

            this.Text = this.Text + " " + CommonVariables.MDS_ID;

            sockectService.StartConnectMMS();
        }

        private void btn_ConnectMainServer_Click(object sender, EventArgs e)
        {
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (logLength != CommonVariables.LogTool.GetLogMsg.Length)
            {
                if (logLength != CommonVariables.LogTool.GetLogMsg.Length)
                {
                    try
                    {
                        txt_Log.Text = CommonVariables.LogTool.GetLogMsg;
                        logLength = txt_Log.Text.Length;
                    }
                    catch (Exception ex)
                    {
                        txt_Log.Text = ex.Message + ex.StackTrace;
                    }

                }
            }
        }
    }
}
