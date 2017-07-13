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
            // 
            // label1
            // 
            this.label_name.AutoSize = true;
            this.label_name.Location = new System.Drawing.Point(13, 13);
            this.label_name.Name = "label1";
            this.label_name.Size = new System.Drawing.Size(35, 13);
            this.label_name.TabIndex = 2;
            this.label_name.Text = "label1";
            // 
            // label2
            // 
            this.label_percentage.AutoSize = true;
            this.label_percentage.Location = new System.Drawing.Point(13, 32);
            this.label_percentage.Name = "label2";
            this.label_percentage.Size = new System.Drawing.Size(35, 13);
            this.label_percentage.TabIndex = 3;
            this.label_percentage.Text = "label2";
            // 
            // label3
            // 
            this.label_timeleft.AutoSize = true;
            this.label_timeleft.Location = new System.Drawing.Point(13, 94);
            this.label_timeleft.Name = "label3";
            this.label_timeleft.Size = new System.Drawing.Size(35, 13);
            this.label_timeleft.TabIndex = 4;
            this.label_timeleft.Text = "label3";
            // 
            // label4
            // 
            this.label_speed.AutoSize = true;
            this.label_speed.Location = new System.Drawing.Point(13, 114);
            this.label_speed.Name = "label4";
            this.label_speed.Size = new System.Drawing.Size(35, 13);
            this.label_speed.TabIndex = 5;
            this.label_speed.Text = "label4";
            // 
            // SendDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(473, 136);
            this.Controls.Add(this.label_speed);
            this.Controls.Add(this.label_timeleft);
            this.Controls.Add(this.label_percentage);
            this.Controls.Add(this.label_name);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.progressBar1);
            this.Name = "SendDialog";
            this.Text = "SendDialog";
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
    }
}