using System.Windows;
using System.Windows.Controls;

namespace WPFSCADA
{
    public static class ButtonAssist
    {
        public static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.RegisterAttached(
                "IsHighlighted",
                typeof(bool),
                typeof(ButtonAssist),
                new FrameworkPropertyMetadata(false));

        public static bool GetIsHighlighted(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsHighlightedProperty);
        }

        public static void SetIsHighlighted(DependencyObject obj, bool value)
        {
            obj.SetValue(IsHighlightedProperty, value);
        }
    }
}
