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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFSCADA.Controls
{
    /// <summary>
    /// Interaction logic for ProcessChillRoom.xaml
    /// </summary>
    public partial class ProcessChillRoomControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private double _setPoint;
        public double SetPoint {get => _setPoint; set { if (_setPoint != value) {_setPoint = value; OnPropertyChanged(nameof(SetPoint)); } } }
        private double _evpTemp;
        public double EvpTemp { get => _evpTemp; set { if (_evpTemp != value) { _evpTemp = value; OnPropertyChanged(nameof(EvpTemp)); } } }
        private double _chillRoomTemp;
        public double ChillRoomTemp { get => _chillRoomTemp; set {if (_chillRoomTemp != value) { _chillRoomTemp = value; OnPropertyChanged(nameof(ChillRoomTemp)); } } }
        private bool _fan1RunFB;
        public bool Fan1RunFB { get => _fan1RunFB; set { if (_fan1RunFB != value)  { _fan1RunFB = value; OnPropertyChanged(nameof(Fan1RunFB)); }} }
        private bool _fan2RunFB;
        public bool Fan2RunFB { get => _fan2RunFB; set { if (_fan2RunFB != value) { _fan2RunFB = value;  OnPropertyChanged(nameof(Fan2RunFB)); } } }
        private bool _ssvFb;
        public bool SsvFb {  get => _ssvFb; set { if (_ssvFb != value) { _ssvFb = value; OnPropertyChanged(nameof(SsvFb)); } } }
        private bool _lsvFb;
        public bool LsvFb { get => _lsvFb; set {if (_lsvFb != value) { _lsvFb = value; OnPropertyChanged(nameof(LsvFb)); }} }
        private int _freezeHrs;
        public int FreezeHrs {get => _freezeHrs; set { if (_freezeHrs != value) { _freezeHrs = value; OnPropertyChanged(nameof(FreezeHrs)); } }}
        private int _freezeMins;
        public int FreezeMins{ get => _freezeMins; set {if (_freezeMins != value) { _freezeMins = value; OnPropertyChanged(nameof(FreezeMins));  } } }
        private int _defrostHrsSetpoint;
        public int DefrostHrsSetpoint { get => _defrostHrsSetpoint; set { if (_defrostHrsSetpoint != value) { _defrostHrsSetpoint = value; OnPropertyChanged(nameof(DefrostHrsSetpoint));  } } }
        private int _defrostMinsSetpoint;
        public int DefrostMinsSetpoint{ get => _defrostMinsSetpoint; set  { if (_defrostMinsSetpoint != value) { _defrostMinsSetpoint = value; OnPropertyChanged(nameof(DefrostMinsSetpoint)); } }}
        private int _defrostHrs;
        public int DefrostHrs {get => _defrostHrs; set { if (_defrostHrs != value) { _defrostHrs = value; OnPropertyChanged(nameof(DefrostHrs)); } } }
        private int _defrostMins;
        public int DefrostMins { get => _defrostMins; set { if (_defrostMins != value) {_defrostMins = value; OnPropertyChanged(nameof(DefrostMins));}} }
        public ProcessChillRoomControl()
        {
            InitializeComponent();
            //UpdateControlsForRemoteLocal(RemoteLocalToggle?.IsChecked == true);
            // attach handlers (in case Toggle events aren't wired in XAML)
            DataContext = this;
            RemoteLocalToggle.Checked += RemoteLocalToggle_Checked;
            RemoteLocalToggle.Unchecked += RemoteLocalToggle_Unchecked;
        }
        private async Task ReadProcessChillRoomDataAsync()
        {
            // TODO:
            // Read actual PLC tags when tag mapping is finalized.

            // Example:
            // SetPoint = await ReadTag<double>(_setPointTag);
            // EvpTemp = await ReadTag<double>(_evpTempTag);
            // ChillRoomTemp = await ReadTag<double>(_chillRoomTempTag);
            // Fan1RunFB = await ReadTag<bool>(_fan1RunFbTag);
            // Fan2RunFB = await ReadTag<bool>(_fan2RunFbTag);
            // SsvFb = await ReadTag<bool>(_ssvFbTag);
            // LsvFb = await ReadTag<bool>(_lsvFbTag);
        }
        private async Task WriteProcessChillRoomCommandAsync()
        {
            // TODO:
            // Implement PLC writes after command tags are finalized.
        }
        private void RemoteLocalToggle_Checked(object sender, RoutedEventArgs e)
        {
            // Checked == LOCAL
            //UpdateControlsForRemoteLocal(true);
        }

        private void RemoteLocalToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            // Unchecked == REMOTE

        }
    }
}
