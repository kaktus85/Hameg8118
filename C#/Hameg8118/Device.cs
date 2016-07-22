using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Hameg8118
{
    /// <summary>
    /// A class for keeping the parameters of the connected device, including current measurement values and settings
    /// </summary>
    public class Device : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged; // occurs when property is changed, inherited from interface
                                                                    
        private XY values; // current values as on the display of HM8118
        private string deviceInfo; // response to *IDN? query
        
        // HM8118 settings
        private Mode mode;
        private int frequencyIndex;
        private double voltage;
        private Trigger trigger;
        private Model model;
        private Averaging averaging;
        private Speed speed;

        // <CONSTRUCTORS>
        // </CONSTRUCTORS>

        // <METHODS>

        /// <summary>
        /// This method should be called when a property is changed
        /// </summary>
        /// <param name="caller">Name of the calling property</param>
        private void NotifyPropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(caller)); }
        }

        /// <summary>
        /// Overrides default ToString method
        /// </summary>
        /// <returns>Returns device info from *IDN? query</returns>
        public override string ToString()
        {
            return deviceInfo;
        }

        // </METHODS>

        // <PROPERTIES>

        /// <summary>
        /// Gets physical unit of X value
        /// </summary>
        public string XUnit
        {
            get
            {
                switch (mode)
                {
                    case Mode.LQ:
                    case Mode.LR:
                        return "H";
                    case Mode.CD:
                    case Mode.CR:
                        return "F";
                    case Mode.RQ:
                    case Mode.RX:
                    case Mode.ZTheta:
                        return "Ω";
                    case Mode.YTheta:
                    case Mode.GB:
                        return "S";
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets physical unit of Y value
        /// </summary>
        public string YUnit
        {
            get
            {
                switch (mode)
                {
                    case Mode.LR:
                    case Mode.CR:
                    case Mode.RX:
                        return "Ω";
                    case Mode.ZTheta:
                    case Mode.YTheta:
                        return "°";
                    case Mode.GB:
                        return "S";
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets symbol of X value
        /// </summary>
        public string XSymbol
        {
            get
            {
                switch (mode)
                {
                    case Mode.LQ:
                    case Mode.LR:
                        if (model == Model.Series) { return "Ls"; }
                        else { return "Lp"; }
                    case Mode.CD:
                    case Mode.CR:
                        if (model == Model.Series) { return "Cs"; }
                        else { return "Cp"; }
                    case Mode.RQ:
                        if (model == Model.Series) { return "Rs"; }
                        else { return "Rp"; }
                    case Mode.ZTheta:
                        return "Z";
                    case Mode.YTheta:
                        return "Y";
                    case Mode.RX:
                        return "R";
                    case Mode.GB:
                        return "G";
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        ///  Gets symbol of Y value
        /// </summary>
        public string YSymbol
        {
            get
            {
                switch (mode)
                {
                    case Mode.LQ:
                    case Mode.RQ:
                        return "Q";
                    case Mode.LR:
                    case Mode.CR:
                        if (model == Model.Series) { return "Rs"; }
                        else { return "Rp"; }
                    case Mode.CD:
                        return "D";
                    case Mode.ZTheta:
                    case Mode.YTheta:
                        return "Θ";
                    case Mode.RX:
                        return "X";
                    case Mode.GB:
                        return "B";
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets or sets the measured values X and Y
        /// </summary>
        public XY Values
        {
            get
            {
                return values;
            }
            set
            {
                values = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the device info - should be the response to *IDN? query
        /// </summary>
        public string DeviceInfo
        {
            get
            {
                return deviceInfo;
            }
            set
            {
                deviceInfo = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets measurement mode
        /// </summary>
        public Mode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets frequency index (not frequency in Hz!)
        /// </summary>
        public int FrequencyIndex
        {
            get
            {
                return frequencyIndex;
            }
            set
            {
                frequencyIndex = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets measurement voltage (RMS)
        /// </summary>
        public double Voltage // Vrms
        {
            get
            {
                return voltage;
            }
            set
            {
                voltage = value;
                NotifyPropertyChanged();
            }
        }


        /// <summary>
        /// Gets or sets trigger
        /// </summary>
        public Trigger Trigger
        {
            get
            {
                return trigger;
            }
            set
            {
                trigger = value;
                NotifyPropertyChanged();
            }
        }


        /// <summary>
        /// Gets or sets equivalent circuit (model)
        /// </summary>
        public Model Model
        {
            get
            {
                return model;
            }
            set
            {
                model = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets averaging
        /// </summary>
        public Averaging Averaging
        {
            get
            {
                return averaging;
            }
            set
            {
                averaging = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets measurement speed (rate)
        /// </summary>
        public Speed Speed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
                NotifyPropertyChanged();
            }
        }

        // </PROPERTIES>

        // <EVENT HANDLERS>
        // </EVENT HANDLERS>                     
    }
}
