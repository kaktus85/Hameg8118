using System.ComponentModel;
using System.Windows;

namespace Hameg8118
{
    /// <summary>
    /// Interaction logic for Compensation.xaml
    /// </summary>
    public partial class Compensation : Window
    {
        bool allowClose = false; // do not allow the user to close the window

        // <CONSTRUCTORS>        

        /// <summary>
        /// Constructor
        /// </summary>
        public Compensation()
        {
            InitializeComponent();
        }

        // </CONSTRUCTORS>

        // <METHODS>

        /// <summary>
        /// Closes the window
        /// </summary>
        public void ForceClose()
        {
            allowClose = true;
            Close();        
        }

        // </METHODS>

        // <PROPERTIES>        
        // </PROPERTIES>

        // <EVENT HANDLERS>

        /// <summary>
        /// Occurs when window has been commanded to close; used for preventing the window from closing
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Parameter</param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = !allowClose; // do not close window if not allowed
        }

        // </EVENT HANDLERS>
    }
}
