namespace ClientGUI
{
    partial class Form1
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
            this.btnReg = new System.Windows.Forms.Button();
            this.chatWindow = new System.Windows.Forms.RichTextBox();
            this.textRegName = new System.Windows.Forms.TextBox();
            this.btnUnreg = new System.Windows.Forms.Button();
            this.btnLogin = new System.Windows.Forms.Button();
            this.btnUnlogin = new System.Windows.Forms.Button();
            this.listOfClients = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.sendMessageText = new System.Windows.Forms.RichTextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnReg
            // 
            this.btnReg.Location = new System.Drawing.Point(420, 63);
            this.btnReg.Name = "btnReg";
            this.btnReg.Size = new System.Drawing.Size(113, 27);
            this.btnReg.TabIndex = 0;
            this.btnReg.Text = "Registration";
            this.btnReg.UseVisualStyleBackColor = true;
            this.btnReg.Click += new System.EventHandler(this.btnReg_Click);
            // 
            // chatWindow
            // 
            this.chatWindow.Location = new System.Drawing.Point(152, 37);
            this.chatWindow.Name = "chatWindow";
            this.chatWindow.Size = new System.Drawing.Size(262, 220);
            this.chatWindow.TabIndex = 1;
            this.chatWindow.Text = "";
            // 
            // textRegName
            // 
            this.textRegName.Location = new System.Drawing.Point(420, 37);
            this.textRegName.Name = "textRegName";
            this.textRegName.Size = new System.Drawing.Size(113, 20);
            this.textRegName.TabIndex = 2;
            // 
            // btnUnreg
            // 
            this.btnUnreg.Location = new System.Drawing.Point(420, 96);
            this.btnUnreg.Name = "btnUnreg";
            this.btnUnreg.Size = new System.Drawing.Size(113, 26);
            this.btnUnreg.TabIndex = 0;
            this.btnUnreg.Text = "Unregistration";
            this.btnUnreg.UseVisualStyleBackColor = true;
            this.btnUnreg.Click += new System.EventHandler(this.btnUnreg_Click);
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(420, 163);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(113, 44);
            this.btnLogin.TabIndex = 0;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // btnUnlogin
            // 
            this.btnUnlogin.Location = new System.Drawing.Point(420, 213);
            this.btnUnlogin.Name = "btnUnlogin";
            this.btnUnlogin.Size = new System.Drawing.Size(113, 44);
            this.btnUnlogin.TabIndex = 0;
            this.btnUnlogin.Text = "Unlogin";
            this.btnUnlogin.UseVisualStyleBackColor = true;
            this.btnUnlogin.Click += new System.EventHandler(this.btnUnlogin_Click);
            // 
            // listOfClients
            // 
            this.listOfClients.Location = new System.Drawing.Point(9, 37);
            this.listOfClients.Name = "listOfClients";
            this.listOfClients.Size = new System.Drawing.Size(127, 220);
            this.listOfClients.TabIndex = 1;
            this.listOfClients.Text = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Clients online:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(149, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Chat window:";
            // 
            // sendMessageText
            // 
            this.sendMessageText.Location = new System.Drawing.Point(152, 263);
            this.sendMessageText.Name = "sendMessageText";
            this.sendMessageText.Size = new System.Drawing.Size(262, 51);
            this.sendMessageText.TabIndex = 1;
            this.sendMessageText.Text = "";
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(420, 263);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(113, 51);
            this.btnSend.TabIndex = 0;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.btnSend);
            this.groupBox1.Controls.Add(this.sendMessageText);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textRegName);
            this.groupBox1.Controls.Add(this.listOfClients);
            this.groupBox1.Controls.Add(this.chatWindow);
            this.groupBox1.Controls.Add(this.btnUnreg);
            this.groupBox1.Controls.Add(this.btnUnlogin);
            this.groupBox1.Controls.Add(this.btnLogin);
            this.groupBox1.Controls.Add(this.btnReg);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(540, 325);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Chat on an actor model";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(417, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Registration Name:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(561, 348);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "Client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnReg;
        public System.Windows.Forms.RichTextBox chatWindow;
        public System.Windows.Forms.TextBox textRegName;
        private System.Windows.Forms.Button btnUnreg;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnUnlogin;
        public System.Windows.Forms.RichTextBox listOfClients;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RichTextBox sendMessageText;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
    }
}

