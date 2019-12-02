namespace TireManufacturing
{
    partial class FormStopReason
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormStopReason));
            this.btn_Stop = new System.Windows.Forms.Button();
            this.ListBox_StopReason = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // btn_Stop
            // 
            this.btn_Stop.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.btn_Stop.Location = new System.Drawing.Point(127, 368);
            this.btn_Stop.Name = "btn_Stop";
            this.btn_Stop.Size = new System.Drawing.Size(113, 34);
            this.btn_Stop.TabIndex = 0;
            this.btn_Stop.Text = "אישור";
            this.btn_Stop.UseVisualStyleBackColor = true;
            this.btn_Stop.Click += new System.EventHandler(this.btn_Stop_Click);
            // 
            // ListBox_StopReason
            // 
            this.ListBox_StopReason.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.ListBox_StopReason.FormattingEnabled = true;
            this.ListBox_StopReason.ItemHeight = 19;
            this.ListBox_StopReason.Location = new System.Drawing.Point(29, 29);
            this.ListBox_StopReason.Name = "ListBox_StopReason";
            this.ListBox_StopReason.Size = new System.Drawing.Size(311, 327);
            this.ListBox_StopReason.TabIndex = 1;
            // 
            // FormStopReason
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(383, 450);
            this.Controls.Add(this.ListBox_StopReason);
            this.Controls.Add(this.btn_Stop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormStopReason";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "סיבות עצירה";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormStopReason_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_Stop;
        private System.Windows.Forms.ListBox ListBox_StopReason;
    }
}