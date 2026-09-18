

using FluentModbus;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using VirtualEMS.DataServices;
using VirtualEMS.Library;
using WPFSCADA.Controls;

namespace WPFSCADA.PopUps
{
    public partial class ColdStoreDetailsWindow : Window
    {
        private Dictionary<int, double> TagUpdateActions = new Dictionary<int, double>();
        private DispatcherTimer _liveValuesTimer;
        private readonly SemaphoreSlim _plcLock = new(1, 1);
        private IODevice _plcDevice;
        private readonly Dictionary<int, Tag> _popupTags =  new Dictionary<int, Tag>();
        private bool _configurationLoaded = false;
        private bool _isUpdatingUI = false;
        private bool _isWriting = false;
        private const int LocalFbTagId = 1;
        private const int RemoteFbTagId = 2;
        private const int HeaterFbTagId = 3;
        private const int Fan1TripTagId = 4;
        private const int Fan1RunTagId = 5;
        private const int Fan2TripTagId = 6;
        private const int Fan2RunTagId = 7;
        private const int Fan3TripTagId = 8;
        private const int Fan3RunTagId = 9;
        private const int Fan4TripTagId = 10;
        private const int Fan4RunTagId = 11;
        private const int PowerFbTagId = 12;
        private const int AutoManualTagId = 13;
        private const int FansStartTagId = 14;
        private const int FansStopTagId = 15;
        private const int LsvOpenTagId = 16;
        private const int LsvCloseTagId = 17;
        private const int SsvOpenTagId = 18;
        private const int SsvCloseTagId = 19;
        private const int HgsvOpenTagId = 20;
        private const int HgsvCloseTagId = 21;
        private const int HgrsvOpenTagId = 22;
        private const int HgrsvCloseTagId = 23;
        private const int ManualDefrostTagId = 24;
        private const int Cycle1BypassTagId = 25;
        private const int Cycle2BypassTagId = 26;
        private const int Cycle3BypassTagId = 27;
        private const int FreezingModeTagId = 28;
        private const int DefrostingModeTagId = 29;
        private const int LsvOpenCommandTagId = 30;
        private const int SsvOpenCommandTagId = 31;
        private const int HgsvOpenCommandTagId = 32;
        private const int HgrsvOpenCommandTagId = 33;
        private const int FansOnCommandTagId = 34;
        private const int LiquidDrainOutTagId = 35;
        private const int CoolingStartTagId = 36;
        private const int CycleReachedTagId = 37;
        private const int RoomTemperatureTagId = 38;
        private const int EvaporatorTemperatureTagId = 39;
        private const int TemperatureSetPointTagId = 40;
        private const int LiquidDrainSetPointTagId = 41;
        private const int DefrostSetPointTagId = 42;
        private const int FreezingElapsedHoursTagId = 43;
        private const int FreezingElapsedMinutesTagId = 44;
        private const int LiquidDrainElapsedTagId = 45;
        private const int DefrostElapsedTagId = 46;
        private const int RunHoursTagId = 47;
        private const int Cycle1HoursTagId = 48;
        private const int Cycle1MinutesTagId = 49;
        private const int Cycle2HoursTagId = 50;
        private const int Cycle2MinutesTagId = 51;
        private const int Cycle3HoursTagId = 52;
        private const int Cycle3MinutesTagId = 53;
        private readonly string _unitName;

        public ColdStoreDetailsWindow(string unitName)
        {
            InitializeComponent();
            _unitName =  string.IsNullOrWhiteSpace(unitName) ? "COLD STORE" : unitName;
            UnitNameText.Text = _unitName;

            if (!LoadPLCConfiguration())
            {
                MessageBox.Show("Unable to load PLC configuration or tags.","Configuration Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                ShowTab(1);
                StartLivePolling();
            }
        }
        // OPTIONAL COMPATIBILITY CONSTRUCTOR
        //
        // This prevents ColdStore1 from breaking immediately.
        //
        // IMPORTANT:
        // We only take UnitName from ColdStoreUnit.
        // We DO NOT subscribe to PropertyChanged.
        // We DO NOT use its live values.
        public ColdStoreDetailsWindow(ColdStoreUnit coldStoreUnit) : this(string.IsNullOrWhiteSpace(coldStoreUnit?.UnitName) ? "COLD STORE" : coldStoreUnit.UnitName)
        {
        }
        private void StartLivePolling()
        {
            _liveValuesTimer = new DispatcherTimer();
            _liveValuesTimer.Interval = TimeSpan.FromMilliseconds(500);
            _liveValuesTimer.Tick += LiveValuesTimer_Tick;
            _liveValuesTimer.Start();
        }
        private async void LiveValuesTimer_Tick(object sender, EventArgs e)
        {
            if (_isWriting)
                return;
            await GetLiveValuesFromPLC();
        }
        private async Task GetLiveValuesFromPLC()
        {
            if (!_configurationLoaded || _plcDevice == null ||  _popupTags.Count == 0)
            {
                return;
            }
            await _plcLock.WaitAsync();
            try
            {
                await ReadPopupTagsFromPLCAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PLC read error: {ex}");
            }
            finally
            {
                _plcLock.Release();
            }
        }
        private async Task ReadPopupTagsFromPLCAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    var tags =  _popupTags.Values.ToList();

                    if (tags.Count == 0)
                        return;

                    var plc = _plcDevice;
                    if (plc == null ||  string.IsNullOrWhiteSpace(plc.IpAddress))
                    {
                        return;
                    }
                    using var client = new ModbusTcpClient();
                    client.Connect(new IPEndPoint(IPAddress.Parse(plc.IpAddress), 502));
                    if (!client.IsConnected)
                        return;

                    const int unitIdentifier = 1;
                    // BUILD REGISTER BLOCKS
                    foreach (var block in BuildRegisterBlocks(tags))
                    {
                        byte[] readBytes;
                        try
                        {
                            readBytes = client.ReadHoldingRegistersAsync<byte>(unitIdentifier, block.StartAddress, block.RegisterCount * 2)
                                    .GetAwaiter()
                                    .GetResult()
                                    .ToArray();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"PLC block read error: {ex.Message}");
                            continue;
                        }
                        foreach (var tag in block.Tags)
                        {
                            int buffer = (tag.Address - block.StartAddress) * 2;
                            try
                            {
                                if (tag.Type == DataType.WBOOL)
                                {
                                    var bitArray = new BitArray(new byte[]
                                    {
                                        readBytes[buffer + 1],
                                        readBytes[buffer]
                                    });
                                    int value =  Convert.ToInt32(bitArray[tag.Bit]);
                                    TagUpdateActions[tag.Id] = value;
                                    continue;
                                }

                                double numericValue = DecodeValue(readBytes, buffer, tag.Type);
                                TagUpdateActions[tag.Id] = numericValue;
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Tag {tag.Id} read error: {ex.Message}");
                            }
                        }
                    }
                    // UPDATE UI ON UI THREAD
                    Dispatcher.Invoke(UpdatePopupFromPLC);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ReadPopupTagsFromPLCAsync error: {ex}");
                }
            });
        }    
        private void UpdatePopupFromPLC()
        {
            if (_isUpdatingUI)
                return;
            try
            {
                _isUpdatingUI = true;
                bool isRemote = GetBool(RemoteFbTagId);
                bool isLocal = GetBool(LocalFbTagId);
                if (isRemote)
                {
                    ControlModeText.Text = "REMOTE";
                    ControlModeText.Foreground = new SolidColorBrush(Color.FromRgb(22, 156, 74));
                }
                else if (isLocal)
                {
                    ControlModeText.Text = "LOCAL";
                    ControlModeText.Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128));
                }
                else
                {
                    ControlModeText.Text = "--";
                }
                bool isAuto = GetBool(AutoManualTagId);
                UpdateOperatingModeButtons(isAuto);
                bool power = GetBool(PowerFbTagId);

                //PowerFeedbackText.Text =  power ? "ON" : "OFF";
                //PowerFeedbackText.Foreground = power
                //        ? new SolidColorBrush(
                //            Color.FromRgb(
                //                22,
                //                156,
                //                74))
                //        : new SolidColorBrush(
                //            Color.FromRgb(
                //                214,
                //                40,
                //                40));
                //PowerFeedbackIndicator.Fill =
                //    power
                //        ? new SolidColorBrush(
                //            Color.FromRgb(
                //                22,
                //                156,
                //                74))
                //        : new SolidColorBrush(
                //            Color.FromRgb(
                //                214,
                //                40,
                //                40));

                bool manualDefrost =GetBool(ManualDefrostTagId);
                DefrostModeCheckBox.IsChecked = manualDefrost;
                UpdateDefrostMode(manualDefrost);

                UpdateSingleFanStatus(GetBool(Fan1RunTagId), GetBool(Fan1TripTagId), Fan1StatusBox, Fan1StatusText);
                UpdateSingleFanStatus(GetBool(Fan2RunTagId), GetBool(Fan2TripTagId), Fan2StatusBox, Fan2StatusText);
                UpdateSingleFanStatus(GetBool(Fan3RunTagId), GetBool(Fan3TripTagId), Fan3StatusBox, Fan3StatusText);
                UpdateSingleFanStatus(GetBool(Fan4RunTagId), GetBool(Fan4TripTagId), Fan4StatusBox, Fan4StatusText);

                UpdateValveStatus(GetBool(LsvOpenTagId), GetBool(LsvCloseTagId), LiquidValveOpenButton, LiquidValveCloseButton);
                UpdateValveStatus(GetBool(SsvOpenTagId), GetBool(SsvCloseTagId), SuctionValveOpenButton, SuctionValveCloseButton);
                UpdateValveStatus(GetBool(HgsvOpenTagId), GetBool(HgsvCloseTagId), HotGasSolenoidOpenButton, HotGasSolenoidCloseButton);
                UpdateValveStatus(GetBool(HgrsvOpenTagId), GetBool(HgrsvCloseTagId), HotGasReturnOpenButton, HotGasReturnCloseButton);

                bool heater = GetBool(HeaterFbTagId);
                HeaterStatusText.Text = heater ? "ON": "OFF";
                bool cycle1Bypass = GetBool(Cycle1BypassTagId);
                bool cycle2Bypass = GetBool(Cycle2BypassTagId);
                bool cycle3Bypass = GetBool(Cycle3BypassTagId);
                Cycle1BypassCheckBox.IsChecked = cycle1Bypass;
                Cycle2BypassCheckBox.IsChecked = cycle2Bypass;
                Cycle3BypassCheckBox.IsChecked = cycle3Bypass;
                Cycle1TimeButton.IsEnabled = !cycle1Bypass;
                Cycle2TimeButton.IsEnabled = !cycle2Bypass;
                Cycle3TimeButton.IsEnabled = !cycle3Bypass;

                SetCycleTime(Cycle1TimeButton, GetInt(Cycle1HoursTagId), GetInt(Cycle1MinutesTagId));
                SetCycleTime(Cycle2TimeButton, GetInt(Cycle2HoursTagId), GetInt(Cycle2MinutesTagId));
                SetCycleTime(Cycle3TimeButton, GetInt(Cycle3HoursTagId), GetInt(Cycle3MinutesTagId));
                LiquidDrainSetPointButton.Content = FormatMinutes(GetInt(LiquidDrainSetPointTagId));
                DefrostingSetPointButton.Content = FormatMinutes(GetInt(DefrostSetPointTagId));
                FreezingElapsedText.Text = $"{GetInt(FreezingElapsedHoursTagId):00} : " +  $"{GetInt(FreezingElapsedMinutesTagId):00}";
                LiquidDrainElapsedText.Text = FormatMinutes( GetInt(LiquidDrainElapsedTagId));
                DefrostingElapsedText.Text = FormatMinutes(GetInt(DefrostElapsedTagId));
                // MAINTENANCE
                //RunHoursText.Text = GetInt(RunHoursTagId).ToString();
                UpdateControlAvailability(isRemote, isAuto);
            }
            finally
            {
                _isUpdatingUI = false;
            }
        }
        private void UpdateOperatingModeButtons(bool isAuto)
        {
            if (isAuto)
            {
                AutoModeButton.Background = new SolidColorBrush(Color.FromRgb(30, 115, 232));
                AutoModeButton.Foreground = Brushes.White;
                ManualModeButton.Background = Brushes.Transparent;
                ManualModeButton.Foreground = new SolidColorBrush(Color.FromRgb( 75, 85, 99));
            }
            else
            {
                ManualModeButton.Background = new SolidColorBrush(Color.FromRgb(30, 115, 232));
                ManualModeButton.Foreground = Brushes.White;
                AutoModeButton.Background = Brushes.Transparent;
                AutoModeButton.Foreground = new SolidColorBrush(Color.FromRgb( 75, 85,99));
            }
        }
        private void UpdateControlAvailability(bool isRemote, bool isAuto)
        {
            // Operating mode
            AutoModeButton.IsEnabled = isRemote;
            ManualModeButton.IsEnabled = isRemote;
            // Manual-only operations
            bool manualAvailable = isRemote && !isAuto;
            FansOnButton.IsEnabled = manualAvailable;
            FansOffButton.IsEnabled = manualAvailable;
            DefrostModeCheckBox.IsEnabled = manualAvailable;
            // Valves
            LiquidValveOpenButton.IsEnabled = isRemote;
            LiquidValveCloseButton.IsEnabled = isRemote;
            SuctionValveOpenButton.IsEnabled = isRemote;
            SuctionValveCloseButton.IsEnabled = isRemote;
            HotGasReturnOpenButton.IsEnabled = isRemote;
            HotGasReturnCloseButton.IsEnabled = isRemote;
            HotGasSolenoidOpenButton.IsEnabled = isRemote;
            HotGasSolenoidCloseButton.IsEnabled = isRemote;
        }
        private void UpdateDefrostMode(bool active)
        {
            if (active)
            {
                DefrostModeText.Text ="MANUAL";
                DefrostModeText.Foreground = new SolidColorBrush(Color.FromRgb(22, 156, 74));
            }
            else
            {
                DefrostModeText.Text = "MANUAL";
                DefrostModeText.Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128));
            }
        }
        private void UpdateSingleFanStatus(bool isRunning,bool isTripped, Border statusBox, TextBlock statusText)
        {
            if (isTripped)
            {
                statusBox.Background = new SolidColorBrush(Color.FromRgb(255,235, 235));
                statusText.Text = "TRIP";
                statusText.Foreground = new SolidColorBrush(Color.FromRgb( 214, 40, 40));
                return;
            }
            if (isRunning)
            {
                statusBox.Background = new SolidColorBrush( Color.FromRgb(232, 247, 238));
                statusText.Text = "ON";
                statusText.Foreground = new SolidColorBrush(Color.FromRgb(22, 156, 74));
            }
            else
            {
                statusBox.Background = new SolidColorBrush(Color.FromRgb(255, 235, 235));
                statusText.Text = "OFF";
                statusText.Foreground = new SolidColorBrush(Color.FromRgb(214, 40, 40));
            }
        }
        private void UpdateValveStatus(bool isOpen, bool isClosed, Button openButton, Button closeButton)
        {
            var activeOpenBrush = new SolidColorBrush( Color.FromRgb(22, 156, 74));
            var activeCloseBrush = new SolidColorBrush( Color.FromRgb(214, 40, 40));
            var inactiveBrush = new SolidColorBrush(Color.FromRgb(243, 244, 246));
            var inactiveForeground = new SolidColorBrush(Color.FromRgb(75, 85, 99));
            if (isOpen)
            {
                openButton.Background = activeOpenBrush;
                openButton.Foreground = Brushes.White;
                closeButton.Background = inactiveBrush;
                closeButton.Foreground = inactiveForeground;
            }
            else if (isClosed)
            {
                openButton.Background = inactiveBrush;
                openButton.Foreground = inactiveForeground;
                closeButton.Background = activeCloseBrush;
                closeButton.Foreground = Brushes.White;
            }
            else
            {
                openButton.Background = inactiveBrush;
                openButton.Foreground = inactiveForeground;
                closeButton.Background = inactiveBrush;
                closeButton.Foreground = inactiveForeground;
            }
        }
        private void SetCycleTime(Button button, int hours, int minutes)
        {
            button.Content = $"{hours:00} : {minutes:00}";
        }
        private string FormatMinutes(int totalMinutes)
        {
            if (totalMinutes < 0)
                totalMinutes = 0;
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;
            return $"{hours:00} : {minutes:00}";
        }
        private bool GetBool(int tagId)
        {
            return TagUpdateActions.ContainsKey(tagId) && TagUpdateActions[tagId] != 0;
        }
        private int GetInt(int tagId)
        {
            if (!TagUpdateActions.ContainsKey(tagId))
                return 0;
            return Convert.ToInt32(TagUpdateActions[tagId]);
        }
        private async void AutoModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingUI)
                return;
            bool remote = GetBool(RemoteFbTagId);
            if (!remote)
                return;
            if (GetBool(AutoManualTagId))
                return;
            bool confirmed = ShowConfirmation( "Confirm Operating Mode", $"Are you sure you want to switch " + $"{_unitName} to AUTO mode?");
            if (!confirmed)
                return;
            bool success = await WriteTagBitAsync(AutoManualTagId, true);
            if (!success)
            {
                ShowWriteError("AUTO mode");
                return;
            }
        }
        private async void ManualModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingUI)
                return;
            bool remote = GetBool(RemoteFbTagId);
            if (!remote)
                return;
            if (!GetBool(AutoManualTagId))
                return;
            bool confirmed = ShowConfirmation("Confirm Operating Mode",$"Are you sure you want to switch " + $"{_unitName} to MANUAL mode?");
            if (!confirmed)
                return;
            bool success = await WriteTagBitAsync(AutoManualTagId, false);
            if (!success)
            {
                ShowWriteError("MANUAL mode");
                return;
            }
        }
        private async void DefrostModeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingUI)
                return;
            if (!GetBool(RemoteFbTagId) ||  GetBool(AutoManualTagId))
            {
                return;
            }
            bool success = await WriteTagBitAsync(ManualDefrostTagId, true);
            if (!success)
            {
                ShowWriteError("Manual Defrost");
            }
        }
        private async void DefrostModeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingUI)
                return;
            if (!GetBool(RemoteFbTagId) ||  GetBool(AutoManualTagId))
            {
                return;
            }
            bool success = await WriteTagBitAsync(ManualDefrostTagId, false);
            if (!success)
            {
                ShowWriteError("Manual Defrost");
            }
        }
        private async void FansOnButton_Click(object sender, RoutedEventArgs e)
        {
            if (!GetBool(RemoteFbTagId) || GetBool(AutoManualTagId))
                return;
            bool confirmed = ShowConfirmation("Confirm Fans Operation", $"Are you sure you want to START " + $"the fans for {_unitName}?");
            if (!confirmed)
                return;
            bool success = await SetFanCommandAsync(true);
            if (!success)
            {
                ShowWriteError("Fans START");
                return;
            }
        }       
        private async void FansOffButton_Click(object sender, RoutedEventArgs e)
        {
            if (!GetBool(RemoteFbTagId) || GetBool(AutoManualTagId))
                return;
            bool confirmed = ShowConfirmation("Confirm Fans Operation", $"Are you sure you want to STOP " + $"the fans for {_unitName}?");
            if (!confirmed)
                return;
            bool success = await SetFanCommandAsync(false);
            if (!success)
            {
                ShowWriteError("Fans STOP");
                return;
            }
        }
        private async Task<bool> SetFanCommandAsync(bool start)
        {
            if (!_configurationLoaded)
                return false;
            if (!_popupTags.TryGetValue(FansStartTagId, out Tag startTag))
                return false;
            if (!_popupTags.TryGetValue(FansStopTagId, out Tag stopTag))
                return false;
            if (_plcDevice == null)
                return false;
            if (_isWriting)
                return false;
            if (!GetBool(RemoteFbTagId) || GetBool(AutoManualTagId))
            {
                return false;
            }
            _isWriting = true;
            await _plcLock.WaitAsync();
            try
            {
                return await Task.Run(() =>
                {
                    try
                    {
                        ushort startRegister = ReadHoldingRegisterSync( _plcDevice.IpAddress, 502, 1, startTag.Address);
                        ushort startMask = (ushort)(1 << startTag.Bit);
                        ushort newStartRegister = start? (ushort)( startRegister | startMask): (ushort)( startRegister &  ~startMask);
                        WriteHoldingRegisterSync(  _plcDevice.IpAddress,  502, 1, startTag.Address, newStartRegister);
                        ushort stopRegister = ReadHoldingRegisterSync( _plcDevice.IpAddress, 502, 1, stopTag.Address);
                        ushort stopMask = (ushort)(1 << stopTag.Bit);
                        ushort newStopRegister = start ? (ushort)( stopRegister &  ~stopMask): (ushort)(stopRegister | stopMask);
                        WriteHoldingRegisterSync( _plcDevice.IpAddress, 502, 1, stopTag.Address,newStopRegister);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Fan command error: {ex}");
                        return false;
                    }
                });
            }
            finally
            {
                _plcLock.Release();
                _isWriting = false;
            }
        }      
        private async Task ExecuteValveCommand(int openTagId, int closeTagId, bool open, string valveName)
        {
            if (!GetBool(RemoteFbTagId))
                return;
            bool confirmed = ShowConfirmation("Confirm Valve Operation", $"Are you sure you want to " + $"{(open ? "OPEN" : "CLOSE")} " + $"the {valveName} on {_unitName}?");
            if (!confirmed)
                return;
            // Prevent the opposite command from remaining active.
            bool firstWrite = await WriteTagBitAsync( open ? closeTagId : openTagId, false);
            if (!firstWrite)
            {
                ShowWriteError($"{(open ? "OPEN" : "CLOSE")} {valveName}");
                return;
            }
            // Activate requested state.
            bool secondWrite = await WriteTagBitAsync( open ? openTagId : closeTagId, true);
            if (!secondWrite)
            {
                ShowWriteError( $"{(open ? "OPEN" : "CLOSE")} {valveName}");
            }
        }
        private async void LiquidValveOpenButton_Click(object sender,RoutedEventArgs e)
        {
            await ExecuteValveCommand(LsvOpenTagId,LsvCloseTagId, true,"LSV");
        }

        private async void LiquidValveCloseButton_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteValveCommand(LsvOpenTagId, LsvCloseTagId, false,"LSV");
        }
        private async void SuctionValveOpenButton_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteValveCommand(SsvOpenTagId, SsvCloseTagId, true, "SSV");
        }
        private async void SuctionValveCloseButton_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteValveCommand(SsvOpenTagId, SsvCloseTagId, false, "SSV");
        }
        private async void HotGasSolenoidOpenButton_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteValveCommand(HgsvOpenTagId, HgsvCloseTagId, true, "HGSV");
        }
        private async void HotGasSolenoidCloseButton_Click( object sender, RoutedEventArgs e)
        {
            await ExecuteValveCommand( HgsvOpenTagId, HgsvCloseTagId, false, "HGSV");
        }
        private async void HotGasReturnOpenButton_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteValveCommand( HgrsvOpenTagId, HgrsvCloseTagId, true,"HGRSV");
        }
        private async void HotGasReturnCloseButton_Click( object sender, RoutedEventArgs e)
        {
            await ExecuteValveCommand(HgrsvOpenTagId, HgrsvCloseTagId,false, "HGRSV");
        }
        private string SelectDefrostTime(string cycleName, string currentTime)
        {
            var dialog = new DefrostTimeDialog(  _unitName,  cycleName, currentTime)
                {
                    Owner = this
                };
            if (dialog.ShowDialog() == true)
                return dialog.SelectedTime;
            return null;
        }
        private async void Cycle1TimeButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedTime = SelectDefrostTime("CYCLE 1", Cycle1TimeButton.Content?.ToString());
            if (string.IsNullOrWhiteSpace(selectedTime))
                return;
            if (!TryParseTime(selectedTime, out int hours, out int minutes))
                return;
            bool hoursWritten = await WriteNumericTagAsync(Cycle1HoursTagId, hours);
            if (!hoursWritten)
            {
                ShowWriteError("Cycle 1 Hours");
                return;
            }
            bool minutesWritten = await WriteNumericTagAsync(Cycle1MinutesTagId, minutes);
            if (!minutesWritten)
            {
                ShowWriteError("Cycle 1 Minutes");
                return;
            }
        }
        private async void Cycle2TimeButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedTime = SelectDefrostTime( "CYCLE 2", Cycle2TimeButton.Content?.ToString());
            if (string.IsNullOrWhiteSpace( selectedTime))
                return;
            if (!TryParseTime(selectedTime, out int hours,out int minutes))
                return;
            if (!await WriteNumericTagAsync(Cycle2HoursTagId,hours))
            {
                ShowWriteError("Cycle 2 Hours");
                return;
            }
            if (!await WriteNumericTagAsync(Cycle2MinutesTagId, minutes))
            {
                ShowWriteError("Cycle 2 Minutes");
                return;
            }
        }
        private async void Cycle3TimeButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedTime = SelectDefrostTime("CYCLE 3", Cycle3TimeButton.Content?.ToString());
            if (string.IsNullOrWhiteSpace(selectedTime))
                return;
            if (!TryParseTime(selectedTime, out int hours, out int minutes))
                return;
            if (!await WriteNumericTagAsync(Cycle3HoursTagId,hours))
            {
                ShowWriteError("Cycle 3 Hours");
                return;
            }
            if (!await WriteNumericTagAsync(Cycle3MinutesTagId, minutes))
            {
                ShowWriteError("Cycle 3 Minutes");
                return;
            }
        }
        private async void LiquidDrainSetPointButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedTime = SelectDefrostTime( "LIQUID DRAIN MODE", LiquidDrainSetPointButton.Content?.ToString());
            if (string.IsNullOrWhiteSpace(selectedTime))
                return;
            if (!TryParseTime(selectedTime, out int hours, out int minutes))
                return;
            int totalMinutes = (hours * 60) + minutes;
            bool success = await WriteNumericTagAsync(LiquidDrainSetPointTagId, totalMinutes);
            if (!success)
            {
                ShowWriteError("Liquid Drain Set Point");
            }
        }
        private async void DefrostingSetPointButton_Click(object sender,RoutedEventArgs e)
        {
            string selectedTime = SelectDefrostTime("DEFROSTING MODE", DefrostingSetPointButton.Content?.ToString());
            if (string.IsNullOrWhiteSpace(selectedTime))
                return;
            if (!TryParseTime(selectedTime, out int hours, out int minutes))
                return;
            int totalMinutes = (hours * 60) + minutes;
            bool success = await WriteNumericTagAsync(DefrostSetPointTagId, totalMinutes);
            if (!success)
            {
                ShowWriteError("Defrost Set Point");
            }
        }
        private async void Cycle1BypassCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingUI)
                return;
            await WriteTagBitAsync(Cycle1BypassTagId, true);
        }
        private async void Cycle1BypassCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingUI)
                return;
            await WriteTagBitAsync(Cycle1BypassTagId, false);
        }
        private async void Cycle2BypassCheckBox_Checked( object sender, RoutedEventArgs e)
        {
            if (_isUpdatingUI)
                return;
            await WriteTagBitAsync(Cycle2BypassTagId, true);
        }
        private async void Cycle2BypassCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingUI)
                return;
            await WriteTagBitAsync(Cycle2BypassTagId, false);
        }
        private async void Cycle3BypassCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingUI)
                return;
            await WriteTagBitAsync(Cycle3BypassTagId, true);
        }
        private async void Cycle3BypassCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingUI)
                return;
            await WriteTagBitAsync(Cycle3BypassTagId, false);
        }
        private bool TryParseTime(string value, out int hours, out int minutes)
        {
            hours = 0;
            minutes = 0;
            if (string.IsNullOrWhiteSpace(value))
                return false;
            string cleaned = value.Trim();
            string[] parts = cleaned.Split(':');
            if (parts.Length != 2)
                return false;
            if (!int.TryParse(parts[0].Trim(),  out hours))
                return false;
            if (!int.TryParse(parts[1].Trim(), out minutes))
                return false;
            return
                hours >= 0 && minutes >= 0 && minutes < 60;
        }
        private async Task<bool> WriteTagBitAsync(int tagId, bool value)
        {
            if (!_configurationLoaded)
                return false;
            if (!_popupTags.TryGetValue(tagId, out Tag tag))
            {
                System.Diagnostics.Debug.WriteLine($"Tag {tagId} not found.");
                return false;
            }
            if (_plcDevice == null || string.IsNullOrWhiteSpace( _plcDevice.IpAddress))
            {
                return false;
            }
            if (_isWriting)
                return false;
            _isWriting = true;
            await _plcLock.WaitAsync();
            try
            {
                return await Task.Run(() =>
                {
                    try
                    {
                        ushort currentRegister =  ReadHoldingRegisterSync( _plcDevice.IpAddress,  502,  1, tag.Address);
                        ushort mask = (ushort)(1 << tag.Bit);
                        ushort newRegister;
                        if (value)
                        {
                            newRegister = (ushort)(currentRegister |  mask);
                        }
                        else
                        {
                            newRegister = (ushort)(currentRegister &  ~mask);
                        }
                        WriteHoldingRegisterSync( _plcDevice.IpAddress,  502, 1, tag.Address, newRegister);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine( $"PLC bit write error: {ex}");
                        return false;
                    }
                });
            }
            finally
            {
                _plcLock.Release();
                _isWriting = false;
            }
        }
        private async Task<bool> WriteNumericTagAsync(int tagId, int value)
        {
            if (!_configurationLoaded)
                return false;
            if (!_popupTags.TryGetValue(tagId, out Tag tag))
                return false;
            if (_plcDevice == null || string.IsNullOrWhiteSpace(_plcDevice.IpAddress))
                return false;
            if (_isWriting)
                return false;
            _isWriting = true;
            await _plcLock.WaitAsync();
            using var client = new ModbusTcpClient();
            try
            {
                return await Task.Run(() =>
                {
                    try
                    {
                        client.Connect(new IPEndPoint(IPAddress.Parse(_plcDevice.IpAddress), 502));
                        if (!client.IsConnected)
                            return false;
                        const int unitIdentifier = 1;
                        WriteInt16(client, unitIdentifier, tag.Address, (short)value);
                        Debug.WriteLine( $"PLC NUMERIC WRITE SUCCESS: " + $"Tag={tag.Name}, " +  $"Address={tag.Address}, " +  $"Value={value}");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"PLC numeric write failed: {ex}");
                        return false;
                    }
                    finally
                    {
                        if (client.IsConnected)
                            client.Disconnect();
                    }
                });
            }
            finally
            {
                _plcLock.Release();
                _isWriting = false;
            }
        }
        private ushort ReadHoldingRegisterSync(string ipAddress,int port, byte unitId, int address)
        {
            using TcpClient client = new TcpClient();
            client.Connect( ipAddress, port);
            using NetworkStream stream = client.GetStream();
            ushort transactionId = (ushort)Random.Shared.Next( 1,  ushort.MaxValue);
            byte[] request =
            {
                (byte)(transactionId >> 8),
                (byte)(transactionId & 0xFF),
                0x00,
                0x00,
                0x00,
                0x06,
                unitId,
                0x03,
                (byte)(address >> 8),
                (byte)(address & 0xFF),
                0x00,
                0x01
            };
            stream.Write(request, 0, request.Length);
            byte[] header = ReadExact(stream, 9);
            byte function = header[7];
            if ((function & 0x80) != 0)
            {
                byte[] error = ReadExact(stream, 2);
                throw new InvalidOperationException( $"PLC returned Modbus exception: {error[1]}");
            }
            if (function != 0x03)
            {
                throw new InvalidOperationException($"Unexpected Modbus function: {function}");
            }
            int byteCount =  header[8];
            if (byteCount < 2)
            {
                throw new InvalidOperationException( "Invalid Modbus register response.");
            }
            byte[] data = ReadExact(stream, byteCount);
            return
                (ushort)((data[0] << 8) | data[1]);
        }
        private void WriteInt16(ModbusTcpClient modbusClient, int unitIdentifier, int registerAddress, short value)
        {
            ushort writeValue = unchecked((ushort)value);
            ushort swappedValue = (ushort)((writeValue >> 8) | (writeValue << 8));
            modbusClient.WriteSingleRegister(unitIdentifier, registerAddress, swappedValue);
        }
        private void WriteHoldingRegisterSync(string ipAddress, int port, byte unitId, int address, ushort value)
        {
            using TcpClient client =  new TcpClient();
            client.Connect(ipAddress, port);
            using NetworkStream stream =  client.GetStream();
            ushort transactionId = (ushort)Random.Shared.Next( 1, ushort.MaxValue);
            byte[] request =
            {
                (byte)(transactionId >> 8),
                (byte)(transactionId & 0xFF),
                0x00,
                0x00,
                0x00,
                0x06,
                unitId,
                0x06,
                (byte)(address >> 8),
                (byte)(address & 0xFF),
                (byte)(value >> 8),
                (byte)(value & 0xFF)
            };
            stream.Write(request, 0, request.Length);
            byte[] response = ReadExact(stream, 12);
            if (response[7] != 0x06)
            {
                if ((response[7] & 0x80) != 0)
                {
                    throw new InvalidOperationException($"PLC Modbus exception code: {response[8]}");
                }
                throw new InvalidOperationException( $"Unexpected Modbus response: {response[7]}");
            }
            int returnedAddress = (response[8] << 8) |  response[9];
            ushort returnedValue = (ushort)((response[10] << 8) | response[11]);
            if (returnedAddress != address || returnedValue != value)
            {
                throw new InvalidOperationException( "PLC did not confirm the requested register write.");
            }
        }
        private static byte[] ReadExact( NetworkStream stream,int count)
        {
            byte[] buffer = new byte[count];
            int offset = 0;
            while (offset < count)
            {
                int read = stream.Read(buffer, offset,  count - offset);
                if (read <= 0)
                {
                    throw new InvalidOperationException( "PLC connection closed before response was received.");
                }
                offset += read;
            }
            return buffer;
        }
        private List<RegisterBlock> BuildRegisterBlocks(List<Tag> tags)
        {
            const int maxGap = 5;
            const int maxRegisters = 100;
            var blocks = new List<RegisterBlock>();
            var sorted = tags.OrderBy(t => t.Address)
                    .ToList();
            int start = -1;
            int count = 0;
            List<Tag> current = null;
            foreach (var tag in sorted)
            {
                int required = GetRegisterCount(tag.Type);
                if (current == null)
                {
                    start = tag.Address;
                    count = required;
                    current = new List<Tag> { tag };
                    continue;
                }
                int nextAddress = tag.Address;
                bool tooFar = nextAddress - (start + count) > maxGap;
                bool tooLarge = nextAddress - start +  required > maxRegisters;
                if (tooFar || tooLarge)
                {
                    blocks.Add(new RegisterBlock
                        {
                            StartAddress = start,
                            RegisterCount = count,
                            Tags = current
                        });
                    start = tag.Address;
                    count = required;
                    current = new List<Tag>{ tag };
                }
                else
                {
                    int end = tag.Address + required;
                    count = Math.Max(count, end - start);
                    current.Add(tag);
                }
            }
            if (current != null && current.Count > 0)
            {
                blocks.Add(new RegisterBlock
                    {
                        StartAddress = start,
                        RegisterCount = count,
                        Tags = current
                    });
            }
            return blocks;
        }
        private static int GetRegisterCount(DataType type)
        {
            return type switch
            {
                DataType.INT => 1,
                DataType.UINT => 1,
                DataType.WBOOL => 1,
                DataType.DINT => 2,
                DataType.UDINT => 2,
                DataType.REAL => 2,
                DataType.INT64 => 4,
                _ => 1
            };
        }
        private static double DecodeValue(byte[] buf, int offset, DataType type, bool swapRegs = false)
        {
            switch (type)
            {
                case DataType.INT:
                    return BitConverter.ToInt16(new[] { buf[offset + 1], buf[offset] }, 0);
                case DataType.UINT:
                case DataType.WBOOL:
                    return BitConverter.ToUInt16(new[] { buf[offset + 1], buf[offset] }, 0);
                case DataType.DINT:
                    return BitConverter.ToInt32(new[] { buf[offset + 3], buf[offset + 2], buf[offset + 1], buf[offset] }, 0);
                case DataType.UDINT:
                    return BitConverter.ToUInt32(new[] { buf[offset + 3], buf[offset + 2], buf[offset + 1], buf[offset] }, 0);
                case DataType.REAL:
                    return swapRegs
                        ? BitConverter.ToSingle(new[] { buf[offset + 3], buf[offset + 2], buf[offset + 1], buf[offset] }, 0)
                        : BitConverter.ToSingle(new[] { buf[offset + 1], buf[offset], buf[offset + 3], buf[offset + 2] }, 0);
                case DataType.INT64:
                    {
                        float hi = BitConverter.ToSingle(new[] { buf[offset + 7], buf[offset + 6], buf[offset + 5], buf[offset + 4] }, 0);
                        float lo = BitConverter.ToSingle(new[] { buf[offset + 1], buf[offset], buf[offset + 3], buf[offset + 2] }, 0);
                        return hi * 4294967.296 + (lo / 1000.0);
                    }
                default:
                    throw new NotSupportedException($"DecodeValue: unsupported {type}");
            }
        }
        private class RegisterBlock
        {
            public int StartAddress { get; set; }
            public int RegisterCount { get; set; }
            public List<Tag> Tags { get; set; }
        }
        private iDbRepository GetRepository()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConfigDBConnString"] ?.ConnectionString;
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return null;
                }
                var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connectionString).Options;
                var context = new AppDbContext(options);
                return new dbRepository(context);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Repository error: {ex}");
                return null;
            }
        }
        private bool LoadPLCConfiguration()
        {
            try
            {
                var repository = GetRepository();
                if (repository == null)
                    return false;

                _plcDevice = repository.GetIODevices().ToList().FirstOrDefault(d => d.DeviceType == IODeviceType.PLC ||d.DeviceType == IODeviceType.EthernetDevice);
                if (_plcDevice == null)
                {
                    System.Diagnostics.Debug.WriteLine( "PLC device not found.");
                    return false;
                }
                if (string.IsNullOrWhiteSpace(_plcDevice.IpAddress))
                {
                    System.Diagnostics.Debug.WriteLine("PLC IP address not configured.");
                    return false;
                }
                // Load required tags ONCE
                int[] popupTagIds =
                {
            LocalFbTagId,
            RemoteFbTagId,
            HeaterFbTagId,

            Fan1TripTagId,
            Fan1RunTagId,
            Fan2TripTagId,
            Fan2RunTagId,
            Fan3TripTagId,
            Fan3RunTagId,
            Fan4TripTagId,
            Fan4RunTagId,

            PowerFbTagId,
            AutoManualTagId,

            FansStartTagId,
            FansStopTagId,

            LsvOpenTagId,
            LsvCloseTagId,
            SsvOpenTagId,
            SsvCloseTagId,
            HgsvOpenTagId,
            HgsvCloseTagId,
            HgrsvOpenTagId,
            HgrsvCloseTagId,

            ManualDefrostTagId,

            Cycle1BypassTagId,
            Cycle2BypassTagId,
            Cycle3BypassTagId,

            FreezingModeTagId,
            DefrostingModeTagId,

            LsvOpenCommandTagId,
            SsvOpenCommandTagId,
            HgsvOpenCommandTagId,
            HgrsvOpenCommandTagId,

            FansOnCommandTagId,

            LiquidDrainOutTagId,
            CoolingStartTagId,
            CycleReachedTagId,

            RoomTemperatureTagId,
            EvaporatorTemperatureTagId,
            TemperatureSetPointTagId,

            LiquidDrainSetPointTagId,
            DefrostSetPointTagId,

            FreezingElapsedHoursTagId,
            FreezingElapsedMinutesTagId,
            LiquidDrainElapsedTagId,
            DefrostElapsedTagId,

            RunHoursTagId,

            Cycle1HoursTagId,
            Cycle1MinutesTagId,
            Cycle2HoursTagId,
            Cycle2MinutesTagId,
            Cycle3HoursTagId,
            Cycle3MinutesTagId
        };
                var tags = repository.GetTags().Where(t => t.DeviceId == _plcDevice.Id && popupTagIds.Contains(t.Id))
                        .ToList();
                _popupTags.Clear();
                foreach (var tag in tags)
                {
                    _popupTags[tag.Id] = tag;
                }
                var missingTags = popupTagIds.Where(id => !_popupTags.ContainsKey(id))
                        .ToList();
                if (missingTags.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("Missing popup tags: " + string.Join(", ", missingTags));
                }
                _configurationLoaded = true;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PLC configuration error: {ex}");
                return false;
            }
        }
        private bool ShowConfirmation(string title,string message)
        {
            var dialog = new ConfirmationDialog(title, message)
                {
                    Owner = Window.GetWindow(this)
                };
            return  dialog.ShowDialog() == true;
        }
        private void ShowWriteError(string operation)
        {
            MessageBox.Show($"Unable to send '{operation}' command to the PLC.\n\n" +  "Please check the PLC connection.", "PLC Communication Error", MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        private void ControlsTabButton_Click(object sender, RoutedEventArgs e)
        {
            ShowTab(1);
        }
        private void CyclesTabButton_Click(object sender, RoutedEventArgs e)
        {
            ShowTab(2);
        }
        private void EventsTabButton_Click(object sender, RoutedEventArgs e)
        {
            ShowTab(3);
        }
        private void MaintenanceTabButton_Click(object sender,RoutedEventArgs e)
        {
            ShowTab(4);
        }
        //private void ShowTab(int tab)
        //{
        //    ControlsContent.Visibility = tab == 1? Visibility.Visible: Visibility.Collapsed;
        //    CyclesContent.Visibility = tab == 2? Visibility.Visible: Visibility.Collapsed;
        //    EventsContent.Visibility = tab == 3? Visibility.Visible: Visibility.Collapsed;
        //    MaintenanceContent.Visibility = tab == 4? Visibility.Visible: Visibility.Collapsed;
        //    ControlsTabButton.Background = tab == 1 ? new SolidColorBrush(Color.FromRgb(240,242, 244)): Brushes.White;
        //    CyclesTabButton.Background = tab == 2? new SolidColorBrush(Color.FromRgb(240,242,244)): Brushes.White;
        //    EventsTabButton.Background = tab == 3? new SolidColorBrush(Color.FromRgb(240, 242, 244)): Brushes.White;
        //    MaintenanceTabButton.Background = tab == 4? new SolidColorBrush(Color.FromRgb(240,242, 244)): Brushes.White;
        //}
        private void ShowTab(int tab)
        {
            // Hide all contents
            ControlsContent.Visibility = Visibility.Collapsed;
            CyclesContent.Visibility = Visibility.Collapsed;
            EventsContent.Visibility = Visibility.Collapsed;
            MaintenanceContent.Visibility = Visibility.Collapsed;

            // Reset all tab buttons
            ResetTabButtons();

            switch (tab)
            {
                case 1:

                    ControlsContent.Visibility =
                        Visibility.Visible;

                    ActivateTabButton(
                        ControlsTabButton);

                    break;

                case 2:

                    CyclesContent.Visibility =
                        Visibility.Visible;

                    ActivateTabButton(
                        CyclesTabButton);

                    break;

                case 3:

                    EventsContent.Visibility =
                        Visibility.Visible;

                    ActivateTabButton(
                        EventsTabButton);

                    break;

                case 4:

                    MaintenanceContent.Visibility =
                        Visibility.Visible;

                    ActivateTabButton(
                        MaintenanceTabButton);

                    break;
            }
        }
        private void ResetTabButtons()
        {
            ControlsTabButton.Background =
                Brushes.Transparent;

            ControlsTabButton.Foreground =
                new SolidColorBrush(
                    Color.FromRgb(75, 85, 99));


            CyclesTabButton.Background =
                Brushes.Transparent;

            CyclesTabButton.Foreground =
                new SolidColorBrush(
                    Color.FromRgb(75, 85, 99));


            EventsTabButton.Background =
                Brushes.Transparent;

            EventsTabButton.Foreground =
                new SolidColorBrush(
                    Color.FromRgb(75, 85, 99));


            MaintenanceTabButton.Background =
                Brushes.Transparent;

            MaintenanceTabButton.Foreground =
                new SolidColorBrush(
                    Color.FromRgb(75, 85, 99));
        }
        private void ActivateTabButton(Button button)
        {
            button.Background =
                new SolidColorBrush(
                    Color.FromRgb(55, 65, 81));   // #374151

            button.Foreground = Brushes.White;
        }
        private void CloseButton_Click( object sender, RoutedEventArgs e)
        {
            StopLivePolling();
            Close();
        }
        private void StopLivePolling()
        {
            if (_liveValuesTimer == null)
                return;
            _liveValuesTimer.Stop();
            _liveValuesTimer.Tick -= LiveValuesTimer_Tick;
            _liveValuesTimer = null;
        }
        protected override void OnClosed(EventArgs e)
        {
            StopLivePolling();
            base.OnClosed(e);
        }
    }
}