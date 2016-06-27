namespace Xugl.ImmediatelyChat.Test
{
    partial class FrmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btn_StartServer = new System.Windows.Forms.Button();
            this.txt_Log = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.txt_ip = new System.Windows.Forms.TextBox();
            this.txt_port = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btn_StartServer
            // 
            this.btn_StartServer.Location = new System.Drawing.Point(387, 12);
            this.btn_StartServer.Name = "btn_StartServer";
            this.btn_StartServer.Size = new System.Drawing.Size(125, 21);
            this.btn_StartServer.TabIndex = 0;
            this.btn_StartServer.Text = "Connect Server";
            this.btn_StartServer.UseVisualStyleBackColor = true;
            this.btn_StartServer.Click += new System.EventHandler(this.btn_StartServer_Click);
            // 
            // txt_Log
            // 
            this.txt_Log.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_Log.Location = new System.Drawing.Point(12, 39);
            this.txt_Log.Multiline = true;
            this.txt_Log.Name = "txt_Log";
            this.txt_Log.ReadOnly = true;
            this.txt_Log.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txt_Log.Size = new System.Drawing.Size(593, 293);
            this.txt_Log.TabIndex = 2;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // txt_ip
            // 
            this.txt_ip.Location = new System.Drawing.Point(30, 12);
            this.txt_ip.Name = "txt_ip";
            this.txt_ip.Size = new System.Drawing.Size(100, 21);
            this.txt_ip.TabIndex = 3;
            // 
            // txt_port
            // 
            this.txt_port.Location = new System.Drawing.Point(161, 13);
            this.txt_port.Name = "txt_port";
            this.txt_port.Size = new System.Drawing.Size(100, 21);
            this.txt_port.TabIndex = 4;
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(617, 355);
            this.Controls.Add(this.txt_port);
            this.Controls.Add(this.txt_ip);
            this.Controls.Add(this.txt_Log);
            this.Controls.Add(this.btn_StartServer);
            this.Name = "FrmMain";
            this.Text = "Message Main Server";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_StartServer;
        private System.Windows.Forms.TextBox txt_Log;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox txt_ip;
        private System.Windows.Forms.TextBox txt_port;
    }
}

