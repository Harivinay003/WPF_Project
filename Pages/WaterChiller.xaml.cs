using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace WPFSCADA.Pages
{
    public partial class WaterChiller : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs(propertyName));
        }
        private bool _leftTopValve = false;
        public bool LeftTopValve { get => _leftTopValve; set {if (_leftTopValve != value)  { _leftTopValve = value; OnPropertyChanged(nameof(LeftTopValve)); } } }
        private bool _leftBottomValve = true;
        public bool LeftBottomValve {get => _leftBottomValve; set{ if (_leftBottomValve != value) { _leftBottomValve = value; OnPropertyChanged(nameof(LeftBottomValve)); } }}
        private bool _rightTopValve = false;
        public bool RightTopValve {get => _rightTopValve; set { if (_rightTopValve != value) { _rightTopValve = value; OnPropertyChanged(nameof(RightTopValve));  } } }
        private bool _rightBottomValve = true;
        public bool RightBottomValve { get => _rightBottomValve; set { if (_rightBottomValve != value)  { _rightBottomValve = value; OnPropertyChanged(nameof(RightBottomValve)); } } }
        private double _leftChillWaterTemperature;
        public double LeftChillWaterTemperature { get => _leftChillWaterTemperature; set { if (_leftChillWaterTemperature != value) { _leftChillWaterTemperature = value; OnPropertyChanged(nameof(LeftChillWaterTemperature)); } }  }
        private double _leftEvaporatorTemperature;
        public double LeftEvaporatorTemperature { get => _leftEvaporatorTemperature; set {if (_leftEvaporatorTemperature != value) { _leftEvaporatorTemperature = value;  OnPropertyChanged(nameof(LeftEvaporatorTemperature)); } } }
        private double _rightChillWaterTemperature;
        public double RightChillWaterTemperature{get => _rightChillWaterTemperature; set {if (_rightChillWaterTemperature != value) { _rightChillWaterTemperature = value; OnPropertyChanged(nameof(RightChillWaterTemperature)); } } }
        private double _rightEvaporatorTemperature;
        public double RightEvaporatorTemperature { get => _rightEvaporatorTemperature; set {if (_rightEvaporatorTemperature != value)  {  _rightEvaporatorTemperature = value; OnPropertyChanged(nameof(RightEvaporatorTemperature)); } } }
        private double _leftFlowMeter;
        public double LeftFlowMeter { get => _leftFlowMeter; set {if (_leftFlowMeter != value) { _leftFlowMeter = value; OnPropertyChanged(nameof(LeftFlowMeter)); } } }
        private double _rightFlowMeter;
        public double RightFlowMeter { get => _rightFlowMeter; set { if (_rightFlowMeter != value) {_rightFlowMeter = value; OnPropertyChanged(nameof(RightFlowMeter)); } }}
        private double _leftCirPump;
        public double LeftCirPump {get => _leftCirPump; set { if (_leftCirPump != value)  { _leftCirPump = value; OnPropertyChanged(nameof(LeftCirPump)); } } }
        private double _leftDisPump;
        public double LeftDisPump {get => _leftDisPump; set {if (_leftDisPump != value) { _leftDisPump = value; OnPropertyChanged(nameof(LeftDisPump));} } }
        private double _rightCirPump;
        public double RightCirPump { get => _rightCirPump;set{ if (_rightCirPump != value) { _rightCirPump = value; OnPropertyChanged(nameof(RightCirPump)); } } }
        private double _rightDisPump;
        public double RightDisPump{ get => _rightDisPump; set {if (_rightDisPump != value){_rightDisPump = value;  OnPropertyChanged(nameof(RightDisPump)); } }}
        private bool _leftCirPumpRunning;
        public bool LeftCirPumpRunning { get => _leftCirPumpRunning; set { if (_leftCirPumpRunning != value) { _leftCirPumpRunning = value;OnPropertyChanged(nameof(LeftCirPumpRunning)); } } }
        private bool _leftDisPumpRunning;
        public bool LeftDisPumpRunning { get => _leftDisPumpRunning; set { if (_leftDisPumpRunning != value) { _leftDisPumpRunning = value; OnPropertyChanged(nameof(LeftDisPumpRunning)); } } }
        private bool _rightCirPumpRunning;
        public bool RightCirPumpRunning{ get => _rightCirPumpRunning; set { if (_rightCirPumpRunning != value) { _rightCirPumpRunning = value;  OnPropertyChanged(nameof(RightCirPumpRunning)); } } }
        private bool _rightDisPumpRunning;
        public bool RightDisPumpRunning{ get => _rightDisPumpRunning; set{ if (_rightDisPumpRunning != value) {_rightDisPumpRunning = value;  OnPropertyChanged(nameof(RightDisPumpRunning)); } } }
        public WaterChiller()
        {
            InitializeComponent();
            DataContext = this;

            CreateHorizontalGasFlow(PipeAnimationCanvas,0, 1800, 50, Colors.White, 14, 3.0);
            CreateHorizontalLiquidFlow( PipeAnimationCanvas, 0, 1800, 120);

            InitializeDefaultValues();

            RemoteLocalToggle1.Checked += RemoteLocalToggle_Checked;
            RemoteLocalToggle1.Unchecked += RemoteLocalToggle_Unchecked;
        }
        private void CreateHorizontalGasFlow( Canvas canvas, double startX,double endX,double centerY,Color coreColor,int count, double baseDurationSeconds)
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

                brush.GradientStops.Add(new GradientStop(Color.FromArgb( 220, coreColor.R, coreColor.G, coreColor.B), 0.0));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb( 0, coreColor.R, coreColor.G, coreColor.B), 1.0));
                ellipse.Fill = brush;
                Canvas.SetTop(ellipse, centerY - h / 2);
                canvas.Children.Add(ellipse);
                var translate = new TranslateTransform( startX - w, 0);
                var scaleTransform = new ScaleTransform( 1, 1,  w / 2, h / 2);
                var group =  new TransformGroup();

                group.Children.Add(scaleTransform);
                group.Children.Add(translate);
                ellipse.RenderTransform = group;
                double duration = baseDurationSeconds *(0.75 + rnd.NextDouble() * 0.5);
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
                opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame( 0, KeyTime.FromPercent(0)));
                opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame( 0.85,KeyTime.FromPercent(0.06)));
                opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame( 0.85, KeyTime.FromPercent(0.92)));
                opacityAnimation.KeyFrames.Add( new LinearDoubleKeyFrame( 0, KeyTime.FromPercent(1)));

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
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty,scaleAnimation);
                scaleTransform.BeginAnimation( ScaleTransform.ScaleYProperty,scaleAnimation);
            }
        }
        private void CreateHorizontalLiquidFlow(Canvas canvas, double startX, double endX, double centerY)
        {
            var liquidFlow = new Path
            {
                Data = new LineGeometry(new Point(startX, centerY),new Point(endX, centerY)),
                // Bright moving highlight
                Stroke = new SolidColorBrush(Color.FromRgb(255, 248, 225)),
                StrokeThickness = 4,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeDashArray = new DoubleCollection { 10,2 },
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
            var dashAnimation =new DoubleAnimation
                {
                    From = 0,
                    To = -24,
                    Duration =TimeSpan.FromSeconds(1.8),
                    RepeatBehavior = RepeatBehavior.Forever
                };
            liquidFlow.BeginAnimation( Shape.StrokeDashOffsetProperty,dashAnimation);
        }
        private void InitializeDefaultValues()
        {
            RightChillWaterTemperature = 10;
            RightEvaporatorTemperature = 20;

            LeftChillWaterTemperature = 15;
            LeftEvaporatorTemperature = 30;
            LeftFlowMeter = 15;
            RightFlowMeter = 20;
            LeftCirPump = 10;
            LeftDisPump = 15;
            RightCirPump = 20;
            RightDisPump = 5;
            LeftCirPumpRunning = true;
            LeftDisPumpRunning = false;
            RightCirPumpRunning = true;
            RightDisPumpRunning = false;
            LeftTopValve = true;
            LeftBottomValve = true;
            RightTopValve = false;
            RightBottomValve = true;
        }
        private void UpdateControlsForRemoteLocal(bool isLocal)
        {
        }
        private void RemoteLocalToggle_Checked(
            object sender,
            RoutedEventArgs e)
        {
            UpdateControlsForRemoteLocal(true);
        }

        private void RemoteLocalToggle_Unchecked(
            object sender,
            RoutedEventArgs e)
        {
            UpdateControlsForRemoteLocal(false);
        }
    }
}