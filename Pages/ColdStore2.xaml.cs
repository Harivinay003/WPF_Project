using FluentModbus;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using VirtualEMS.DataServices;
using VirtualEMS.Library;
using WPFSCADA.Configuration;
using WPFSCADA.Controls;
using WPFSCADA.Helpers;
using Page = System.Windows.Controls.Page;
using WPFSCADA.Services;

namespace WPFSCADA.Pages
{
    /// <summary>
    /// Interaction logic for ColdStore2.xaml
    /// </summary>
    public partial class ColdStore2 : Page, System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));

        private double _coldStoreTemp;
        public double ColdStoreTemp { get => _coldStoreTemp; set { if (_coldStoreTemp != value) { _coldStoreTemp = value; OnPropertyChanged(nameof(ColdStoreTemp)); } } }

        private double _setPoint;
        public double SetPoint { get => _setPoint; set { if (_setPoint != value) { _setPoint = value; OnPropertyChanged(nameof(SetPoint)); } } }

        public iDbRepository Repository { get; private set; }

        private readonly Dictionary<string, string> AssignedTags = new Dictionary<string, string>();

        private Dictionary<string, int> ControlTagIds = new Dictionary<string, int>();

        public Dictionary<int, double> TagUpdateActions { get; private set; } = new Dictionary<int, double>();

        public List<int> TagIdsToUpdate { get; private set; } = new List<int>();

        private DispatcherTimer _liveValuesTimer;

        private bool _isConfigurationMode;

        private FrameworkElement _lastTooltipElement;

        private PLCService _plcService;

        private ConfigurationService _configurationService;

        private bool _isReadingPLC;

        public bool IsConfigurationMode
        {
            get => _isConfigurationMode;
            set
            {
                if (_isConfigurationMode == value)
                    return;

                _isConfigurationMode = value;

                // Pass configuration mode to child controls
                if (CS2U1 != null)
                    CS2U1.IsConfigurationMode = value;

                if (CS2U2 != null)
                    CS2U2.IsConfigurationMode = value;

                // Update button text
                if (ConfigurationButton != null)
                {
                    ConfigurationButton.Content =
                        value ? "Normal Mode" : "Configuration Mode";
                }

                // Remove configuration tooltips when leaving configuration mode
                if (!value)
                {
                    ClearConfigurationTooltips(CS2U1);
                    ClearConfigurationTooltips(CS2U2);
                }
            }
        }

        public ColdStore2()
        {
            InitializeComponent();
            this.DataContext = this;

            CS2U1.PageName = nameof(ColdStore2);
            CS2U2.PageName = nameof(ColdStore2);

            AddHandler(
        UIElement.PreviewMouseDownEvent,
        new MouseButtonEventHandler(ConfigurationMouseDown),
        true);

            AddHandler(
    UIElement.PreviewMouseMoveEvent,
    new MouseEventHandler(ConfigurationMouseMove),
    true);

            Repository = GetRepository();

            _plcService = new PLCService(Repository);

            _configurationService = new ConfigurationService(Repository);

            Loaded += ColdStore2_Loaded;

        }

        private async void ColdStore2_Loaded(object sender, RoutedEventArgs e)
        {

            ConfigurePage();

            await GetLiveValuesFromPLC();

            _liveValuesTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(250)
            };

            _liveValuesTimer.Tick += async (s, args) =>
            {
                if (_isReadingPLC)
                    return;

                _isReadingPLC = true;

                try
                {
                    await GetLiveValuesFromPLC();
                }
                finally
                {
                    _isReadingPLC = false;
                }
            };

            _liveValuesTimer.Start();

            //GetLiveValues();
        }

        private async Task GetLiveValuesFromPLC()
        {
            try
            {
                var values =
                    await _plcService.ReadTagsAsync(
                        ControlTagIds.Values);

                foreach (var item in values)
                {
                    TagUpdateActions[item.Key] = item.Value;
                }

                // Page-specific code stays here
                UpdatePageProperties();

                UpdateColdStoreUnit(
                    CS2U1,
                    "CS2U1");

                UpdateColdStoreUnit(
                    CS2U2,
                    "CS2U2");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Error updating ColdStore1 values: {ex}");
            }
        }

        private DependencyObject FindConfiguredElement(DependencyObject source)
        {
            DependencyObject current = source;

            while (current != null)
            {
                string configKey = TagProperties.GetConfigKey(current);

                if (!string.IsNullOrWhiteSpace(configKey))
                    return current;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private void ConfigurePageControls(int pageId)
        {
            foreach (var child in
                     GetDirectConfiguredControls(this))
            {
                var configKey =
                    TagProperties.GetConfigKey(child);

                if (string.IsNullOrWhiteSpace(configKey))
                    continue;

                string controlName =
                    $"ColdStore2.{configKey}";

                _configurationService.EnsurePageItem(
                    pageId,
                    controlName,
                    configKey);
            }
        }

        private IEnumerable<DependencyObject>
    GetDirectConfiguredControls(
        DependencyObject parent)
        {
            int count =
                VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < count; i++)
            {
                var child =
                    VisualTreeHelper.GetChild(parent, i);

                var configKey =
                    TagProperties.GetConfigKey(child);

                if (!string.IsNullOrWhiteSpace(configKey))
                {
                    yield return child;
                }

                // Don't go inside CS2U1 / CS2U2
                if (child == CS2U1 || child == CS2U2)
                    continue;

                foreach (var descendant in
                         GetDirectConfiguredControls(child))
                {
                    yield return descendant;
                }
            }
        }

        private void ConfigureControl(DependencyObject element)
        {
            if (!IsConfigurationMode)
                return;

            var configKey =
                TagProperties.GetConfigKey(element);

            if (string.IsNullOrWhiteSpace(configKey))
                return;

            string unitName;
            string controlName;

            if (IsDescendantOf(element, CS2U1))
            {
                unitName = "CS2U1";

                controlName =
                    $"{unitName}.{configKey}";
            }
            else if (IsDescendantOf(element, CS2U2))
            {
                unitName = "CS2U2";

                controlName =
                    $"{unitName}.{configKey}";
            }
            else
            {
                // Control belongs directly to ColdStore2
                unitName = "ColdStore2";

                controlName =
                    $"{unitName}.{configKey}";
            }

            var page =
                Repository.GetPageByName("ColdStore2");

            if (page == null)
            {
                MessageBox.Show(
                    "ColdStore2 page was not found.");

                return;
            }

            var pageItem =
                Repository.GetPageItem(
                    page.Id,
                    controlName);

            if (pageItem == null)
            {
                MessageBox.Show(
                    $"No PageItem found for:\n{controlName}");

                return;
            }

            var window =
                new TagSelectionWindow(
                    Repository,
                    pageItem,
                    unitName);

            window.Owner =
                Window.GetWindow(this);

            bool? result =
                window.ShowDialog();

            if (result == true)
            {
                LoadAssignedTags(page.Id);

                LoadControlTagMappings(page.Id);

                //MessageBox.Show(
                //    $"Tag configured successfully for:\n{controlName}");
            }
        }

        private bool IsDescendantOf(
    DependencyObject element,
    DependencyObject parent)
        {
            DependencyObject current = element;

            while (current != null)
            {
                if (current == parent)
                    return true;

                current = VisualTreeHelper.GetParent(current);
            }

            return false;
        }

        private void ConfigureUnit(
    ColdStoreUnit unit,
    string unitName,
    int pageId)
        {
            var controls =
                unit.GetConfiguredControls().ToList();

            foreach (var control in controls)
            {
                var configKey =
                    TagProperties.GetConfigKey(control);

                if (string.IsNullOrWhiteSpace(configKey))
                    continue;

                string controlName =
                    $"{unitName}.{configKey}";

                _configurationService.EnsurePageItem(
                    pageId,
                    controlName,
                    configKey);
            }
        }

        private void ConfigurePage()
        {
            int pageId = 2;

            // 1. Configure controls directly on ColdStore2
            ConfigurePageControls(pageId);

            // 2. Configure controls inside CS2U1
            ConfigureUnit(
                CS2U1,
                "CS2U1",
                pageId);

            // 3. Configure controls inside CS2U2
            ConfigureUnit(
                CS2U2,
                "CS2U2",
                pageId);

            // 4. Load existing tag assignments
            LoadAssignedTags(pageId);

            // 5. Load control -> tag ID mappings
            LoadControlTagMappings(pageId);

            // 6. Get live values
            // GetLiveValues();
        }



        //this is for toolTip show and already selected  tag show while clicking the control again.
        private void LoadAssignedTags(int pageId)
        {
            AssignedTags.Clear();

            var controlNames =
                new List<string>();

            // -----------------------------------------
            // Page controls
            // -----------------------------------------

            foreach (var control in
                     GetDirectConfiguredControls(this))
            {
                var configKey =
                    TagProperties.GetConfigKey(control);

                if (string.IsNullOrWhiteSpace(configKey))
                    continue;

                controlNames.Add(
                    $"ColdStore2.{configKey}");
            }

            // -----------------------------------------
            // CS1U1
            // -----------------------------------------

            foreach (var control in
                     CS2U1.GetConfiguredControls())
            {
                var configKey =
                    TagProperties.GetConfigKey(control);

                if (string.IsNullOrWhiteSpace(configKey))
                    continue;

                controlNames.Add(
                    $"CS2U1.{configKey}");
            }

            // -----------------------------------------
            // CS1U2
            // -----------------------------------------

            foreach (var control in
                     CS2U2.GetConfiguredControls())
            {
                var configKey =
                    TagProperties.GetConfigKey(control);

                if (string.IsNullOrWhiteSpace(configKey))
                    continue;

                controlNames.Add(
                    $"CS2U2.{configKey}");
            }

            // -----------------------------------------
            // DATABASE WORK
            // -----------------------------------------

            var result =
                _configurationService.LoadAssignedTags(
                    pageId,
                    controlNames);

            foreach (var item in result)
            {
                AssignedTags[item.Key] =
                    item.Value;
            }
        }



        private void LoadControlTagMappings(int pageId)
        {
            ControlTagIds.Clear();

            var mappings =
                _configurationService
                    .LoadControlTagMappings(pageId);

            foreach (var item in mappings)
            {
                ControlTagIds[item.Key] =
                    item.Value;
            }
        }

        private int? GetTagId(string controlName)
        {
            if (ControlTagIds.TryGetValue(
                controlName,
                out int tagId))
            {
                return tagId;
            }

            return null;
        }

        private void ConfigurationMouseDown(
     object sender,
     MouseButtonEventArgs e)
        {

            if (!IsConfigurationMode)
                return;

            var source = e.OriginalSource as DependencyObject;

            if (source == null)
                return;

            var element = FindConfiguredElement(source);

            if (element == null)
                return;

            ConfigureControl(element);

            e.Handled = true;
        }

        private void ConfigurationMouseMove(
    object sender,
    MouseEventArgs e)
        {
            if (!IsConfigurationMode)
                return;

            var source = e.OriginalSource as DependencyObject;

            if (source == null)
                return;

            var element = FindConfiguredElement(source);

            if (element == null)
                return;

            ShowAssignedTagTooltip(element);
        }

        //private void ClearLastTooltip()
        //{
        //    if (_lastTooltipElement != null)
        //    {
        //        _lastTooltipElement.ToolTip = null;
        //        _lastTooltipElement = null;
        //    }
        //}

        private void ClearConfigurationTooltips(
    DependencyObject parent)
        {
            if (parent == null)
                return;

            if (parent is FrameworkElement element)
            {
                element.ToolTip = null;
            }

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childrenCount; i++)
            {
                var child =
                    VisualTreeHelper.GetChild(parent, i);

                ClearConfigurationTooltips(child);
            }
        }

        private void ShowAssignedTagTooltip(
    DependencyObject element)
        {
            if (!IsConfigurationMode)
                return;

            string configKey =
                TagProperties.GetConfigKey(element);

            if (string.IsNullOrWhiteSpace(configKey))
                return;

            string unitName = null;

            if (IsDescendantOf(element, CS2U1))
            {
                unitName = "CS2U1";
            }
            else if (IsDescendantOf(element, CS2U2))
            {
                unitName = "CS2U2";
            }
            else
            {
                unitName = "ColdStore2";
            }

            if (string.IsNullOrWhiteSpace(unitName))
                return;

            string controlName =
                $"{unitName}.{configKey}";

            if (element is not FrameworkElement frameworkElement)
                return;

            string tooltipText;

            if (AssignedTags.TryGetValue(
                controlName,
                out string tagName))
            {
                tooltipText =
                    $"Control : {controlName}\n" +
                    $"Tag     : {tagName}";
            }
            else
            {
                tooltipText =
                    $"Control : {controlName}\n" +
                    $"Tag     : Not Assigned";
            }

            ToolTipService.SetToolTip(
                frameworkElement,
                tooltipText);

            ToolTipService.SetInitialShowDelay(
                frameworkElement,
                100);

            ToolTipService.SetShowDuration(
                frameworkElement,
                10000);
        }

        private void ConfigurationButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            IsConfigurationMode = !IsConfigurationMode;

            CS2U1.IsConfigurationMode = IsConfigurationMode;
            CS2U2.IsConfigurationMode = IsConfigurationMode;

            ConfigurationButton.Content =
                IsConfigurationMode
                    ? "Normal Mode"
                    : "Configuration Mode";

            // Clear tooltip when leaving configuration mode
            if (!IsConfigurationMode)
            {
                ClearConfigurationToolTips();
            }
        }

        private void ClearConfigurationToolTips()
        {
            foreach (var control in CS2U1.GetConfiguredControls())
            {
                if (control is FrameworkElement fe)
                    fe.ToolTip = null;
            }

            foreach (var control in CS2U2.GetConfiguredControls())
            {
                if (control is FrameworkElement fe)
                    fe.ToolTip = null;
            }
        }

        private void UpdatePageProperties()
        {
            foreach (var mapping in ControlTagIds)
            {
                string controlName = mapping.Key;

                if (!controlName.StartsWith(
                    "ColdStore2."))
                {
                    continue;
                }

                int tagId = mapping.Value;

                if (!TagUpdateActions.TryGetValue(
                    tagId,
                    out double value))
                {
                    continue;
                }

                string propertyName =
                    controlName.Substring(
                        "ColdStore2.".Length);

                SetPropertyValue(
                    this,
                    propertyName,
                    value);
            }
        }

        private void UpdateColdStoreUnit(
    ColdStoreUnit unit,
    string unitName)
        {
            foreach (var mapping in ControlTagIds)
            {
                string controlName = mapping.Key;

                int tagId = mapping.Value;

                if (!controlName.StartsWith(
                    unitName + "."))
                {
                    continue;
                }

                if (!TagUpdateActions.TryGetValue(
                    tagId,
                    out double value))
                {
                    continue;
                }

                string propertyName =
                    controlName.Substring(
                        unitName.Length + 1);

                SetPropertyValue(
                    unit,
                    propertyName,
                    value);
            }
        }


        //MAIN

        private void SetPropertyValue(
    object target,
    string propertyName,
    double value)
        {
            var property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Public |
                BindingFlags.Instance);

            if (property == null || !property.CanWrite)
                return;

            object convertedValue;

            if (property.PropertyType == typeof(bool))
            {
                convertedValue = value != 0;
            }
            else if (property.PropertyType == typeof(int))
            {
                convertedValue = (int)value;
            }
            else if (property.PropertyType == typeof(double))
            {
                convertedValue = value;
            }
            else if (property.PropertyType == typeof(float))
            {
                convertedValue = (float)value;
            }
            else if (property.PropertyType == typeof(decimal))
            {
                convertedValue = (decimal)value;
            }
            else if (property.PropertyType.IsEnum)
            {
                convertedValue = Enum.ToObject(
                    property.PropertyType,
                    (int)value);
            }
            else
            {
                return;
            }

            property.SetValue(
                target,
                convertedValue);
        }

        private iDbRepository GetRepository()
        {
            try
            {
                // If your app uses App.xaml.cs to store the repository, access it like this:
                // var app = Application.Current as App;
                // return app?.Repository;

                // Create a new context using the connection string from config
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConfigDBConnString"]?.ConnectionString;
                if (string.IsNullOrEmpty(connectionString))
                {
                    System.Diagnostics.Debug.WriteLine("Error: ConfigDBConnString not found in configuration.");
                    return null;
                }

                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;

                var context = new AppDbContext(options);
                return new dbRepository(context);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating repository: {ex.Message}");
                return null;
            }
        }
    }
}
