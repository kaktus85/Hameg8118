using System;

namespace Hameg8118
{
    // <DELEGATES>

    public delegate void QueueEmpty();
    public delegate void DeviceIdentified(string identificationDetails);
    public delegate void DeviceReady();
    public delegate void PortClosed(Exception innerException);
    public delegate void DataUpdated(Transaction transaction);
    public delegate void Update(bool status);
    public delegate void NewValue(Device device);
    public delegate void GUIDelegate(); // for updating GUI from different thread

    // </DELEGATES>

    // <ENUMS>

    public enum Mode { Auto, LQ, LR, CD, CR, RQ, ZTheta, YTheta, RX, GB/*, NTheta, M*/ }; // PMOD    
    public enum Trigger { Continuous, Manual }; // MMOD
    public enum Model { Series, Parallel, Auto }; // CIRC
    public enum Averaging { None, Medium = 2 }; // AVGM
    public enum Speed { Fast, Medium, Slow }; // RATE
    public enum Compensate { SingleFrequency, AllFrequencies }; // CALL   
    public enum TimeUnits { ms, s, min };
    public enum Commands { Identify, Reset, Ready, Averaging, Model, Mode, Frequency, Voltage, Speed, Trigger, Values, MeasureSingle, SetCompensate, CompensateOpen, CompensateShort, Wait, BiasMode, BiasVoltage, BiasCurrent, ConstantVoltage };
    public enum TriggerStates { Undefined, Manual, Continuous, Sweep };
    public enum Response { NoResponse, ExpectsResponse };
    public enum BiasMode { Off = 0, Internal = 1, External = 2 }; //BIAS
    public enum ConstantVoltage { Off = 0, On = 1 }; //CONV

    // </ENUMS>

    /// <summary>
    /// Global constants
    /// </summary>
    class Global
    {
        public const string Identification = "HM8118";

        public const string GitHubURL = "https://github.com/kaktus85/Hameg8118"; // URL of GitHub repository of this project

        public const string NumberFormat = "G6";
        public const string VoltageNumberFormat = "F2";
        public const string CurrentNumberFormat = "F3";
        public const char Delimiter = '\t'; // delimiter used in exported data       

        // serial port settings, specific to HM8118 parameters        
        public const int ShortReadTimeout = 10000; // 10 seconds, used for normal communication
        public const int LongReadTimeout = 180000; // 3 minutes, used for compensations
        public const System.IO.Ports.Parity Parity = System.IO.Ports.Parity.None;
        public const int WriteTimeout = 10000;
        public const int BaudRate = 9600;
        public const int DataBits = 8;
        public const System.IO.Ports.StopBits StopBits = System.IO.Ports.StopBits.One;
        public const string NewLine = "\r";
        public const int Delay = 15; // delay in milliseconds for asynchronous serial port operations                

        // HM8118 list of available frequencies
        public static readonly int[] Frequencies = new int[]{ 20, 24, 25, 30, 36, 40, 45, 50, 60, 72, 75, 80,
                                                              90, 100, 120, 150, 180, 200, 240, 250, 300, 360, 400, 450,
                                                              500, 600, 720, 750, 800, 900, 1000, 1200, 1500, 1800, 2000, 2400,
                                                              2500, 3000, 3600, 4000, 4500, 5000, 6000, 7200, 7500, 8000, 9000, 10000,
                                                              12000, 15000, 18000, 20000, 24000, 25000, 30000, 36000, 40000, 45000, 50000, 60000,
                                                              72000, 75000, 80000, 90000, 100000, 120000, 150000, 180000, 200000};
    }
}
