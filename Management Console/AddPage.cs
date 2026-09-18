using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using VirtualEMS.DataServices;
using VirtualEMS.Library;

namespace ManagementConsole
{
    public partial class AddPage : Form
    {
        private readonly FormMode _mode;
        private readonly Page _page;

        private readonly BindingList<PageItem> _items = new();

        public AddPage()
        {
            InitializeComponent();
            _mode = FormMode.Add;
        }

        public AddPage(Page page)
        {
            InitializeComponent();
            _mode = FormMode.Edit;
            _page = page;
        }

        private void AddPage_Load(object sender, EventArgs e)
        {
            SetupGrid();

            if (_mode == FormMode.Edit && _page != null)
            {
                txtName.Text = _page.Name;
                txtDescription.Text = _page.Title;
                txtGroup.Text = _page.Group;

                btnCreate.Text = "Update";

                LoadItemsFromDb();
            }
        }
        private void SetupGrid()
        {
            dgItems.AutoGenerateColumns = false;
            dgItems.Columns.Clear();

            dgItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Name",
                HeaderText = "Name"
            });

            dgItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Title",
                HeaderText = "Title"
            });

            dgItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = "Description"
            });

            dgItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DeviceName",
                HeaderText = "Device"
            });

            dgItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ParameterList",
                HeaderText = "Parameters"
            });

            dgItems.DataSource = _items;
        }
        private void LoadItemsFromDb()
        {
            using var context = CreateContext();

            var items = context.PageItems
                .Include(i => i.PageItemTags)
                    .ThenInclude(t => t.Tag)
                        .ThenInclude(t => t.FieldDevice)
                .Include(i => i.PageItemParameters)
                    .ThenInclude(p => p.SerialDevice)
                .Include(i => i.PageItemParameters)
                    .ThenInclude(p => p.SerialDeviceParameter)
                .Where(i => i.PageId == _page.Id)
                .AsNoTracking()
                .ToList();

            _items.Clear();

            foreach (var item in items)
            {
                FillDisplayFields(item);
                _items.Add(item);
            }
        }
        private void btnAddItem_Click(object sender, EventArgs e)
        {
            using var form = new AddPageItem();

            if (form.ShowDialog() == DialogResult.OK)
            {
                var item = form.Result;

                FillDisplayFields(item);

                _items.Add(item);
            }
        }
        private void btnEditItem_Click(object sender, EventArgs e)
        {
            if (dgItems.CurrentRow?.DataBoundItem is not PageItem item)
                return;

            using var form = new AddPageItem(item);

            if (form.ShowDialog() == DialogResult.OK)
            {
                var edited = form.Result;

                item.Name = edited.Name;
                item.Title = edited.Title;
                item.Description = edited.Description;

                item.PageItemTags = edited.PageItemTags;
                item.PageItemParameters = edited.PageItemParameters;

                FillDisplayFields(item);

                dgItems.Refresh();
            }
        }
        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Name required");
                return;
            }

            if (_mode == FormMode.Add)
                AddToDb();
            else
                UpdateInDb();

            DialogResult = DialogResult.OK;
            Close();
        }
        private void AddToDb()
        {
            using var context = CreateContext();

            var page = new Page
            {
                Name = txtName.Text.Trim(),
                Title = txtDescription.Text.Trim(),
                Group = txtGroup.Text.Trim()
            };

            context.Pages.Add(page);
            context.SaveChanges();

            SaveItems(context, page.Id);

            context.SaveChanges();
        }
        private void UpdateInDb()
        {
            using var context = CreateContext();

            var dbPage = context.Pages
                .Include(p => p.PageItems)
                    .ThenInclude(i => i.PageItemTags)
                .Include(p => p.PageItems)
                    .ThenInclude(i => i.PageItemParameters)
                .FirstOrDefault(p => p.Id == _page.Id);

            if (dbPage == null) return;

            dbPage.Name = txtName.Text.Trim();
            dbPage.Title = txtDescription.Text.Trim();
            dbPage.Group = txtGroup.Text.Trim();

            UpdateItems(context, dbPage);

            context.SaveChanges();
        }
        private void SaveItems(AppDbContext context, int pageId)
        {
            foreach (var item in _items)
            {
                var dbItem = new PageItem
                {
                    PageId = pageId,
                    Name = item.Name,
                    Title = item.Title,
                    Description = item.Description
                };

                context.PageItems.Add(dbItem);
                context.SaveChanges();

                SaveItemTags(context, dbItem.Id, item.PageItemTags);
                SaveItemParameters(context, dbItem.Id, item.PageItemParameters);
            }
        }
        private void UpdateItems(AppDbContext context, Page dbPage)
        {
            var existingItems = dbPage.PageItems.ToList();

            foreach (var item in _items)
            {
                var existingItem = existingItems.FirstOrDefault(i => i.Id == item.Id);

                if (existingItem != null)
                {
                    existingItem.Name = item.Name;
                    existingItem.Title = item.Title;
                    existingItem.Description = item.Description;

                    UpdateItemTags(context, existingItem, item.PageItemTags);
                    UpdateItemParameters(context, existingItem, item.PageItemParameters);

                    existingItems.Remove(existingItem);
                }
                else
                {
                    var newItem = new PageItem
                    {
                        PageId = dbPage.Id,
                        Name = item.Name,
                        Title = item.Title,
                        Description = item.Description
                    };

                    context.PageItems.Add(newItem);
                    context.SaveChanges();

                    SaveItemTags(context, newItem.Id, item.PageItemTags);
                    SaveItemParameters(context, newItem.Id, item.PageItemParameters);
                }
            }

            foreach (var removeItem in existingItems)
            {
                context.PageItemTags.RemoveRange(removeItem.PageItemTags);
                context.PageItemParameters.RemoveRange(removeItem.PageItemParameters);
                context.PageItems.Remove(removeItem);
            }
        }
        private void SaveItemTags(AppDbContext context, int pageItemId, List<PageItemTag> tags)
        {
            if (tags == null) return;

            foreach (var tag in tags)
            {
                context.PageItemTags.Add(new PageItemTag
                {
                    PageItemId = pageItemId,
                    TagId = tag.TagId
                });
            }
        }
        private void SaveItemParameters(AppDbContext context, int pageItemId, List<PageItemParameter> parameters)
        {
            if (parameters == null) return;

            foreach (var param in parameters)
            {
                context.PageItemParameters.Add(new PageItemParameter
                {
                    PageItemId = pageItemId,
                    SerialDeviceId = param.SerialDeviceId,
                    SerialDeviceParameterId = param.SerialDeviceParameterId,
                    Formula = param.Formula
                });
            }
        }
        private void UpdateItemTags(AppDbContext context, PageItem item, List<PageItemTag> newTags)
        {
            var existing = item.PageItemTags.ToList();

            foreach (var tag in newTags ?? new List<PageItemTag>())
            {
                if (!existing.Any(t => t.TagId == tag.TagId))
                {
                    context.PageItemTags.Add(new PageItemTag
                    {
                        PageItemId = item.Id,
                        TagId = tag.TagId
                    });
                }
                else
                {
                    existing.RemoveAll(t => t.TagId == tag.TagId);
                }
            }

            context.PageItemTags.RemoveRange(existing);
        }
        private void UpdateItemParameters(AppDbContext context, PageItem item, List<PageItemParameter> newParams)
        {
            var existing = item.PageItemParameters.ToList();

            foreach (var param in newParams ?? new List<PageItemParameter>())
            {
                var match = existing.FirstOrDefault(p =>
                    p.SerialDeviceId == param.SerialDeviceId &&
                    p.SerialDeviceParameterId == param.SerialDeviceParameterId);

                if (match != null)
                {
                    match.Formula = param.Formula;
                    existing.Remove(match);
                }
                else
                {
                    context.PageItemParameters.Add(new PageItemParameter
                    {
                        PageItemId = item.Id,
                        SerialDeviceId = param.SerialDeviceId,
                        SerialDeviceParameterId = param.SerialDeviceParameterId,
                        Formula = param.Formula
                    });
                }
            }

            context.PageItemParameters.RemoveRange(existing);
        }
        private void FillDisplayFields(PageItem item)
        {
            var fieldDevice = item.PageItemTags?
                .Select(t => Main.Tags.FirstOrDefault(x => x.Id == t.TagId)?.FieldDevice?.Name)
                .FirstOrDefault();

            var serialDevice = item.PageItemParameters?
                .Select(p => Main.SerialDevices.FirstOrDefault(x => x.Id == p.SerialDeviceId)?.Name)
                .FirstOrDefault();

            item.DeviceName = fieldDevice ?? serialDevice ?? "";

            var tagNames = item.PageItemTags?
                .Select(t => Main.Tags.FirstOrDefault(x => x.Id == t.TagId)?.Name);

            var paramNames = item.PageItemParameters?
                .Select(p => Main.SerialDeviceParameters.FirstOrDefault(x => x.Id == p.SerialDeviceParameterId)?.Name);

            item.ParameterList = string.Join(", ",
                (tagNames ?? Enumerable.Empty<string>())
                .Concat(paramNames ?? Enumerable.Empty<string>())
                .Where(x => !string.IsNullOrEmpty(x)));
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

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgItems.CurrentRow?.DataBoundItem is not PageItem item)
            {
                MessageBox.Show("Select an item to delete");
                return;
            }

            string message =
                $"Delete Item?\n\n" +
                $"Name: {item.Name}\n" +
                $"Device: {item.DeviceName}\n" +
                $"Parameters: {item.ParameterList}";

            var confirm = MessageBox.Show(
                message,
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
                return;

            _items.Remove(item);

            dgItems.Refresh();

        }
    }
}