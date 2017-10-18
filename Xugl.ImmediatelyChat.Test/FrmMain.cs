using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xugl.ImmediatelyChat.Common;
using Xugl.ImmediatelyChat.Core;

namespace Xugl.ImmediatelyChat.Test
{
    public partial class FrmMain : Form
    {
        private int logLength=0;
        TestUPDListener lister = null;
        public FrmMain()
        {
            InitializeComponent();
            timer1.Interval = 100;
            timer1.Enabled = true;

            txt_ip.Text = "10.2.29.123";
            txt_port.Text = "30001";
        }


        private void btn_StartServer_Click(object sender, EventArgs e)
        {
            Stack<string> pop = new Stack<string>();

            string st = pop.Pop();

            //lister = new TestUPDListener(txt_ip.Text, Convert.ToInt32(txt_port.Text));

            //TestUPDListener testUPDListener = new TestUPDListener();
            //testUPDListener.TestSendUDPToService(txt_ip.Text.ToString(),Convert.ToInt32(txt_port.Text.ToString()));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            if (logLength != CommonVariables.LogTool.GetLogMsg.Length)
            {
                try
                {
                    txt_Log.Text = CommonVariables.LogTool.GetLogMsg;
                    logLength = CommonVariables.LogTool.GetLogMsg.Length;
                }
                catch (Exception ex)
                {
                    txt_Log.Text = ex.Message + ex.StackTrace;
                }

            }

        }

        private void FrmMain_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //lister.SendFile(txtPath.Text, txt_ip.Text, Convert.ToInt32(txt_port.Text));
        }
    }
}
