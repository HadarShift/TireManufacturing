namespace TireManufacturing
{
    partial class SelfInspection
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelfInspection));
            this.panel1 = new System.Windows.Forms.Panel();
            this.lbl_ConstForQA = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.lbl_upperline = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.dataGridViewTireComponent = new System.Windows.Forms.DataGridView();
            this.gpbSpecDataLevelA = new System.Windows.Forms.GroupBox();
            this.lbl_lowerline = new System.Windows.Forms.Label();
            this.txt_barcode = new System.Windows.Forms.TextBox();
            this.lbl_Product = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txt_comment = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.txt_Manager = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lbl_catalognum = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lbl_machine = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lbl_shift = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lbl_date = new System.Windows.Forms.Label();
            this.lbl_MachineWrk = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTireComponent)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lbl_upperline);
            this.panel1.Controls.Add(this.lbl_ConstForQA);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.dataGridViewTireComponent);
            this.panel1.Controls.Add(this.gpbSpecDataLevelA);
            this.panel1.Controls.Add(this.lbl_lowerline);
            this.panel1.Controls.Add(this.txt_barcode);
            this.panel1.Controls.Add(this.lbl_Product);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.lbl_catalognum);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.lbl_machine);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.lbl_shift);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.lbl_date);
            this.panel1.Controls.Add(this.lbl_MachineWrk);
            this.panel1.Location = new System.Drawing.Point(2, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1214, 637);
            this.panel1.TabIndex = 0;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // lbl_ConstForQA
            // 
            this.lbl_ConstForQA.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lbl_ConstForQA.AutoSize = true;
            this.lbl_ConstForQA.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lbl_ConstForQA.Location = new System.Drawing.Point(544, 609);
            this.lbl_ConstForQA.Name = "lbl_ConstForQA";
            this.lbl_ConstForQA.Size = new System.Drawing.Size(76, 29);
            this.lbl_ConstForQA.TabIndex = 252;
            this.lbl_ConstForQA.Text = "*******";
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.Crimson;
            this.button2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.button2.Location = new System.Drawing.Point(1029, 606);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(185, 31);
            this.button2.TabIndex = 253;
            this.button2.Text = "דיווח פסולות";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // lbl_upperline
            // 
            this.lbl_upperline.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbl_upperline.AutoSize = true;
            this.lbl_upperline.BackColor = System.Drawing.Color.Transparent;
            this.lbl_upperline.Font = new System.Drawing.Font("Arial", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lbl_upperline.Location = new System.Drawing.Point(-2550, -16);
            this.lbl_upperline.Name = "lbl_upperline";
            this.lbl_upperline.Size = new System.Drawing.Size(4064, 75);
            this.lbl_upperline.TabIndex = 251;
            this.lbl_upperline.Text = "_________________________________________________________________________________" +
    "_______________________________";
            this.lbl_upperline.Click += new System.EventHandler(this.lbl_upperline_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = global::TireManufacturing.Properties.Resources.line;
            this.pictureBox1.Location = new System.Drawing.Point(759, 52);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(10, 274);
            this.pictureBox1.TabIndex = 250;
            this.pictureBox1.TabStop = false;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.CornflowerBlue;
            this.button1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.button1.Location = new System.Drawing.Point(10, 607);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(185, 31);
            this.button1.TabIndex = 1;
            this.button1.Text = "שמור נתונים";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // dataGridViewTireComponent
            // 
            this.dataGridViewTireComponent.AllowUserToAddRows = false;
            this.dataGridViewTireComponent.AllowUserToDeleteRows = false;
            this.dataGridViewTireComponent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewTireComponent.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTireComponent.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewTireComponent.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTireComponent.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewTireComponent.EnableHeadersVisualStyles = false;
            this.dataGridViewTireComponent.Location = new System.Drawing.Point(40, 367);
            this.dataGridViewTireComponent.Name = "dataGridViewTireComponent";
            this.dataGridViewTireComponent.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.dataGridViewTireComponent.Size = new System.Drawing.Size(1068, 234);
            this.dataGridViewTireComponent.TabIndex = 247;
            this.dataGridViewTireComponent.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dataGridViewTireComponent_CellBeginEdit);
            this.dataGridViewTireComponent.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewTireComponent_CellClick);
            this.dataGridViewTireComponent.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewTireComponent_CellEndEdit);
            this.dataGridViewTireComponent.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewTireComponent_CellEnter);
            // 
            // gpbSpecDataLevelA
            // 
            this.gpbSpecDataLevelA.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.gpbSpecDataLevelA.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gpbSpecDataLevelA.Location = new System.Drawing.Point(857, 76);
            this.gpbSpecDataLevelA.Name = "gpbSpecDataLevelA";
            this.gpbSpecDataLevelA.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.gpbSpecDataLevelA.Size = new System.Drawing.Size(251, 211);
            this.gpbSpecDataLevelA.TabIndex = 242;
            this.gpbSpecDataLevelA.TabStop = false;
            this.gpbSpecDataLevelA.Text = "בקרת ציוד מכונה";
            // 
            // lbl_lowerline
            // 
            this.lbl_lowerline.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbl_lowerline.AutoSize = true;
            this.lbl_lowerline.Font = new System.Drawing.Font("Arial", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lbl_lowerline.Location = new System.Drawing.Point(-22, 257);
            this.lbl_lowerline.Name = "lbl_lowerline";
            this.lbl_lowerline.Size = new System.Drawing.Size(4064, 75);
            this.lbl_lowerline.TabIndex = 249;
            this.lbl_lowerline.Text = "_________________________________________________________________________________" +
    "_______________________________";
            // 
            // txt_barcode
            // 
            this.txt_barcode.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.txt_barcode.Location = new System.Drawing.Point(359, 335);
            this.txt_barcode.Name = "txt_barcode";
            this.txt_barcode.Size = new System.Drawing.Size(373, 31);
            this.txt_barcode.TabIndex = 246;
            this.txt_barcode.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txt_barcode.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txt_barcode_KeyPress);
            // 
            // lbl_Product
            // 
            this.lbl_Product.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbl_Product.AutoSize = true;
            this.lbl_Product.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lbl_Product.Location = new System.Drawing.Point(738, 335);
            this.lbl_Product.Name = "lbl_Product";
            this.lbl_Product.Size = new System.Drawing.Size(142, 29);
            this.lbl_Product.TabIndex = 245;
            this.lbl_Product.Text = "ברקוד מרכיב";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.groupBox2.Controls.Add(this.txt_comment);
            this.groupBox2.Controls.Add(this.label17);
            this.groupBox2.Controls.Add(this.txt_Manager);
            this.groupBox2.Controls.Add(this.label16);
            this.groupBox2.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(40, 198);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.groupBox2.Size = new System.Drawing.Size(662, 113);
            this.groupBox2.TabIndex = 244;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "אישור מנהל";
            // 
            // txt_comment
            // 
            this.txt_comment.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_comment.Location = new System.Drawing.Point(45, 73);
            this.txt_comment.Name = "txt_comment";
            this.txt_comment.Size = new System.Drawing.Size(508, 26);
            this.txt_comment.TabIndex = 235;
            // 
            // label17
            // 
            this.label17.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.label17.Location = new System.Drawing.Point(599, 75);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(46, 19);
            this.label17.TabIndex = 234;
            this.label17.Text = "הערה";
            // 
            // txt_Manager
            // 
            this.txt_Manager.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_Manager.Location = new System.Drawing.Point(45, 38);
            this.txt_Manager.Name = "txt_Manager";
            this.txt_Manager.Size = new System.Drawing.Size(508, 26);
            this.txt_Manager.TabIndex = 233;
            this.txt_Manager.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txt_Manager_KeyPress);
            this.txt_Manager.Leave += new System.EventHandler(this.txt_Manager_Leave);
            // 
            // label16
            // 
            this.label16.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.label16.Location = new System.Drawing.Point(574, 40);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(71, 19);
            this.label16.TabIndex = 232;
            this.label16.Text = "מס\' מנהל";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.groupBox1.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(451, 76);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.groupBox1.Size = new System.Drawing.Size(251, 116);
            this.groupBox1.TabIndex = 243;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "בקרת בטיחות";
            // 
            // lbl_catalognum
            // 
            this.lbl_catalognum.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbl_catalognum.AutoSize = true;
            this.lbl_catalognum.BackColor = System.Drawing.Color.Red;
            this.lbl_catalognum.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lbl_catalognum.Location = new System.Drawing.Point(101, 8);
            this.lbl_catalognum.Name = "lbl_catalognum";
            this.lbl_catalognum.Size = new System.Drawing.Size(31, 22);
            this.lbl_catalognum.TabIndex = 241;
            this.lbl_catalognum.Text = "***";
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.label7.Location = new System.Drawing.Point(256, 8);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(54, 22);
            this.label7.TabIndex = 240;
            this.label7.Text = "מק\"ט";
            // 
            // lbl_machine
            // 
            this.lbl_machine.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbl_machine.AutoSize = true;
            this.lbl_machine.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lbl_machine.Location = new System.Drawing.Point(423, 8);
            this.lbl_machine.Name = "lbl_machine";
            this.lbl_machine.Size = new System.Drawing.Size(31, 22);
            this.lbl_machine.TabIndex = 239;
            this.lbl_machine.Text = "***";
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.label5.Location = new System.Drawing.Point(491, 8);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 22);
            this.label5.TabIndex = 238;
            this.label5.Text = ":מכונה";
            // 
            // lbl_shift
            // 
            this.lbl_shift.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbl_shift.AutoSize = true;
            this.lbl_shift.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lbl_shift.Location = new System.Drawing.Point(671, 8);
            this.lbl_shift.Name = "lbl_shift";
            this.lbl_shift.Size = new System.Drawing.Size(31, 22);
            this.lbl_shift.TabIndex = 237;
            this.lbl_shift.Text = "***";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.label2.Location = new System.Drawing.Point(708, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 22);
            this.label2.TabIndex = 236;
            this.label2.Text = ":משמרת";
            // 
            // lbl_date
            // 
            this.lbl_date.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbl_date.AutoSize = true;
            this.lbl_date.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lbl_date.Location = new System.Drawing.Point(853, 8);
            this.lbl_date.Name = "lbl_date";
            this.lbl_date.Size = new System.Drawing.Size(116, 22);
            this.lbl_date.TabIndex = 235;
            this.lbl_date.Text = "**/**/**** **:**";
            // 
            // lbl_MachineWrk
            // 
            this.lbl_MachineWrk.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbl_MachineWrk.AutoSize = true;
            this.lbl_MachineWrk.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lbl_MachineWrk.Location = new System.Drawing.Point(1058, 8);
            this.lbl_MachineWrk.Name = "lbl_MachineWrk";
            this.lbl_MachineWrk.Size = new System.Drawing.Size(66, 22);
            this.lbl_MachineWrk.TabIndex = 234;
            this.lbl_MachineWrk.Text = ":תאריך";
            // 
            // SelfInspection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1216, 677);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "SelfInspection";
            this.Text = "מרכיבים וטופס בקרה עצמית";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SelfInspection_FormClosing);
            this.Load += new System.EventHandler(this.SelfInspection_Load);
            this.Shown += new System.EventHandler(this.SelfInspection_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTireComponent)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox gpbSpecDataLevelA;
        private System.Windows.Forms.Label lbl_catalognum;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lbl_machine;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lbl_shift;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lbl_date;
        private System.Windows.Forms.Label lbl_MachineWrk;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txt_comment;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox txt_Manager;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.DataGridView dataGridViewTireComponent;
        private System.Windows.Forms.TextBox txt_barcode;
        private System.Windows.Forms.Label lbl_Product;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lbl_lowerline;
        private System.Windows.Forms.Label lbl_upperline;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lbl_ConstForQA;
        private System.Windows.Forms.Button button2;
    }
}