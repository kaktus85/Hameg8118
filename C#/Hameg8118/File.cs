using System;
using System.Text;
using System.IO;

namespace Hameg8118
{
    /// <summary>
    /// Log file creation and writing
    /// </summary>
    class File
    {
        private StreamWriter file;
        private string filePath;
        private StringBuilder stringBuilder; // for quicker creation of strings from elements
        private DateTime lastLog = DateTime.MinValue; // datetime of the last log to file
        private double lastLogDeadTime; /* Time difference between last two logs and the logging period. 
                                           While the logging period can be, for example, 1 second, the actual logging intervals will be longer.
                                           To compensate for this, the next logging interval is shortened by the value the previous logging interval was longer.
                                           This keeps the overall data rate as defined in logging settings. */

        // <CONSTRUCTORS>
        
        /// <summary>
        /// Create new file and write header
        /// </summary>
        /// <param name="filePath">Path to the file in filesystem</param>
        /// <param name="deviceInfo">Device identification info (should be from *IDN? query)</param>
        public File(string filePath, string deviceInfo)
        {            
            this.filePath = filePath;
            file = new StreamWriter(filePath, true, new UTF8Encoding());            
            file.AutoFlush = true; // write data immediately to file to prevent data loss
            stringBuilder = new StringBuilder();

            // first line - device identification
            stringBuilder.Append("Hameg HM8118");
            stringBuilder.Append(Global.Delimiter);
            stringBuilder.Append(deviceInfo);
            file.WriteLine(stringBuilder.ToString());

            // second line - headers for values
            stringBuilder.Clear();
            stringBuilder.Append("Date and time");
            stringBuilder.Append(Global.Delimiter);
            stringBuilder.Append("X value");
            stringBuilder.Append(Global.Delimiter);
            stringBuilder.Append("Y value");
            stringBuilder.Append(Global.Delimiter);
            stringBuilder.Append("X symbol");
            stringBuilder.Append(Global.Delimiter);
            stringBuilder.Append("Y symbol");
            stringBuilder.Append(Global.Delimiter);
            stringBuilder.Append("X unit");
            stringBuilder.Append(Global.Delimiter);
            stringBuilder.Append("Y unit");
            stringBuilder.Append(Global.Delimiter);
            stringBuilder.Append("Frequency, Hz");
            stringBuilder.Append(Global.Delimiter);
            stringBuilder.Append("Test voltage, Vrms");
            stringBuilder.Append(Global.Delimiter);
            stringBuilder.Append("Mode");
            stringBuilder.Append(Global.Delimiter);
            stringBuilder.Append("Model");
            stringBuilder.Append(Global.Delimiter);
            stringBuilder.Append("Averaging");
            stringBuilder.Append(Global.Delimiter);
            stringBuilder.Append("Measurement speed");
            stringBuilder.Append(Global.Delimiter);
            stringBuilder.Append("Trigger");
            stringBuilder.Append(Global.Delimiter);

            file.WriteLine(stringBuilder.ToString());
        }

        // </CONSTRUCTORS>

        // <METHODS>
        
        /// <summary>
        /// Closes the file
        /// </summary>
        public void Close()
        {          
            try
            {
                file.Close();
            }  
            catch (Exception) { }
            finally
            {
                filePath = null;
            }            
        }
        
        /// <summary>
        /// Writes a single line of data to the file
        /// </summary>
        /// <param name="device">Device with current parameters and values</param>
        /// <param name="loggingSettings">Settings that affect logging period and writing milliseconds in timestamps</param>
        public void WriteData(Device device, LoggingSettings loggingSettings)
        {
            if ((device.Values.X == null) && (device.Values.Y == null)) // if both values are not measured (null), then do not write anything
            {
                return;
            }

            DateTime now = DateTime.Now;
            if ((now - lastLog).TotalSeconds >= (loggingSettings.Seconds() - lastLogDeadTime)) // only log if the logging period - the dead time is lower than the period of time passed from the last log
            {
                lastLogDeadTime = (now - lastLog).TotalSeconds - loggingSettings.Seconds(); // calculate new dead time
                if (lastLogDeadTime < 0) // only shorten the time
                {
                    lastLogDeadTime = 0;
                }
                lastLog = now;

                stringBuilder.Clear();
                // date and time
                stringBuilder.Append(now.ToShortDateString());
                stringBuilder.Append(" ");
                stringBuilder.Append(now.ToLongTimeString());                
                if (loggingSettings.IncludeMilliseconds)
                {
                    stringBuilder.Append(".");
                    stringBuilder.Append(string.Format("{0,0:D3}", now.Millisecond)); // append milliseconds with leading zeroes
                }     
                stringBuilder.Append(Global.Delimiter);
                // x value
                if (device.Values.X != null)
                {
                    stringBuilder.Append(((double)(device.Values.X)).ToString(Global.NumberFormat));
                }
                else
                {
                    stringBuilder.Append("N/A");
                }
                stringBuilder.Append(Global.Delimiter);
                // y value
                if (device.Values.Y != null)
                {
                    stringBuilder.Append(((double)(device.Values.Y)).ToString(Global.NumberFormat));
                }
                else
                {
                    stringBuilder.Append("N/A");
                }
                // x symbol
                stringBuilder.Append(device.XSymbol);
                stringBuilder.Append(Global.Delimiter);
                // y symbol
                stringBuilder.Append(device.YSymbol);
                stringBuilder.Append(Global.Delimiter);
                // x unit
                stringBuilder.Append(device.XUnit);
                stringBuilder.Append(Global.Delimiter);
                // y unit
                stringBuilder.Append(device.YUnit);
                stringBuilder.Append(Global.Delimiter);
                // frequency
                stringBuilder.Append(Global.Frequencies[device.FrequencyIndex]);
                stringBuilder.Append(Global.Delimiter);
                // voltage
                stringBuilder.Append(device.Voltage);
                stringBuilder.Append(Global.Delimiter);
                // mode
                stringBuilder.Append(device.Mode.Name());
                stringBuilder.Append(Global.Delimiter);
                // model
                stringBuilder.Append(device.Model);
                stringBuilder.Append(Global.Delimiter);
                // averaging
                stringBuilder.Append(device.Averaging);
                stringBuilder.Append(Global.Delimiter);
                // measurement speed
                stringBuilder.Append(device.Speed);
                stringBuilder.Append(Global.Delimiter);
                // trigger
                stringBuilder.Append(device.Trigger);
                stringBuilder.Append(Global.Delimiter);

                file.WriteLine(stringBuilder.ToString());
            }
        }

        // </METHODS>

        // <PROPERTIES>
                
        /// <summary>
        /// Gets the file path
        /// </summary>
        public string FilePath
        {
            get
            {
                return filePath;
            }
        }

        // </PROPERTIES>

        // <EVENT HANDLERS>
        // </EVENT HANDLERS>
    }
}
