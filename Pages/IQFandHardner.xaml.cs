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
using WPFSCADA.PopUps;

namespace WPFSCADA.Pages
{
    public partial class IQFandHardner : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs(propertyName));
        }
        private bool _topRedValve;
        public bool TopRedValve { get => _topRedValve; set {if (_topRedValve != value) { _topRedValve = value; OnPropertyChanged(nameof(TopRedValve)); } } }
        private bool _topBlueValve;
        public bool TopBlueValve { get => _topBlueValve;  set {if (_topBlueValve != value) { _topBlueValve = value; OnPropertyChanged(nameof(TopBlueValve)); } } }
        private bool _topPurpleValve;
        public bool TopPurpleValve { get => _topPurpleValve;  set{ if (_topPurpleValve != value) { _topPurpleValve = value; OnPropertyChanged(nameof(TopPurpleValve)); } } }
        private bool _topOrangeValve;
        public bool TopOrangeValve { get => _topOrangeValve; set { if (_topOrangeValve != value) { _topOrangeValve = value; OnPropertyChanged(nameof(TopOrangeValve)); } } }
        private bool _bottomRedValve;
        public bool BottomRedValve { get => _bottomRedValve; set { if (_bottomRedValve != value) { _bottomRedValve = value; OnPropertyChanged(nameof(BottomRedValve)); } } }
        private bool _bottomBlueValve;
        public bool BottomBlueValve { get => _bottomBlueValve;  set { if (_bottomBlueValve != value) { _bottomBlueValve = value; OnPropertyChanged(nameof(BottomBlueValve)); } } }
        private bool _bottomPurpleValve;
        public bool BottomPurpleValve { get => _bottomPurpleValve; set { if (_bottomPurpleValve != value) { _bottomPurpleValve = value; OnPropertyChanged(nameof(BottomPurpleValve)); } }  }
        private bool _bottomOrangeValve;
        public bool BottomOrangeValve { get => _bottomOrangeValve;  set { if (_bottomOrangeValve != value) { _bottomOrangeValve = value;  OnPropertyChanged(nameof(BottomOrangeValve)); } } }
        private double _topEvaporatorTemperature;
        public double TopEvaporatorTemperature { get => _topEvaporatorTemperature;  set { if (_topEvaporatorTemperature != value) { _topEvaporatorTemperature = value; OnPropertyChanged(nameof(TopEvaporatorTemperature)); }  } }
        private double _topAirTemperature;
        public double TopAirTemperature { get => _topAirTemperature; set { if (_topAirTemperature != value) { _topAirTemperature = value; OnPropertyChanged(nameof(TopAirTemperature)); } } }
        private double _topSetPoint;
        public double TopSetPoint { get => _topSetPoint; set {if (_topSetPoint != value) { _topSetPoint = value; OnPropertyChanged(nameof(TopSetPoint)); } } }
        private double _topElapsed;
        public double TopElapsed { get => _topElapsed; set { if (_topElapsed != value) { _topElapsed = value;  OnPropertyChanged(nameof(TopElapsed)); } } }
        private double _bottomEvaporatorTemperature;
        public double BottomEvaporatorTemperature { get => _bottomEvaporatorTemperature;  set { if (_bottomEvaporatorTemperature != value)  { _bottomEvaporatorTemperature = value; OnPropertyChanged(nameof(BottomEvaporatorTemperature)); } } }
        private double _bottomAirTemperature;
        public double BottomAirTemperature { get => _bottomAirTemperature; set { if (_bottomAirTemperature != value) {_bottomAirTemperature = value; OnPropertyChanged(nameof(BottomAirTemperature)); } } }
        private double _bottomSetPoint;
        public double BottomSetPoint {get => _bottomSetPoint; set { if (_bottomSetPoint != value) { _bottomSetPoint = value; OnPropertyChanged(nameof(BottomSetPoint)); } } }
        private double _bottomElapsed;
        public double BottomElapsed { get => _bottomElapsed; set { if (_bottomElapsed != value) { _bottomElapsed = value; OnPropertyChanged(nameof(BottomElapsed)); } } }
        public IQFandHardner()
        {
            InitializeComponent();
            DataContext = this;
            InitializeDefaultValues();
            Loaded += (s, e) =>
            {
                // RED GAS
                CreateVerticalGasFlow(VerticalPipeAnimationCanvas, 20, 0, 1050, count: 14, baseDurationSeconds: 3.0);
                // BLUE GAS
                CreateVerticalGasFlow(VerticalPipeAnimationCanvas,70, 0, 1050, count: 14, baseDurationSeconds: 3.0);
                // YELLOW LIQUID
                CreateVerticalLiquidFlow(VerticalPipeAnimationCanvas, 120, 0, 1050);
            };
        }
        private void TopSetPointEdit_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new NumericValueDialog("IQF","SET POINT",TopSetPoint,"min");
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
            {
                double newValue = dialog.SelectedValue;
                // TODO:
                // Write newValue to top Set Point PLC tag
            }
        }
        private void BottomSetPointEdit_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new NumericValueDialog( "HARDNER", "SET POINT",  BottomSetPoint, "min");
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
            {
                double newValue = dialog.SelectedValue;
                // TODO:
                // Write newValue to bottom  Set Point PLC tag
            }
        }
        private void CreateVerticalGasFlow(Canvas canvas,double centerX, double startY, double endY, int count, double baseDurationSeconds)
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
                    Effect = new BlurEffect
                    {
                        Radius = 4
                    },
                    IsHitTestVisible = false
                };

                // White glowing gas puff
                var brush = new RadialGradientBrush();
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(220, 255, 255, 255), 0.0));
                brush.GradientStops.Add( new GradientStop( Color.FromArgb(0, 255, 255, 255), 1.0));
                ellipse.Fill = brush;
                Canvas.SetLeft( ellipse, centerX - w / 2);

                canvas.Children.Add(ellipse);
                var translate =  new TranslateTransform( 0,startY - h);
                var scaleTransform = new ScaleTransform( 1, 1, w / 2, h / 2);
                var group = new TransformGroup();
                group.Children.Add(scaleTransform);
                group.Children.Add(translate);

                ellipse.RenderTransform = group;
                double duration = baseDurationSeconds * (0.75 + rnd.NextDouble() * 0.5);

                // Spread the puffs along the pipe
                double beginDelay = (i / (double)count) * duration;

                var moveAnimation = new DoubleAnimation
                    {
                        From = startY - h,
                        To = endY + h,
                        Duration = TimeSpan.FromSeconds(duration),
                        RepeatBehavior =RepeatBehavior.Forever,
                        BeginTime =TimeSpan.FromSeconds(beginDelay)
                    };

                var opacityAnimation = new DoubleAnimationUsingKeyFrames
                    {
                        Duration = TimeSpan.FromSeconds(duration),
                        RepeatBehavior = RepeatBehavior.Forever,
                        BeginTime = TimeSpan.FromSeconds(beginDelay)
                    };

                opacityAnimation.KeyFrames.Add( new LinearDoubleKeyFrame( 0, KeyTime.FromPercent(0)));
                opacityAnimation.KeyFrames.Add( new LinearDoubleKeyFrame( 0.85, KeyTime.FromPercent(0.06)));
                opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame( 0.85, KeyTime.FromPercent(0.92)));
                opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(0,KeyTime.FromPercent(1)));

                var scaleAnimation = new DoubleAnimation
                    {
                        From = 0.85,
                        To = 1.25,
                        Duration = TimeSpan.FromSeconds(1.2 + rnd.NextDouble()),
                        AutoReverse = true,
                        RepeatBehavior =  RepeatBehavior.Forever
                    };

                translate.BeginAnimation(TranslateTransform.YProperty, moveAnimation);
                ellipse.BeginAnimation(Ellipse.OpacityProperty, opacityAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty,scaleAnimation);
            }
        }
        private void CreateVerticalLiquidFlow(Canvas canvas, double x,double startY, double endY)
        {
            var liquidFlow = new Path
            {
                Data = new LineGeometry(new Point(x, startY), new Point(x, endY)),
                // Moving liquid = yellow
                //Stroke = new SolidColorBrush( Color.FromRgb(255, 213, 79)),
                Stroke = new SolidColorBrush(Color.FromRgb(255, 248, 225)),
                
                StrokeThickness = 4,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap =PenLineCap.Round,
                StrokeDashArray = new DoubleCollection { 10, 2 },
                IsHitTestVisible = false,
                Effect = new DropShadowEffect
                    {
                        Color = Color.FromRgb(255, 213,79),
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
            liquidFlow.BeginAnimation( Shape.StrokeDashOffsetProperty, dashAnimation);
        }
        private void InitializeDefaultValues()
        {
            TopEvaporatorTemperature = 10;
            TopAirTemperature= 20;
            TopSetPoint = 15;
            TopElapsed = 30;
            BottomEvaporatorTemperature = 15;
            BottomAirTemperature = 20;
            BottomSetPoint = 10;
            BottomElapsed = 15;
        }
    }
}