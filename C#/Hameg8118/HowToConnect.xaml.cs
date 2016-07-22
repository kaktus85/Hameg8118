using System.Windows;
using System.Diagnostics;
using System.Windows.Input;

namespace Hameg8118
{
    /// <summary>
    /// Interaction logic for HowToConnect.xaml
    /// </summary>
    public partial class HowToConnect : Window
    {
        // <CONSTRUCTORS>

        /// <summary>
        /// Constructor
        /// </summary>
        public HowToConnect()
        {
            InitializeComponent();
        }

        // </CONSTRUCTORS>

        // <METHODS>
        // </METHODS>

        // <PROPERTIES>
        // </PROPERTIES>

        // <EVENT HANDLERS>       

        /// <summary>
        /// Handles clicks to download driver button
        /// </summary>
        /// <param name="sender">Download driver button</param>
        /// <param name="e">Parameters</param>
        private void buttonDownload_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.rohde-schwarz.com/us/driver/hm8118"));
        }

        /// <summary>
        /// Handles clicks to opening device manager
        /// </summary>
        /// <param name="sender">Open device manager button</param>
        /// <param name="e">Parameters</param>
        private void buttonDeviceManager_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("devmgmt.msc");
        }

        /// <summary>
        /// Handles clicks to close button
        /// </summary>
        /// <param name="sender">Close button</param>
        /// <param name="e">Parameters</param>
        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {            
            Close();
        }

        /// <summary>
        /// Handles ESC key to close the window
        /// </summary>
        /// <param name="sender">About window</param>
        /// <param name="e">Event arguments, identify the key that has been pressed</param>
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        // </EVENT HANDLERS>
    }
}
