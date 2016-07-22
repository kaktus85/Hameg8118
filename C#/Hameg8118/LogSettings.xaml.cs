using System;
using System.Windows;
using System.Windows.Input;

namespace Hameg8118
{    
    /// <summary>
    /// Interaction logic for LogSettings.xaml
    /// </summary>
    /// 
    public partial class LogSettings : Window
    {                
        LoggingSettings newSettings;

        // <CONSTRUCTORS>

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="currentSettings">Initial settings to display</param>
        public LogSettings(LoggingSettings currentSettings)
        {
            InitializeComponent();
            newSettings = currentSettings;
            comboBoxUnit.Items.Clear();
            foreach(string unit in Enum.GetNames(typeof(TimeUnits)))
            {
                comboBoxUnit.Items.Add(unit);                
            }
            comboBoxUnit.SelectedIndex = (int)(currentSettings.TimeUnit);

            textBoxInterval.Text = currentSettings.Interval.ToString();
            checkBoxMilliseconds.IsChecked = currentSettings.IncludeMilliseconds;
        }

        // </CONSTRUCTORS>

        // <METHODS>
        // </METHODS>

        // <PROPERTIES>
        // </PROPERTIES>

        // <EVENT HANDLERS>        

        /// <summary>
        /// Shows the window as dialog window and outputs settings as changed by user
        /// </summary>
        /// <param name="newSettings">Reference to settings that will be taken from the GUI</param>
        /// <returns>Returns standard result of built in Window.ShowDialog() method</returns>
        public bool? ShowDialog(out LoggingSettings newSettings)
        {            
            bool? result = ShowDialog();
            newSettings = this.newSettings;
            return result;
        }

        /// <summary>
        /// Handles clicks to cancel button - do not alter settings
        /// </summary>
        /// <param name="sender">Cancel button</param>
        /// <param name="e">Parameters</param>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Handles clicks to OK button - create new settings
        /// </summary>
        /// <param name="sender">OK button</param>
        /// <param name="e">Parameters</param>
        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            double interval;            
            if (double.TryParse(textBoxInterval.Text, out interval))
            {
                if (checkBoxMilliseconds.IsChecked != null)
                {
                    newSettings = new LoggingSettings(interval, (TimeUnits)(comboBoxUnit.SelectedIndex), (bool)(checkBoxMilliseconds.IsChecked));
                }
                else
                {
                    newSettings = new LoggingSettings(interval, (TimeUnits)(comboBoxUnit.SelectedIndex), false);
                }
            }
            DialogResult = true;            
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
                buttonCancel_Click(sender, null);
            }
        }

        // </EVENT HANDLERS>
    }
}
