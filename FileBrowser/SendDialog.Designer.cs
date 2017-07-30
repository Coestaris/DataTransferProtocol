namespace FileBrowser
{
    partial class SendDialog
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
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.button1 = new System.Windows.Forms.Button();
            this.label_name = new System.Windows.Forms.Label();
            this.label_percentage = new System.Windows.Forms.Label();
            this.label_timeleft = new System.Windows.Forms.Label();
            this.label_speed = new System.Windows.Forms.Label();
            this.textBox_pcName = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox_deviceName = new System.Windows.Forms.TextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 51);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(449, 33);
            this.progressBar1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(357, 94);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(104, 32);
            this.button1.TabIndex = 1;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label_name
            // 
            this.label_name.AutoSize = true;
            this.label_name.Location = new System.Drawing.Point(13, 13);
            this.label_name.Name = "label_name";
            this.label_name.Size = new System.Drawing.Size(35, 13);
            this.label_name.TabIndex = 2;
            this.label_name.Text = "label1";
            // 
            // label_percentage
            // 
            this.label_percentage.AutoSize = true;
            this.label_percentage.Location = new System.Drawing.Point(13, 32);
            this.label_percentage.Name = "label_percentage";
            this.label_percentage.Size = new System.Drawing.Size(35, 13);
            this.label_percentage.TabIndex = 3;
            this.label_percentage.Text = "label2";
            // 
            // label_timeleft
            // 
            this.label_timeleft.AutoSize = true;
            this.label_timeleft.Location = new System.Drawing.Point(13, 94);
            this.label_timeleft.Name = "label_timeleft";
            this.label_timeleft.Size = new System.Drawing.Size(35, 13);
            this.label_timeleft.TabIndex = 4;
            this.label_timeleft.Text = "label3";
            // 
            // label_speed
            // 
            this.label_speed.AutoSize = true;
            this.label_speed.Location = new System.Drawing.Point(13, 114);
            this.label_speed.Name = "label_speed";
            this.label_speed.Size = new System.Drawing.Size(35, 13);
            this.label_speed.TabIndex = 5;
            this.label_speed.Text = "label4";
            // 
            // textBox_pcName
            // 
            this.textBox_pcName.Location = new System.Drawing.Point(14, 15);
            this.textBox_pcName.Name = "textBox_pcName";
            this.textBox_pcName.Size = new System.Drawing.Size(402, 20);
            this.textBox_pcName.TabIndex = 6;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(422, 15);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(47, 23);
            this.button2.TabIndex = 7;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox_deviceName
            // 
            this.textBox_deviceName.Location = new System.Drawing.Point(14, 44);
            this.textBox_deviceName.Name = "textBox_deviceName";
            this.textBox_deviceName.Size = new System.Drawing.Size(402, 20);
            this.textBox_deviceName.TabIndex = 8;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(356, 82);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(104, 33);
            this.button4.TabIndex = 10;
            this.button4.Text = "button4";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button4);
            this.panel1.Controls.Add(this.textBox_deviceName);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.textBox_pcName);
            this.panel1.Location = new System.Drawing.Point(1, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(515, 123);
            this.panel1.TabIndex = 11;
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "file.file";
            this.openFileDialog.Filter = "Any File|*.*";
            // 
            // SendDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(483, 149);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label_speed);
            this.Controls.Add(this.label_timeleft);
            this.Controls.Add(this.label_percentage);
            this.Controls.Add(this.label_name);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.progressBar1);
            this.Name = "SendDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SendDialog";
            this.Load += new System.EventHandler(this.SendDialog_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label_name;
        private System.Windows.Forms.Label label_percentage;
        private System.Windows.Forms.Label label_timeleft;
        private System.Windows.Forms.Label label_speed;
        private System.Windows.Forms.TextBox textBox_pcName;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox_deviceName;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}