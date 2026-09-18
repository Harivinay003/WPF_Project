namespace ManagementConsole
{
    partial class AddPageItem
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
            txtTitle = new TextBox();
            txtDescription = new TextBox();
            txtName = new TextBox();
            btnCancel = new Button();
            btnCreate = new Button();
            lbTitle = new Label();
            label2 = new Label();
            label1 = new Label();
            cmbSerialDevice = new ComboBox();
            label4 = new Label();
            label5 = new Label();
            flpParameters = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // txtTitle
            // 
            txtTitle.Location = new Point(155, 105);
            txtTitle.Name = "txtTitle";
            txtTitle.Size = new Size(217, 23);
            txtTitle.TabIndex = 8;
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(155, 76);
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(217, 23);
            txtDescription.TabIndex = 9;
            // 
            // txtName
            // 
            txtName.Location = new Point(155, 47);
            txtName.Name = "txtName";
            txtName.Size = new Size(217, 23);
            txtName.TabIndex = 10;
            // 
            // btnCancel
            // 
            btnCancel.CausesValidation = false;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Location = new Point(257, 313);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "CANCEL";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnCreate
            // 
            btnCreate.CausesValidation = false;
            btnCreate.DialogResult = DialogResult.OK;
            btnCreate.FlatStyle = FlatStyle.Flat;
            btnCreate.Location = new Point(155, 313);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(75, 23);
            btnCreate.TabIndex = 7;
            btnCreate.Text = "CREATE";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;
            // 
            // lbTitle
            // 
            lbTitle.AutoSize = true;
            lbTitle.Location = new Point(39, 108);
            lbTitle.Name = "lbTitle";
            lbTitle.Size = new Size(33, 15);
            lbTitle.TabIndex = 3;
            lbTitle.Text = "Title:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(39, 79);
            label2.Name = "label2";
            label2.Size = new Size(70, 15);
            label2.TabIndex = 4;
            label2.Text = "Description:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(39, 50);
            label1.Name = "label1";
            label1.Size = new Size(42, 15);
            label1.TabIndex = 5;
            label1.Text = "Name:";
            // 
            // cmbSerialDevice
            // 
            cmbSerialDevice.FormattingEnabled = true;
            cmbSerialDevice.Location = new Point(155, 142);
            cmbSerialDevice.Name = "cmbSerialDevice";
            cmbSerialDevice.Size = new Size(217, 23);
            cmbSerialDevice.TabIndex = 13;
            cmbSerialDevice.SelectedIndexChanged += cmbSerialDevice_SelectionChangeCommitted;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(39, 142);
            label4.Name = "label4";
            label4.Size = new Size(42, 15);
            label4.TabIndex = 15;
            label4.Text = "Device";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(39, 188);
            label5.Name = "label5";
            label5.Size = new Size(61, 15);
            label5.TabIndex = 16;
            label5.Text = "Parameter";
            // 
            // flpParameters
            // 
            flpParameters.AutoScroll = true;
            flpParameters.FlowDirection = FlowDirection.TopDown;
            flpParameters.Location = new Point(155, 171);
            flpParameters.Name = "flpParameters";
            flpParameters.Size = new Size(363, 100);
            flpParameters.TabIndex = 18;
            flpParameters.WrapContents = false;
            // 
            // AddPageItem
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(530, 353);
            Controls.Add(flpParameters);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(cmbSerialDevice);
            Controls.Add(txtTitle);
            Controls.Add(txtDescription);
            Controls.Add(txtName);
            Controls.Add(btnCancel);
            Controls.Add(btnCreate);
            Controls.Add(lbTitle);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "AddPageItem";
            StartPosition = FormStartPosition.CenterParent;
            Text = "PageItem";
            Load += AddPageItem_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtTitle;
        private TextBox txtDescription;
        private TextBox txtName;
        private Button btnCancel;
        private Button btnCreate;
        private Label lbTitle;
        private Label label2;
        private Label label1;
        private ComboBox cmbSerialDevice;
        private Label label4;
        private Label label5;
        private FlowLayoutPanel flpParameters;
    }
}