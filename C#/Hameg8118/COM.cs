using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.ComponentModel;
using System.Threading;

namespace Hameg8118
{
    /// <summary>
    /// Manages connection to HM8118 via serial port
    /// </summary>
    class COM
    {
        // serial port
        private SerialPort port;
        private int readTimeout = Global.ShortReadTimeout;

        // asynchronous transaction processing
        private BackgroundWorker comWorker = new BackgroundWorker();
        private Queue<Transaction> transactionsQueue = new Queue<Transaction>();
        private Transaction lastTransaction;

        // events
        public event DataUpdated DataUpdated; // occurs when new data from HM8118 has been processed
        public event QueueEmpty TransactionQueueEmpty; // occurs when the last transaction has been processed
        public event DeviceIdentified DeviceIdentified; // occurs when there is a change in identification of the device (both from unidentified to identified and vice versa)
        public event DeviceReady DeviceReady; // occurs after device has been identified and reset
        public event PortClosed PortClosed; // occurs when the serial port has been closed

        // finite state machine
        private DeviceState state; // the actual state
        private DeviceState newState; // the state that the device should be changed to via some methods

        // flags for asynchronous processing and temporary portname
        private bool connect;
        private bool disconnect;
        private bool reset;
        private string tempPortName;


        // <CONSTRUCTORS>

        /// <summary>
        /// Initializes port but does not open it
        /// </summary>
        public COM()
        {
            // serial port
            port = new SerialPort();
            port.Parity = Global.Parity;
            port.StopBits = Global.StopBits;
            port.ReadTimeout = readTimeout;
            port.WriteTimeout = Global.WriteTimeout;
            port.BaudRate = Global.BaudRate;
            port.DataBits = Global.DataBits;
            port.NewLine = Global.NewLine;

            // asynchronous transaction processing
            comWorker.WorkerReportsProgress = false;
            comWorker.WorkerSupportsCancellation = true;
            comWorker.DoWork += ComWorker_DoWork;
            comWorker.RunWorkerAsync();

            // finite state machine
            state = DeviceState.GetDefaultState(port);
            newState = state;            
        }

        // </CONSTRUCTORS>

        // <METHODS>
        
        /// <summary>
        /// Connects to selected serial port
        /// </summary>
        /// <param name="portName">Serial port name ("COM#")</param>
        public void Connect(string portName)
        {
            tempPortName = portName;
            connect = true;
            //state = state.Connect(portName);
        }
        
        /// <summary>
        /// Disconnects from serial port
        /// </summary>
        public void Disconnect()
        {
            disconnect = true;
            //state = state.Disconnect();
        }
        
        /// <summary>
        /// Enqueues a transaction that does not expect response (return parameter)
        /// </summary>
        /// <param name="transaction">Transaction to enqueue</param>
        public void Send(Transaction transaction)
        {
            transactionsQueue.Enqueue(transaction);
        }
        
        /// <summary>
        /// Enqueues a transaction that expects response (return parameter)
        /// </summary>
        /// <param name="transaction">Transaction to enqueue</param>
        public void Query(Transaction transaction)
        {
            // A special treatment is needed for compensation since the full compensation can take up to 2 minutes during which the device does not respond. Hence, the read timeout has to be adjusted.
            if ((transaction.Command == Commands.CompensateOpen) || (transaction.Command == Commands.CompensateShort))
            {
                port.ReadTimeout = Global.LongReadTimeout;
            }
            else
            {
                port.ReadTimeout = Global.ShortReadTimeout;
            }

            // add expect response flag if it has not been already set
            if (transaction.ExpectsResponse == Response.NoResponse)
            {
                transaction = new Transaction(transaction.Command, Response.ExpectsResponse);
            }
            transactionsQueue.Enqueue(transaction);
        }
        
        /// <summary>
        /// Clear all items in queue
        /// </summary>
        public void ClearQueue()
        {            
            transactionsQueue.Clear();
        }
        
        /// <summary>
        /// Discard all data in both the input and output buffer of the serial port
        /// </summary>
        public void Flush()
        {
            if (port.IsOpen)
            {
                Thread.Sleep(Global.Delay); // wait for all data to arrive
                port.DiscardInBuffer();
                port.DiscardOutBuffer();
            }
        }
        
        /// <summary>
        /// Resets the device
        /// </summary>
        public void Reset()
        {
            reset = true;
            //if (state is ResettableState) // only devices that have been identified can be reset
            //{
            //   // ((ResettableState)state).Reset();
            //    state = new Identified(port);
            //}
        }
        
        /// <summary>
        /// Clear the invocation list of TransactionQueueEmpty event
        /// </summary>
        public void TransactionQueueEmptyClear()
        {
            TransactionQueueEmpty = null;
        }

        // </METHODS>

        // <PROPERTIES>

        /// <summary>
        /// Gets the name of the serial port
        /// </summary>
        public string PortName
        {
            get
            {
                return port.PortName;
            }
        }
        
        /// <summary>
        /// Gets the state of the finite state machine
        /// </summary>
        public DeviceState State
        {
            get { return state; }
        }

        // </PROPERTIES>

        // <EVENT HANDLERS>        

        /// <summary>
        /// The main loop that processes enqueued transactions
        /// </summary>
        /// <param name="sender">Background worker</param>
        /// <param name="e">Event arguments</param>
        private void ComWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!((BackgroundWorker)sender).CancellationPending)
            {
                Thread.Sleep(Global.Delay); // reduce CPU load                                                                

                // process flags
                if (disconnect)
                {
                    disconnect = false;
                    state = state.Disconnect();
                }

                if (connect)
                {
                    connect = false;
                    state = state.Connect(tempPortName);                    
                }               
                
                if (reset)
                {
                    reset = false;
                    if (state is ResettableState)
                    {
                        state = new Identified(port);
                    }
                }

                // continue with state-specific actions

                if (state is Disconnected)
                {
                    ClearQueue();
                    Exception innerException = ((Disconnected)state).RemoveInnerException(); 

                    // raise events
                    if (DeviceIdentified != null)
                    {
                        DeviceIdentified(null);
                    }
                    if ((innerException != null) && (PortClosed != null))
                    {
                        PortClosed(innerException);
                    }
                }
                else if (state is Connected)
                {
                    ClearQueue();
                    string connectionInfo;
                    state = ((Connected)state).Identify(Global.Identification, out connectionInfo);
                    
                    // raise events
                    if ((state is Identified) && (DeviceIdentified != null))
                    {
                        DeviceIdentified(connectionInfo);
                    }
                }
                else if (state is Identified)
                {
                    ClearQueue();
                    Flush();
                    state = ((Identified)state).Reset();

                    // raise events
                    if (state is Ready)
                    {                     
                        if (DeviceReady != null) { DeviceReady(); }
                    }                    
                }
                else if (state is Ready)
                {
                    if (transactionsQueue.Count > 0)
                    {
                        lastTransaction = transactionsQueue.Dequeue();
                        string command = lastTransaction.Command.Command();
                        
                        if (lastTransaction.ExpectsResponse == Response.ExpectsResponse) // query commands
                        {
                            string response;

                            // Terminate all commands which expect response with a question mark (if not already present). With the exception of compensate commands which expect response but do not have question mark.
                            if ((lastTransaction.Command.Command().Contains("?") == false) && (lastTransaction.Command != Commands.CompensateOpen) && (lastTransaction.Command != Commands.CompensateShort))
                            {
                                command += "?";
                            }

                            state = ((Ready)state).Query(command, out response); // query the device

                            Transaction outputTransaction = new Transaction(lastTransaction.Command, lastTransaction.ExpectsResponse); // make a copy of the transaction to pass out

                            outputTransaction.AddResponse(response); // add the received response

                            // raise events
                            if ((DataUpdated != null) && (state is Ready))
                            {
                                DataUpdated(outputTransaction);
                            }
                        }
                        else // send commands
                        {
                            if (lastTransaction.Parameter == null) // without parameter
                            {
                                state = ((Ready)state).Send(command);
                            }
                            else // with parameter
                            {
                                state = ((Ready)state).Send(command + " " + lastTransaction.Parameter);
                            }
                        }
                    }                    
                    else if (TransactionQueueEmpty != null) // raise event if transaction queue is empty
                    {
                        TransactionQueueEmpty();
                    }
                }                
            }
        }

        // </EVENT HANDLERS>
    }
}
