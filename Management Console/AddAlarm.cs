using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
using System.Linq;
using System.Windows.Forms;
using VirtualEMS.DataServices;
using VirtualEMS.Library;

namespace ManagementConsole
{
    public partial class AddAlarm : Form
    {
        private AlarmTag editTag;
        private AlarmParameter editParameter;

        public AddAlarm()
        {
            InitializeComponent();
        }

        public AddAlarm(AlarmTag tag)
        {
            InitializeComponent();
            editTag = tag;
        }

        public AddAlarm(AlarmParameter parameter)
        {
            InitializeComponent();
            editParameter = parameter;
        }

        private AppDbContext CreateContext()
        {
            return new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(Main.ConnectionString)
                .Options);
        }

        private void AddAlarm_Load(object sender, EventArgs e)
        {
            LoadDevices();

            if (editTag != null)
            {
                tbName.Text = editTag.Name;
                tbDescription.Text = editTag.Description;

                cmbDevice.SelectedValue = editTag.FieldDeviceId;

                LoadParameters();

                cmbParameter.SelectedValue = editTag.TagId;

                tbLow.Text = editTag.LowSetPoint?.ToString();
                tbHigh.Text = editTag.HighSetPoint?.ToString();

                cbCritical.Checked = editTag.Critical;
            }

            if (editParameter != null)
            {
                tbName.Text = editParameter.Name;
                tbDescription.Text = editParameter.Description;

                cmbDevice.SelectedValue = editParameter.SerialDeviceId;

                LoadParameters();

                cmbParameter.SelectedValue = editParameter.SerialDeviceParameterId;

                tbLow.Text = editParameter.LowSetPoint?.ToString();
                tbHigh.Text = editParameter.HighSetPoint?.ToString();

                cbCritical.Checked = editParameter.Critical;
            }
        }
        private void LoadDevices()
        {
            var devices = Main.FieldDevices
                .Select(d => new DeviceItem
                {
                    Id = d.Id,
                    Name = d.Name,
                    Type = "Field"
                })
                .Concat(Main.SerialDevices.Select(d => new DeviceItem
                {
                    Id = d.Id,
                    Name = d.Name,
                    Type = "Serial"
                }))
                .ToList();

            cmbDevice.DataSource = devices;
            cmbDevice.DisplayMember = "Name";
            cmbDevice.ValueMember = "Id";
        }
        private void cmbDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadParameters();
        }
        private void LoadParameters()
        {
            var device = cmbDevice.SelectedItem as DeviceItem;

            if (device == null) return;

            if (device.Type == "Field")
            {
                cmbParameter.DataSource = Main.Tags
                    .Where(t => t.FieldDeviceId == device.Id)
                    .ToList();

                cmbParameter.DisplayMember = "Name";
                cmbParameter.ValueMember = "Id";
            }
            else
            {
                var serialDevice = Main.SerialDevices
                    .FirstOrDefault(d => d.Id == device.Id);

                if (serialDevice == null) return;

                var paramIds = Main.SerialDeviceRegisters
                    .Where(r => r.DriverId == serialDevice.DriverId)
                    .Select(r => r.ParameterId)
                    .Distinct()
                    .ToList();

                cmbParameter.DataSource = Main.SerialDeviceParameters
                    .Where(p => paramIds.Contains(p.Id))
                    .ToList();

                cmbParameter.DisplayMember = "Name";
                cmbParameter.ValueMember = "Id";
            }
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            using var context = CreateContext();

            var device = cmbDevice.SelectedItem as DeviceItem;

            if (device == null) return;

            if (device.Type == "Field")
            {
                AlarmTag alarm;

                if (editTag != null)
                    alarm = context.AlarmTags.Find(editTag.Id);
                else
                {
                    alarm = new AlarmTag();
                    context.AlarmTags.Add(alarm);
                }

                alarm.Name = tbName.Text;
                alarm.Description = tbDescription.Text;
                alarm.FieldDeviceId = device.Id;
                alarm.TagId = (int)cmbParameter.SelectedValue;
                alarm.LowSetPoint = string.IsNullOrEmpty(tbLow.Text) ? null : float.Parse(tbLow.Text);
                alarm.HighSetPoint = string.IsNullOrEmpty(tbHigh.Text) ? null : float.Parse(tbHigh.Text);
                alarm.Critical = cbCritical.Checked;
            }
            else
            {
                AlarmParameter alarm;

                if (editParameter != null)
                    alarm = context.AlarmParameters.Find(editParameter.Id);
                else
                {
                    alarm = new AlarmParameter();
                    context.AlarmParameters.Add(alarm);
                }

                alarm.Name = tbName.Text;
                alarm.Description = tbDescription.Text;
                alarm.SerialDeviceId = device.Id;
                alarm.SerialDeviceParameterId = (int)cmbParameter.SelectedValue;
                alarm.LowSetPoint = string.IsNullOrEmpty(tbLow.Text) ? null : float.Parse(tbLow.Text);
                alarm.HighSetPoint = string.IsNullOrEmpty(tbHigh.Text) ? null : float.Parse(tbHigh.Text);
                alarm.Critical = cbCritical.Checked;
            }

            context.SaveChanges();

            DialogResult = DialogResult.OK;
            Close();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
    public class DeviceItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}