using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Diagnostics;

namespace Hameg8118
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        List<Attribution> attributions; // all attributions
        
        // <CONTRUCTORS>

        /// <summary>
        /// Constructor
        /// </summary>
        public About()
        {
            InitializeComponent();
            DataContext = this;
            attributions = new List<Attribution>();

            // add attributions
            Attribution a;

            a = new Attribution();
            a.InternalName = "Code for finding friendly COM port name";
            a.OriginalName = "How to programmatically find a COM port by friendly name";
            a.Author = "Dario Santarelli";
            a.Licence = "public domain";
            a.File = string.Empty;
            a.URL = "https://dariosantarelli.wordpress.com/2010/10/18/c-how-to-programmatically-find-a-com-port-by-friendly-name";
            attributions.Add(a);

            a = new Attribution();
            a.InternalName = "AI plugin for conversion of vector graphics to XAML";
            a.OriginalName = "XAML Export Plug-In Version 0.2 (PC/64)";
            a.Author = "Mike Swanson";
            a.Licence = "freeware";
            a.File = "XAMLExport64.aip";
            a.URL = "http://www.mikeswanson.com/xamlexport";
            attributions.Add(a);

            a = new Attribution();
            a.InternalName = "SVG Icon of a clipboard";
            a.OriginalName = "react-clipboard-icon";
            a.Author = "Zeno Rocha";
            a.Licence = "MIT License";
            a.File = "clipboard.js";
            a.URL = "https://github.com/zenorocha";
            attributions.Add(a);

            listViewAttribution.ItemsSource = attributions; // show attributions in listview
        }

        // </CONTRUCTORS>

        // <METHODS>
        // </METHODS>

        // <PROPERTIES>

        /// <summary>
        /// Gets the version number as defined in assembly information
        /// </summary>
        public string Version
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        // </PROPERTIES>

        // <EVENT HANDLERS>

        /// <summary>
        /// Handles clickable URL in attributions
        /// </summary>
        /// <param name="sender">Clickable URL in attributions</param>
        /// <param name="e">Event arguments</param>
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
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

        /// <summary>
        /// Handles clicks to logo and opens GitHub repository
        /// </summary>
        /// <param name="sender">Logo</param>
        /// <param name="e">Event arguments</param>
        private void logo_MouseUp(object sender, MouseButtonEventArgs e)
        {            
            Process.Start(new ProcessStartInfo(Global.GitHubURL));
        }
                
        /// <summary>
        /// Handles copying from attribution list without header
        /// </summary>
        /// <param name="sender">A line or lines from attribution list</param>
        /// <param name="e">Event arguments</param>
        private void listViewAttributionCopy_Click(object sender, RoutedEventArgs e)
        {
            string lines = string.Empty;

            // copy all selected lines to clipboard
            for (int i = 0; i < listViewAttribution.SelectedItems.Count; i++)
            {
                lines += ((Attribution)listViewAttribution.SelectedItems[i]).ToString();
                if (i < listViewAttribution.SelectedItems.Count - 1)
                {
                    lines += Environment.NewLine; ;
                }
            }
            try
            {
                Clipboard.SetText(lines);
            }
            catch (System.Runtime.InteropServices.COMException) { } // clipboard not accessible  
        }

        /// <summary>
        /// Handles copying from attribution list with header
        /// </summary>
        /// <param name="sender">A line or lines from attribution list</param>
        /// <param name="e">Event arguments</param>
        private void listViewAttributionCopyWithHeader_Click(object sender, RoutedEventArgs e)
        {
            string lines = string.Empty;

            // header                 
            GridView gv = (GridView)(listViewAttribution.View);
            for (int i = 0; i < gv.Columns.Count; i++)
            {
                lines += ((TextBlock)(gv.Columns[i].Header)).Text;
                if (i < gv.Columns.Count - 1)
                {
                    lines += Global.Delimiter;
                }
            }

            lines += Environment.NewLine;

            // copy all selected lines to clipboard
            for (int i = 0; i < listViewAttribution.SelectedItems.Count; i++)
            {
                lines += ((Attribution)listViewAttribution.SelectedItems[i]).ToString();
                if (i < listViewAttribution.SelectedItems.Count - 1)
                {
                    lines += Environment.NewLine; ;
                }
            }
            try
            {
                Clipboard.SetText(lines);
            }
            catch (System.Runtime.InteropServices.COMException) { } // clipboard not accessible  
        }

        /// <summary>
        /// Handles copying from attribution list via Ctrl+C and Ctrl+D commands
        /// </summary>
        /// <param name="sender">A line or lines from attribution list</param>
        /// <param name="e">Event arguments, identify the key that has been pressed</param>
        private void listViewAttribution_KeyDown(object sender, KeyEventArgs e)
        {
            // copy
            if ((Keyboard.IsKeyDown(Key.C)) && (Keyboard.Modifiers == ModifierKeys.Control))
            {
                listViewAttributionCopy_Click(null, null);
            }

            // copy with header
            if ((Keyboard.IsKeyDown(Key.D)) && (Keyboard.Modifiers == ModifierKeys.Control))
            {
                listViewAttributionCopyWithHeader_Click(null, null);
            }
        }
        
        // </EVENT HANDLERS>
    }
}
