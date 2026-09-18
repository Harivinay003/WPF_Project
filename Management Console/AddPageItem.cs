using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using VirtualEMS.Library;

namespace ManagementConsole
{
    public partial class AddPageItem : Form
    {
        private readonly FormMode _mode;
        private readonly PageItem _editItem;
        private bool _loading;
        private readonly Dictionary<int, TextBox> _formulaMap = new();

        public PageItem Result { get; private set; }

        public AddPageItem()
        {
            InitializeComponent();
            _mode = FormMode.Add;
        }

        public AddPageItem(PageItem item) : this()
        {
            _mode = FormMode.Edit;
            _editItem = item;
        }

        private void AddPageItem_Load(object sender, EventArgs e)
        {
            _loading = true;

            LoadDevices();
            cmbSerialDevice.SelectedIndex = -1;

            if (_mode == FormMode.Edit && _editItem != null)
            {
                txtName.Text = _editItem.Name;
                txtTitle.Text = _editItem.Title;
                txtDescription.Text = _editItem.Description;
                btnCreate.Text = "Update";

                RestoreEditData();
            }

            _loading = false;
        }

        private void LoadDevices()
        {
            var deviceList = new List<DeviceComboItem>();

            deviceList.AddRange(Main.FieldDevices.Select(f => new DeviceComboItem
            {
                Id = f.Id,
                Name = $" {f.Name}",
                Type = "Field"
            }));

            deviceList.AddRange(Main.SerialDevices.Select(s => new DeviceComboItem
            {
                Id = s.Id,
                Name = $"{s.Name}",
                Type = "Serial"
            }));

            cmbSerialDevice.DataSource = deviceList;
            cmbSerialDevice.DisplayMember = "Name";
            cmbSerialDevice.ValueMember = "Id";
        }


        private void LoadTags(int? fieldDeviceId)
        {
            flpParameters.Controls.Clear();
            _formulaMap.Clear();

            var tags = Main.Tags
                .Where(t => t.FieldDeviceId == fieldDeviceId)
                .ToList();

            foreach (var tag in tags)
            {
                var panel = new Panel
                {
                    Width = flpParameters.Width - 25,
                    Height = 30
                };

                var chk = new CheckBox
                {
                    Left = 0,
                    Top = 5,
                    Width = 20,
                    Tag = tag
                };

                var lbl = new Label
                {
                    Left = 25,
                    Top = 7,
                    Width = 220,
                    Text = $"{tag.Name} ({tag.Id})"
                };

                panel.Controls.Add(chk);
                panel.Controls.Add(lbl);

                flpParameters.Controls.Add(panel);
            }
        }

        private void LoadParameters(int deviceId)
        {
            flpParameters.Controls.Clear();
            _formulaMap.Clear();

            var device = Main.SerialDevices.FirstOrDefault(d => d.Id == deviceId);
            if (device == null) return;

            var paramIds = Main.SerialDeviceRegisters
                .Where(r => r.DriverId == device.DriverId)
                .Select(r => r.ParameterId)
                .Distinct()
                .ToList();

            var parameters = Main.SerialDeviceParameters
                .Where(p => paramIds.Contains(p.Id))
                .ToList();

            foreach (var param in parameters)
            {
                var panel = new Panel
                {
                    Width = flpParameters.Width - 25,
                    Height = 30
                };

                var chk = new CheckBox
                {
                    Left = 0,
                    Top = 5,
                    Width = 20,
                    Tag = param
                };

                var lbl = new Label
                {
                    Left = 25,
                    Top = 7,
                    Width = 150,
                    Text = $"{param.Name} ({param.Id})"
                };

                var txtFormula = new TextBox
                {
                    Left = 180,
                    Top = 3,
                    Width = 120
                };

                panel.Controls.Add(chk);
                panel.Controls.Add(lbl);
                panel.Controls.Add(txtFormula);

                flpParameters.Controls.Add(panel);

                _formulaMap[param.Id] = txtFormula;
            }
        }


        private void cmbSerialDevice_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (_loading) return;

            var device = cmbSerialDevice.SelectedItem as DeviceComboItem;
            if (device == null) return;

            if (device.Type == "Field")
                LoadTags(device.Id);
            else
                LoadParameters(device.Id);
        }


        private void RestoreEditData()
        {
            // Restore Serial Parameters
            if (_editItem.PageItemParameters != null && _editItem.PageItemParameters.Any())
            {
                int serialId = _editItem.PageItemParameters.First().SerialDeviceId;

                cmbSerialDevice.SelectedValue = serialId;

                LoadParameters(serialId);

                foreach (Panel pnl in flpParameters.Controls)
                {
                    var chk = pnl.Controls.OfType<CheckBox>().First();
                    var param = chk.Tag as SerialDeviceParameter;

                    var existing = _editItem.PageItemParameters
                        .FirstOrDefault(p => p.SerialDeviceParameterId == param.Id);

                    if (existing != null)
                    {
                        chk.Checked = true;

                        if (_formulaMap.TryGetValue(param.Id, out var txt))
                            txt.Text = existing.Formula;
                    }
                }
            }
            if (_editItem.PageItemTags != null && _editItem.PageItemTags.Any())
            {
                var firstTag = Main.Tags.FirstOrDefault(t => t.Id == _editItem.PageItemTags.First().TagId);

                if (firstTag != null)
                {
                    cmbSerialDevice.SelectedValue = firstTag.FieldDeviceId;

                    LoadTags(firstTag.FieldDeviceId);

                    var selectedTags = _editItem.PageItemTags.Select(t => t.TagId).ToList();

                    foreach (Panel pnl in flpParameters.Controls)
                    {
                        var chk = pnl.Controls.OfType<CheckBox>().First();
                        var tag = chk.Tag as Tag;

                        if (selectedTags.Contains(tag.Id))
                            chk.Checked = true;
                    }
                }
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            Result ??= new PageItem();

            Result.Name = txtName.Text.Trim();
            Result.Title = txtTitle.Text.Trim();
            Result.Description = txtDescription.Text.Trim();

            Result.PageItemTags = new List<PageItemTag>();
            Result.PageItemParameters = new List<PageItemParameter>();

            foreach (Panel pnl in flpParameters.Controls)
            {
                var chk = pnl.Controls.OfType<CheckBox>().First();

                if (!chk.Checked) continue;

                if (chk.Tag is Tag tag)
                {
                    Result.PageItemTags.Add(new PageItemTag
                    {
                        TagId = tag.Id
                    });
                }
                else if (chk.Tag is SerialDeviceParameter param)
                {
                    var formula = _formulaMap.ContainsKey(param.Id)
                        ? _formulaMap[param.Id].Text
                        : "";

                    Result.PageItemParameters.Add(new PageItemParameter
                    {
                        SerialDeviceId = (int)cmbSerialDevice.SelectedValue,
                        SerialDeviceParameterId = param.Id,
                        Formula = formula
                    });
                }
            }

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

            return true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }

    public class DeviceComboItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}