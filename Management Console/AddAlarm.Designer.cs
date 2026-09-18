namespace ManagementConsole
{
    partial class AddAlarm
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
            tbName = new TextBox();
            tbDescription = new TextBox();
            lbDescription = new Label();
            lbName = new Label();
            lbfieldDevice = new Label();
            lbParameter = new Label();
            cmbParameter = new ComboBox();
            cmbDevice = new ComboBox();
            lbHigh = new Label();
            lbLow = new Label();
            tbLow = new TextBox();
            tbHigh = new TextBox();
            btnSave = new Button();
            lbCritical = new Label();
            cbCritical = new CheckBox();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // tbName
            // 
            tbName.Location = new Point(166, 22);
            tbName.Name = "tbName";
            tbName.Size = new Size(183, 23);
            tbName.TabIndex = 2;
            // 
            // tbDescription
            // 
            tbDescription.Location = new Point(166, 65);
            tbDescription.Name = "tbDescription";
            tbDescription.Size = new Size(183, 23);
            tbDescription.TabIndex = 3;
            // 
            // lbDescription
            // 
            lbDescription.Font = new Font("Segoe UI", 11F);
            lbDescription.Location = new Point(12, 65);
            lbDescription.Name = "lbDescription";
            lbDescription.Size = new Size(100, 23);
            lbDescription.TabIndex = 4;
            lbDescription.Text = "Description";
            lbDescription.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbName
            // 
            lbName.Font = new Font("Segoe UI", 11F);
            lbName.Location = new Point(12, 22);
            lbName.Name = "lbName";
            lbName.Size = new Size(100, 23);
            lbName.TabIndex = 5;
            lbName.Text = "Name";
            lbName.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbfieldDevice
            // 
            lbfieldDevice.Font = new Font("Segoe UI", 11F);
            lbfieldDevice.Location = new Point(12, 117);
            lbfieldDevice.Name = "lbfieldDevice";
            lbfieldDevice.Size = new Size(100, 23);
            lbfieldDevice.TabIndex = 9;
            lbfieldDevice.Text = "Device";
            lbfieldDevice.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbParameter
            // 
            lbParameter.Font = new Font("Segoe UI", 11F);
            lbParameter.Location = new Point(12, 164);
            lbParameter.Name = "lbParameter";
            lbParameter.Size = new Size(100, 23);
            lbParameter.TabIndex = 10;
            lbParameter.Text = " Parameter";
            lbParameter.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // cmbParameter
            // 
            cmbParameter.FormattingEnabled = true;
            cmbParameter.Location = new Point(166, 164);
            cmbParameter.Name = "cmbParameter";
            cmbParameter.Size = new Size(183, 23);
            cmbParameter.TabIndex = 13;
            // 
            // cmbDevice
            // 
            cmbDevice.FormattingEnabled = true;
            cmbDevice.Location = new Point(166, 117);
            cmbDevice.Name = "cmbDevice";
            cmbDevice.Size = new Size(183, 23);
            cmbDevice.TabIndex = 14;
            cmbDevice.SelectedIndexChanged += cmbDevice_SelectedIndexChanged;
            // 
            // lbHigh
            // 
            lbHigh.Font = new Font("Segoe UI", 11F);
            lbHigh.Location = new Point(12, 263);
            lbHigh.Name = "lbHigh";
            lbHigh.Size = new Size(114, 23);
            lbHigh.TabIndex = 15;
            lbHigh.Text = "High Set Point";
            lbHigh.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbLow
            // 
            lbLow.Font = new Font("Segoe UI", 11F);
            lbLow.Location = new Point(12, 212);
            lbLow.Name = "lbLow";
            lbLow.Size = new Size(100, 23);
            lbLow.TabIndex = 16;
            lbLow.Text = "Low Set Point";
            lbLow.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tbLow
            // 
            tbLow.Location = new Point(166, 212);
            tbLow.Name = "tbLow";
            tbLow.Size = new Size(183, 23);
            tbLow.TabIndex = 17;
            // 
            // tbHigh
            // 
            tbHigh.Location = new Point(166, 263);
            tbHigh.Name = "tbHigh";
            tbHigh.Size = new Size(183, 23);
            tbHigh.TabIndex = 18;
            // 
            // btnSave
            // 
            btnSave.Font = new Font("Segoe UI", 11F);
            btnSave.Location = new Point(137, 371);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(80, 31);
            btnSave.TabIndex = 19;
            btnSave.Text = "Create";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // lbCritical
            // 
            lbCritical.Font = new Font("Segoe UI", 11F);
            lbCritical.Location = new Point(12, 311);
            lbCritical.Name = "lbCritical";
            lbCritical.Size = new Size(100, 23);
            lbCritical.TabIndex = 20;
            lbCritical.Text = "Crictical";
            lbCritical.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // cbCritical
            // 
            cbCritical.AutoSize = true;
            cbCritical.Location = new Point(166, 311);
            cbCritical.Name = "cbCritical";
            cbCritical.Size = new Size(15, 14);
            cbCritical.TabIndex = 21;
            cbCritical.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Font = new Font("Segoe UI", 11F);
            btnCancel.Location = new Point(244, 371);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 31);
            btnCancel.TabIndex = 22;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // AddAlarm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(369, 412);
            Controls.Add(btnCancel);
            Controls.Add(cbCritical);
            Controls.Add(lbCritical);
            Controls.Add(btnSave);
            Controls.Add(tbHigh);
            Controls.Add(tbLow);
            Controls.Add(lbLow);
            Controls.Add(lbHigh);
            Controls.Add(cmbDevice);
            Controls.Add(cmbParameter);
            Controls.Add(lbParameter);
            Controls.Add(lbfieldDevice);
            Controls.Add(lbName);
            Controls.Add(lbDescription);
            Controls.Add(tbDescription);
            Controls.Add(tbName);
            Name = "AddAlarm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "AddAlarm";
            Load += AddAlarm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox tbName;
        private TextBox tbDescription;
        private Label lbDescription;
        private Label lbName;
        private ComboBox comboBox1;
        private Label lbfieldDevice;
        private Label lbParameter;
        private ComboBox cmbSerialDevice;
        private ComboBox cmbParameter;
        private ComboBox cmbDevice;
        private Label lbHigh;
        private Label lbLow;
        private TextBox tbLow;
        private TextBox tbHigh;
        private Button btnSave;
        private Label lbCritical;
        private CheckBox cbCritical;
        private Button btnCancel;
    }
}