using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WPFSCADA.Helpers
{
    public static class TagProperties
    {
        public static readonly DependencyProperty ConfigKeyProperty =
            DependencyProperty.RegisterAttached(
                "ConfigKey",
                typeof(string),
                typeof(TagProperties),
                new PropertyMetadata(null));

        public static void SetConfigKey(
            DependencyObject element,
            string value)
        {
            element.SetValue(ConfigKeyProperty, value);
        }

        public static string GetConfigKey(
            DependencyObject element)
        {
            return (string)element.GetValue(ConfigKeyProperty);
        }
    }
}
