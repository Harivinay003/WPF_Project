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
using WPFSCADA.Services;
using Page = System.Windows.Controls.Page;
using WPFSCADA.Services;

namespace WPFSCADA.Pages
{
    /// <summary>
    /// Interaction logic for ColdStore1.xaml
    /// </summary>
    public partial class ColdStore1 : Page, System.ComponentModel.INotifyPropertyChanged
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
                if (CS1U1 != null)
                    CS1U1.IsConfigurationMode = value;

                if (CS1U2 != null)
                    CS1U2.IsConfigurationMode = value;

                // Update button text
                if (ConfigurationButton != null)
                {
                    ConfigurationButton.Content =
                        value ? "Normal Mode" : "Configuration Mode";
                }

                // Remove configuration tooltips when leaving configuration mode
                if (!value)
                {
                    ClearConfigurationTooltips(CS1U1);
                    ClearConfigurationTooltips(CS1U2);
                }
            }
        }


        public ColdStore1()
        {
            InitializeComponent();
            this.DataContext = this;

            CS1U1.PageName = nameof(ColdStore1);
            CS1U2.PageName = nameof(ColdStore1);

            AddHandler(
        UIElement.PreviewMouseDownEvent,
        new MouseButtonEventHandler(ConfigurationMouseDown),
        true);

            AddHandler(
    UIElement.PreviewMouseMoveEvent,
    new MouseEventHandler(ConfigurationMouseMove),
    true);

            //if (CS1U1 != null)
            //{
            //    CS1U1.ManualDefrostTagId = unitLeftmanualDefrostTagId;
            //    CS1U1.AutoManualTagId = isAutomatedLeftTagId;
            //    CS1U1.UnitName = "COLDSTORE 1 UNIT 1";
            //}


            // Initialize with default values
            //ColdStoreTemp = 19.23;
            //SetPoint = 18.00;

            //PopulateTagIds();

            //// Initialize ColdStoreUnit instances with default values
            //InitializeDefaultValues();

            // Initialize the repository
            Repository = GetRepository();

            _plcService = new PLCService(Repository);

            _configurationService = new ConfigurationService(Repository);


        //    CS1U1.ConfigurationControlSelected +=
        //ColdUnit_ConfigurationControlSelected;

        //    CS1U2.ConfigurationControlSelected +=
        //        ColdUnit_ConfigurationControlSelected;

        Loaded += ColdStore1_Loaded;


            //Task.Run(() => GetLiveValuesFromPLC());

            //// Initialize timer to run GetLiveValues every 500ms
            //_liveValuesTimer = new DispatcherTimer();
            //_liveValuesTimer.Interval = TimeSpan.FromMilliseconds(250);
            ////_liveValuesTimer.Tick += (sender, e) => GetLiveValuesFromDB();
            //_liveValuesTimer.Tick += async (sender, e) => await GetLiveValuesFromPLC();
            //_liveValuesTimer.Start();


            //GetLiveValues();

            //// Initialize timer to run GetLiveValues every 500ms
            //_liveValuesTimer = new DispatcherTimer();
            //_liveValuesTimer.Interval = TimeSpan.FromMilliseconds(500);
            //_liveValuesTimer.Tick += (sender, e) => GetLiveValues();
            //_liveValuesTimer.Start();
        }

        private async void ColdStore1_Loaded(object sender, RoutedEventArgs e)
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
                    CS1U1,
                    "CS1U1");

                UpdateColdStoreUnit(
                    CS1U2,
                    "CS1U2");
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

        //private void ConfigureControl(DependencyObject element)
        //{
        //    if (!IsConfigurationMode)
        //        return;

        //    var configKey =
        //        TagProperties.GetConfigKey(element);

        //    if (string.IsNullOrWhiteSpace(configKey))
        //        return;

        //    string unitName = null;

        //    if (IsDescendantOf(element, CS1U1))
        //    {
        //        unitName = "CS1U1";
        //    }
        //    else if (IsDescendantOf(element, CS1U2))
        //    {
        //        unitName = "CS1U2";
        //    }

        //    if (string.IsNullOrWhiteSpace(unitName))
        //        return;

        //    string controlName =
        //        $"{unitName}.{configKey}";

        //    var page = Repository.GetPageByName("ColdStore1");

        //    if (page == null)
        //    {
        //        MessageBox.Show("ColdStore1 page was not found.");
        //        return;
        //    }

        //    var pageItem =
        //        Repository.GetPageItem(page.Id, controlName);

        //    if (pageItem == null)
        //    {
        //        MessageBox.Show(
        //            $"No PageItem found for '{controlName}'.");
        //        return;
        //    }

        //    var window =
        //        new WPFSCADA.Configuration.TagSelectionWindow(
        //            Repository,
        //            pageItem);

        //    window.Owner = Window.GetWindow(this);

        //    window.ShowDialog();
        //}

        //private void ConfigurePageControls(int pageId)
        //{
        //    foreach (var child in GetDirectConfiguredControls(this))
        //    {
        //        var configKey =
        //            TagProperties.GetConfigKey(child);

        //        if (string.IsNullOrWhiteSpace(configKey))
        //            continue;

        //        string controlName =
        //            $"ColdStore1.{configKey}";

        //        var pageItem =
        //            Repository.GetPageItem(
        //                pageId,
        //                controlName);

        //        if (pageItem == null)
        //        {
        //            Repository.CreatePageItem(
        //                new PageItem
        //                {
        //                    PageId = pageId,
        //                    Name = controlName,
        //                    Title = configKey,
        //                    Description =
        //                        $"Configuration for {controlName}"
        //                });
        //        }
        //    }
        //}

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
                    $"ColdStore1.{configKey}";

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

                // Don't go inside CS1U1 / CS1U2
                if (child == CS1U1 || child == CS1U2)
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

            if (IsDescendantOf(element, CS1U1))
            {
                unitName = "CS1U1";

                controlName =
                    $"{unitName}.{configKey}";
            }
            else if (IsDescendantOf(element, CS1U2))
            {
                unitName = "CS1U2";

                controlName =
                    $"{unitName}.{configKey}";
            }
            else
            {
                // Control belongs directly to ColdStore1
                unitName = "ColdStore1";

                controlName =
                    $"{unitName}.{configKey}";
            }

            var page =
                Repository.GetPageByName("ColdStore1");

            if (page == null)
            {
                MessageBox.Show(
                    "ColdStore1 page was not found.");

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
            ;
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
            int pageId = 1;

            // 1. Configure controls directly on ColdStore1
            ConfigurePageControls(pageId);

            // 2. Configure controls inside CS1U1
            ConfigureUnit(
                CS1U1,
                "CS1U1",
                pageId);

            // 3. Configure controls inside CS1U2
            ConfigureUnit(
                CS1U2,
                "CS1U2",
                pageId);

            // 4. Load existing tag assignments
            LoadAssignedTags(pageId);

            // 5. Load control -> tag ID mappings
            LoadControlTagMappings(pageId);

            // 6. Get live values
            // GetLiveValues();
        }

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
                    $"ColdStore1.{configKey}");
            }

            // -----------------------------------------
            // CS1U1
            // -----------------------------------------

            foreach (var control in
                     CS1U1.GetConfiguredControls())
            {
                var configKey =
                    TagProperties.GetConfigKey(control);

                if (string.IsNullOrWhiteSpace(configKey))
                    continue;

                controlNames.Add(
                    $"CS1U1.{configKey}");
            }

            // -----------------------------------------
            // CS1U2
            // -----------------------------------------

            foreach (var control in
                     CS1U2.GetConfiguredControls())
            {
                var configKey =
                    TagProperties.GetConfigKey(control);

                if (string.IsNullOrWhiteSpace(configKey))
                    continue;

                controlNames.Add(
                    $"CS1U2.{configKey}");
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

        //    private void LoadAssignedTagsForUnit(
        //ColdStoreUnit unit,
        //string unitName,
        //int pageId)
        //    {
        //        var controls =
        //            unit.GetConfiguredControls().ToList();

        //        foreach (var control in controls)
        //        {
        //            var configKey =
        //                TagProperties.GetConfigKey(control);

        //            if (string.IsNullOrWhiteSpace(configKey))
        //                continue;

        //            string controlName =
        //                $"{unitName}.{configKey}";

        //            var pageItem =
        //                Repository.GetPageItem(
        //                    pageId,
        //                    controlName);

        //            if (pageItem == null)
        //                continue;

        //            var pageItemTag =
        //                Repository.GetPageItemTag(pageItem.Id);

        //            if (pageItemTag == null)
        //                continue;

        //            if (pageItemTag.Tag == null)
        //                continue;

        //            AssignedTags[controlName] =
        //                pageItemTag.Tag.Name;
        //        }
        //    }

        //private void LoadControlTagMappings(int pageId)
        //{
        //    ControlTagIds.Clear();

        //    var pageItemTags =
        //        Repository.GetPageItemTags(pageId).ToList();


        //    foreach (var item in pageItemTags)
        //    {
        //        if (item.PageItem == null)
        //            continue;

        //        ControlTagIds[item.PageItem.Name] =
        //            item.TagId;
        //    }
        //}

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

        //    private int? GetTagId(string controlName)
        //    {
        //        if (ControlTagIds.TryGetValue(
        //            controlName,
        //            out int tagId))
        //        {
        //            return tagId;
        //        }

        //        return null;
        //    }

        //    private void ColdUnit_ConfigurationControlSelected(
        //ColdStoreUnit unit,
        //DependencyObject element,
        //string configKey)
        //    {
        //        string unitName;

        //        if (unit == CS1U1)
        //        {
        //            unitName = "CS1U1";
        //        }
        //        else if (unit == CS1U2)
        //        {
        //            unitName = "CS1U2";
        //        }
        //        else
        //        {
        //            MessageBox.Show("Unknown ColdStoreUnit");
        //            return;
        //        }

        //        string controlName =
        //            $"{unitName}.{configKey}";

        //        MessageBox.Show(
        //            $"Unit: {unitName}\n" +
        //            $"ConfigKey: {configKey}\n" +
        //            $"PageItem Name: {controlName}");
        //    }

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

            if (IsDescendantOf(element, CS1U1))
            {
                unitName = "CS1U1";
            }
            else if (IsDescendantOf(element, CS1U2))
            {
                unitName = "CS1U2";
            }
            else
            {
                unitName = "ColdStore1";
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

        private void SetTooltip(
    DependencyObject element,
    string text)
        {
            if (element is FrameworkElement frameworkElement)
            {
                frameworkElement.ToolTip = text;
            }
        }


        private void ConfigurationButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            IsConfigurationMode = !IsConfigurationMode;

            CS1U1.IsConfigurationMode = IsConfigurationMode;
            CS1U2.IsConfigurationMode = IsConfigurationMode;

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
            foreach (var control in CS1U1.GetConfiguredControls())
            {
                if (control is FrameworkElement fe)
                    fe.ToolTip = null;
            }

            foreach (var control in CS1U2.GetConfiguredControls())
            {
                if (control is FrameworkElement fe)
                    fe.ToolTip = null;
            }
        }



        private Tag GetTagForControl(string controlName)
        {
            var page = Repository.GetPageByName("ColdStore1");

            if (page == null)
                return null;

            var pageItem = Repository.GetPageItem(
                page.Id,
                controlName);

            if (pageItem == null)
                return null;

            var pageItemTag = Repository.GetPageItemTag(
                pageItem.Id);

            if (pageItemTag == null)
                return null;

            return pageItemTag.Tag;
        }


        //private void GetLiveValues()
        //{
        //    var tagIds =
        //        ControlTagIds.Values
        //            .Distinct()
        //            .ToList();

        //    TagUpdateActions =
        //        Repository.GetLiveValuesByTagIds(tagIds);

        //    // Page-level values
        //    UpdatePageProperties();

        //    // CS1U1 values
        //    UpdateColdStoreUnit(
        //        CS1U1,
        //        "CS1U1");

        //    // CS1U2 values
        //    UpdateColdStoreUnit(
        //        CS1U2,
        //        "CS1U2");
        //}

        private void UpdatePageProperties()
        {
            foreach (var mapping in ControlTagIds)
            {
                string controlName = mapping.Key;

                if (!controlName.StartsWith(
                    "ColdStore1."))
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
                        "ColdStore1.".Length);

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
                    Debug.WriteLine(
        $"NO VALUE: {controlName}, TagId={tagId}");
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

        //    private void SetPropertyValue(
        //object target,
        //string propertyName,
        //double value)
        //    {
        //        var property = target.GetType().GetProperty(
        //            propertyName,
        //            BindingFlags.Public |
        //            BindingFlags.Instance);

        //        if (property == null || !property.CanWrite)
        //            return;

        //        object convertedValue;

        //        if (property.PropertyType == typeof(bool))
        //        {
        //            convertedValue = value != 0;
        //        }
        //        else if (property.PropertyType == typeof(int))
        //        {
        //            convertedValue = (int)value;
        //        }
        //        else if (property.PropertyType == typeof(double))
        //        {
        //            convertedValue = value;
        //        }
        //        else if (property.PropertyType == typeof(float))
        //        {
        //            convertedValue = (float)value;
        //        }
        //        else if (property.PropertyType == typeof(decimal))
        //        {
        //            convertedValue = (decimal)value;
        //        }
        //        else if (property.PropertyType.IsEnum)
        //        {
        //            convertedValue = Enum.ToObject(
        //                property.PropertyType,
        //                (int)value);
        //        }
        //        else
        //        {
        //            return;
        //        }

        //        property.SetValue(
        //            target,
        //            convertedValue);
        //    }

        private void SetPropertyValue(
    object target,
    string propertyName,
    double value)
        {
            var property =
                target.GetType().GetProperty(
                    propertyName,
                    BindingFlags.Public |
                    BindingFlags.Instance);

            if (property == null)
            {
                Debug.WriteLine(
                    $"PROPERTY NOT FOUND: " +
                    $"{target.GetType().Name}.{propertyName}");

                return;
            }

            if (!property.CanWrite)
            {
                Debug.WriteLine(
                    $"PROPERTY READ ONLY: " +
                    $"{target.GetType().Name}.{propertyName}");

                return;
            }

            //Debug.WriteLine(
            //    $"SETTING: " +
            //    $"{target.GetType().Name}.{propertyName} = {value}");

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
                Debug.WriteLine(
                    $"UNSUPPORTED PROPERTY TYPE: " +
                    $"{property.PropertyType}");

                return;
            }

            property.SetValue(
                target,
                convertedValue);
        }

        //    private void SetPropertyValue(
        //object target,
        //string propertyName,
        //double value)
        //    {
        //        System.Diagnostics.Debug.WriteLine(
        //            $"SET PROPERTY: {target.GetType().Name}.{propertyName} = {value}");

        //        var property =
        //            target.GetType().GetProperty(
        //                propertyName,
        //                BindingFlags.Public |
        //                BindingFlags.Instance);

        //        if (property == null)
        //        {
        //            System.Diagnostics.Debug.WriteLine(
        //                $"PROPERTY NOT FOUND: {propertyName}");

        //            return;
        //        }

        //        if (!property.CanWrite)
        //            return;

        //        object convertedValue;

        //        if (property.PropertyType == typeof(bool))
        //        {
        //            convertedValue = value != 0;
        //        }
        //        else if (property.PropertyType == typeof(int))
        //        {
        //            convertedValue = (int)value;
        //        }
        //        else if (property.PropertyType == typeof(double))
        //        {
        //            convertedValue = value;
        //        }
        //        else if (property.PropertyType == typeof(float))
        //        {
        //            convertedValue = (float)value;
        //        }
        //        else if (property.PropertyType == typeof(decimal))
        //        {
        //            convertedValue = (decimal)value;
        //        }
        //        else
        //        {
        //            System.Diagnostics.Debug.WriteLine(
        //                $"UNSUPPORTED PROPERTY TYPE: {property.PropertyType}");

        //            return;
        //        }

        //        System.Diagnostics.Debug.WriteLine(
        //            $"CONVERTED: {propertyName} = {convertedValue}");

        //        property.SetValue(target, convertedValue);
        //    }

        //private void GetLiveValues()
        //{
        //    TagUpdateActions = Repository.GetLiveValuesByTagIds(TagIdsToUpdate);
        //    ColdStoreTemp = TagUpdateActions.ContainsKey(ColdStoreTempTagId) ? TagUpdateActions[ColdStoreTempTagId] : 0.0; // Default value if not found
        //    SetPoint = TagUpdateActions.ContainsKey(SetPointTagId) ? TagUpdateActions[SetPointTagId] : 0.0; // Default value if not found
        //    //assign tagupdateactions to the ColdStoreUnit controls
        //    if (ColdUnitLeft != null)
        //    {
        //        ColdUnitLeft.IsRemote = TagUpdateActions.ContainsKey(isRemoteLeftTagId) ? (TagUpdateActions[isRemoteLeftTagId] != 0) : false;
        //        ColdUnitLeft.IsAutomated = TagUpdateActions.ContainsKey(isAutomatedLeftTagId) ? (TagUpdateActions[isAutomatedLeftTagId] != 0) : false;
        //        ColdUnitLeft.Fan1RunFB = TagUpdateActions.ContainsKey(fan1RunFBLeftTagId) ? (TagUpdateActions[fan1RunFBLeftTagId] != 0) : false;
        //        ColdUnitLeft.Fan2RunFB = TagUpdateActions.ContainsKey(fan2RunFBLeftTagId) ? (TagUpdateActions[fan2RunFBLeftTagId] != 0) : false;
        //        ColdUnitLeft.Fan3RunFB = TagUpdateActions.ContainsKey(fan3RunFBLeftTagId) ? (TagUpdateActions[fan3RunFBLeftTagId] != 0) : false;
        //        ColdUnitLeft.Fan4RunFB = TagUpdateActions.ContainsKey(fan4RunFBLeftTagId) ? (TagUpdateActions[fan4RunFBLeftTagId] != 0) : false;
        //        ColdUnitLeft.UnitRunFB = TagUpdateActions.ContainsKey(unitRunFBLeftTagId) ? (TagUpdateActions[unitRunFBLeftTagId] != 0) : false;
        //        ColdUnitLeft.EvaporatorTemp = TagUpdateActions.ContainsKey(evaporatorTempLeftTagId) ? TagUpdateActions[evaporatorTempLeftTagId] : 0.0; // Default value if not found
        //        ColdUnitLeft.LiquidDrainMinsSetpoint = TagUpdateActions.ContainsKey(liquidDrainMinsSetpointLeftTagId) ? (int)TagUpdateActions[liquidDrainMinsSetpointLeftTagId] : 0; // Default value if not found
        //        ColdUnitLeft.DefrostMinsSetpoint = TagUpdateActions.ContainsKey(defrostMinsSetpointLeftTagId) ? (int)TagUpdateActions[defrostMinsSetpointLeftTagId] : 0; // Default value if not found
        //        ColdUnitLeft.LiquidDrainMins = TagUpdateActions.ContainsKey(liquidDrainMinsLeftTagId) ? (int)TagUpdateActions[liquidDrainMinsLeftTagId] : 0; // Default value if not found
        //        ColdUnitLeft.DefrostMins = TagUpdateActions.ContainsKey(defrostMinsLeftTagId) ? (int)TagUpdateActions[defrostMinsLeftTagId] : 0; // Default value if not found
        //        ColdUnitLeft.FreezeHrs = TagUpdateActions.ContainsKey(freezeHrsLeftTagId) ? (int)TagUpdateActions[freezeHrsLeftTagId] : 0; // Default value if not found
        //        ColdUnitLeft.FreezeMins = TagUpdateActions.ContainsKey(freezeMinsLeftTagId) ? (int)TagUpdateActions[freezeMinsLeftTagId] : 0; // Default value if not found
        //        ColdStoreUnit.MODE mode = ColdStoreUnit.MODE.IDEAL;
        //        if (TagUpdateActions.ContainsKey(defrostModeLeftTagId) && (TagUpdateActions[defrostModeLeftTagId] != 0))
        //        {
        //            mode = ColdStoreUnit.MODE.DEFROSTING;
        //        }
        //        else if (TagUpdateActions.ContainsKey(freezingModeLeftTagId) && (TagUpdateActions[freezingModeLeftTagId] != 0))
        //        {
        //            mode = ColdStoreUnit.MODE.FREEZING;
        //        }
        //        ColdUnitLeft.LsvFb = TagUpdateActions.ContainsKey(lsvFBLeftTagId) ? (TagUpdateActions[lsvFBLeftTagId] != 0) : false;
        //        ColdUnitLeft.SsvFb = TagUpdateActions.ContainsKey(ssvFBLeftTagId) ? (TagUpdateActions[ssvFBLeftTagId] != 0) : false;
        //        ColdUnitLeft.HgsvFb = TagUpdateActions.ContainsKey(hgsvFBLeftTagId) ? (TagUpdateActions[hgsvFBLeftTagId] != 0) : false;
        //        ColdUnitLeft.HgrsvFb = TagUpdateActions.ContainsKey(hgrsvFBLeftTagId) ? (TagUpdateActions[hgrsvFBLeftTagId] != 0) : false;
        //        ColdUnitLeft.Mode = mode;
        //    }
        //    if (ColdUnitRight != null)
        //    {
        //        //ColdUnitRight.IsRemote = TagUpdateActions.ContainsKey(isRemoteRightTagId) ? (TagUpdateActions[isRemoteRightTagId] != 0) : false;
        //        //ColdUnitRight.IsAutomated = TagUpdateActions.ContainsKey(isAutomatedRightTagId) ? (TagUpdateActions[isAutomatedRightTagId] != 0) : false;
        //        //ColdUnitRight.Fan1RunFB = TagUpdateActionsBool.ContainsKey(fan1RunFBRightTagId) ? TagUpdateActionsBool[fan1RunFBRightTagId] : false ;
        //        //ColdUnitRight.Fan2RunFB = TagUpdateActionsBool.ContainsKey(fan2RunFBRightTagId) ? TagUpdateActionsBool[fan2RunFBRightTagId] : false;
        //        //ColdUnitRight.Fan3RunFB = TagUpdateActionsBool.ContainsKey(fan3RunFBRightTagId) ? TagUpdateActionsBool[fan3RunFBRightTagId] : false;
        //        //ColdUnitRight.Fan4RunFB = TagUpdateActionsBool.ContainsKey(fan4RunFBRightTagId) ? TagUpdateActionsBool[fan4RunFBRightTagId] : false;
        //        //ColdUnitRight.UnitRunFB = TagUpdateActionsBool.ContainsKey(unitRunFBRightTagId) ? TagUpdateActionsBool[unitRunFBRightTagId] : false;
        //        //ColdUnitRight.EvaporatorTemp = TagUpdateActions.ContainsKey(evaporatorTempRightTagId) ? TagUpdateActions[evaporatorTempRightTagId] : 0.0; // Default value if not found
        //        //ColdUnitRight.LiquidDrainMinsSetpoint = TagUpdateActions.ContainsKey(liquidDrainMinsSetpointRightTagId) ? (int)TagUpdateActions[liquidDrainMinsSetpointRightTagId] : 0; // Default value if not found
        //        //ColdUnitRight.DefrostMinsSetpoint = TagUpdateActions.ContainsKey(defrostMinsSetpointRightTagId) ? (int)TagUpdateActions[defrostMinsSetpointRightTagId] : 0; // Default value if not found
        //        //ColdUnitRight.LiquidDrainMins = TagUpdateActions.ContainsKey(liquidDrainMinsRightTagId) ? (int)TagUpdateActions[liquidDrainMinsRightTagId] : 0; // Default value if not found
        //        //ColdUnitRight.DefrostMins = TagUpdateActions.ContainsKey(defrostMinsRightTagId) ? (int)TagUpdateActions[defrostMinsRightTagId] : 0; // Default value if not found
        //        //ColdUnitRight.FreezeHrs = TagUpdateActions.ContainsKey(freezeHrsRightTagId) ? (int)TagUpdateActions[freezeHrsRightTagId] : 0; // Default value if not found
        //        //ColdUnitRight.FreezeMins = TagUpdateActions.ContainsKey(freezeMinsRightTagId) ? (int)TagUpdateActions[freezeMinsRightTagId] : 0; // Default value if not found
        //        //ColdStoreUnit.MODE mode = ColdStoreUnit.MODE.IDEAL;
        //        //if (TagUpdateActions.ContainsKey(defrostModeRightTagId))
        //        //{
        //        //    mode = ColdStoreUnit.MODE.DEFROSTING;
        //        //}
        //        //else if (TagUpdateActions.ContainsKey(freezingModeRightTagId))
        //        //{
        //        //    mode = ColdStoreUnit.MODE.FREEZING;
        //        //}
        //        //ColdUnitRight.LsvFb = TagUpdateActionsBool.ContainsKey(lsvFBRightTagId);
        //        //ColdUnitRight.SsvFb = TagUpdateActionsBool.ContainsKey(ssvFBRightTagId);
        //        //ColdUnitRight.HgsvFb = TagUpdateActionsBool.ContainsKey(hgsvFBRightTagId);
        //        //ColdUnitRight.HgrsvFb = TagUpdateActionsBool.ContainsKey(hgrsvFBRightTagId);
        //        //ColdUnitRight.Mode = mode;
        //    }
        //}

        //private void PopulateTagIds()
        //{
        //    TagIdsToUpdate.Add(ColdStoreTempTagId);
        //    TagIdsToUpdate.Add(SetPointTagId);

        //    // Add tag IDs for ColdStoreUnitLeft
        //    TagIdsToUpdate.Add(isRemoteLeftTagId);
        //    TagIdsToUpdate.Add(isAutomatedLeftTagId);
        //    TagIdsToUpdate.Add(fan1RunFBLeftTagId);
        //    TagIdsToUpdate.Add(fan2RunFBLeftTagId);
        //    TagIdsToUpdate.Add(fan3RunFBLeftTagId);
        //    TagIdsToUpdate.Add(fan4RunFBLeftTagId);
        //    TagIdsToUpdate.Add(unitRunFBLeftTagId);
        //    TagIdsToUpdate.Add(evaporatorTempLeftTagId);
        //    TagIdsToUpdate.Add(liquidDrainMinsSetpointLeftTagId);
        //    TagIdsToUpdate.Add(defrostMinsSetpointLeftTagId);
        //    TagIdsToUpdate.Add(liquidDrainMinsLeftTagId);
        //    TagIdsToUpdate.Add(defrostMinsLeftTagId);
        //    TagIdsToUpdate.Add(freezeHrsLeftTagId);
        //    TagIdsToUpdate.Add(freezeMinsLeftTagId);
        //    TagIdsToUpdate.Add(defrostModeLeftTagId);
        //    TagIdsToUpdate.Add(freezingModeLeftTagId);
        //    TagIdsToUpdate.Add(lsvFBLeftTagId);
        //    TagIdsToUpdate.Add(lsvOpenLeftTagId);
        //    TagIdsToUpdate.Add(lsvCloseLeftTagId);
        //    TagIdsToUpdate.Add(ssvFBLeftTagId);
        //    TagIdsToUpdate.Add(ssvOpenLeftTagId);
        //    TagIdsToUpdate.Add(ssvCloseLeftTagId);
        //    TagIdsToUpdate.Add(hgsvFBLeftTagId);
        //    TagIdsToUpdate.Add(hgsvOpenLeftTagId);
        //    TagIdsToUpdate.Add(hgsvCloseLeftTagId);
        //    TagIdsToUpdate.Add(hgrsvFBLeftTagId);
        //    TagIdsToUpdate.Add(hgrsvOpenLeftTagId);
        //    TagIdsToUpdate.Add(hgrsvCloseLeftTagId);

        //    // Add tag IDs for ColdStoreUnitRight
        //    TagIdsToUpdate.Add(isRemoteRightTagId);
        //    TagIdsToUpdate.Add(isAutomatedRightTagId);
        //    TagIdsToUpdate.Add(fan1RunFBRightTagId);
        //    TagIdsToUpdate.Add(fan2RunFBRightTagId);
        //    TagIdsToUpdate.Add(fan3RunFBRightTagId);
        //    TagIdsToUpdate.Add(fan4RunFBRightTagId);
        //    TagIdsToUpdate.Add(unitRunFBRightTagId);
        //    TagIdsToUpdate.Add(evaporatorTempRightTagId);
        //    TagIdsToUpdate.Add(liquidDrainMinsSetpointRightTagId);
        //    TagIdsToUpdate.Add(defrostMinsSetpointRightTagId);
        //    TagIdsToUpdate.Add(liquidDrainMinsRightTagId);
        //    TagIdsToUpdate.Add(defrostMinsRightTagId);
        //    TagIdsToUpdate.Add(freezeHrsRightTagId);
        //    TagIdsToUpdate.Add(freezeMinsRightTagId);
        //    TagIdsToUpdate.Add(defrostModeRightTagId);
        //    TagIdsToUpdate.Add(freezingModeRightTagId);
        //    TagIdsToUpdate.Add(lsvFBRightTagId);
        //    TagIdsToUpdate.Add(lsvOpenRightTagId);
        //    TagIdsToUpdate.Add(lsvCloseRightTagId);
        //    TagIdsToUpdate.Add(ssvFBRightTagId);
        //    TagIdsToUpdate.Add(ssvOpenRightTagId);
        //    TagIdsToUpdate.Add(ssvCloseRightTagId);
        //    TagIdsToUpdate.Add(hgsvFBRightTagId);
        //    TagIdsToUpdate.Add(hgsvOpenRightTagId);
        //    TagIdsToUpdate.Add(hgsvCloseRightTagId);
        //    TagIdsToUpdate.Add(hgrsvFBRightTagId);
        //    TagIdsToUpdate.Add(hgrsvOpenRightTagId);
        //    TagIdsToUpdate.Add(hgrsvCloseRightTagId);

        //}

        //private void InitializeDefaultValues()
        //{
        //    if (ColdUnitLeft != null)
        //    {
        //        ColdUnitLeft.UnitName = "Cold Store Unit - 1";
        //        ColdUnitLeft.EvaporatorTemp = 0.0;
        //        ColdUnitLeft.FreezeHrs = 0;
        //        ColdUnitLeft.FreezeMins = 0;
        //        ColdUnitLeft.DefrostMinsSetpoint = 0;
        //        ColdUnitLeft.LiquidDrainMinsSetpoint = 0;
        //        ColdUnitLeft.FreezeHrs = 0;
        //        ColdUnitLeft.FreezeMins = 0;
        //        ColdUnitLeft.DefrostMins = 0;
        //        ColdUnitLeft.LiquidDrainMins = 0;
        //        ColdUnitLeft.IsRemote = false;
        //        ColdUnitLeft.IsAutomated = false;
        //        ColdUnitLeft.Fan1RunFB = false;
        //        ColdUnitLeft.Fan2RunFB = false;
        //        ColdUnitLeft.Fan3RunFB = false;
        //        ColdUnitLeft.Fan4RunFB = false;
        //        ColdUnitLeft.UnitRunFB = false;
        //        ColdUnitLeft.Mode = WPFSCADA.Controls.ColdStoreUnit.MODE.DEFROSTING;
        //        ColdUnitLeft.LiquidDrainMinsSetpoint = 0;
        //    }

        //    if (ColdUnitRight != null)
        //    {
        //        ColdUnitRight.UnitName = "Cold Store Unit - 2";
        //        ColdUnitRight.EvaporatorTemp = 0.0;
        //        ColdUnitRight.Fan1RunFB = false;
        //        ColdUnitRight.Fan2RunFB = false;
        //        ColdUnitRight.Fan3RunFB = false;
        //        ColdUnitRight.Fan4RunFB = false;
        //        ColdUnitRight.UnitRunFB = false;
        //        ColdUnitRight.Mode = WPFSCADA.Controls.ColdStoreUnit.MODE.FREEZING;
        //        ColdUnitRight.LiquidDrainMinsSetpoint = 0;
        //    }
        //}



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
