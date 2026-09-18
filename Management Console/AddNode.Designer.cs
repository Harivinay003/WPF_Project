namespace ManagementConsole
{
    partial class AddNode
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
            lbType = new Label();
            lbTitle = new Label();
            lbDescription = new Label();
            lbName = new Label();
            cmbType = new ComboBox();
            lbParent = new Label();
            lbFeeder = new Label();
            lbFieldDevice = new Label();
            cmbParent = new ComboBox();
            cmbFeeder = new ComboBox();
            cmbFieldDevice = new ComboBox();
            txtFormula = new TextBox();
            label1 = new Label();
            SuspendLayout();
            // 
            // txtTitle
            // 
            txtTitle.Location = new Point(136, 81);
            txtTitle.Name = "txtTitle";
            txtTitle.Size = new Size(169, 23);
            txtTitle.TabIndex = 28;
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(136, 50);
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(169, 23);
            txtDescription.TabIndex = 29;
            // 
            // txtName
            // 
            txtName.Location = new Point(136, 17);
            txtName.Name = "txtName";
            txtName.Size = new Size(169, 23);
            txtName.TabIndex = 30;
            // 
            // btnCancel
            // 
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Location = new Point(230, 282);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 25;
            btnCancel.Text = "CANCEL";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnCreate
            // 
            btnCreate.FlatStyle = FlatStyle.Flat;
            btnCreate.Location = new Point(136, 282);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(75, 23);
            btnCreate.TabIndex = 26;
            btnCreate.Text = "CREATE";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;
            // 
            // lbType
            // 
            lbType.AutoSize = true;
            lbType.Location = new Point(36, 120);
            lbType.Name = "lbType";
            lbType.Size = new Size(35, 15);
            lbType.TabIndex = 21;
            lbType.Text = "Type:";
            // 
            // lbTitle
            // 
            lbTitle.AutoSize = true;
            lbTitle.Location = new Point(36, 89);
            lbTitle.Name = "lbTitle";
            lbTitle.Size = new Size(33, 15);
            lbTitle.TabIndex = 22;
            lbTitle.Text = "Title:";
            // 
            // lbDescription
            // 
            lbDescription.AutoSize = true;
            lbDescription.Location = new Point(36, 58);
            lbDescription.Name = "lbDescription";
            lbDescription.Size = new Size(70, 15);
            lbDescription.TabIndex = 23;
            lbDescription.Text = "Description:";
            // 
            // lbName
            // 
            lbName.AutoSize = true;
            lbName.Location = new Point(36, 25);
            lbName.Name = "lbName";
            lbName.Size = new Size(42, 15);
            lbName.TabIndex = 24;
            lbName.Text = "Name:";
            // 
            // cmbType
            // 
            cmbType.FormattingEnabled = true;
            cmbType.Location = new Point(136, 117);
            cmbType.Name = "cmbType";
            cmbType.Size = new Size(169, 23);
            cmbType.TabIndex = 31;
            cmbType.SelectedIndexChanged += cmbType_SelectedIndexChanged;
            // 
            // lbParent
            // 
            lbParent.AutoSize = true;
            lbParent.Location = new Point(36, 155);
            lbParent.Name = "lbParent";
            lbParent.Size = new Size(41, 15);
            lbParent.TabIndex = 32;
            lbParent.Text = "Parent";
            // 
            // lbFeeder
            // 
            lbFeeder.AutoSize = true;
            lbFeeder.Location = new Point(36, 188);
            lbFeeder.Name = "lbFeeder";
            lbFeeder.Size = new Size(42, 15);
            lbFeeder.TabIndex = 33;
            lbFeeder.Text = "Feeder";
            // 
            // lbFieldDevice
            // 
            lbFieldDevice.AutoSize = true;
            lbFieldDevice.Location = new Point(36, 217);
            lbFieldDevice.Name = "lbFieldDevice";
            lbFieldDevice.Size = new Size(70, 15);
            lbFieldDevice.TabIndex = 34;
            lbFieldDevice.Text = "Field Device";
            // 
            // cmbParent
            // 
            cmbParent.FormattingEnabled = true;
            cmbParent.Location = new Point(136, 152);
            cmbParent.Name = "cmbParent";
            cmbParent.Size = new Size(169, 23);
            cmbParent.TabIndex = 35;
            // 
            // cmbFeeder
            // 
            cmbFeeder.FormattingEnabled = true;
            cmbFeeder.Location = new Point(136, 180);
            cmbFeeder.Name = "cmbFeeder";
            cmbFeeder.Size = new Size(169, 23);
            cmbFeeder.TabIndex = 36;
            cmbFeeder.SelectedIndexChanged += cmbFeeder_SelectedIndexChanged;
            // 
            // cmbFieldDevice
            // 
            cmbFieldDevice.FormattingEnabled = true;
            cmbFieldDevice.Location = new Point(136, 214);
            cmbFieldDevice.Name = "cmbFieldDevice";
            cmbFieldDevice.Size = new Size(169, 23);
            cmbFieldDevice.TabIndex = 37;
            cmbFieldDevice.SelectedIndexChanged += cmbFieldDevice_SelectedIndexChanged;
            // 
            // txtFormula
            // 
            txtFormula.Location = new Point(136, 251);
            txtFormula.Name = "txtFormula";
            txtFormula.Size = new Size(169, 23);
            txtFormula.TabIndex = 41;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(36, 254);
            label1.Name = "label1";
            label1.Size = new Size(51, 15);
            label1.TabIndex = 40;
            label1.Text = "Formula";
            // 
            // AddNode
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(398, 317);
            Controls.Add(txtFormula);
            Controls.Add(label1);
            Controls.Add(cmbFieldDevice);
            Controls.Add(cmbFeeder);
            Controls.Add(cmbParent);
            Controls.Add(lbFieldDevice);
            Controls.Add(lbFeeder);
            Controls.Add(lbParent);
            Controls.Add(cmbType);
            Controls.Add(txtTitle);
            Controls.Add(txtDescription);
            Controls.Add(txtName);
            Controls.Add(btnCancel);
            Controls.Add(btnCreate);
            Controls.Add(lbType);
            Controls.Add(lbTitle);
            Controls.Add(lbDescription);
            Controls.Add(lbName);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "AddNode";
            StartPosition = FormStartPosition.CenterParent;
            Text = "AddNode";
            Load += AddNode_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox txtTitle;
        private TextBox txtDescription;
        private TextBox txtName;
        private Button btnCancel;
        private Button btnCreate;
        private Label lbType;
        private Label lbTitle;
        private Label lbDescription;
        private Label lbName;
        private ComboBox cmbType;
        private Label lbParent;
        private Label lbFeeder;
        private Label lbFieldDevice;
        private ComboBox cmbParent;
        private ComboBox cmbFeeder;
        private ComboBox cmbFieldDevice;
        private TextBox txtFormula;
        private Label label1;
    }
}