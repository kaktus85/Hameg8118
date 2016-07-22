using System;

namespace Hameg8118
{    
    public class Transaction
    {
        private Commands command;
        private string parameter;
        private Response expectsResponse = Hameg8118.Response.NoResponse;
        private string response;

        // <CONSTRUCTORS>

        /// <summary>
        /// Constructor for command without parameter
        /// </summary>
        /// <param name="command">Command to send to device</param>
        public Transaction(Commands command)
        {
            this.command = command;
            if (command.Command().Contains("?"))
            {
                expectsResponse = Hameg8118.Response.ExpectsResponse;
            }            
        }

        /// <summary>
        /// Constructor for command with parameter
        /// </summary>
        /// <param name="command">Command to send to device</param>
        /// <param name="parameter">Parameter of the command</param>
        public Transaction(Commands command, string parameter)
        {
            this.command = command;
            this.parameter = parameter;            
            expectsResponse = Hameg8118.Response.NoResponse;
        }

        /// <summary>
        /// Constructor for command without parameter but with response from device
        /// </summary>
        /// <param name="command">Command to send to device</param>
        /// <param name="response">Determines whether response from the device is to be expected</param>
        public Transaction(Commands command, Response response)
        {
            this.command = command;
            if (response == Hameg8118.Response.ExpectsResponse)
            {                
                expectsResponse = Hameg8118.Response.ExpectsResponse;
            }            
        }

        // </CONSTRUCTORS>

        // <METHODS>

        /// <summary>
        /// Adds response to the transaction, can be called only once for each transaction
        /// </summary>
        /// <param name="response">Response to add</param>
        public void AddResponse(string response)
        {
            if (this.response == null)
            {
                this.response = response;
            }
            else
            {
                throw new InvalidOperationException("This transaction already contains a response");
            }
        }

        // </METHODS>

        // <PROPERTIES>

        public Commands Command { get { return command; } }
        public string Parameter { get { return parameter; } }
        public Response ExpectsResponse { get { return expectsResponse; } }
        public string Response { get { return response; } }

        // </PROPERTIES>

        // <EVENT HANDLERS>
        // </EVENT HANDLERS>
    }
}
