namespace Hameg8118
{
    /// <summary>
    /// Extensions for enumerations
    /// </summary>
    static class EnumExtensions
    {
        /// <summary>
        /// Returns a string representation of Mode enumeration
        /// </summary>
        /// <param name="mode">Mode</param>
        /// <returns>String representation of Mode enumeration</returns>
        public static string Name(this Mode mode)
        {
            switch (mode)
            {
                case Mode.Auto: return "Auto";
                case Mode.LQ: return "L-Q";
                case Mode.LR: return "L-R";
                case Mode.CD: return "C-D";
                case Mode.CR: return "C-R";
                case Mode.RQ: return "R-Q";
                case Mode.ZTheta: return "Z-Θ";
                case Mode.YTheta: return "Y-Θ";
                case Mode.RX: return "R-X";
                case Mode.GB: return "G-B";

                default: return string.Empty;
            }
        }

        /// <summary>
        /// Returns a string representation of BiasMode enumeration
        /// </summary>
        /// <param name="biasMode">Mode</param>
        /// <returns>String representation of Mode enumeration</returns>
        public static string Name(this BiasMode biasMode)
        {
            switch (biasMode)
            {
                case BiasMode.Off: return "Off";
                case BiasMode.Internal: return "Internal";
                case BiasMode.External: return "External";
                default: return string.Empty;
            }
        }

        /// <summary>
        /// Returns a string representation of ConstantVoltage enumeration
        /// </summary>
        /// <param name="constantVoltage">Mode</param>
        /// <returns>String representation of Mode enumeration</returns>
        public static string Name(this ConstantVoltage constantVoltage)
        {
            switch (constantVoltage)
            {
                case ConstantVoltage.Off: return "Off";
                case ConstantVoltage.On: return "On";
                default: return string.Empty;
            }
        }

        /// <summary>
        /// Returns command that has to be sent over serial port to the device
        /// </summary>
        /// <param name="command">Command</param>
        /// <returns>Command to device</returns>
        public static string Command(this Commands command)
        {
            switch (command)
            {
                case Commands.Identify: return "*IDN?";
                case Commands.Reset: return "*RST";
                case Commands.Ready: return "*OPC?";
                case Commands.Averaging: return "AVGM";
                case Commands.Model: return "CIRC";
                case Commands.Mode: return "PMOD";
                case Commands.Frequency: return "FREQ";
                case Commands.Voltage: return "VOLT";
                case Commands.Speed: return "RATE";
                case Commands.Trigger: return "MMOD";
                case Commands.Values: return "XALL?";
                case Commands.MeasureSingle: return "*TRG";
                case Commands.SetCompensate: return "CALL";
                case Commands.CompensateOpen: return "CROP";
                case Commands.CompensateShort: return "CRSH";
                case Commands.Wait: return "*WAI";
                case Commands.BiasMode: return "BIAS";
                case Commands.BiasVoltage: return "VBIA";
                case Commands.BiasCurrent: return "IBIA";
                case Commands.ConstantVoltage: return "CONV";
                default: return string.Empty;
            }
        }
    }
}