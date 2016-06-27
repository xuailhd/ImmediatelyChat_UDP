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

namespace Xugl.ImmediatelyChat.Test
{
    public partial class FrmMain : Form
    {
        private int logLength=0;

        public FrmMain()
        {
            InitializeComponent();
            timer1.Interval = 100;
            timer1.Enabled = true;

            txt_ip.Text = "192.168.1.2";
            txt_port.Text = "30001";
        }


        private void btn_StartServer_Click(object sender, EventArgs e)
        {
            TestUPDListener testUPDListener = new TestUPDListener();
            testUPDListener.TestSendUDPToService(txt_ip.Text.ToString(),Convert.ToInt32(txt_port.Text.ToString()));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(logLength!=CommonVariables.LogTool.GetLogMsg.Length)
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

        private void FrmMain_Load(object sender, EventArgs e)
        {

        }
    }
}
