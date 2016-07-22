using System;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Hameg8118
{
    /// <summary>
    /// Core program logic, a singleton
    /// </summary>
    class Program : INotifyPropertyChanged
    {                
        static Program program = new Program(); // singleton

        private COM port;
        private File file;
        private Device device;
        public LoggingSettings loggingSettings;

        // events
        public event PortClosed PortClosed; // occurs when there is an error which results to port close                                            
        public event NewValue NewValues; // occurs when new measured values become available
        public event Update CompensationFinished; // occurs when attempt at compensation has failed
        public event PropertyChangedEventHandler PropertyChanged; // occurs when property is changed, inherited from INotifyPropertyChanged interface

        // finite state machine of trigger state
        private TriggerStates triggerState = TriggerStates.Undefined;
        
        // sweep
        private int? repeats;
        private int frequencyIndexBeforeSweep; // save previous setting before going to sweep so it can be reloaded after sweep finishes
        private TriggerStates triggerStateBeforeSweep = TriggerStates.Continuous; // save previous setting before going to sweep so it can be reloaded after sweep finishes

        // <CONSTRUCTORS>

        /// <summary>
        /// Constructor
        /// </summary>
        private Program()
        {
            // serial port
            port = new COM();            
            port.PortClosed += Port_PortClosed; ; // pass port closed event
            port.DeviceIdentified += Port_AssignDeviceInfo;
            port.DeviceReady += Port_MeasurementModeInitialize;
            port.DataUpdated += Port_DataUpdated;
            CompensationFinished += (bool b) => port.ClearQueue();

            device = new Device();

            // logging
            LogManualAndSweep = true;
            NewValues += WriteToFile;
            loggingSettings = new LoggingSettings(1, TimeUnits.s, false); // default logging interval
        }
        // </CONSTRUCTORS>

        // <STATIC METHODS>
        
        /// <summary>
        /// Returns one instance of program (singleton)
        /// </summary>
        /// <returns>Reference to Program instance</returns>
        public static Program GetInstance()
        {
            return program;
        }

        // </STATIC METHODS>

        // <INSTANCE METHODS>

        /// <summary>
        /// This method should be called when a property is changed
        /// </summary>
        /// <param name="caller">Name of the calling property</param>
        private void NotifyPropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(caller)); }
        }
        
        /// <summary>
        /// Connect to device on selected serial port
        /// </summary>
        /// <param name="portName">Serial port name</param>
        public void Connect(string portName)
        {            
            TriggerState = TriggerStates.Undefined;
            port.Connect(portName);                               
        }        
        
        /// <summary>
        /// Disconnect from device
        /// </summary>
        public void Disconnect()
        {
            PortClosed = null;
            if (CompensationFinished != null)
            {
                CompensationFinished(false);
            }            
            TriggerState = TriggerStates.Undefined;
            port.Disconnect();
        }
        
        /// <summary>
        /// Reset device to default state
        /// </summary>
        public void Reset()
        {
            port.Reset();            
        }
        
        /// <summary>
        /// Queries all settings from device
        /// </summary>
        private void QuerySettings()
        {
            port.Query(new Transaction(Commands.Mode));
            port.Query(new Transaction(Commands.Frequency));
            port.Query(new Transaction(Commands.Voltage));
            port.Query(new Transaction(Commands.Trigger));
            port.Query(new Transaction(Commands.Model));            
            port.Query(new Transaction(Commands.Averaging));
            port.Query(new Transaction(Commands.Speed));
        }
        
        /// <summary>
        /// Queries both measurement values from device
        /// </summary>
        private void QueryValues()
        {
            port.Send(new Transaction(Commands.Wait));
            port.Query(new Transaction(Commands.Values));
        }      
        
        /// <summary>
        /// Sets new trigger
        /// </summary>
        /// <param name="trigger">Trigger</param>
        public void SetTrigger(Trigger trigger)
        {
            if (trigger == Trigger.Manual)
            {
                TriggerState = TriggerStates.Manual;
            }
            else
            {
                TriggerState = TriggerStates.Continuous;
            }            
        }
        
        /// <summary>
        /// Sets new model
        /// </summary>
        /// <param name="model">Model</param>
        public void SetModel(Model model)
        {
            Set(Commands.Model, (int)model);
        }
        
        /// <summary>
        /// Sets new averaging
        /// </summary>
        /// <param name="averaging">Averaging</param>
        public void SetAveraging(Averaging averaging)
        {
            Set(Commands.Averaging, (int)averaging);
        }
        
        /// <summary>
        /// Sets new measurement speed
        /// </summary>
        /// <param name="speed">Speed</param>
        public void SetSpeed(Speed speed)
        {
            Set(Commands.Speed, (int)speed);
        }        
        
        /// <summary>
        /// Trigger a single measurement and get the new measured values
        /// </summary>
        public void MeasureSingle()
        {
            if (device.Trigger == Trigger.Continuous) // switch to manual trigger if not selected
            {
                TriggerState = TriggerStates.Manual;                
            }            
            port.Send(new Transaction(Commands.MeasureSingle));
            QueryValues();
        }
        
        /// <summary>
        /// Perform open compensation 
        /// </summary>
        /// <param name="compensate">Compensate single frequency or all frequencies</param>
        public void CompensateOpen(Compensate compensate)
        {
            port.Send(new Transaction(Commands.SetCompensate, ((int)compensate).ToString()));
            port.Query(new Transaction(Commands.CompensateOpen));            
        }

        /// <summary>
        /// Perform short compensation 
        /// </summary>
        /// <param name="compensate">Compensate single frequency or all frequencies</param>
        public void CompensateShort(Compensate compensate)
        {            
            port.Send(new Transaction(Commands.SetCompensate, ((int)compensate).ToString()));            
            port.Query(new Transaction(Commands.CompensateShort));
        }
        
        /// <summary>
        /// Set new voltage level for the test signal
        /// </summary>
        /// <param name="voltage">Voltage in Vrms</param>
        public void SetVoltage(double voltage)
        {
            if ((voltage >= 0.05) && (voltage <= 1.5)) // validate values
            {
                Set(Commands.Voltage, voltage.ToString(CultureInfo.InvariantCulture));
            }            
        }
        
        /// <summary>
        /// Set new mode
        /// </summary>
        /// <param name="mode">Mode</param>
        public void SetMode(Mode mode)
        {
            Set(Commands.Mode, (int)mode);
        }
        
        /// <summary>
        /// Sets new frequency from list of available frequencies
        /// </summary>
        /// <param name="frequencyIndex">Index of the frequency item in the list of frequencies</param>
        public void SetFrequency(int frequencyIndex)
        {
            if (frequencyIndex >= 0)
            {
                Set(Commands.Frequency, Global.Frequencies[frequencyIndex]);
            }            
        }

        /// <summary>
        /// Generic method that sets new setting (command + parameter) and queries back if the new setting took effect
        /// The method removes all unprocessed requests before sending the new one
        /// </summary>
        /// <param name="command">Command</param>
        /// <param name="parameter">Setting parameter (value)</param>
        private void Set(Commands command, object parameter)
        {            
            port.ClearQueue();            
            port.Send(new Transaction(command, parameter.ToString()));
            port.Query(new Transaction(command));            
            QuerySettings();            
        }
        
        /// <summary>
        /// Set up the system for sweep
        /// </summary>
        /// <param name="startFrequencyIndex">Starting frequency index</param>
        /// <param name="endFrequencyIndex">Final frequency index</param>
        /// <param name="repeats">Number of repeats of the whole sweep, repeat indefinitely if null</param>
        public void SetupSweep(int startFrequencyIndex, int endFrequencyIndex, int? repeats)
        {
            if ((startFrequencyIndex >= 0) && (endFrequencyIndex >= 0))
            {
                if (TriggerState == TriggerStates.Sweep) // sweep already running, cancel it and run new sweep
                {
                    CancelSweep();
                }
                
                triggerStateBeforeSweep = TriggerState; // save device state

                TriggerState = TriggerStates.Sweep;

                if (TriggerState == TriggerStates.Sweep)
                {               
                    RemainingRepeats = repeats;
                    frequencyIndexBeforeSweep = device.FrequencyIndex; // save the currently selected frequency
                    RunSweep(startFrequencyIndex, endFrequencyIndex);
                    port.TransactionQueueEmpty += () => CleanAfterSweep(startFrequencyIndex, endFrequencyIndex);
                }                
            }
        }

        /// <summary>
        /// Cancel sweep and return to previous state
        /// </summary>
        public void CancelSweep()
        {
            TriggerState = triggerStateBeforeSweep;
        }

        /// <summary>
        /// Run a single sweep
        /// </summary>
        /// <param name="startFrequencyIndex">Starting frequency index</param>
        /// <param name="endFrequencyIndex">Final frequency index</param>
        private void RunSweep(int startFrequencyIndex, int endFrequencyIndex)
        {
            if ((RemainingRepeats > 0) || (RemainingRepeats == null))
            {                       
                if (startFrequencyIndex < endFrequencyIndex) // from lower frequency to higher
                {
                    for (int i = startFrequencyIndex; i <= endFrequencyIndex; i++)
                    {
                        port.Send(new Transaction(Commands.Frequency, Global.Frequencies[i].ToString()));
                        port.Send(new Transaction(Commands.MeasureSingle));
                        port.Send(new Transaction(Commands.Wait));
                        port.Query(new Transaction(Commands.Frequency));
                        port.Query(new Transaction(Commands.Values));
                    }
                }
                else
                {
                    for (int i = startFrequencyIndex; i >= endFrequencyIndex; i--) // from higher frequency to lower
                    {
                        port.Send(new Transaction(Commands.Frequency, Global.Frequencies[i].ToString()));
                        port.Send(new Transaction(Commands.MeasureSingle));
                        port.Send(new Transaction(Commands.Wait));
                        port.Query(new Transaction(Commands.Frequency));
                        port.Query(new Transaction(Commands.Values));
                    }
                }
            }
        }

        /// <summary>
        /// Restore previous settings after exiting from sweep 
        /// </summary>
        /// <param name="startFrequencyIndex">Starting frequency index</param>
        /// <param name="endFrequencyIndex">Final frequency index</param>
        private void CleanAfterSweep(int startFrequencyIndex, int endFrequencyIndex)
        {            
            if (RemainingRepeats != null)
            {
                RemainingRepeats--;
            }

            if ((RemainingRepeats <= 0) || (TriggerState != TriggerStates.Sweep))
            {
                CancelSweep();
                RemainingRepeats = 0;                
                port.Send(new Transaction(Commands.Frequency, Global.Frequencies[frequencyIndexBeforeSweep].ToString()));
                port.Query(new Transaction(Commands.Frequency));                                
            }
            else
            {
                RunSweep(startFrequencyIndex, endFrequencyIndex);
            }
        }        
        
        /// <summary>
        /// Create new file
        /// </summary>
        /// <param name="path">File path</param>
        public void NewFile(string path)
        {
            CloseFile();
            file = new File(path, device.DeviceInfo);
            NotifyPropertyChanged(nameof(FilePath));
        }
        
        /// <summary>
        /// Close current file
        /// </summary>
        public void CloseFile()
        {
            if (file != null)
            {
                file.Close();
            }
            NotifyPropertyChanged(nameof(FilePath));
        }
        
        /// <summary>
        /// Write a line of data to file
        /// </summary>
        /// <param name="device">Device parameters and measured values</param>
        private void WriteToFile(Device device)
        {
            if (file != null)
            {
                if ((LogContinuous && (Device.Trigger == Trigger.Continuous)) || (LogManualAndSweep && (Device.Trigger == Trigger.Manual)))
                {
                    file.WriteData(device, loggingSettings);
                }
            }
        }

        // </INSTANCE METHODS>

        // <PROPERTIES>
        
        /// <summary>
        /// Gets serial port name
        /// </summary>
        public string PortName
        {
            get
            {
                return port.PortName;
            }
        }
        
        /// <summary>
        /// Gets current device settings
        /// </summary>
        public Device Device
        {
            get { return device; }
        }
        
        /// <summary>
        /// Gets the device state (FSM)
        /// </summary>
        public DeviceState State
        {
            get { return port.State; }
        }
        
        /// <summary>
        /// Gets whether a frequency sweep is going
        /// </summary>
        public bool Sweep
        {
            get
            {
                return (TriggerState == TriggerStates.Sweep);
            }
        }
        
        /// <summary>
        /// Gets the number of remaining repeats or sets the number of repeats
        /// </summary>
        public int? RemainingRepeats
        {
            get
            {
                return repeats;
            }
            private set
            {
                repeats = value;
                NotifyPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets log file path
        /// </summary>
        public string FilePath
        {
            get
            {
                if (file != null)
                {
                    return file.FilePath;
                }
                return null;
            }            
        }
        
        /// <summary>
        /// Gets or sets logging of manually triggered measurements and sweeps
        /// </summary>
        public bool LogManualAndSweep { get; set; }
        
        /// <summary>
        /// Gets or sets logging of continuous measurements
        /// </summary>
        public bool LogContinuous { get; set; }
        
        /// <summary>
        /// Gets or sets the trigger state
        /// </summary>
        public TriggerStates TriggerState
        {
            get
            {
                if ((port.State is Ready) == false)
                {
                    TriggerState = triggerState; // run the set procedure to update trigger to undefined state if not ready
                }                
                return triggerState;
            }
            private set
            {                
                if (port.State is Ready)
                {
                    triggerState = value;
                }
                else
                {
                    triggerState = TriggerStates.Undefined; // undefined if not ready
                }

                port.TransactionQueueEmptyClear(); // clear the invocation list of transaction queue empty event
                port.ClearQueue(); // clear transaction queue

                switch (triggerState) // manage triggers and invocation list of transaction queue empty event
                {
                    case TriggerStates.Undefined:
                        repeats = 0;               
                        break;
                    case TriggerStates.Manual:
                        repeats = 0;
                        port.Send(new Transaction(Commands.Trigger, ((int)(Trigger.Manual)).ToString()));
                        port.Query(new Transaction(Commands.Trigger));
                        port.TransactionQueueEmpty += QuerySettings;
                        break;
                    case TriggerStates.Continuous:
                        repeats = 0;
                        port.Send(new Transaction(Commands.Trigger, ((int)(Trigger.Continuous)).ToString()));
                        port.Query(new Transaction(Commands.Trigger));
                        port.TransactionQueueEmpty += QuerySettings;
                        port.TransactionQueueEmpty += QueryValues;
                        break;
                    case TriggerStates.Sweep:
                        port.Send(new Transaction(Commands.Trigger, ((int)(Trigger.Manual)).ToString()));
                        port.Query(new Transaction(Commands.Trigger));
                        NotifyPropertyChanged(nameof(Sweep));
                        break;
                    default:
                        break;
                }
            }
        }
        // </PROPERTIES>

        // <EVENT HANDLERS>

        /// <summary>
        /// Pass port closed event
        /// </summary>
        /// <param name="innerException"></param>
        private void Port_PortClosed(Exception innerException)
        {
            if (PortClosed != null)
            {
                PortClosed(innerException);
            }
        }

        /// <summary>
        /// Handles when data arrives at serial port
        /// </summary>
        /// <param name="transaction">Transaction that has been completed</param>
        private void Port_DataUpdated(Transaction transaction)
        {
            switch (transaction.Command)
            {                
                case Commands.Averaging:
                    {
                        try { device.Averaging = (Averaging)(int.Parse(transaction.Response)); }
                        catch (Exception) { Reset(); }
                        break;
                    }
                case Commands.Frequency:
                    {
                        try { device.FrequencyIndex = Array.IndexOf(Global.Frequencies, int.Parse(transaction.Response)); }
                        catch (Exception) { Reset(); }
                        break;
                    }
                case Commands.Mode:
                    {
                        try
                        { device.Mode = (Mode)(int.Parse(transaction.Response)); }
                        catch (Exception) { Reset(); }
                        break;
                    }
                case Commands.Model:
                    {
                        try { device.Model = (Model)(int.Parse(transaction.Response)); }
                        catch (Exception) { Reset(); }
                        break;
                    }
                case Commands.Speed:
                    {
                        try { device.Speed = (Speed)(int.Parse(transaction.Response)); }
                        catch (Exception) { Reset(); }
                        break;
                    }
                case Commands.Trigger:
                    {
                        try { device.Trigger = (Trigger)(int.Parse(transaction.Response)); }
                        catch (Exception) { Reset(); }
                        break;
                    }
                case Commands.Voltage:
                    {
                        try { device.Voltage = double.Parse(transaction.Response, CultureInfo.InvariantCulture);}
                        catch (Exception) { Reset(); }
                        break;
                    }
                case Commands.Values:
                    {
                        try
                        {
                            string[] values = transaction.Response.Split(','); // HM8118 sends comma separated values
                            XY parsedValues = new XY();
                            parsedValues.X = double.Parse(values[0], CultureInfo.InvariantCulture); // HM8118 sends decimal point
                            parsedValues.Y = double.Parse(values[1], CultureInfo.InvariantCulture);
                            device.Values = parsedValues;

                            if (NewValues != null) // raise event
                            {
                                NewValues(device);
                            }
                        }
                        catch (Exception) { Reset(); }
                        break;
                    }
                case Commands.CompensateOpen:
                case Commands.CompensateShort:
                    {
                        if (CompensationFinished != null)
                        {
                            CompensationFinished(transaction.Response.Contains("0")); // 0 indicates succesful compensation, -1 indicates failure
                        }
                        port.Flush();         
                        break;
                    }                
            }
        }

        /// <summary>
        /// Assigns device info - a string representation of the device 
        /// </summary>
        /// <param name="deviceInfo">Device info to assign</param>
        private void Port_AssignDeviceInfo(string deviceInfo)
        {
            device.DeviceInfo = deviceInfo;
        }

        /// <summary>
        /// Initializes MeasurementMode after device transfers to ready state
        /// </summary>
        private void Port_MeasurementModeInitialize()
        {
            TriggerState = TriggerStates.Continuous;
            port.Send(new Transaction(Commands.Mode, ((int)Mode.ZTheta).ToString())); // set default mode Z-θ
            port.Query(new Transaction(Commands.Mode));
        }

        // </EVENT HANDLERS>
    }
}
