namespace ManagementConsole
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            leftPanel = new Panel();
            lbTrends = new Label();
            btnTrends = new Button();
            lbTags = new Label();
            btnTags = new Button();
            lbAlarms = new Label();
            btnAlarm = new Button();
            btnPage = new Button();
            btnNode = new Button();
            lbPage = new Label();
            lbNode = new Label();
            lblDeviceTypes = new Label();
            lblSerialDevices = new Label();
            lblFieldDevices = new Label();
            lblEthernetDevices = new Label();
            btnDeviceTypes = new Button();
            btnSerialDevices = new Button();
            btnFieldDevices = new Button();
            btnEthernetDevbices = new Button();
            topPanel = new Panel();
            btnEdit = new Button();
            btnAddNew = new Button();
            dgView = new DataGridView();
            btnDelete = new Button();
            leftPanel.SuspendLayout();
            topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgView).BeginInit();
            SuspendLayout();
            // 
            // leftPanel
            // 
            leftPanel.Controls.Add(lbTrends);
            leftPanel.Controls.Add(btnTrends);
            leftPanel.Controls.Add(lbTags);
            leftPanel.Controls.Add(btnTags);
            leftPanel.Controls.Add(lbAlarms);
            leftPanel.Controls.Add(btnAlarm);
            leftPanel.Controls.Add(btnPage);
            leftPanel.Controls.Add(btnNode);
            leftPanel.Controls.Add(lbPage);
            leftPanel.Controls.Add(lbNode);
            leftPanel.Controls.Add(lblDeviceTypes);
            leftPanel.Controls.Add(lblSerialDevices);
            leftPanel.Controls.Add(lblFieldDevices);
            leftPanel.Controls.Add(lblEthernetDevices);
            leftPanel.Controls.Add(btnDeviceTypes);
            leftPanel.Controls.Add(btnSerialDevices);
            leftPanel.Controls.Add(btnFieldDevices);
            leftPanel.Controls.Add(btnEthernetDevbices);
            leftPanel.Dock = DockStyle.Left;
            leftPanel.Font = new Font("Century Gothic", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            leftPanel.Location = new Point(0, 0);
            leftPanel.Name = "leftPanel";
            leftPanel.Size = new Size(110, 848);
            leftPanel.TabIndex = 0;
            // 
            // lbTrends
            // 
            lbTrends.Location = new Point(15, 733);
            lbTrends.Name = "lbTrends";
            lbTrends.Size = new Size(77, 20);
            lbTrends.TabIndex = 11;
            lbTrends.Text = "Trends";
            lbTrends.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnTrends
            // 
            btnTrends.BackgroundImage = Properties.Resources.trends;
            btnTrends.BackgroundImageLayout = ImageLayout.Zoom;
            btnTrends.FlatStyle = FlatStyle.Flat;
            btnTrends.Location = new Point(28, 680);
            btnTrends.Name = "btnTrends";
            btnTrends.Size = new Size(50, 50);
            btnTrends.TabIndex = 10;
            btnTrends.UseVisualStyleBackColor = true;
            btnTrends.Click += btnTrends_Click;
            // 
            // lbTags
            // 
            lbTags.Location = new Point(15, 657);
            lbTags.Name = "lbTags";
            lbTags.Size = new Size(77, 20);
            lbTags.TabIndex = 9;
            lbTags.Text = "Tags";
            lbTags.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnTags
            // 
            btnTags.BackgroundImage = Properties.Resources.tags;
            btnTags.BackgroundImageLayout = ImageLayout.Zoom;
            btnTags.FlatStyle = FlatStyle.Flat;
            btnTags.Location = new Point(28, 604);
            btnTags.Name = "btnTags";
            btnTags.Size = new Size(50, 50);
            btnTags.TabIndex = 8;
            btnTags.UseVisualStyleBackColor = true;
            btnTags.Click += btnTags_Click;
            // 
            // lbAlarms
            // 
            lbAlarms.Location = new Point(15, 581);
            lbAlarms.Name = "lbAlarms";
            lbAlarms.Size = new Size(77, 20);
            lbAlarms.TabIndex = 7;
            lbAlarms.Text = "Alarms";
            lbAlarms.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnAlarm
            // 
            btnAlarm.BackgroundImage = Properties.Resources.alarms;
            btnAlarm.BackgroundImageLayout = ImageLayout.Zoom;
            btnAlarm.FlatStyle = FlatStyle.Flat;
            btnAlarm.Location = new Point(28, 528);
            btnAlarm.Name = "btnAlarm";
            btnAlarm.Size = new Size(50, 50);
            btnAlarm.TabIndex = 6;
            btnAlarm.UseVisualStyleBackColor = true;
            btnAlarm.Click += btnAlarm_Click;
            // 
            // btnPage
            // 
            btnPage.BackgroundImage = Properties.Resources.landing_page;
            btnPage.BackgroundImageLayout = ImageLayout.Zoom;
            btnPage.FlatStyle = FlatStyle.Flat;
            btnPage.Location = new Point(28, 452);
            btnPage.Name = "btnPage";
            btnPage.Size = new Size(50, 50);
            btnPage.TabIndex = 5;
            btnPage.UseVisualStyleBackColor = true;
            btnPage.Click += btnPage_Click;
            // 
            // btnNode
            // 
            btnNode.BackgroundImage = Properties.Resources.heirarchy;
            btnNode.BackgroundImageLayout = ImageLayout.Zoom;
            btnNode.FlatStyle = FlatStyle.Flat;
            btnNode.Location = new Point(28, 376);
            btnNode.Name = "btnNode";
            btnNode.Size = new Size(50, 50);
            btnNode.TabIndex = 4;
            btnNode.UseVisualStyleBackColor = true;
            btnNode.Click += btnNode_Click;
            // 
            // lbPage
            // 
            lbPage.Location = new Point(15, 505);
            lbPage.Name = "lbPage";
            lbPage.Size = new Size(77, 20);
            lbPage.TabIndex = 3;
            lbPage.Text = "Pages";
            lbPage.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbNode
            // 
            lbNode.Location = new Point(15, 429);
            lbNode.Name = "lbNode";
            lbNode.Size = new Size(77, 20);
            lbNode.TabIndex = 2;
            lbNode.Text = "Nodes";
            lbNode.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblDeviceTypes
            // 
            lblDeviceTypes.Location = new Point(9, 338);
            lblDeviceTypes.Name = "lblDeviceTypes";
            lblDeviceTypes.Size = new Size(95, 35);
            lblDeviceTypes.TabIndex = 1;
            lblDeviceTypes.Text = "Serial Device Drivers";
            lblDeviceTypes.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblSerialDevices
            // 
            lblSerialDevices.Location = new Point(15, 247);
            lblSerialDevices.Name = "lblSerialDevices";
            lblSerialDevices.Size = new Size(77, 35);
            lblSerialDevices.TabIndex = 1;
            lblSerialDevices.Text = "Serial Devices";
            lblSerialDevices.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblFieldDevices
            // 
            lblFieldDevices.Location = new Point(15, 156);
            lblFieldDevices.Name = "lblFieldDevices";
            lblFieldDevices.Size = new Size(77, 35);
            lblFieldDevices.TabIndex = 1;
            lblFieldDevices.Text = "Field Devices";
            lblFieldDevices.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblEthernetDevices
            // 
            lblEthernetDevices.Location = new Point(15, 65);
            lblEthernetDevices.Name = "lblEthernetDevices";
            lblEthernetDevices.Size = new Size(77, 35);
            lblEthernetDevices.TabIndex = 1;
            lblEthernetDevices.Text = "IO Devices";
            lblEthernetDevices.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnDeviceTypes
            // 
            btnDeviceTypes.BackgroundImage = Properties.Resources.em6400ng1;
            btnDeviceTypes.BackgroundImageLayout = ImageLayout.Zoom;
            btnDeviceTypes.FlatStyle = FlatStyle.Flat;
            btnDeviceTypes.Location = new Point(28, 285);
            btnDeviceTypes.Name = "btnDeviceTypes";
            btnDeviceTypes.Size = new Size(50, 50);
            btnDeviceTypes.TabIndex = 1;
            btnDeviceTypes.UseVisualStyleBackColor = true;
            btnDeviceTypes.Click += btnDeviceTypes_Click;
            // 
            // btnSerialDevices
            // 
            btnSerialDevices.BackgroundImage = Properties.Resources.em6400ng1;
            btnSerialDevices.BackgroundImageLayout = ImageLayout.Zoom;
            btnSerialDevices.FlatStyle = FlatStyle.Flat;
            btnSerialDevices.Location = new Point(28, 194);
            btnSerialDevices.Name = "btnSerialDevices";
            btnSerialDevices.Size = new Size(50, 50);
            btnSerialDevices.TabIndex = 1;
            btnSerialDevices.UseVisualStyleBackColor = true;
            btnSerialDevices.Click += btnSerialDevices_Click;
            // 
            // btnFieldDevices
            // 
            btnFieldDevices.BackgroundImage = Properties.Resources.Gateway;
            btnFieldDevices.BackgroundImageLayout = ImageLayout.Zoom;
            btnFieldDevices.FlatStyle = FlatStyle.Flat;
            btnFieldDevices.Location = new Point(28, 103);
            btnFieldDevices.Name = "btnFieldDevices";
            btnFieldDevices.Size = new Size(50, 50);
            btnFieldDevices.TabIndex = 1;
            btnFieldDevices.UseVisualStyleBackColor = true;
            btnFieldDevices.Click += btnFieldDevices_Click;
            // 
            // btnEthernetDevbices
            // 
            btnEthernetDevbices.BackgroundImage = Properties.Resources.ion7500;
            btnEthernetDevbices.BackgroundImageLayout = ImageLayout.Zoom;
            btnEthernetDevbices.FlatStyle = FlatStyle.Flat;
            btnEthernetDevbices.Location = new Point(28, 12);
            btnEthernetDevbices.Name = "btnEthernetDevbices";
            btnEthernetDevbices.Size = new Size(50, 50);
            btnEthernetDevbices.TabIndex = 1;
            btnEthernetDevbices.UseVisualStyleBackColor = true;
            btnEthernetDevbices.Click += btnEthernetDevbices_Click;
            // 
            // topPanel
            // 
            topPanel.BorderStyle = BorderStyle.FixedSingle;
            topPanel.Controls.Add(btnDelete);
            topPanel.Controls.Add(btnEdit);
            topPanel.Controls.Add(btnAddNew);
            topPanel.Dock = DockStyle.Top;
            topPanel.Font = new Font("Century Gothic", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            topPanel.Location = new Point(110, 0);
            topPanel.Name = "topPanel";
            topPanel.Size = new Size(1215, 45);
            topPanel.TabIndex = 1;
            // 
            // btnEdit
            // 
            btnEdit.DialogResult = DialogResult.OK;
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.Location = new Point(133, 11);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(95, 23);
            btnEdit.TabIndex = 1;
            btnEdit.Text = "Edit";
            btnEdit.UseVisualStyleBackColor = true;
            btnEdit.Click += btnEdit_Click;
            // 
            // btnAddNew
            // 
            btnAddNew.DialogResult = DialogResult.OK;
            btnAddNew.FlatStyle = FlatStyle.Flat;
            btnAddNew.Location = new Point(5, 11);
            btnAddNew.Name = "btnAddNew";
            btnAddNew.Size = new Size(95, 23);
            btnAddNew.TabIndex = 0;
            btnAddNew.Text = "Add New";
            btnAddNew.UseVisualStyleBackColor = true;
            btnAddNew.Click += btnAddNew_Click;
            // 
            // dgView
            // 
            dgView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgView.BackgroundColor = Color.White;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Control;
            dataGridViewCellStyle2.Font = new Font("Century Gothic", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle2.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgView.Dock = DockStyle.Fill;
            dgView.Location = new Point(110, 45);
            dgView.Name = "dgView";
            dgView.Size = new Size(1215, 803);
            dgView.TabIndex = 2;
            // 
            // btnDelete
            // 
            btnDelete.DialogResult = DialogResult.OK;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.Location = new Point(261, 11);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(95, 23);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1325, 848);
            Controls.Add(dgView);
            Controls.Add(topPanel);
            Controls.Add(leftPanel);
            Name = "Main";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ManagementConsole";
            WindowState = FormWindowState.Maximized;
            Load += Main_Load;
            leftPanel.ResumeLayout(false);
            topPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgView).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel leftPanel;
        private Button btnEthernetDevbices;
        private Label lblEthernetDevices;
        private Label lblDeviceTypes;
        private Label lblSerialDevices;
        private Label lblFieldDevices;
        private Button btnDeviceTypes;
        private Button btnSerialDevices;
        private Button btnFieldDevices;
        private Panel topPanel;
        private Button btnAddNew;
        private DataGridView dgView;
        private Button btnPage;
        private Button btnNode;
        private Label lbPage;
        private Label lbNode;
        private Button btnEdit;
        private Label lbAlarms;
        private Button btnAlarm;
        private Label lbTags;
        private Button btnTags;
        private Label lbTrends;
        private Button btnTrends;
        private Button btnDelete;
    }
}
