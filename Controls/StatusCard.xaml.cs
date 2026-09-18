using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFSCADA.Controls
{
    public partial class StatusCard : UserControl
    {
        public StatusCard()
        {
            InitializeComponent();
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title),
                typeof(string),
                typeof(StatusCard));

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value),
                typeof(string),
                typeof(StatusCard));

        public string Unit
        {
            get => (string)GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register(nameof(Unit),
                typeof(string),
                typeof(StatusCard));
        public bool IsEditable
        {
            get => (bool)GetValue(IsEditableProperty);
            set => SetValue(IsEditableProperty, value);
        }

        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register(
                nameof(IsEditable),
                typeof(bool),
                typeof(StatusCard),
                new PropertyMetadata(false));
        public event RoutedEventHandler EditClicked;

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditClicked?.Invoke(this, e);
        }
        public string Status
        {
            get => (string)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(nameof(Status),
                typeof(string),
                typeof(StatusCard),
                new PropertyMetadata("Normal"));

        public Brush AccentBrush
        {
            get => (Brush)GetValue(AccentBrushProperty);
            set => SetValue(AccentBrushProperty, value);
        }

        public static readonly DependencyProperty AccentBrushProperty =
            DependencyProperty.Register(nameof(AccentBrush),
                typeof(Brush),
                typeof(StatusCard),
                new PropertyMetadata(Brushes.DodgerBlue));
    }
}