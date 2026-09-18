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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFSCADA.PopUps;

namespace WPFSCADA.Controls
{
    /// <summary>
    /// Interaction logic for HSCompressorControl.xaml
    /// </summary>
    public partial class HSCompressorControl : UserControl
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
        private string _name;
        public string UnitName { get => _name; set { if (_name != value) { _name = value; OnPropertyChanged(nameof(UnitName)); } } }
        private double _runHours;
        public double RunHours { get => _runHours; set { if (_runHours != value) { _runHours = value;  OnPropertyChanged(nameof(RunHours)); } } }
        private double _runMinutes;
        public double RunMinutes { get => _runMinutes; set { if (_runMinutes != value) { _runMinutes = value;  OnPropertyChanged(nameof(RunMinutes)); } } }
        private double _oilPumpHours;
        public double OilPumpHours { get => _oilPumpHours; set { if (_oilPumpHours != value) { _oilPumpHours = value; OnPropertyChanged(nameof(OilPumpHours)); } } }
        private double _oilPumpMinutes;
        public double OilPumpMinutes { get => _oilPumpMinutes; set { if (_oilPumpMinutes != value) { _oilPumpMinutes = value; OnPropertyChanged(nameof(OilPumpMinutes)); } } }
        private double _slideValvePercent;
        public double SlideValvePercent { get => _slideValvePercent; set { if (_slideValvePercent != value) { _slideValvePercent = value; OnPropertyChanged(nameof(SlideValvePercent)); AnimateSlideValve(_slideValvePercent); } } }
        private double _suctionPressure;
        public double SuctionPressure { get => _suctionPressure; set { if (_suctionPressure != value) { _suctionPressure = value; OnPropertyChanged(nameof(SuctionPressure)); } } }
        private double _suctionTemperature;
        public double SuctionTemperature { get => _suctionTemperature; set { if (_suctionTemperature != value) { _suctionTemperature = value; OnPropertyChanged(nameof(SuctionTemperature)); } } }
        private double _dischargePressure;
        public double DischargePressure { get => _dischargePressure;  set { if (_dischargePressure != value)  { _dischargePressure = value; OnPropertyChanged(nameof(DischargePressure)); } } }
        private double _dischargeTemperature;
        public double DischargeTemperature {get => _dischargeTemperature;  set { if (_dischargeTemperature != value) { _dischargeTemperature = value; OnPropertyChanged(nameof(DischargeTemperature)); } } }
        private double _oilPressure;
        public double OilPressure { get => _oilPressure; set { if (_oilPressure != value)  { _oilPressure = value; OnPropertyChanged(nameof(OilPressure)); } } }
        private double _oilTemperature;
        public double OilTemperature { get => _oilTemperature; set { if (_oilTemperature != value) { _oilTemperature = value; OnPropertyChanged(nameof(OilTemperature)); } } }
        private double _oilPressureBP;
        public double OilPressureBP { get => _oilPressureBP; set { if (_oilPressureBP != value)  { _oilPressureBP = value;   OnPropertyChanged(nameof(OilPressureBP)); } } }
        private double _deltaFP;
        public double DeltaFP { get => _deltaFP; set {if (_deltaFP != value) { _deltaFP = value; OnPropertyChanged(nameof(DeltaFP)); } } }
        private double _deltaOP;
        public double DeltaOP { get => _deltaOP; set { if (_deltaOP != value) { _deltaOP = value;   OnPropertyChanged(nameof(DeltaOP)); } }  }
        private double _oilSEPTemperature;
        public double OilSEPTemperature { get => _oilSEPTemperature; set { if (_oilSEPTemperature != value) { _oilSEPTemperature = value; OnPropertyChanged(nameof(OilSEPTemperature)); } }}      
        private double _ccSetPoint;
        public double CCSetPoint {get => _ccSetPoint; set { if (_ccSetPoint != value) { _ccSetPoint = value;  OnPropertyChanged(nameof(CCSetPoint)); } } }
        private double _spAlarm;
        public double SPAlarm { get => _spAlarm; set {if (_spAlarm != value) { _spAlarm = value; OnPropertyChanged(nameof(SPAlarm)); } } }
        private double _spCutout;
        public double SPCutout { get => _spCutout; set { if (_spCutout != value) { _spCutout = value; OnPropertyChanged(nameof(SPCutout)); }}}
        private double _motorCurrent;
        public double MotorCurrent {get => _motorCurrent; set { if (_motorCurrent != value) { _motorCurrent = value; OnPropertyChanged(nameof(MotorCurrent)); } }  }
        private double _motorPower;
        public double MotorPower {get => _motorPower; set { if (_motorPower!= value)  {_motorPower = value; OnPropertyChanged(nameof(MotorPower)); } } }
        private double _motorSpeed;
        public double MotorSpeed { get => _motorSpeed; set { if (_motorSpeed != value) {_motorSpeed = value; OnPropertyChanged(nameof(MotorSpeed)); } } }
        private string _slideValveStatus = "";
        public string SlideValveStatus{get => _slideValveStatus; set { if (_slideValveStatus != value) { _slideValveStatus = value; OnPropertyChanged(nameof(SlideValveStatus)); } } }
        private string _compStartStop = "";
        public string CompStartStop { get => _compStartStop; set { if (_compStartStop != value) { _compStartStop = value; OnPropertyChanged(nameof(CompStartStop)); } } }
        private string _compLocalRemote = "";
        public string CompLocalRemote {get => _compLocalRemote; set{if (_compLocalRemote != value) { _compLocalRemote = value; OnPropertyChanged(nameof(CompLocalRemote)); } } }
        private string _compAutoManual = "";
        public string CompAutoManual { get => _compAutoManual; set {if (_compAutoManual != value)  { _compAutoManual = value; OnPropertyChanged(nameof(CompAutoManual)); } } }
        private bool _topMotor;
        public bool CompressorTopMotor {get => _topMotor; set { if (_topMotor != value)  { _topMotor = value; OnPropertyChanged(nameof(CompressorTopMotor)); }} }
        private bool _bottomMotor;
        public bool CompressorBottomMotor { get => _bottomMotor; set {if (_bottomMotor != value) { _bottomMotor = value; OnPropertyChanged(nameof(CompressorBottomMotor)); } } }
        private bool _isAutoMode = true;
        private bool _isMaster = true;
        public HSCompressorControl()
        {
            InitializeComponent();

            // Make the UserControl itself the binding source
            DataContext = this;
        }
        public bool ShowMasterSlave
        {
            get => (bool)GetValue(ShowMasterSlaveProperty);
            set => SetValue(ShowMasterSlaveProperty, value);
        }

        public static readonly DependencyProperty ShowMasterSlaveProperty = DependencyProperty.Register(nameof(ShowMasterSlave),typeof(bool), typeof(HSCompressorControl), new PropertyMetadata(true));
        private void CompressorImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var dialog = new CompressorDetailsWindow(CompressorName, ShowMasterSlave);
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }
        private void CCSetPointEdit_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PopUps.NumericValueDialog( CompressorName, "CC SET POINT", CCSetPoint, "bar");
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
            {
                double newValue = dialog.SelectedValue;
                // TODO:
                // Write newValue to the CC Set Point PLC tag
            }
        }
        private void SPAlarmEdit_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PopUps.NumericValueDialog(CompressorName, "SP ALARM", SPAlarm, "bar");
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
            {
                double newValue = dialog.SelectedValue;
                // TODO:
                // Write newValue to the spalram PLC tag
            }
        }
        private void SPCutoutEdit_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PopUps.NumericValueDialog(CompressorName, "SP CUTOUT", SPCutout, "bar");
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
            {
                double newValue = dialog.SelectedValue;
                // TODO:
                // Write newValue to the Cutout PLC tag
            }
        }
        private void StartButton1_Click(object sender, RoutedEventArgs e)
        {
            //if (!ShowConfirmation($"Are you sure you want to START the for {CompressorName}?"))
            //    return;

            // Execute START command here
        }
        private void StopButton1_Click(object sender, RoutedEventArgs e)
        {
            //if (!ShowConfirmation("Are you sure you want to STOP the compressor?"))
                //return;

            // Execute STOP command here
        }
        private void ResetButton1_Click(object sender, RoutedEventArgs e)
        {
            //if (!ShowConfirmation("Are you sure you want to RESET the compressor alarm?"))
            //    return;

            // Execute ALARM RESET command here
        }
        private bool ShowConfirmation(string title ,string message)
        {
            var dialog = new PopUps.ConfirmationDialog(title, message);

            dialog.Owner = Window.GetWindow(this);

            bool? result = dialog.ShowDialog();

            return result == true;
        }
        private void AnimateSlideValve(double percent)
        {
            double maxWidth = 100;     // Width of progress area
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 0,
                To = maxWidth * percent / 100,
                Duration = TimeSpan.FromSeconds(1.2),
                EasingFunction = new CubicEase()
            };
            SlideValveFill.BeginAnimation(WidthProperty, animation);
        }
        public string CompressorName
        {
            get { return (string)GetValue(CompressorNameProperty); }
            set { SetValue(CompressorNameProperty, value); }
        }
        public static readonly DependencyProperty CompressorNameProperty = DependencyProperty.Register(nameof(CompressorName),
        typeof(string),
        typeof(HSCompressorControl),
        new PropertyMetadata("COMPRESSOR"));
    }
}