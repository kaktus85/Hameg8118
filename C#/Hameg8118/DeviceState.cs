using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace Hameg8118
{    
    /// <summary>
    /// The base class for implementing the finite state machine (FSM) of device states
    /// </summary>
    abstract class DeviceState
    {
        internal SerialPort port; // serial port for which the FSM is implemented

        // <METHODS>

        /// <summary>
        /// Returns the default state of the FSM
        /// </summary>
        /// <param name="port">Serial port</param>
        /// <returns>Default state</returns>
        public static DeviceState GetDefaultState(SerialPort port)
        {
            return new Disconnected(port);
        }
        
        /// <summary>
        /// Disconnects from serial port 
        /// </summary>
        /// <param name="innerException">Optional parameter for referencing the exception that caused the port to close</param>
        /// <returns>New state</returns>
        public DeviceState Disconnect(Exception innerException = null)
        {            
            try
            {
                if (port.IsOpen)
                {
                    port.Close();
                }
                while (port.IsOpen) { Thread.Sleep(Global.Delay); } // wait for the port to close
                return new Disconnected(port, innerException);
            }
            catch (Exception ex)
            {
                return new Disconnected(port, ex);
            }
        }

        /// <summary>
        /// Connects to serial port
        /// </summary>
        /// <param name="portName">Serial port name ("COM#")</param>
        /// <returns>New state</returns>
        public DeviceState Connect(string portName)
        {
            Disconnect();
            try
            {
                port.PortName = portName;
                port.Open();
                while (!port.IsOpen) { Thread.Sleep(Global.Delay); } // wait for the port to open
                port.ReadExisting(); // flush port
                return new Connected(port);
            }
            catch (Exception ex)
            {
                return new Disconnected(port, ex);
            }
        }

        // </METHODS>

        // <PROPERTIES>
        // </PROPERTIES>

        // <EVENT HANDLERS>
        // </EVENT HANDLERS>
    }

    /// <summary>
    /// The base class for implementing states that can send the reset command
    /// </summary>
    abstract class ResettableState : DeviceState
    {
        // <METHODS>

        /// <summary>
        /// Reset the device
        /// </summary>
        /// <returns>New state</returns>
        public DeviceState Reset() // reset the device, thus begin from known state
        {
            try
            {
                port.WriteLine(Commands.Reset.Command());
                Thread.Sleep(2500);
                return new Ready(port);
            }
            catch (Exception ex)
            {
                return Disconnect(ex);
            }
        }

        // </METHODS>

        // <PROPERTIES>
        // </PROPERTIES>

        // <EVENT HANDLERS>
        // </EVENT HANDLERS>
    }

    /// <summary>
    /// The state with no COM port connection - the default state
    /// </summary>
    class Disconnected : DeviceState
    {
        private Exception innerException;

        // <CONSTRUCTORS>
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="port">Serial port</param>
        /// <param name="innerException">Optional reference to the exception that caused the port to disconnect</param>
        public Disconnected(SerialPort port, Exception innerException = null)
        {
            this.port = port;
            this.innerException = innerException;
        }

        // </CONSTRUCTORS>

        // <METHODS>

        /// <summary>
        /// Removes the inner exception and returns it, used to report the exception only once
        /// </summary>
        /// <returns>Inner exception</returns>
        public Exception RemoveInnerException()
        {
            Exception returnedException = innerException; // save the exception in another reference
            innerException = null; // remove the exception reference internally
            return returnedException;
        }

        // </METHODS>

        // <PROPERTIES>
        // </PROPERTIES>

        // <EVENT HANDLERS>
        // </EVENT HANDLERS>
    }
    
    /// <summary>
    /// The state when a COM port is connected but the device is of unknown type
    /// </summary>
    class Connected : DeviceState
    {
        // <CONSTRUCTORS>

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="port">Serial port</param>
        public Connected(SerialPort port)
        {
            this.port = port;
        }

        // </CONSTRUCTORS>

        // <METHODS>
        
        /// <summary>
        /// Queries the connected device to identify itself
        /// </summary>
        /// <param name="idenfification">Command for identification</param>
        /// <param name="response">Received response to identification query from the device</param>
        /// <returns>New state</returns>
        public DeviceState Identify(string idenfification, out string response)
        {            
            try
            {
                port.WriteLine(Commands.Identify.Command());
                response = port.ReadLine();
                if (response.Contains(idenfification))
                {
                    // identification succeeded
                    return new Identified(port);
                }
                else
                {
                    // identification failed
                    throw new IOException("Wrong device");
                }
            }
            catch (Exception ex)
            {
                response = null;
                return Disconnect(ex);
            }
        }

        // </METHODS>

        // <PROPERTIES>
        // </PROPERTIES>

        // <EVENT HANDLERS>
        // </EVENT HANDLERS> 
    }
    
    /// <summary>
    /// The state where the device is identified but not yet reset
    /// </summary>
    class Identified : ResettableState
    {
        // <CONSTRUCTORS>

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="port">Serial port</param>
        public Identified(SerialPort port)
        {
            this.port = port;
        }

        // </CONSTRUCTORS>

        // <METHODS>
        // </METHODS>

        // <PROPERTIES>
        // </PROPERTIES>

        // <EVENT HANDLERS>
        // </EVENT HANDLERS>
    }
    
    /// <summary>
    /// The state where the device is ready to send and receive data
    /// </summary>
    class Ready : ResettableState
    {
        // <CONSTRUCTORS>

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="port">Serial port</param>
        public Ready(SerialPort port)
        {
            this.port = port;
        }

        // </CONSTRUCTORS>

        // <METHODS>
        
        /// <summary>
        /// Send data without expecting response from device
        /// </summary>
        /// <param name="data">Data to send</param>
        /// <returns>New state</returns>
        public DeviceState Send(string data)
        {
            try
            {
                port.WriteLine(data);
                Thread.Sleep(Global.Delay);
                return this;
            }
            catch (Exception ex)
            {
                return Disconnect(ex);
            }
        }
        
        /// <summary>
        /// Send data and read response from device
        /// </summary>
        /// <param name="data">Data to send</param>
        /// <param name="response">Response from device</param>
        /// <returns>New state</returns>
        public DeviceState Query(string data, out string response)
        {
            try
            {
                port.WriteLine(data);
                response = port.ReadLine();                
                Thread.Sleep(Global.Delay);
                port.ReadExisting(); // flush buffer
                return this;
            }
            catch (Exception ex)
            {
                response = null;
                return Disconnect(ex);
            }
        }

        // </METHODS>

        // <PROPERTIES>
        // </PROPERTIES>

        // <EVENT HANDLERS>
        // </EVENT HANDLERS>
    }
}
