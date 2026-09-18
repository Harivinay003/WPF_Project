namespace ManagementConsole
{
    partial class AddPage
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
            txtDescription = new TextBox();
            txtName = new TextBox();
            btnCancel = new Button();
            btnCreate = new Button();
            lbTitle = new Label();
            label1 = new Label();
            dgItems = new DataGridView();
            btnAdd = new Button();
            button2 = new Button();
            txtGroup = new TextBox();
            Group = new Label();
            btnDelete = new Button();
            ((System.ComponentModel.ISupportInitialize)dgItems).BeginInit();
            SuspendLayout();
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(552, 23);
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(169, 23);
            txtDescription.TabIndex = 25;
            // 
            // txtName
            // 
            txtName.Location = new Point(314, 23);
            txtName.Name = "txtName";
            txtName.Size = new Size(169, 23);
            txtName.TabIndex = 26;
            // 
            // btnCancel
            // 
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Location = new Point(119, 516);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 23;
            btnCancel.Text = "CANCEL";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnCreate
            // 
            btnCreate.FlatStyle = FlatStyle.Flat;
            btnCreate.Location = new Point(12, 516);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(75, 23);
            btnCreate.TabIndex = 24;
            btnCreate.Text = "CREATE";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;
            // 
            // lbTitle
            // 
            lbTitle.AutoSize = true;
            lbTitle.Location = new Point(513, 26);
            lbTitle.Name = "lbTitle";
            lbTitle.Size = new Size(33, 15);
            lbTitle.TabIndex = 21;
            lbTitle.Text = "Title:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(266, 26);
            label1.Name = "label1";
            label1.Size = new Size(42, 15);
            label1.TabIndex = 22;
            label1.Text = "Name:";
            // 
            // dgItems
            // 
            dgItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgItems.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgItems.Location = new Point(12, 106);
            dgItems.Name = "dgItems";
            dgItems.Size = new Size(756, 404);
            dgItems.TabIndex = 27;
            // 
            // btnAdd
            // 
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.Location = new Point(12, 77);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(75, 23);
            btnAdd.TabIndex = 28;
            btnAdd.Text = "Add";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAddItem_Click;
            // 
            // button2
            // 
            button2.FlatStyle = FlatStyle.Flat;
            button2.Location = new Point(119, 77);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 29;
            button2.Text = "Edit";
            button2.UseVisualStyleBackColor = true;
            button2.Click += btnEditItem_Click;
            // 
            // txtGroup
            // 
            txtGroup.Location = new Point(60, 23);
            txtGroup.Name = "txtGroup";
            txtGroup.Size = new Size(169, 23);
            txtGroup.TabIndex = 31;
            // 
            // Group
            // 
            Group.AutoSize = true;
            Group.Location = new Point(12, 26);
            Group.Name = "Group";
            Group.Size = new Size(43, 15);
            Group.TabIndex = 30;
            Group.Text = "Group:";
            // 
            // btnDelete
            // 
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.Location = new Point(224, 77);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(75, 23);
            btnDelete.TabIndex = 32;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // AddPage
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(780, 570);
            Controls.Add(btnDelete);
            Controls.Add(txtGroup);
            Controls.Add(Group);
            Controls.Add(button2);
            Controls.Add(btnAdd);
            Controls.Add(dgItems);
            Controls.Add(txtDescription);
            Controls.Add(txtName);
            Controls.Add(btnCancel);
            Controls.Add(btnCreate);
            Controls.Add(lbTitle);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "AddPage";
            StartPosition = FormStartPosition.CenterParent;
            Text = "AddPage";
            Load += AddPage_Load;
            ((System.ComponentModel.ISupportInitialize)dgItems).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtDescription;
        private TextBox txtName;
        private Button btnCancel;
        private Button btnCreate;
        private Label lbTitle;
        private Label label1;
        private DataGridView dgItems;
        private Button btnAdd;
        private Button button2;
        private TextBox txtGroup;
        private Label Group;
        private Button btnDelete;
    }
}