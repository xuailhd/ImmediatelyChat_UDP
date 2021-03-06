﻿namespace Xugl.ImmediatelyChat.MessageDataServer
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

            if(sockectService!=null)
            {
                sockectService.StopMDSService();
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
            this.txt_Log = new System.Windows.Forms.TextBox();
            this.btn_ConnectMainServer = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // txt_Log
            // 
            this.txt_Log.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_Log.Location = new System.Drawing.Point(12, 41);
            this.txt_Log.Multiline = true;
            this.txt_Log.Name = "txt_Log";
            this.txt_Log.ReadOnly = true;
            this.txt_Log.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txt_Log.Size = new System.Drawing.Size(598, 324);
            this.txt_Log.TabIndex = 3;
            // 
            // btn_ConnectMainServer
            // 
            this.btn_ConnectMainServer.Location = new System.Drawing.Point(22, 12);
            this.btn_ConnectMainServer.Name = "btn_ConnectMainServer";
            this.btn_ConnectMainServer.Size = new System.Drawing.Size(125, 23);
            this.btn_ConnectMainServer.TabIndex = 2;
            this.btn_ConnectMainServer.Text = "Connect Portal Server";
            this.btn_ConnectMainServer.UseVisualStyleBackColor = true;
            this.btn_ConnectMainServer.Click += new System.EventHandler(this.btn_ConnectMainServer_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 377);
            this.Controls.Add(this.txt_Log);
            this.Controls.Add(this.btn_ConnectMainServer);
            this.Name = "FrmMain";
            this.Text = "Message Data Server";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_Log;
        private System.Windows.Forms.Button btn_ConnectMainServer;
        private System.Windows.Forms.Timer timer1;
    }
}

