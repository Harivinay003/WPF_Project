using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Reflection;
using System.Linq;

namespace WPFSCADA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer dateTimeTimer;
        public MainWindow()
        {
            InitializeComponent();
            // start date/time updater
            dateTimeTimer = new DispatcherTimer();
            dateTimeTimer.Interval = TimeSpan.FromSeconds(1);
            dateTimeTimer.Tick += (s, e) => DateTimeLabel.Text = DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss");
            dateTimeTimer.Start();
            // show initial content
            PageTitle.Text = "OVERVIEW";
            //mainWindow.Navigate(new WPFSCADA.Pages.Overview());
            mainWindow.Navigate(new WPFSCADA.Pages.Condenser1());
        }

        // Set all left menu buttons to the regular menu style and apply the
        // highlight style to the clicked button.
        private void UpdateLeftMenuStyles(Button clicked)
        {
            var menuStyle = (Style)FindResource("menuButtonStyle");
            var highlightStyle = (Style)FindResource("highlightMenuButtonStyle");

            scadaButton.Style = menuStyle;
            usersButton.Style = menuStyle;
            settingsButton.Style = menuStyle;
            reportsButton.Style = menuStyle;
            trendsBbutton.Style = menuStyle;
            alarmsButton.Style = menuStyle;
            eventsBbutton.Style = menuStyle;
            graphicsButton.Style = menuStyle;
            emsButton.Style = menuStyle;

            if (clicked != null)
                clicked.Style = highlightStyle;
        }

        private void scadaButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateLeftMenuStyles((Button)sender);
            mainWindow.Navigate(new WPFSCADA.Pages.Overview());
            PageTitle.Text = "OVERVIEW";
        }

        private void usersButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateLeftMenuStyles((Button)sender);
            PageTitle.Text = "USERS";
        }
        private void settingsButton_Click(object sender, RoutedEventArgs e)
        { 
            UpdateLeftMenuStyles((Button)sender);
            PageTitle.Text = "SETTINGS";
        }

        private void reportsButton_Click(object sender, RoutedEventArgs e) 
        { 
            UpdateLeftMenuStyles((Button)sender); 
            PageTitle.Text = "REPORTS";
        }
        private void trendsBbutton_Click(object sender, RoutedEventArgs e)
        {
            UpdateLeftMenuStyles((Button)sender);
            PageTitle.Text = "TRENDS";
        }
        private void alarmsButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateLeftMenuStyles((Button)sender);
            PageTitle.Text = "ALARMS";
        }
        private void eventsBbutton_Click(object sender, RoutedEventArgs e)
        {
            UpdateLeftMenuStyles((Button)sender);
            PageTitle.Text = "EVENTS";
        }
        private void graphicsButton_MouseEnter(object sender, RoutedEventArgs e)
        {
            // Toggle popup
            if (graphicsPopup.IsOpen)
            {
                graphicsPopup.IsOpen = false;
                return;
            }

            graphicsPopupPanel.Children.Clear();

            var pageTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(Page).IsAssignableFrom(t) && t.Namespace == "WPFSCADA.Pages")
                .OrderBy(t => t.Name)
                .ToList();

            foreach (var pt in pageTypes)
            {
                // instantiate the page briefly to read its Title property for display
                if(pt.Name == "Overview")
                    continue;
                string displayName = pt.Name;
                try
                {
                    var temp = Activator.CreateInstance(pt) as Page;
                    if (temp != null && !string.IsNullOrWhiteSpace(temp.Title))
                        displayName = temp.Title;
                }
                catch
                {
                    // ignore instantiation errors and fall back to type name
                }

                var btn = new Button
                {
                    Content = displayName,
                    Tag = pt,
                    Style = (Style)FindResource("menuButtonSmallStyle"),
                    Width = 150,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0,4,0,4)
                };
                btn.Click += (s, ev) =>
                {
                    var t = (Type)((Button)s).Tag;
                    var page = (Page)Activator.CreateInstance(t);
                    UpdateLeftMenuStyles((Button)sender);
                    mainWindow.Navigate(page);
                    PageTitle.Text = displayName;
                    graphicsPopup.IsOpen = false;
                };
                graphicsPopupPanel.Children.Add(btn);
            }

            graphicsPopup.PlacementTarget = (UIElement)sender;
            graphicsPopup.IsOpen = true;
        }        
        private void emsButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateLeftMenuStyles((Button)sender);
            PageTitle.Text = "ENERGY MONITORING";
        }

    }
}
