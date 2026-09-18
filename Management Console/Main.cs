using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using VirtualEMS.DataServices;
using VirtualEMS.Library;

namespace ManagementConsole
{
    public partial class Main : Form
    {
        int flag = 0;

        public static List<AlarmTag> AlarmTags = new List<AlarmTag>();
        public static List<AlarmParameter> AlarmParameters = new List<AlarmParameter>();
        public static List<Category> Categories = new List<Category>();
        public static List<IODevice> IODevices = new List<IODevice>();
        public static List<FieldDevice> FieldDevices = new List<FieldDevice>();
        public static List<SerialDevice> SerialDevices = new List<SerialDevice>();
        public static List<SerialDeviceDriver> SerialDeviceDrivers = new List<SerialDeviceDriver>();
        public static List<SerialDeviceParameter> SerialDeviceParameters = new List<SerialDeviceParameter>();
        public static List<SerialDeviceReadBlock> SerialDeviceReadBlocks = new List<SerialDeviceReadBlock>();
        public static List<SerialDeviceRegister> SerialDeviceRegisters = new List<SerialDeviceRegister>();
        public static List<Tag> Tags = new List<Tag>();
        public static List<Node> Nodes = new List<Node>();
        public static List<Page> Pages = new List<Page>();
        public static List<TrendTag> TrendTags = new List<TrendTag>();
        public static List<TrendParameter> TrendParameters = new List<TrendParameter>();
        public static string ConnectionString = string.Empty;
        public Main()
        {
            InitializeComponent();
        }
        private void Main_Load(object sender, EventArgs e)
        {
            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["ConfigDBConnString"].ConnectionString;
                if (string.IsNullOrWhiteSpace(ConnectionString))
                {
                    MessageBox.Show("Connection string 'ConfigDBConnString' not found.", "Configuration error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(ConnectionString)
                    .Options;

                using var context = new AppDbContext(options);
                var repo = new dbRepository(context);

                // Populate static lists
                AlarmTags = repo.GetAlarmTags().ToList();
                AlarmParameters = repo.GetAlarmParameters().ToList();
                Categories = repo.GetCategories().ToList();
                IODevices = repo.GetIODevices().ToList();
                FieldDevices = repo.GetFieldDevices().ToList();
                SerialDevices = repo.GetSerialDevices().ToList();
                SerialDeviceDrivers = repo.GetSerialDeviceDrivers().ToList();
                SerialDeviceParameters = repo.GetSerialDeviceParameters().ToList();
                SerialDeviceReadBlocks = repo.GetSerialDeviceReadBlocks().ToList();
                SerialDeviceRegisters = repo.GetSerialDeviceRegisters().ToList();
                Tags = repo.GetTags().ToList();
                Nodes = repo.GetNodes().ToList();
                Pages = repo.GetPages().ToList();
                TrendTags = repo.GetTrendTags().ToList();
                TrendParameters = repo.GetTrendParameters().ToList();
                //Trends = repo.GetTrends().ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize database: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Default view
                btnEthernetDevbices.PerformClick();
            }
        }
        private void btnEthernetDevbices_Click(object sender, EventArgs e)
        {
            ResetGrid();
            lblEthernetDevices.Font = new Font(lblEthernetDevices.Font, FontStyle.Bold);
            lblFieldDevices.Font = new Font(lblFieldDevices.Font, FontStyle.Regular);
            lblSerialDevices.Font = new Font(lblSerialDevices.Font, FontStyle.Regular);
            lblDeviceTypes.Font = new Font(lblDeviceTypes.Font, FontStyle.Regular);
            lbNode.Font = new Font(lbNode.Font, FontStyle.Regular);
            lbPage.Font = new Font(lbPage.Font, FontStyle.Regular);
            lbAlarms.Font = new Font(lbAlarms.Font, FontStyle.Regular);
            lbTags.Font = new Font(lbTags.Font, FontStyle.Regular);
            lbTrends.Font = new Font(lbTrends.Font, FontStyle.Regular);
            flag = 0;
            dgView.DataSource = IODevices;
            //dgView.DataSource = IODevices.Where(d => d.DeviceType == IODeviceType.EthernetDevice || d.DeviceType == IODeviceType.PLC).ToList();
        }

        private void btnFieldDevices_Click(object sender, EventArgs e)
        {
            ResetGrid();
            lblFieldDevices.Font = new Font(lblFieldDevices.Font, FontStyle.Bold);
            lblEthernetDevices.Font = new Font(lblEthernetDevices.Font, FontStyle.Regular);
            lblSerialDevices.Font = new Font(lblSerialDevices.Font, FontStyle.Regular);
            lblDeviceTypes.Font = new Font(lblDeviceTypes.Font, FontStyle.Regular);
            lbNode.Font = new Font(lbNode.Font, FontStyle.Regular);
            lbPage.Font = new Font(lbPage.Font, FontStyle.Regular);
            lbAlarms.Font = new Font(lbAlarms.Font, FontStyle.Regular);
            lbTags.Font = new Font(lbTags.Font, FontStyle.Regular);
            lbTrends.Font = new Font(lbTrends.Font, FontStyle.Regular);
            flag = 1;
            var list = FieldDevices.Select(f => new
            {
                f.Id,
                f.Name,
                f.Description,
                IODevice = f.IODevice != null ? f.IODevice.Name : "",
                f.RunFBId
            }).ToList();
            dgView.DataSource = list;
        }
        private void btnSerialDevices_Click(object sender, EventArgs e)
        {
            ResetGrid();
            lblSerialDevices.Font = new Font(lblSerialDevices.Font, FontStyle.Bold);
            lblFieldDevices.Font = new Font(lblFieldDevices.Font, FontStyle.Regular);
            lblEthernetDevices.Font = new Font(lblEthernetDevices.Font, FontStyle.Regular);
            lblDeviceTypes.Font = new Font(lblDeviceTypes.Font, FontStyle.Regular);
            lbNode.Font = new Font(lbNode.Font, FontStyle.Regular);
            lbPage.Font = new Font(lbPage.Font, FontStyle.Regular);
            lbAlarms.Font = new Font(lbAlarms.Font, FontStyle.Regular);
            lbTags.Font = new Font(lbTags.Font, FontStyle.Regular);
            lbTrends.Font = new Font(lbTrends.Font, FontStyle.Regular);
            flag = 2;
            dgView.DataSource = SerialDevices;
            if (dgView.Columns["Driver"] != null)
            {
                dgView.Columns["Driver"].Visible = false;
            }
            if (dgView.Columns["Gateway"] != null)
            {
                dgView.Columns["Gateway"].Visible = false;
            }
        }
        private void btnDeviceTypes_Click(object sender, EventArgs e)
        {
            ResetGrid();
            lblDeviceTypes.Font = new Font(lblDeviceTypes.Font, FontStyle.Bold);
            lblSerialDevices.Font = new Font(lblSerialDevices.Font, FontStyle.Regular);
            lblFieldDevices.Font = new Font(lblFieldDevices.Font, FontStyle.Regular);
            lblEthernetDevices.Font = new Font(lblEthernetDevices.Font, FontStyle.Regular);
            lbNode.Font = new Font(lbNode.Font, FontStyle.Regular);
            lbPage.Font = new Font(lbPage.Font, FontStyle.Regular);
            lbAlarms.Font = new Font(lbAlarms.Font, FontStyle.Regular);
            lbTags.Font = new Font(lbTags.Font, FontStyle.Regular);
            lbTrends.Font = new Font(lbTrends.Font, FontStyle.Regular);
            flag = 3;
            dgView.DataSource = SerialDeviceDrivers;
        }
        private void btnNode_Click(object sender, EventArgs e)
        {
            ResetGrid();
            lblDeviceTypes.Font = new Font(lblDeviceTypes.Font, FontStyle.Regular);
            lblSerialDevices.Font = new Font(lblSerialDevices.Font, FontStyle.Regular);
            lblFieldDevices.Font = new Font(lblFieldDevices.Font, FontStyle.Regular);
            lblEthernetDevices.Font = new Font(lblEthernetDevices.Font, FontStyle.Regular);
            lbNode.Font = new Font(lbNode.Font, FontStyle.Bold);
            lbPage.Font = new Font(lbPage.Font, FontStyle.Regular);
            lbAlarms.Font = new Font(lbAlarms.Font, FontStyle.Regular);
            lbTags.Font = new Font(lbTags.Font, FontStyle.Regular);
            lbTrends.Font = new Font(lbTrends.Font, FontStyle.Regular);
            flag = 4;
            dgView.DataSource = Nodes;
            NodeColumns();
        }
        private void btnPage_Click(object sender, EventArgs e)
        {
            ResetGrid();
            lblDeviceTypes.Font = new Font(lblDeviceTypes.Font, FontStyle.Regular);
            lblSerialDevices.Font = new Font(lblSerialDevices.Font, FontStyle.Regular);
            lblFieldDevices.Font = new Font(lblFieldDevices.Font, FontStyle.Regular);
            lblEthernetDevices.Font = new Font(lblEthernetDevices.Font, FontStyle.Regular);
            lbNode.Font = new Font(lbNode.Font, FontStyle.Regular);
            lbPage.Font = new Font(lbPage.Font, FontStyle.Bold);
            lbAlarms.Font = new Font(lbAlarms.Font, FontStyle.Regular);
            lbTags.Font = new Font(lbTags.Font, FontStyle.Regular);
            lbTrends.Font = new Font(lbTrends.Font, FontStyle.Regular);
            flag = 5;
            dgView.DataSource = Pages;
        }
        private void btnAlarm_Click(object sender, EventArgs e)
        {
            ResetGrid();
            lblDeviceTypes.Font = new Font(lblDeviceTypes.Font, FontStyle.Regular);
            lblSerialDevices.Font = new Font(lblSerialDevices.Font, FontStyle.Regular);
            lblFieldDevices.Font = new Font(lblFieldDevices.Font, FontStyle.Regular);
            lblEthernetDevices.Font = new Font(lblEthernetDevices.Font, FontStyle.Regular);
            lbNode.Font = new Font(lbNode.Font, FontStyle.Regular);
            lbPage.Font = new Font(lbPage.Font, FontStyle.Regular);
            lbAlarms.Font = new Font(lbAlarms.Font, FontStyle.Bold);
            lbTags.Font = new Font(lbTags.Font, FontStyle.Regular);
            lbTrends.Font = new Font(lbTrends.Font, FontStyle.Regular);
            flag = 6;
            var alarmList = new List<AlarmViewModel>();

            alarmList.AddRange(
                AlarmTags.Select(a => new AlarmViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    Device = a.FieldDevice.Name,
                    Parameter = a.Tag.Name,
                    LowSetPoint = a.LowSetPoint,
                    HighSetPoint = a.HighSetPoint,
                    Critical = a.Critical,
                    Type = "Tag"
                })
            );

            alarmList.AddRange(
                AlarmParameters.Select(a => new AlarmViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    Device = a.SerialDevice.Name,
                    Parameter = a.SerialDeviceParameter.Name,
                    LowSetPoint = a.LowSetPoint,
                    HighSetPoint = a.HighSetPoint,
                    Critical = a.Critical,
                    Type = "Parameter"
                })
            );
            dgView.DataSource = alarmList;
            if (dgView.Columns["Type"] != null)
            {
                dgView.Columns["Type"].Visible = false;
            }
        }
        private void btnTags_Click(object sender, EventArgs e)
        {
            ResetGrid();
            lblDeviceTypes.Font = new Font(lblDeviceTypes.Font, FontStyle.Regular);
            lblSerialDevices.Font = new Font(lblSerialDevices.Font, FontStyle.Regular);
            lblFieldDevices.Font = new Font(lblFieldDevices.Font, FontStyle.Regular);
            lblEthernetDevices.Font = new Font(lblEthernetDevices.Font, FontStyle.Regular);
            lbNode.Font = new Font(lbNode.Font, FontStyle.Regular);
            lbPage.Font = new Font(lbPage.Font, FontStyle.Regular);
            lbAlarms.Font = new Font(lbAlarms.Font, FontStyle.Regular);
            lbTags.Font = new Font(lbTags.Font, FontStyle.Bold);
            lbTrends.Font = new Font(lbTrends.Font, FontStyle.Regular);
            flag = 7;
            var list = Tags.Select(t => new
            {
                t.Id,
                t.Name,
                t.Description,
                t.Type,
                t.Units,
                t.Address,
                t.Bit,
                t.Formula,
                t.SP_Min,
                t.SP_Max,
                t.Scale_Min,
                t.Scale_Max,
                t.StorageType,
                Device = t.Device != null ? t.Device.Name : "",
                FieldDevice = t.FieldDevice != null ? t.FieldDevice.Name : ""
            }).ToList();
            dgView.DataSource = list;
        }
        private void btnTrends_Click(object sender, EventArgs e)
        {
            ResetGrid();
            lblDeviceTypes.Font = new Font(lblDeviceTypes.Font, FontStyle.Regular);
            lblSerialDevices.Font = new Font(lblSerialDevices.Font, FontStyle.Regular);
            lblFieldDevices.Font = new Font(lblFieldDevices.Font, FontStyle.Regular);
            lblEthernetDevices.Font = new Font(lblEthernetDevices.Font, FontStyle.Regular);
            lbNode.Font = new Font(lbNode.Font, FontStyle.Regular);
            lbPage.Font = new Font(lbPage.Font, FontStyle.Regular);
            lbAlarms.Font = new Font(lbAlarms.Font, FontStyle.Regular);
            lbTags.Font = new Font(lbTags.Font, FontStyle.Regular);
            lbTrends.Font = new Font(lbTrends.Font, FontStyle.Bold);
            flag = 8;

            using var context = CreateContext();

            TrendTags = context.TrendTags
                .Include(t => t.Tag)
                .Include(t => t.FieldDevice)
                .AsNoTracking()
                .ToList();

            TrendParameters = context.TrendParameters
                .Include(p => p.SerialDeviceParameter)
                .Include(p => p.SerialDevice)
                .AsNoTracking()
                .ToList();

            var tagGroups = TrendTags
                .GroupBy(t => new { t.FieldDeviceId, t.FieldDevice.Name })
                .Select(g => new TrendGroupView
                {
                    DeviceId = g.Key.FieldDeviceId,
                    Device = g.Key.Name,
                    Parameters = string.Join(", ", g.Select(x => x.Tag.Name)),
                    ParameterIds = g.Select(x => x.Id).ToList(),
                    Type = "Tag"
                });

            var paramGroups = TrendParameters
                .GroupBy(p => new { p.SerialDeviceId, p.SerialDevice.Name })
                .Select(g => new TrendGroupView
                {
                    DeviceId = g.Key.SerialDeviceId,
                    Device = g.Key.Name,
                    Parameters = string.Join(", ", g.Select(x => x.SerialDeviceParameter.Name)),
                    ParameterIds = g.Select(x => x.Id).ToList(),
                    Type = "Parameter"
                });

            dgView.DataSource = tagGroups.Concat(paramGroups).ToList();

            if (dgView.Columns["Type"] != null)
                dgView.Columns["Type"].Visible = false;

            //if (dgView.Columns["DeviceId"] != null)
            //    dgView.Columns["DeviceId"].Visible = false;
        }
        private void btnAddNew_Click(object sender, EventArgs e)
        {
            switch (flag)
            {
                case 0:
                    AddIODevice ethernetDevice = new AddIODevice();
                    if (ethernetDevice.ShowDialog() == DialogResult.OK)
                        RefreshEthernetDevices();
                    break;
                case 1:
                    AddFieldDevice addFieldDevice = new AddFieldDevice();
                    if (addFieldDevice.ShowDialog() == DialogResult.OK)
                        RefreshFieldDevices();
                    break;
                case 2:
                    AddSerialDevice serialDevice = new AddSerialDevice();
                    if (serialDevice.ShowDialog() == DialogResult.OK)
                        RefreshSerialDevices();
                    break;
                case 3:
                    AddDeviceType addDeviceType = new AddDeviceType();
                    if (addDeviceType.ShowDialog() == DialogResult.OK)
                        RefreshDeviceTypes();
                    break;
                case 4:
                    AddNode addNode = new AddNode();
                    if (addNode.ShowDialog() == DialogResult.OK)
                        RefreshNodes();
                    break;
                case 5:
                    AddPage addPage = new AddPage();
                    if (addPage.ShowDialog() == DialogResult.OK)
                        RefreshPages();
                    break;
                case 6:
                    AddAlarm addAlarm = new AddAlarm();
                    if (addAlarm.ShowDialog() == DialogResult.OK)
                        RefreshAlarms();
                    break;
                case 7:
                    AddTag addTag = new AddTag();
                    if (addTag.ShowDialog() == DialogResult.OK)
                        RefreshTags();
                    break;
                case 8:
                    AddTrend addTrend = new AddTrend();
                    if (addTrend.ShowDialog() == DialogResult.OK)
                        RefreshTrends();
                    break;
            }
        }
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a record to edit");
                return;
            }
            switch (flag)
            {
                case 0:
                    {
                        var device = dgView.SelectedRows[0].DataBoundItem as IODevice;
                        if (device == null) return;
                        using (var form = new AddIODevice(device))
                        {
                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshEthernetDevices();
                        }
                        break;
                    }
                case 1:
                    {
                        int id = (int)dgView.SelectedRows[0].Cells["Id"].Value;
                        var fieldDevice = FieldDevices.First(f => f.Id == id);
                        if (fieldDevice == null) return;
                        using (var form = new AddFieldDevice(fieldDevice))
                        {
                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshFieldDevices();
                        }
                        break;
                    }
                case 2:
                    {
                        var serial = dgView.SelectedRows[0].DataBoundItem as SerialDevice;
                        if (serial == null) return;
                        using (var form = new AddSerialDevice(serial))
                        {
                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshSerialDevices();
                        }
                        break;
                    }
                case 3:
                    {
                        var driver = dgView.SelectedRows[0].DataBoundItem as SerialDeviceDriver;
                        if (driver == null) return;
                        using (var form = new AddDeviceType(driver))
                        {
                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshDeviceTypes();
                        }
                        break;
                    }
                case 4:
                    {
                        var node = dgView.SelectedRows[0].DataBoundItem as Node;
                        if (node == null) return;
                        using (var form = new AddNode(node))
                        {
                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshNodes();
                        }
                        break;
                    }
                case 5:
                    {
                        var page = dgView.SelectedRows[0].DataBoundItem as Page;
                        if (page == null) return;
                        using (var form = new AddPage(page))
                        {
                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshPages();
                        }
                        break;
                    }
                case 6:
                    {
                        var selected = dgView.SelectedRows[0].DataBoundItem as AlarmViewModel;
                        if (selected == null) return;
                        if (selected.Type == "Tag")
                        {
                            var tag = AlarmTags.FirstOrDefault(a => a.Id == selected.Id);

                            if (tag == null) return;

                            AddAlarm form = new AddAlarm(tag);

                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshAlarms();
                        }
                        else if (selected.Type == "Parameter")
                        {
                            var param = AlarmParameters.FirstOrDefault(a => a.Id == selected.Id);

                            if (param == null) return;

                            AddAlarm form = new AddAlarm(param);

                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshAlarms();
                        }
                        break;
                    }
                case 7:
                    {
                        int id = (int)dgView.SelectedRows[0].Cells["Id"].Value;
                        var tag = Tags.First(f => f.Id == id);
                        if (tag == null) return;
                        using (var form = new AddTag(tag))
                        {
                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshTags();
                        }
                        break;
                    }
                case 8:
                    {
                        var selected = dgView.SelectedRows[0].DataBoundItem as TrendGroupView;

                        if (selected == null) return;

                        if (selected.Type == "Tag")
                        {
                            var tags = TrendTags
                                .Where(t => t.FieldDeviceId == selected.DeviceId)
                                .ToList();

                            if (!tags.Any()) return;

                            AddTrend form = new AddTrend(tags);

                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshTrends();
                        }
                        else
                        {
                            var parameters = TrendParameters
                                .Where(p => p.SerialDeviceId == selected.DeviceId)
                                .ToList();

                            if (!parameters.Any()) return;

                            AddTrend form = new AddTrend(parameters);

                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshTrends();
                        }

                        break;
                    }
            }
        }
        //private void btnDelete_Click(object sender, EventArgs e)
        //{
        //    if (dgView.SelectedRows.Count == 0)
        //    {
        //        MessageBox.Show("Please select a record to delete");
        //        return;
        //    }

        //    using var context = CreateContext();

        //    switch (flag)
        //    {
        //        case 4: // Node
        //            {
        //                var selectedNode = dgView.SelectedRows[0].DataBoundItem as Node;

        //                if (selectedNode == null) return;

        //                var node = context.Nodes
        //                    .Include(n => n.Parent)
        //                    .FirstOrDefault(n => n.Id == selectedNode.Id);

        //                if (node == null) return;

        //                bool hasChildren = context.Nodes.Any(n => n.ParentId == node.Id);

        //                string message =
        //                    $"Delete Node?\n\n" +
        //                    $"Name: {node.Name}\n" +
        //                    $"Type: {node.Type}\n" +
        //                    $"Parent: {node.Parent?.Name ?? "None"}\n";

        //                if (hasChildren)
        //                {
        //                    message += "\n⚠ This node has child nodes. Cannot delete.";
        //                    MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //                    return;
        //                }

        //                var confirm = MessageBox.Show(message, "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        //                if (confirm != DialogResult.Yes)
        //                    return;

        //                context.Nodes.Remove(node);
        //                context.SaveChanges();

        //                RefreshNodes();
        //                break;
        //            }
        //        case 6:
        //            {
        //                var selected = dgView.SelectedRows[0].DataBoundItem as AlarmViewModel;

        //                if (selected == null) return;

        //                string message =
        //                    $"Delete Alarm?\n\n" +
        //                    $"Name: {selected.Name}\n" +
        //                    $"Device: {selected.Device}\n" +
        //                    $"Parameter: {selected.Parameter}";

        //                var confirm = MessageBox.Show(message, "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        //                if (confirm != DialogResult.Yes)
        //                    return;

        //                if (selected.Type == "Tag")
        //                {
        //                    var alarm = context.AlarmTags.Find(selected.Id);
        //                    if (alarm != null)
        //                        context.AlarmTags.Remove(alarm);
        //                }
        //                else
        //                {
        //                    var alarm = context.AlarmParameters.Find(selected.Id);
        //                    if (alarm != null)
        //                        context.AlarmParameters.Remove(alarm);
        //                }

        //                context.SaveChanges();
        //                RefreshAlarms();
        //                break;
        //            }

        //        case 8:
        //            {
        //                var selected = dgView.SelectedRows[0].DataBoundItem as TrendGroupView;

        //                if (selected == null) return;

        //                string message =
        //                    $"Delete Trend?\n\n" +
        //                    $"Device: {selected.Device}\n" +
        //                    $"Parameters:\n{selected.Parameters}";

        //                var confirm = MessageBox.Show(message, "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        //                if (confirm != DialogResult.Yes)
        //                    return;

        //                if (selected.Type == "Tag")
        //                {
        //                    var trends = context.TrendTags
        //                        .Where(t => t.FieldDeviceId == selected.DeviceId);

        //                    context.TrendTags.RemoveRange(trends);
        //                }
        //                else
        //                {
        //                    var trends = context.TrendParameters
        //                        .Where(t => t.SerialDeviceId == selected.DeviceId);

        //                    context.TrendParameters.RemoveRange(trends);
        //                }

        //                context.SaveChanges();
        //                RefreshTrends();
        //                break;
        //            }

        //        default:
        //            //MessageBox.Show("Delete not implemented for this section");
        //            break;
        //    }
        //}
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a record to delete");
                return;
            }

            using var context = CreateContext();

            switch (flag)
            {
                case 0:
                    {
                        var device = dgView.SelectedRows[0].DataBoundItem as IODevice;
                        if (device == null) return;

                        string msg = $"Delete IO Device?\n\nName: {device.Name}";
                        if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

                        // delete dependent field devices
                        var fieldDevices = context.FieldDevices.Where(f => f.IODeviceId == device.Id);
                        context.FieldDevices.RemoveRange(fieldDevices);

                        context.IODevices.Remove(device);
                        context.SaveChanges();

                        RefreshFieldDevices();
                        RefreshEthernetDevices();
                        break;
                    }

                case 1:
                    {
                        int id = (int)dgView.SelectedRows[0].Cells["Id"].Value;

                        var field = context.FieldDevices.FirstOrDefault(f => f.Id == id);
                        if (field == null) return;

                        string msg = $"Delete Field Device?\n\nName: {field.Name}";
                        if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

                        // delete tags + trends + alarms
                        context.Tags.RemoveRange(context.Tags.Where(t => t.FieldDeviceId == id));
                        context.TrendTags.RemoveRange(context.TrendTags.Where(t => t.FieldDeviceId == id));
                        context.AlarmTags.RemoveRange(context.AlarmTags.Where(a => a.FieldDeviceId == id));

                        context.FieldDevices.Remove(field);
                        context.SaveChanges();

                        RefreshTags();
                        RefreshAlarms();
                        RefreshTrends();
                        RefreshFieldDevices();
                        break;
                    }

                case 2:
                    {
                        var serial = dgView.SelectedRows[0].DataBoundItem as SerialDevice;
                        if (serial == null) return;

                        string msg = $"Delete Serial Device?\n\nName: {serial.Name}";
                        if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

                        context.TrendParameters.RemoveRange(
                            context.TrendParameters.Where(p => p.SerialDeviceId == serial.Id));

                        context.AlarmParameters.RemoveRange(
                            context.AlarmParameters.Where(a => a.SerialDeviceId == serial.Id));

                        context.SerialDevices.Remove(serial);
                        context.SaveChanges();

                        RefreshAlarms();
                        RefreshTrends();
                        RefreshSerialDevices();
                        break;
                    }
                case 3: 
                    {
                        var driver = dgView.SelectedRows[0].DataBoundItem as SerialDeviceDriver;

                        if (driver == null) return;

                        var serialDevices = context.SerialDevices
                            .Where(s => s.DriverId == driver.Id)
                            .ToList();

                        int count = serialDevices.Count;

                        string message =
                            $"Delete Driver?\n\n" +
                            $"Name: {driver.Name}\n" +
                            $"Linked Devices: {count}\n\n" +
                            $"This will delete all related devices and data.";

                        var confirm = MessageBox.Show(
                            message,
                            "Confirm Delete",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (confirm != DialogResult.Yes)
                            return;

                        var serialIds = serialDevices.Select(s => s.Id).ToList();

                        context.TrendParameters.RemoveRange(
                            context.TrendParameters.Where(p => serialIds.Contains(p.SerialDeviceId)));

                        context.AlarmParameters.RemoveRange(
                            context.AlarmParameters.Where(a => serialIds.Contains(a.SerialDeviceId)));

                        context.SerialDeviceRegisters.RemoveRange(
                            context.SerialDeviceRegisters.Where(r => r.DriverId == driver.Id));

                        context.SerialDeviceReadBlocks.RemoveRange(
                            context.SerialDeviceReadBlocks.Where(r => r.DriverId == driver.Id));

                        context.SerialDevices.RemoveRange(serialDevices);

                        context.SerialDeviceDrivers.Remove(driver);

                        context.SaveChanges();

                        RefreshTrends();
                        RefreshAlarms();
                        RefreshSerialDevices();
                        RefreshDeviceTypes();
                        break;
                    }
                case 4: 
                    {
                        var selectedNode = dgView.SelectedRows[0].DataBoundItem as Node;

                        if (selectedNode == null) return;

                        var node = context.Nodes
                            .Include(n => n.Parent)
                            .FirstOrDefault(n => n.Id == selectedNode.Id);

                        if (node == null) return;

                        bool hasChildren = context.Nodes.Any(n => n.ParentId == node.Id);

                        string message =
                            $"Delete Node?\n\n" +
                            $"Name: {node.Name}\n" +
                            $"Type: {node.Type}\n" +
                            $"Parent: {node.Parent?.Name ?? "None"}\n";

                        if (hasChildren)
                        {
                            message += "\n This node has child nodes. Cannot delete.";
                            MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        var confirm = MessageBox.Show(message, "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                        if (confirm != DialogResult.Yes)
                            return;

                        context.Nodes.Remove(node);
                        context.SaveChanges();

                        RefreshNodes();
                        break;
                    }
                case 5:
                    {
                        var page = dgView.SelectedRows[0].DataBoundItem as Page;
                        if (page == null) return;

                        string message = $"Delete Page?\n\nName: {page.Name}";

                        var confirm = MessageBox.Show(
                            message,
                            "Confirm Delete",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (confirm != DialogResult.Yes)
                            return;

                        var dbPage = context.Pages
                            .Include(p => p.PageItems)
                                .ThenInclude(i => i.PageItemTags)
                            .Include(p => p.PageItems)
                                .ThenInclude(i => i.PageItemParameters)
                            .FirstOrDefault(p => p.Id == page.Id);

                        if (dbPage == null) return;

                        foreach (var item in dbPage.PageItems)
                        {
                            context.PageItemTags.RemoveRange(item.PageItemTags);
                            context.PageItemParameters.RemoveRange(item.PageItemParameters);
                        }

                        context.PageItems.RemoveRange(dbPage.PageItems);

                        context.Pages.Remove(dbPage);

                        context.SaveChanges();

                        RefreshPages();
                        break;
                    }
                case 6:
                    {
                        var selected = dgView.SelectedRows[0].DataBoundItem as AlarmViewModel;
                        if (selected == null) return;

                        string msg =
                            $"Delete Alarm?\n\nName: {selected.Name}\n" +
                            $"Device: {selected.Device}\nParameter: {selected.Parameter}";

                        if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

                        if (selected.Type == "Tag")
                            context.AlarmTags.Remove(context.AlarmTags.Find(selected.Id));
                        else
                            context.AlarmParameters.Remove(context.AlarmParameters.Find(selected.Id));

                        context.SaveChanges();
                        RefreshAlarms();
                        break;
                    }

                case 7:
                    {
                        int id = (int)dgView.SelectedRows[0].Cells["Id"].Value;

                        var tag = context.Tags.FirstOrDefault(t => t.Id == id);
                        if (tag == null) return;

                        string msg = $"Delete Tag?\n\nName: {tag.Name}";
                        if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

                        context.TrendTags.RemoveRange(context.TrendTags.Where(t => t.TagId == id));
                        context.AlarmTags.RemoveRange(context.AlarmTags.Where(a => a.TagId == id));

                        context.Tags.Remove(tag);
                        context.SaveChanges();

                        RefreshTrends();
                        RefreshAlarms();
                        RefreshTags();
                        break;
                    }

                case 8:
                    {
                        var selected = dgView.SelectedRows[0].DataBoundItem as TrendGroupView;
                        if (selected == null) return;

                        string msg =
                            $"Delete Trend?\n\nDevice: {selected.Device}\n" +
                            $"Parameters:\n{selected.Parameters}";

                        if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

                        if (selected.Type == "Tag")
                        {
                            context.TrendTags.RemoveRange(
                                context.TrendTags.Where(t => t.FieldDeviceId == selected.DeviceId));
                        }
                        else
                        {
                            context.TrendParameters.RemoveRange(
                                context.TrendParameters.Where(p => p.SerialDeviceId == selected.DeviceId));
                        }

                        context.SaveChanges();
                        RefreshTrends();
                        break;
                    }

                default:
                    MessageBox.Show("Delete not implemented for this section");
                    break;
            }
        }
        private void ResetGrid()
        {
            dgView.DataSource = null;
            dgView.Columns.Clear();
            dgView.AutoGenerateColumns = true;
        }

        private void NodeColumns()
        {
            dgView.AutoGenerateColumns = false;
            dgView.Columns.Clear();
            dgView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "id",
                HeaderText = "id"
            });
            dgView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Name",
                HeaderText = "Name"
            });
            dgView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Title",
                HeaderText = "Title"
            });
            dgView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Type",
                HeaderText = "Type"
            });
            dgView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ParentName",
                HeaderText = "Parent"
            });
            dgView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FeederName",
                HeaderText = "Feeder"
            });
            dgView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FieldDeviceName",
                HeaderText = "Field Device"
            });
            dgView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Formula",
                HeaderText = "Formula"
            });
            dgView.DataSource = Nodes;
        }
        private AppDbContext CreateContext()
        {
            return new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(ConnectionString)
                    .Options);
        }
        private void RefreshEthernetDevices()
        {
            using var context = CreateContext();

            IODevices = context.IODevices
                .ToList();


            ResetGrid();
            dgView.DataSource = IODevices;
        }
        private void RefreshFieldDevices()
        {
            using var context = CreateContext();

            FieldDevices = context.FieldDevices
                .Include(f => f.IODevice)
                .AsNoTracking()
                .ToList();

            ResetGrid();
            var list = FieldDevices.Select(f => new
            {
                f.Id,
                f.Name,
                f.Description,
                IODevice = f.IODevice != null ? f.IODevice.Name : "",
                f.RunFBId
            }).ToList();

            dgView.DataSource = list;
        }
        private void RefreshSerialDevices()
        {
            using var context = CreateContext();

            SerialDevices = context.SerialDevices
                .Include(s => s.Driver)
                .Include(s => s.Gateway)
                .AsNoTracking()
                .ToList();

            ResetGrid();
            dgView.DataSource = SerialDevices;

            if (dgView.Columns["Driver"] != null)
                dgView.Columns["Driver"].Visible = false;

            if (dgView.Columns["Gateway"] != null)
                dgView.Columns["Gateway"].Visible = false;
        }
        private void RefreshNodes()
        {
            using var context = CreateContext();

            Nodes = context.Nodes
                .Include(n => n.Parent)
                .Include(n => n.Feeder)
                .Include(n => n.FieldDevice)
                .AsNoTracking()
                .ToList();

            ResetGrid();
            NodeColumns();
        }
        private void RefreshPages()
        {
            using var context = CreateContext();

            Pages = context.Pages
                .AsNoTracking()
                .ToList();

            ResetGrid();
            dgView.DataSource = Pages;
        }
        private void RefreshDeviceTypes()
        {
            using var context = CreateContext();

            SerialDeviceDrivers = context.SerialDeviceDrivers
                .AsNoTracking()
                .ToList();

            SerialDeviceRegisters = context.SerialDeviceRegisters
                .AsNoTracking()
                .ToList();

            SerialDeviceReadBlocks = context.SerialDeviceReadBlocks
                .AsNoTracking()
                .ToList();

            ResetGrid();
            dgView.DataSource = SerialDeviceDrivers;
        }

        private void RefreshAlarms()
        {
            using var context = CreateContext();

            AlarmTags = context.AlarmTags
                        .Include(a => a.FieldDevice)
                        .Include(a => a.Tag)
                        .AsNoTracking()
                        .ToList();

            AlarmParameters = context.AlarmParameters
                .Include(a => a.SerialDevice)
                .Include(a => a.SerialDeviceParameter)
                .AsNoTracking()
                .ToList();
            ResetGrid();
            var alarmList = new List<AlarmViewModel>();

            alarmList.AddRange(
                AlarmTags.Select(a => new AlarmViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    Device = a.FieldDevice.Name,
                    Parameter = a.Tag.Name,
                    LowSetPoint = a.LowSetPoint,
                    HighSetPoint = a.HighSetPoint,
                    Critical = a.Critical,
                    Type = "Tag"
                })
            );

            alarmList.AddRange(
                AlarmParameters.Select(a => new AlarmViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    Device = a.SerialDevice.Name,
                    Parameter = a.SerialDeviceParameter.Name,
                    LowSetPoint = a.LowSetPoint,
                    HighSetPoint = a.HighSetPoint,
                    Critical = a.Critical,
                    Type = "Parameter"
                })
            );

            dgView.DataSource = alarmList;
            if (dgView.Columns["Type"] != null)
            {
                dgView.Columns["Type"].Visible = false;
            }
        }
        private void RefreshTags()
        {
            using var context = CreateContext();

            Tags = context.Tags
                .Include(t => t.Device)
                .Include(t => t.FieldDevice)
                .AsNoTracking()
                .ToList();

            ResetGrid();
            var list = Tags.Select(t => new
            {
                t.Id,
                t.Name,
                t.Description,
                t.Type,
                t.Units,
                t.Address,
                t.Bit,
                t.Formula,
                t.SP_Min,
                t.SP_Max,
                t.Scale_Min,
                t.Scale_Max,
                t.StorageType,
                Device = t.Device != null ? t.Device.Name : "",
                FieldDevice = t.FieldDevice != null ? t.FieldDevice.Name : ""
            }).ToList();

            dgView.DataSource = list;
        }

        private void RefreshTrends()
        {
            using var context = CreateContext();

            TrendTags = context.TrendTags
                .Include(t => t.Tag)
                .Include(t => t.FieldDevice)
                .AsNoTracking()
                .ToList();

            TrendParameters = context.TrendParameters
                .Include(p => p.SerialDeviceParameter)
                .Include(p => p.SerialDevice)
                .AsNoTracking()
                .ToList();

            var tagGroups = TrendTags
                .GroupBy(t => new { t.FieldDeviceId, t.FieldDevice.Name })
                .Select(g => new TrendGroupView
                {
                    DeviceId = g.Key.FieldDeviceId,
                    Device = g.Key.Name,
                    Parameters = string.Join(", ", g.Select(x => x.Tag.Name)),
                    ParameterIds = g.Select(x => x.Id).ToList(),
                    Type = "Tag"
                });

            var paramGroups = TrendParameters
                .GroupBy(p => new { p.SerialDeviceId, p.SerialDevice.Name })
                .Select(g => new TrendGroupView
                {
                    DeviceId = g.Key.SerialDeviceId,
                    Device = g.Key.Name,
                    Parameters = string.Join(", ", g.Select(x => x.SerialDeviceParameter.Name)),
                    ParameterIds = g.Select(x => x.Id).ToList(),
                    Type = "Parameter"
                });

            ResetGrid();
            dgView.DataSource = tagGroups.Concat(paramGroups).ToList();

            if (dgView.Columns["Type"] != null)
                dgView.Columns["Type"].Visible = false;
        }
    }
}
