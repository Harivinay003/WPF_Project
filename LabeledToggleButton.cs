using System.Windows;
using System.Windows.Controls.Primitives;

namespace WPFSCADA
{
    public class LabeledToggleButton : ToggleButton
    {
        public static readonly DependencyProperty OffTextProperty =
            DependencyProperty.Register(nameof(OffText), typeof(string), typeof(LabeledToggleButton), new PropertyMetadata("Off"));

        public string OffText
        {
            get => (string)GetValue(OffTextProperty);
            set => SetValue(OffTextProperty, value);
        }

        public static readonly DependencyProperty OnTextProperty =
            DependencyProperty.Register(nameof(OnText), typeof(string), typeof(LabeledToggleButton), new PropertyMetadata("On"));

        public string OnText
        {
            get => (string)GetValue(OnTextProperty);
            set => SetValue(OnTextProperty, value);
        }
    }
}
