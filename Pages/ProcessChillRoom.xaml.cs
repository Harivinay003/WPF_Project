using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFSCADA.Pages
{
    /// <summary>
    /// Interaction logic for ProcessChillRoom.xaml
    /// </summary>
    public partial class ProcessChillRoom : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ProcessChillRoom()
        {
            InitializeComponent();
            DataContext = this;
            CreateHorizontalGasFlow(PipeAnimationCanvas, 0, 1800, 50, Colors.White, 14, 3.0);
            CreateHorizontalLiquidFlow(PipeAnimationCanvas, 0, 1800, 100);
            InitializeDefaultValues();
        }
        private void CreateHorizontalGasFlow(Canvas canvas, double startX, double endX, double centerY, Color coreColor, int count, double baseDurationSeconds)
        {
            var rnd = new Random();
            for (int i = 0; i < count; i++)
            {
                double w = 10 + rnd.NextDouble() * 10;
                double h = w * (0.5 + rnd.NextDouble() * 0.25);

                var ellipse = new Ellipse
                {
                    Width = w,
                    Height = h,
                    Opacity = 0,
                    IsHitTestVisible = false,
                    Effect = new BlurEffect
                    {
                        Radius = 4
                    }
                };

                var brush = new RadialGradientBrush();

                brush.GradientStops.Add(new GradientStop(Color.FromArgb(220, coreColor.R, coreColor.G, coreColor.B), 0.0));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, coreColor.R, coreColor.G, coreColor.B), 1.0));
                ellipse.Fill = brush;
                Canvas.SetTop(ellipse, centerY - h / 2);
                canvas.Children.Add(ellipse);
                var translate = new TranslateTransform(startX - w, 0);
                var scaleTransform = new ScaleTransform(1, 1, w / 2, h / 2);
                var group = new TransformGroup();

                group.Children.Add(scaleTransform);
                group.Children.Add(translate);
                ellipse.RenderTransform = group;
                double duration = baseDurationSeconds * (0.75 + rnd.NextDouble() * 0.5);
                // Spread the gas puffs
                double beginDelay = (i / (double)count) * duration;
                var moveAnimation = new DoubleAnimation
                {
                    From = startX - w,
                    To = endX + w,
                    Duration = TimeSpan.FromSeconds(duration),
                    RepeatBehavior = RepeatBehavior.Forever,
                    BeginTime = TimeSpan.FromSeconds(beginDelay)
                };
                var opacityAnimation = new DoubleAnimationUsingKeyFrames
                {
                    Duration = TimeSpan.FromSeconds(duration),
                    RepeatBehavior = RepeatBehavior.Forever,
                    BeginTime = TimeSpan.FromSeconds(beginDelay)
                };
                opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(0)));
                opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(0.85, KeyTime.FromPercent(0.06)));
                opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(0.85, KeyTime.FromPercent(0.92)));
                opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(1)));

                var scaleAnimation = new DoubleAnimation
                {
                    From = 0.85,
                    To = 1.25,
                    Duration = TimeSpan.FromSeconds(1.2 + rnd.NextDouble()),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };

                translate.BeginAnimation(TranslateTransform.XProperty, moveAnimation);
                ellipse.BeginAnimation(Ellipse.OpacityProperty, opacityAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            }
        }
        private void CreateHorizontalLiquidFlow(Canvas canvas, double startX, double endX, double centerY)
        {
            var liquidFlow = new Path
            {
                Data = new LineGeometry(new Point(startX, centerY), new Point(endX, centerY)),
                // Bright moving highlight
                Stroke = new SolidColorBrush(Color.FromRgb(255, 248, 225)),
                StrokeThickness = 4,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeDashArray = new DoubleCollection { 10, 2 },
                IsHitTestVisible = false,
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(255, 213, 79),
                    BlurRadius = 6,
                    ShadowDepth = 0,
                    Opacity = 0.8
                }
            };
            canvas.Children.Add(liquidFlow);
            var dashAnimation = new DoubleAnimation
            {
                From = 0,
                To = -24,
                Duration = TimeSpan.FromSeconds(1.8),
                RepeatBehavior = RepeatBehavior.Forever
            };
            liquidFlow.BeginAnimation(Shape.StrokeDashOffsetProperty, dashAnimation);
        }
        private void InitializeDefaultValues()
        {
            if (ProcessChillRoomUnitLeft != null)
            {
                ProcessChillRoomUnitLeft.SetPoint = 10;
                ProcessChillRoomUnitLeft.EvpTemp = 20;
                ProcessChillRoomUnitLeft.ChillRoomTemp = 15;
            }
            if (ProcessChillRoomUnitRight != null)
            {
                ProcessChillRoomUnitRight.SetPoint = 10;
                ProcessChillRoomUnitRight.EvpTemp = 20;
                ProcessChillRoomUnitRight.ChillRoomTemp = 15;
            }
        }
    }
}
