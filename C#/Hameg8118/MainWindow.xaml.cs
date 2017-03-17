using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using System.Diagnostics;

namespace Hameg8118
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///    
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged; // occurs when property is changed, inherited from interface
        
        internal Program program;
        private GUIDelegate guiDelegate; // for updating GUI from different thread
        private GUIDelegate compensationWindowCloseDelegate; // close compensation window
        private List<BindingExpression> bindingExpressions = new List<BindingExpression>(); // GUI bindings for updating using events (from different thread)
        private Compensation compensationWindow;
        private About aboutWindow;
        private HowToConnect howToConnectWindow;

        private bool compensationStarted = false;

        // <CONSTRUCTORS>
       
        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this; // default binding data context        

            program = Program.GetInstance();
            Resources.Add("program", program);
            guiDelegate += GUIUpdate; // main GUI update method    
            compensationWindowCloseDelegate += CompensationClose;                        
            program.Device.PropertyChanged += (object sender, PropertyChangedEventArgs e) => Dispatcher.Invoke(guiDelegate, null);
            program.CompensationFinished += (bool b) => Dispatcher.Invoke(compensationWindowCloseDelegate, null);
            program.CompensationFinished += Program_CompensationFinished;

            // fill combo box modes
            Array modes = Enum.GetValues(typeof(Mode));
            foreach (int i in modes)
            {
                comboBoxMode.Items.Add(((Mode)i).Name());
            }
            // fill combo box bias modes
            Array biasmodes = Enum.GetValues(typeof(BiasMode));
            foreach (int i in biasmodes)
            {
                comboBoxBiasMode.Items.Add(((BiasMode)i).Name());
            }

            // fill frequency combo boxes
            foreach (int i in Global.Frequencies)
            {
                string item;
                if (i >= 10000)
                {
                    item = (i / 1000).ToString() + " kHz";
                }
                else
                {
                    item = i.ToString() + " Hz";
                }
                comboBoxFrequency.Items.Add(item);
                comboBoxSweepStart.Items.Add(item);
                comboBoxSweepStop.Items.Add(item);
            }
            comboBoxSweepStart.SelectedIndex = 0; // select first item
            comboBoxSweepStop.SelectedIndex = comboBoxSweepStop.Items.Count - 1; // select last item

            // bindings in code
            // Manual / sweep logging
            Binding menuItemFileLogManualAndSweepBinding = new Binding(nameof(program.LogManualAndSweep));
            menuItemFileLogManualAndSweepBinding.Source = program;
            menuItemFileLogManualAndSweepBinding.Mode = BindingMode.TwoWay;
            menuItemFileLogManualAndSweep.SetBinding(MenuItem.IsCheckedProperty, menuItemFileLogManualAndSweepBinding);
            bindingExpressions.Add(menuItemFileLogManualAndSweep.GetBindingExpression(MenuItem.IsCheckedProperty));

            // Continuous logging
            Binding menuItemFileLogAllBinding = new Binding(nameof(program.LogContinuous));
            menuItemFileLogAllBinding.Source = program;
            menuItemFileLogAllBinding.Mode = BindingMode.TwoWay;
            menuItemFileLogAll.SetBinding(MenuItem.IsCheckedProperty, menuItemFileLogAllBinding);
            bindingExpressions.Add(menuItemFileLogAll.GetBindingExpression(MenuItem.IsCheckedProperty));

            // binding expressions, bindings in XAML
            bindingExpressions.Add(statusBarBottomFilePath.GetBindingExpression(ContentProperty));
            bindingExpressions.Add(statusBarBottomConnection.GetBindingExpression(ContentProperty));

            bindingExpressions.Add(textBlockX.GetBindingExpression(TextBlock.TextProperty));
            bindingExpressions.Add(labelXUnit.GetBindingExpression(Label.ContentProperty));
            bindingExpressions.Add(labelXName.GetBindingExpression(Label.ContentProperty));

            bindingExpressions.Add(textBlockY.GetBindingExpression(TextBlock.TextProperty));
            bindingExpressions.Add(labelYUnit.GetBindingExpression(Label.ContentProperty));
            bindingExpressions.Add(labelYName.GetBindingExpression(Label.ContentProperty));

            bindingExpressions.Add(radioButtonMeasureSingle.GetBindingExpression(RadioButton.IsCheckedProperty));
            bindingExpressions.Add(radioButtonMeasureContinuous.GetBindingExpression(RadioButton.IsCheckedProperty));

            bindingExpressions.Add(radioButtonModelSeries.GetBindingExpression(RadioButton.IsCheckedProperty));
            bindingExpressions.Add(radioButtonModelParallel.GetBindingExpression(RadioButton.IsCheckedProperty));

            bindingExpressions.Add(radioButtonAveragingNone.GetBindingExpression(RadioButton.IsCheckedProperty));
            bindingExpressions.Add(radioButtonAveragingSix.GetBindingExpression(RadioButton.IsCheckedProperty));

            bindingExpressions.Add(radioButtonSpeedFast.GetBindingExpression(RadioButton.IsCheckedProperty));
            bindingExpressions.Add(radioButtonSpeedMedium.GetBindingExpression(RadioButton.IsCheckedProperty));
            bindingExpressions.Add(radioButtonSpeedSlow.GetBindingExpression(RadioButton.IsCheckedProperty));

            bindingExpressions.Add(textBoxVoltage.GetBindingExpression(TextBox.TextProperty));
            bindingExpressions.Add(comboBoxMode.GetBindingExpression(ComboBox.SelectedIndexProperty));
            bindingExpressions.Add(comboBoxFrequency.GetBindingExpression(ComboBox.SelectedIndexProperty));

            bindingExpressions.Add(buttonSweepStartStop.GetBindingExpression(Button.ContentProperty));

            bindingExpressions.Add(comboBoxMode.GetBindingExpression(ComboBox.IsEnabledProperty));
            bindingExpressions.Add(comboBoxFrequency.GetBindingExpression(ComboBox.IsEnabledProperty));
            bindingExpressions.Add(textBoxVoltage.GetBindingExpression(TextBox.IsEnabledProperty));
            bindingExpressions.Add(buttonFrequencyUp.GetBindingExpression(Button.IsEnabledProperty));
            bindingExpressions.Add(buttonFrequencyDown.GetBindingExpression(Button.IsEnabledProperty));
            bindingExpressions.Add(groupBoxCompensation.GetBindingExpression(GroupBox.IsEnabledProperty));

            bindingExpressions.Add(groupBoxCompensation.GetBindingExpression(GroupBox.IsEnabledProperty));
            bindingExpressions.Add(groupBoxTrigger.GetBindingExpression(GroupBox.IsEnabledProperty));
            bindingExpressions.Add(groupBoxModel.GetBindingExpression(GroupBox.IsEnabledProperty));
            bindingExpressions.Add(groupBoxAveraging.GetBindingExpression(GroupBox.IsEnabledProperty));
            bindingExpressions.Add(groupBoxSpeed.GetBindingExpression(GroupBox.IsEnabledProperty));
            bindingExpressions.Add(gridFrequencyInner.GetBindingExpression(Grid.IsEnabledProperty));

            bindingExpressions.Add(labelSweepRepeatsRemaining.GetBindingExpression(Label.ContentProperty));
            bindingExpressions.Add(menuItemFileClose.GetBindingExpression(MenuItem.IsEnabledProperty));
            bindingExpressions.Add(viewBoxInstrument.GetBindingExpression(Viewbox.VisibilityProperty));
            bindingExpressions.Add(viewBoxInstrument.GetBindingExpression(Viewbox.ToolTipProperty));

            bindingExpressions.Add(textBoxBiasVoltage.GetBindingExpression(TextBox.IsEnabledProperty));
            bindingExpressions.Add(textBoxBiasCurrent.GetBindingExpression(TextBox.IsEnabledProperty));
            bindingExpressions.Add(checkBoxConstantVoltage.GetBindingExpression(CheckBox.IsEnabledProperty));
            bindingExpressions.Add(gridBiasVoltageSetting.GetBindingExpression(Grid.VisibilityProperty));
            bindingExpressions.Add(gridBiasCurrentSetting.GetBindingExpression(Grid.VisibilityProperty));
        }

        // </CONSTRUCTORS>

        // <METHODS>

        /// <summary>
        /// This method should be called when a property is changed
        /// </summary>
        /// <param name="caller">Property that changed</param>
        private void NotifyPropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(caller)); }
        }        
        
        /// <summary>
        /// Update all GUI elements with bindings
        /// </summary>
        private void GUIUpdate()
        {
            foreach (BindingExpression be in bindingExpressions)
            {
                be.UpdateTarget();
            }
        }

        // </METHODS>

        // <PROPERTIES>
        
        /// <summary>
        /// Gets string representation of scaled X value but without prefix
        /// For example, 100 000 000 will return as 100 while hiding the "M" prefix
        /// </summary>
        public string XValue
        {
            get
            {
                if ((program.State is Ready) && (program.Device.Values?.X != null))
                {
                    double x = (double)(program.Device.Values.X);
                    Converters.Prefix(ref x);
                    return x.ToString(Global.NumberFormat);
                }
                else { return "-------"; }
            }
        }

        /// <summary>
        /// Gets string representation of scaled Y value but without prefix (where applicable)
        /// For example, degrees will return unscaled, ohms will return scaled
        /// </summary>
        public string YValue
        {
            get
            {
                if ((program.State is Ready) && (program.Device.Values?.Y != null))
                {
                    double y = (double)(program.Device.Values.Y);
                    switch (program.Device.Mode)
                    {
                        case Mode.LQ:
                        case Mode.CD:
                        case Mode.RQ:
                        case Mode.ZTheta:
                        case Mode.YTheta:
                            break;
                        default:
                            Converters.Prefix(ref y);
                            break;
                    }
                    return y.ToString(Global.NumberFormat);
                }
                else { return "-------"; }
            }
        }

        /// <summary>
        /// Gets X unit with prefix according to actual value
        /// For example, 0.01 F will return as "mF"
        /// </summary>
        public string XUnit
        {
            get
            {
                if ((program.State is Ready) && (program.Device.Values?.X != null))
                {
                    double x = (double)(program.Device.Values.X);
                    return Converters.Prefix(ref x) + program.Device.XUnit;
                }
                else { return string.Empty; }
            }
        }

        /// <summary>
        /// Gets Y unit with prefix according to actual value (where applicable)
        /// For example, degrees will always return "°" while Farads may return also "nF", "pF" etc.
        /// </summary>
        public string YUnit
        {
            get
            {
                if ((program.State is Ready) && (program.Device.Values?.Y != null))
                {
                    double y = (double)(program.Device.Values.Y);
                    switch (program.Device.Mode)
                    {
                        case Mode.LQ:
                        case Mode.CD:
                        case Mode.RQ:
                        case Mode.ZTheta:
                        case Mode.YTheta:
                            return program.Device.YUnit;
                        default:
                            return Converters.Prefix(ref y) + program.Device.YUnit;
                    }
                }
                else { return string.Empty; }
            }
        }
                
        /// <summary>
        /// Gets the symbol of the physical quantity associated with X
        /// </summary>
        public string XSymbol
        {
            get
            {
                if (program.State is Ready)
                {
                    return program.Device.XSymbol;
                }
                else { return string.Empty; }
            }
        }
        
        /// <summary>
        /// Gets the symbol of the physical quantity associated with Y
        /// </summary>
        public string YSymbol
        {
            get
            {
                if (program.State is Ready)
                {
                    return program.Device.YSymbol;
                }
                else { return string.Empty; }
            }
        }
        
        /// <summary>
        /// Gets connection details augmented by port name
        /// </summary>
        public string ConnectionDetails
        {
            get
            {
                if (program.Device.DeviceInfo == null)
                {
                    return "not connected";
                }
                else
                {
                    return "[" + program.PortName + "] " + program.Device.DeviceInfo;
                }
            }
        }
        
        /// <summary>
        /// Gets or sets trigger to manual
        /// </summary>
        public bool? TriggerSingle
        {
            get
            {
                if (program.State is Ready)
                {
                    return (program.Device.Trigger == Trigger.Manual);
                }
                else { return null; }
            }
            set
            {
                if (value != null)
                {
                    if (value == true)
                    {
                        program.SetTrigger(Trigger.Manual); // send command to set manual trigger
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets trigger to continuous
        /// </summary>
        public bool? TriggerContinuous
        {
            get
            {
                if (program.State is Ready)
                {
                    return (program.Device.Trigger == Trigger.Continuous);
                }
                else { return null; }
            }
            set
            {
                if (value != null)
                {
                    if (value == true)
                    {
                        program.SetTrigger(Trigger.Continuous); // send command to set continuous trigger
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets or sets automatic circuit model
        /// </summary>
        public bool? ModelAuto
        {
            get
            {                
                if (program.State is Ready)
                {
                    return (program.Device.Model == Model.Auto);
                }
                else { return null; }
            }
            set
            {
                if (value != null)
                {
                    if (value == true)
                    {
                        program.SetModel(Model.Auto); // send command to set automatic model
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets or sets series circuit model
        /// </summary>
        public bool? ModelSeries
        {
            get
            {
                if (program.State is Ready)
                {
                    return (program.Device.Model == Model.Series);
                }
                else { return null; }
            }
            set
            {
                if (value != null)
                {
                    if (value == true)
                    {
                        program.SetModel(Model.Series); // send command to set series model
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets or sets parallel circuit model
        /// </summary>
        public bool? ModelParallel
        {
            get
            {
                if (program.State is Ready)
                {
                    return (program.Device.Model == Model.Parallel);
                }
                else { return null; }
            }
            set
            {
                if (value != null)
                {
                    if (value == true)
                    {
                        program.SetModel(Model.Parallel); // send command to set parallel model
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets or sets no averaging
        /// </summary>
        public bool? AveragingNone
        {
            get
            {
                if (program.State is Ready)
                {
                    return (program.Device.Averaging == Averaging.None);
                }
                else { return null; }
            }
            set
            {
                if (value != null)
                {
                    if (value == true)
                    {
                        program.SetAveraging(Averaging.None);
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets or sets medium averaging
        /// 6 values will be measured, the largest and lowest discarded and the device will return an average of the remaining 4
        /// </summary>
        public bool? AveragingMedium
        {
            get
            {
                if (program.State is Ready)
                {
                    return (program.Device.Averaging == Averaging.Medium);
                }
                else { return null; }
            }
            set
            {
                if (value != null)
                {
                    if (value == true)
                    {
                        program.SetAveraging(Averaging.Medium);
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets or sets fast measurement speed
        /// </summary>
        public bool? SpeedFast
        {
            get
            {
                if (program.State is Ready)
                {
                    return (program.Device.Speed == Speed.Fast);
                }
                else { return null; }
            }
            set
            {
                if (value != null)
                {
                    if (value == true)
                    {
                        program.SetSpeed(Speed.Fast);
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets or sets medium measurement speed
        /// </summary>
        public bool? SpeedMedium
        {
            get
            {
                if (program.State is Ready)
                {
                    return (program.Device.Speed == Speed.Medium);
                }
                else { return null; }
            }
            set
            {
                if (value != null)
                {
                    if (value == true)
                    {
                        program.SetSpeed(Speed.Medium);
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets or sets slow measurement speed
        /// </summary>
        public bool? SpeedSlow
        {
            get
            {
                if (program.State is Ready)
                {
                    return (program.Device.Speed == Speed.Slow);
                }
                else { return null; }
            }
            set
            {
                if (value != null)
                {
                    if (value == true)
                    {
                        program.SetSpeed(Speed.Slow);
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets signal level in Vrms
        /// </summary>
        public string TestSignal
        {
            get
            {
                if (program.State is Ready)
                {
                    if (textBoxVoltage.IsKeyboardFocused) // currently being written
                    {
                        return textBoxVoltage.Text;
                    }
                    return (program.Device.Voltage.ToString(Global.VoltageNumberFormat));
                }
                else { return string.Empty; }
            }
        }
        
        /// <summary>
        /// Gets bias level in V
        /// </summary>
        public string BiasVoltage
        {
            get
            {
                if (program.State is Ready)
                {
                    if (textBoxBiasVoltage.IsKeyboardFocused) // currently being written
                    {
                        return textBoxBiasVoltage.Text;
                    }
                    return (program.Device.BiasVoltage.ToString(Global.VoltageNumberFormat));
                }
                else { return string.Empty; }
            }
        }
        
        /// <summary>
        /// Gets bias level in A
        /// </summary>
        public string BiasCurrent
        {
            get
            {
                if (program.State is Ready)
                {
                    if (textBoxBiasCurrent.IsKeyboardFocused) // currently being written
                    {
                        return textBoxBiasCurrent.Text;
                    }
                    return (program.Device.BiasCurrent.ToString(Global.CurrentNumberFormat));
                }
                else { return string.Empty; }
            }
        }
        
        /// <summary>
        /// Gets index of the device mode
        /// </summary>
        public int ModeIndex
        {
            get
            {
                if (program.State is Ready)
                {
                    return (int)(program.Device.Mode);
                }
                else { return -1; }
            }
        }

        /// <summary>
        /// Gets index of the device bias mode
        /// </summary>
        public int BiasModeIndex
        {
            get
            {
                if (program.State is Ready)
                {
                    return (int)(program.Device.BiasMode);
                }
                else { return -1; }
            }
        }
        
        /// <summary>
        /// Gets index of the frequency (from list of available frequencies)
        /// </summary>
        public int FrequencyIndex
        {
            get
            {
                if (program.State is Ready)
                {
                    return program.Device.FrequencyIndex;
                }
                else { return -1; }
            }
        }
        
        /// <summary>
        /// Gets whether Bias Voltage setting is permitted
        /// </summary>
        public bool BiasVoltageAvailable
        {
            get
            {
                if (program.Device.BiasMode.Equals(BiasMode.Internal))
                    if (program.Device.Mode.Equals(Mode.CD) || program.Device.Mode.Equals(Mode.CR) || program.Device.Mode.Equals(Mode.RX) || program.Device.Mode.Equals(Mode.ZTheta))
                    return true;
                return false;
            }
        }
        /// <summary>
        /// Gets whether Bias Voltage setting is permitted
        /// </summary>
        public bool BiasCurrentAvailable
        {
            get
            {
                if (program.Device.BiasMode.Equals(BiasMode.Internal))
                    if (program.Device.Mode.Equals(Mode.LQ) || program.Device.Mode.Equals(Mode.LR) /*|| program.Device.Mode.Equals(Mode.NTheta) || program.Device.Mode.Equals(Mode.M)*/)
                        return true;
                return false;
            }
        }
        /// <summary>
        /// Gets whether sweep is currently running
        /// </summary>
        public bool Sweep
        {
            get
            {
                return program.Sweep;
            }
        }
        
        /// <summary>
        /// Gets the caption for sweep button (start or stop)
        /// </summary>
        public string SweepButtonContent
        {
            get
            {
                if (Sweep)
                { // sweep is currently running
                    return "Stop";
                }
                else
                {
                    return "Start";
                }
            }
        }
        
        /// <summary>
        /// Gets the string representation of the number of remaining repeats in sweep
        /// </summary>
        public string RemainingRepeats
        {
            get
            {
                if (program.RemainingRepeats != null)
                {
                    if ((int)(program.RemainingRepeats) > 0)
                    {
                        return "Remaining repeats: " + ((int)program.RemainingRepeats).ToString();
                    }
                }
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Gets the string representation of file path for displaying
        /// </summary>
        public string FilePath
        {
            get
            {
                if (IsLogging)
                {
                    return "Log file: " + program.FilePath;
                }
                return "no file";
            }
        }
        
        /// <summary>
        /// Gets whether logging file is open (and thus can be closed)
        /// </summary>
        public bool IsLogging
        {
            get
            {
                return !(string.IsNullOrEmpty(program.FilePath));
            }
        }
        
        /// <summary>
        /// Gets whether the instrument picture is visible or not depending on readiness of the device
        /// </summary>
        public Visibility InstrumentVisibility
        {
            get
            {
                if (program.State is Ready)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Hidden;
                }
            }
        }
        
        /// <summary>
        /// Gets the information about device
        /// </summary>
        public string DeviceName
        {
            get
            {
                return program.Device.DeviceInfo;
            }
        }

        // </PROPERTIES>

        // <EVENT HANDLERS>
        
        /// <summary>
        /// Close compensation window that blocks the GUI
        /// </summary>
        private void CompensationClose()
        {
            if (compensationWindow != null)
            {
                compensationWindow.ForceClose();
                compensationWindow = null;
            }
        }

        /// <summary>
        /// Handles finished compensation and displays error message if the compensation failed
        /// </summary>
        /// <param name="status">Indicates whether compensation succeeded (true) or failed (false)</param>
        private void Program_CompensationFinished(bool status)
        {
            if (!status && compensationStarted)
            {
                MessageBox.Show("Compensation failed", "Compensation", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            compensationStarted = false;
        }

        /// <summary>
        /// Handles actions that result in closing the serial port
        /// Displays error message if the port closed because of an exception
        /// </summary>
        /// <param name="innerException">Exception that caused the closure of serial port</param>
        private void Program_PortClosed(Exception innerException)
        {
            if (innerException != null)
            {
                MessageBox.Show(innerException.Message, "Connection error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Measure single value
        /// </summary>
        /// <param name="sender">Measure button</param>
        /// <param name="e">Parameters</param>
        private void buttonMeasure_Click(object sender, RoutedEventArgs e)
        {
            program.MeasureSingle();
        }
        
        /// <summary>
        /// Set automatic model
        /// </summary>
        /// <param name="sender">Auto button</param>
        /// <param name="e">Parameter</param>
        private void buttonModelAuto_Click(object sender, RoutedEventArgs e)
        {
            program.SetModel(Model.Auto);
        }
        
        /// <summary>
        /// Create list of available COM ports on mouse enter 
        /// </summary>
        /// <param name="sender">Connection menu</param>
        /// <param name="e">Parameter</param>
        private void MenuItemConnection_MouseEnter(object sender, MouseEventArgs e)
        {
            ((MenuItem)sender).Items.Clear();
            foreach (COMPortInfo cpi in COMPortInfo.GetCOMPortsInfo())
            {
                MenuItem port = new MenuItem();
                port.Header = cpi.Description;
                port.Tag = cpi.Name;
                if ((program.PortName == cpi.Name) && (program.State is ResettableState || program.State is Connected))
                { // this port is currently connected and device is at least connected
                    port.IsChecked = true;
                    port.Click += Port_Disconnect;
                }
                else // all other available ports
                {
                    port.Click += Port_Connect;
                }

                ((MenuItem)sender).Items.Add(port);
            }
        }
        
        /// <summary>
        /// Connects to the selected serial port
        /// </summary>
        /// <param name="sender">Menuitem that belongs to port</param>
        /// <param name="e">Parameter</param>
        private void Port_Connect(object sender, RoutedEventArgs e)
        {
            program.PortClosed += Program_PortClosed;
            program.Connect((string)(((MenuItem)sender).Tag));
        }

        /// <summary>
        /// Disconnect from current serial port
        /// </summary>
        /// <param name="sender">MenuItem other than the port that is connected now</param>
        /// <param name="e">Parameter</param>
        private void Port_Disconnect(object sender, RoutedEventArgs e)
        {
            program.PortClosed -= Program_PortClosed;
            program.Disconnect();
        }
        
        /// <summary>
        /// Resets device
        /// </summary>
        /// <param name="sender">Reset button</param>
        /// <param name="e">Parameter</param>
        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            program.Reset();
        }
        
        /// <summary>
        /// Start performing compensation on open circuit
        /// </summary>
        /// <param name="sender">Open compensation button</param>
        /// <param name="e">Parameter</param>
        private void buttonCompensationOpen_Click(object sender, RoutedEventArgs e)
        {
            if ((program.State is Ready) == false)
            {
                return;                
            }

            compensationStarted = true;
            if (radioButtonCompensationAllFrequencies.IsChecked == true)
            {
                program.CompensateOpen(Compensate.AllFrequencies);
                compensationWindow = new Compensation(); // all frequencies take 2 minuts, during which the instrument cannot be operated
                compensationWindow.ShowDialog();                
            }
            else
            {
                program.CompensateOpen(Compensate.SingleFrequency);
            }
        }

        /// <summary>
        /// Start performing compensation on short circuit
        /// </summary>
        /// <param name="sender">Short compensation button</param>
        /// <param name="e">Parameter</param>
        private void buttonCompensationShort_Click(object sender, RoutedEventArgs e)
        {
            if ((program.State is Ready) == false)
            {
                return;
            }

            compensationStarted = true;            
            if (radioButtonCompensationAllFrequencies.IsChecked == true)
            {
                program.CompensateShort(Compensate.AllFrequencies);
                compensationWindow = new Compensation(); // all frequencies take 2 minuts, during which the instrument cannot be operated
                compensationWindow.ShowDialog();               
            }
            else
            {
                program.CompensateShort(Compensate.SingleFrequency);
            }
        }
        
        /// <summary>
        /// Parse and validate values of test signal voltage after user presses Enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxVoltage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                double newVoltage;
                if (double.TryParse(textBoxVoltage.Text, out newVoltage))
                {
                    program.SetVoltage(newVoltage);
                }
                Keyboard.ClearFocus();
            }
        }

        /// <summary>
        /// Parse and validate values of bias voltage after user presses Enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxBiasVoltage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                double newVoltage;
                if (double.TryParse(textBoxBiasVoltage.Text, out newVoltage))
                {
                    program.SetBiasVoltage(newVoltage);
                }
                Keyboard.ClearFocus();
            }
        }

        /// <summary>
        /// Parse and validate values of bias voltage after user presses Enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxBiasCurrent_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                double newCurrent;
                if (double.TryParse(textBoxBiasCurrent.Text, out newCurrent))
                {
                    program.SetBiasCurrent(newCurrent);
                }
                Keyboard.ClearFocus();
            }
        }
        /// <summary>
        /// Handles mode selection
        /// </summary>
        /// <param name="sender">Mode combo box</param>
        /// <param name="e">Parameters</param>
        private void comboBoxMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).IsDropDownOpen) // check if this selection was made by user
            {
                program.SetMode((Mode)(((ComboBox)sender).SelectedIndex));
            }
        }
        
        /// <summary>
        /// Handles bias mode selection
        /// </summary>
        /// <param name="sender">Bias Mode combo box</param>
        /// <param name="e">Parameters</param>
        private void comboBoxBiasMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).IsDropDownOpen) // check if this selection was made by user
            {
                program.SetBiasMode((BiasMode)(((ComboBox)sender).SelectedIndex));
            }
        }

        /// <summary>
        /// Handles Constant Voltage selection
        /// </summary>
        /// <param name="sender">Constant Voltage check box</param>
        /// <param name="e">Parameters</param>
        private void checkBoxConstantVoltage_Checked(object sender, RoutedEventArgs e)
        {
            program.SetConstantVoltage((((CheckBox)sender).IsChecked??false) ? ConstantVoltage.On : ConstantVoltage.Off);
        }


        /// <summary>
        /// Handles frequency selection
        /// </summary>
        /// <param name="sender">Frequency combo box</param>
        /// <param name="e">Parameter</param>
        private void comboBoxFrequency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).IsDropDownOpen) // check if this selection was made by user
            {
                program.SetFrequency(((ComboBox)sender).SelectedIndex);
            }
        }
        
        /// <summary>
        /// Increase frequency
        /// </summary>
        /// <param name="sender">That tiny button with plus on it, next to the frequency combo box</param>
        /// <param name="e">Parameter</param>
        private void buttonFrequencyUp_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxFrequency.SelectedIndex < (Global.Frequencies.Length - 1)) // check for last index
            {
                program.SetFrequency(comboBoxFrequency.SelectedIndex + 1);
            }
        }

        /// <summary>
        /// Decrease frequency
        /// </summary>
        /// <param name="sender">That tiny button with plus on it, next to the frequency combo box</param>
        /// <param name="e">Parameter</param>
        private void buttonFrequencyDown_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxFrequency.SelectedIndex > 0) // check for first index
            {
                program.SetFrequency(comboBoxFrequency.SelectedIndex - 1);
            }
        }
        
        /// <summary>
        /// Start or stop sweep 
        /// </summary>
        /// <param name="sender">Sweep button</param>
        /// <param name="e">Parameter</param>
        private void buttonSweepStartStop_Click(object sender, RoutedEventArgs e)
        {
            if (program.TriggerState == TriggerStates.Sweep)
            {
                // cancel sweep if already running
                program.CancelSweep();
            }
            else
            {
                int? repeats = null;
                if (checkBoxSweepRepeatsInfinite.IsChecked == false)
                {
                    try
                    {
                        repeats = int.Parse(textBoxSweepRepeats.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Number of repeats is invalid", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }                    
                }
                program.SetupSweep(comboBoxSweepStart.SelectedIndex, comboBoxSweepStop.SelectedIndex, repeats);
            }

            GUIUpdate();
        }
        
        /// <summary>
        /// Copy current values in raw form (numbers only)
        /// </summary>
        /// <param name="sender">Context menu at value display</param>
        /// <param name="e">Parameter</param>
        private void MenuItemCopyMainValues_Click(object sender, RoutedEventArgs e)
        {
            XY values = program.Device.Values;
            StringBuilder sb = new StringBuilder();
            sb.Append(values.X);
            sb.Append(Global.Delimiter);
            sb.Append(values.Y);
            try { Clipboard.SetData(DataFormats.Text, sb); }
            catch (System.Runtime.InteropServices.COMException) { } // clipboard not accessible 
        }

        /// <summary>
        /// Copy current values formatted like displayed
        /// </summary>
        /// <param name="sender">Context menu at value display</param>
        /// <param name="e">Parameter</param>
        private void MenuItemCopyMainValuesWithUnits_Click(object sender, RoutedEventArgs e)
        {
            XY values = program.Device.Values;
            StringBuilder sb = new StringBuilder();
            sb.Append(XSymbol);
            sb.Append(" = ");
            sb.Append(XValue);
            sb.Append(" ");
            sb.Append(XUnit);

            sb.Append(Global.Delimiter);

            sb.Append(YSymbol);
            sb.Append(" = ");
            sb.Append(YValue);
            if (YUnit != "°") { sb.Append(" "); } // degrees are without space
            sb.Append(YUnit);
            try { Clipboard.SetData(DataFormats.UnicodeText, sb); }
            catch (System.Runtime.InteropServices.COMException) { } // clipboard not accessible             
        }
        
        /// <summary>
        /// Create a new file
        /// </summary>
        /// <param name="sender">New in File menu</param>
        /// <param name="e">Parameter</param>
        private void menuItemFileNew_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            dialog.Title = "Create New Log";
            dialog.ValidateNames = true;
            dialog.CreatePrompt = false;
            dialog.OverwritePrompt = false;
            if (dialog.ShowDialog() == true)
            {
                program.NewFile(dialog.FileName);
                GUIUpdate();
            }
        }

        /// <summary>
        /// Close file
        /// </summary>
        /// <param name="sender">Close in File menu</param>
        /// <param name="e">Parameter</param>
        private void menuItemFileClose_Click(object sender, RoutedEventArgs e)
        {
            program.CloseFile();
        }                                    
        
        /// <summary>
        /// Logging settings
        /// </summary>
        /// <param name="sender">Settings in File menu</param>
        /// <param name="e">Parameter</param>
        private void menuItemFileLogSettings_Click(object sender, RoutedEventArgs e)
        {
            LogSettings ls = new LogSettings(program.loggingSettings);
            LoggingSettings newLoggingSettings;
            ls.ShowDialog(out newLoggingSettings);
            program.loggingSettings = newLoggingSettings;
        }
        
        /// <summary>
        /// About window
        /// </summary>
        /// <param name="sender">About in Help menu</param>
        /// <param name="e">Parameter</param>
        private void menuItemHelpAbout_Click(object sender, RoutedEventArgs e)
        {
            aboutWindow = new About();
            aboutWindow.ShowDialog();
        }

        /// <summary>
        /// Show connection help window
        /// </summary>
        /// <param name="sender">How to connect in Help menu</param>
        /// <param name="e">Parameter</param>
        private void menuItemHelpHowToConnect_Click(object sender, RoutedEventArgs e)
        {
            howToConnectWindow = new HowToConnect();
            howToConnectWindow.ShowDialog();
        }
        
        /// <summary>
        /// Same as button - start or stop sweep
        /// Sweep can be started or stopped by pressing Space
        /// </summary>
        /// <param name="sender">Keypress on main window</param>
        /// <param name="e">Keypress details</param>
        private void startingWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                buttonSweepStartStop_Click(sender, null);
            }
        }
        
        /// <summary>
        /// Go to github repo 
        /// </summary>
        /// <param name="sender">Logo on the main window</param>
        /// <param name="e">Parameter</param>
        private void logo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo(Global.GitHubURL));
        }

        /// <summary>
        /// Clean up before exiting application
        /// </summary>
        /// <param name="sender">Main window</param>
        /// <param name="e">Parameter</param>
        private void startingWindow_Closing(object sender, CancelEventArgs e)
        {
            try { program.PortClosed -= Program_PortClosed; }
            catch (Exception) { }
            finally
            {
                program.CloseFile();
                program.Disconnect();
            }
        }

        /// <summary>
        /// Quit application
        /// </summary>
        /// <param name="sender">Exit in File menu</param>
        /// <param name="e">Parameter</param>
        private void menuItemFileExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // </EVENT HANDLERS>
    }
}
