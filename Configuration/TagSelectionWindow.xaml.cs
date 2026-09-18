using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VirtualEMS.DataServices;
using VirtualEMS.Library;

namespace WPFSCADA.Configuration
{
    /// <summary>
    /// Interaction logic for TagSelectionWindow.xaml
    /// </summary>
    public partial class TagSelectionWindow : Window
    {
        private readonly iDbRepository repository;
        private readonly PageItem pageItem;
        private readonly string tagSource;

        public Tag SelectedTag { get; private set; }

        public TagSelectionWindow(
    iDbRepository repository,
    PageItem pageItem,
    string tagSource)
        {
            InitializeComponent();

            this.repository = repository;
            this.pageItem = pageItem;
            this.tagSource = tagSource;

            LoadTags();
        }

        private void LoadTags()
        {
            var allTags = repository.GetTags();

            List<Tag> tags;

            if (string.IsNullOrWhiteSpace(tagSource))
            {
                tags = allTags.ToList();
            }
            else
            {
                tags = allTags
                    .Where(t =>
                        t.Name.StartsWith(
                            tagSource + "_",
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            TagsGrid.ItemsSource = tags;

            var pageItemTag =
                repository.GetPageItemTag(pageItem.Id);

            if (pageItemTag == null)
                return;

            var previouslySelectedTag =
                tags.FirstOrDefault(t =>
                    t.Id == pageItemTag.TagId);

            if (previouslySelectedTag != null)
            {
                TagsGrid.SelectedItem =
                    previouslySelectedTag;

                TagsGrid.ScrollIntoView(
                    previouslySelectedTag);
            }
        }

        private void Select_Click(
            object sender,
            RoutedEventArgs e)
        {
            SelectedTag =
                TagsGrid.SelectedItem as Tag;

            if (SelectedTag == null)
            {
                MessageBox.Show(
                    "Please select a Tag.",
                    "Tag Selection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            repository.SavePageItemTag(
                new PageItemTag
                {
                    PageItemId = pageItem.Id,
                    TagId = SelectedTag.Id
                });

            MessageBox.Show(
                $"Tag '{SelectedTag.Name}' assigned successfully.",
                "Tag Selection",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
        }

        private void Cancel_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
