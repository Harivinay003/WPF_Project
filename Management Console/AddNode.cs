using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows.Forms;
using VirtualEMS.DataServices;
using VirtualEMS.Library;

namespace ManagementConsole
{
    public partial class AddNode : Form
    {
        private readonly FormMode _mode;
        private readonly Node _node;
        private bool _isSaving = false;

        public AddNode()
        {
            InitializeComponent();
            _mode = FormMode.Add;
        }

        public AddNode(Node node)
        {
            InitializeComponent();
            _mode = FormMode.Edit;
            _node = node;
        }

        private void AddNode_Load(object sender, EventArgs e)
        {
            cmbType.DataSource = Enum.GetValues(typeof(NodeType));

            cmbParent.DataSource = Main.Nodes;
            cmbParent.DisplayMember = "Name";
            cmbParent.ValueMember = "Id";
            cmbParent.SelectedIndex = -1;

            cmbFeeder.DataSource = Main.SerialDevices;
            cmbFeeder.DisplayMember = "Name";
            cmbFeeder.ValueMember = "Id";
            cmbFeeder.SelectedIndex = -1;

            cmbFieldDevice.DataSource = Main.FieldDevices;
            cmbFieldDevice.DisplayMember = "Name";
            cmbFieldDevice.ValueMember = "Id";
            cmbFieldDevice.SelectedIndex = -1;

            if (_mode == FormMode.Edit && _node != null)
            {
                txtName.Text = _node.Name;
                txtDescription.Text = _node.Description;
                txtTitle.Text = _node.Title;
                txtFormula.Text = _node.Formula;

                cmbType.SelectedItem = _node.Type;

                if (_node.ParentId.HasValue)
                    cmbParent.SelectedValue = _node.ParentId.Value;
                else
                    cmbParent.SelectedIndex = -1;

                if (_node.FeederId.HasValue)
                    cmbFeeder.SelectedValue = _node.FeederId.Value;
                else
                    cmbFeeder.SelectedIndex = -1;

                if (_node.FieldDeviceId.HasValue)
                    cmbFieldDevice.SelectedValue = _node.FieldDeviceId.Value;
                else
                    cmbFieldDevice.SelectedIndex = -1;

                ApplySelectionLogic();

                btnCreate.Text = "Update";
            }
        }

        private void ApplySelectionLogic()
        {
            var type = (NodeType)cmbType.SelectedItem;

            if (type == NodeType.Section)
            {
                cmbFeeder.Enabled = false;
                cmbFieldDevice.Enabled = false;

                cmbFeeder.SelectedIndex = -1;
                cmbFieldDevice.SelectedIndex = -1;
                return;
            }
            if (type == NodeType.Feeder)
            {
                cmbFeeder.Enabled = true;
                cmbFieldDevice.Enabled = false;
                return ;
            }

            if (type == NodeType.FieldDevice)
            {
                cmbFeeder.Enabled = false;
                cmbFieldDevice.Enabled = true;
                return ;
            }

            cmbFeeder.Enabled = true;
            cmbFieldDevice.Enabled = true;
        }

        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplySelectionLogic();
        }

        private void cmbFeeder_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplySelectionLogic();
        }

        private void cmbFieldDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplySelectionLogic();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (_isSaving) return;
            _isSaving = true;

            if (!ValidateForm())
            {
                _isSaving = false;
                return;
            }

            if (_mode == FormMode.Add)
                AddToDb();
            else
                UpdateInDb();

            DialogResult = DialogResult.OK;
            Close();
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Name is required");
                return false;
            }

            if (cmbType.SelectedItem == null)
            {
                MessageBox.Show("Select a node type");
                return false;
            }

            return true;
        }

        private void AddToDb()
        {
            var node = new Node
            {
                Name = txtName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                Title = txtTitle.Text.Trim(),
                Type = (NodeType)cmbType.SelectedItem,

                ParentId = cmbParent.SelectedValue == null
                    ? (int?)null
                    : Convert.ToInt32(cmbParent.SelectedValue),

                FeederId = cmbFeeder.SelectedValue == null
                    ? (int?)null
                    : Convert.ToInt32(cmbFeeder.SelectedValue),

                FieldDeviceId = cmbFieldDevice.SelectedValue == null
                    ? (int?)null
                    : Convert.ToInt32(cmbFieldDevice.SelectedValue),

                Formula = txtFormula.Text.Trim()
            };

            using var context = CreateContext();
            context.Nodes.Add(node);
            context.SaveChanges();
        }

        private void UpdateInDb()
        {
            using var context = CreateContext();

            var dbNode = context.Nodes.FirstOrDefault(n => n.Id == _node.Id);
            if (dbNode == null) return;

            dbNode.Name = txtName.Text.Trim();
            dbNode.Description = txtDescription.Text.Trim();
            dbNode.Title = txtTitle.Text.Trim();
            dbNode.Type = (NodeType)cmbType.SelectedItem;

            dbNode.ParentId = cmbParent.SelectedValue == null
                ? (int?)null
                : Convert.ToInt32(cmbParent.SelectedValue);

            dbNode.FeederId = cmbFeeder.SelectedValue == null
                ? (int?)null
                : Convert.ToInt32(cmbFeeder.SelectedValue);

            dbNode.FieldDeviceId = cmbFieldDevice.SelectedValue == null
                ? (int?)null
                : Convert.ToInt32(cmbFieldDevice.SelectedValue);

            dbNode.Formula = txtFormula.Text.Trim();

            context.SaveChanges();

            _node.Name = dbNode.Name;
            _node.Description = dbNode.Description;
            _node.Title = dbNode.Title;
            _node.Type = dbNode.Type;
            _node.ParentId = dbNode.ParentId;
            _node.FeederId = dbNode.FeederId;
            _node.FieldDeviceId = dbNode.FieldDeviceId;
            _node.Formula = dbNode.Formula;
        }

        private AppDbContext CreateContext()
        {
            return new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(Main.ConnectionString)
                .Options);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}