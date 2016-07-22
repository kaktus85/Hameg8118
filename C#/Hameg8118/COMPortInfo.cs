using System;
using System.Collections.Generic;
using System.Management;

namespace Hameg8118
{
    // Following code for retrieving COM port friendly names is based on code by Dario Santarelli https://dariosantarelli.wordpress.com/2010/10/18/c-how-to-programmatically-find-a-com-port-by-friendly-name/
    // WMI calls to the more obvious Win32_SerialPort run very slowly on certain machines, hence the code below which calls to Win32_PnPEntity. Thank you Microsoft again :/        

    internal class ProcessConnection
    {
        public static ConnectionOptions ProcessConnectionOptions()
        {
            ConnectionOptions options = new ConnectionOptions();
            options.Impersonation = ImpersonationLevel.Impersonate;
            options.Authentication = AuthenticationLevel.Default;
            options.EnablePrivileges = true;
            return options;
        }

        public static ManagementScope ConnectionScope(string machineName, ConnectionOptions options, string path)
        {
            ManagementScope connectScope = new ManagementScope();
            connectScope.Path = new ManagementPath(@"\\" + machineName + path);
            connectScope.Options = options;
            connectScope.Connect();
            return connectScope;
        }
    }

    class COMPortInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public COMPortInfo() { }

        public static List<COMPortInfo> GetCOMPortsInfo()
        {
            List<COMPortInfo> comPortInfoList = new List<COMPortInfo>();

            ConnectionOptions options = ProcessConnection.ProcessConnectionOptions();
            ManagementScope connectionScope = ProcessConnection.ConnectionScope(Environment.MachineName, options, @"\root\CIMV2");

            ObjectQuery objectQuery = new ObjectQuery("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0");
            ManagementObjectSearcher comPortSearcher = new ManagementObjectSearcher(connectionScope, objectQuery);

            using (comPortSearcher)
            {
                string caption = null;
                foreach (ManagementObject obj in comPortSearcher.Get())
                {
                    if (obj != null)
                    {
                        object captionObj = obj["Caption"];
                        if (captionObj != null)
                        {
                            caption = captionObj.ToString();
                            if (caption.Contains("(COM"))
                            {
                                COMPortInfo comPortInfo = new COMPortInfo();
                                comPortInfo.Name = caption.Substring(caption.LastIndexOf("(COM")).Replace("(", string.Empty).Replace(")", string.Empty).Trim();
                                comPortInfo.Description = caption.Trim();
                                comPortInfoList.Add(comPortInfo);
                            }
                        }
                    }
                }
            }
            return comPortInfoList;
        }
    }
}
