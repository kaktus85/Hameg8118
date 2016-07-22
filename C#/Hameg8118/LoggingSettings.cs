using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hameg8118
{
    /// <summary>
    /// Settings of the logging period and whether to include milliseconds in timestamp
    /// </summary>
    public class LoggingSettings
    {
        private double interval;
        private TimeUnits timeUnit;
        bool includeMilliseconds;

        // <CONSTRUCTORS>

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="interval">Logging interval in selected units</param>
        /// <param name="timeUnit">Time units</param>
        /// <param name="includeMilliseconds">Include milliseconds in timestamp</param>
        public LoggingSettings(double interval, TimeUnits timeUnit, bool includeMilliseconds)
        {
            if (interval > 0)
            {
                this.interval = interval;
            }
            else
            {
                this.interval = 0;
            }

            this.timeUnit = timeUnit;
            this.includeMilliseconds = includeMilliseconds;
        }

        // </CONSTRUCTORS>

        // <METHODS>

        /// <summary>
        /// Returns the logging interval in seconds
        /// </summary>
        /// <returns>Logging interval in seconds</returns>
        public double Seconds()
        {
            switch (timeUnit)
            {
                case TimeUnits.ms:
                    return interval / 1000;
                case TimeUnits.s:
                    return interval;
                case TimeUnits.min:
                    return interval * 60;
                default:
                    throw new InvalidOperationException("Unknown time units");
            }
        }

        // </METHODS>

        // <PROPERTIES>

        /// <summary>
        /// Gets the logging interval in selected units
        /// </summary>
        public double Interval
        {
            get
            {
                return interval;
            }
        }

        /// <summary>
        /// Gets the time unit
        /// </summary>
        public TimeUnits TimeUnit
        {
            get
            {
                return timeUnit;
            }
        }

        /// <summary>
        /// Indicates whether to include milliseconds in timestamp
        /// </summary>
        public bool IncludeMilliseconds
        {
            get
            {
                return includeMilliseconds;
            }
        }

        // </PROPERTIES>

        // <EVENT HANDLERS>
        // </EVENT HANDLERS>
    }
}
