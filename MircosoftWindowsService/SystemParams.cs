using System;
using System.Linq;
using System.Management;
using System.Net;
using Microsoft.Win32;
using System.Net.NetworkInformation;

namespace EZLC
{
    public class SystemParams
    {
        private string _oULocation;
       // private string _oULocation=string.Empty;
        DAL objDal = new DAL();

        /// <summary>
        /// Method to fetch the location of the device extracted 
        /// from OU location field from directory
        /// </summary>
        /// <returns>Location</returns>
        public string GetLocation()
        {
            try
            {
                string Keyname = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\State\Machine\";
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(Keyname);
                Object oULocationval = rk.GetValue("Distinguished-Name");
                _oULocation = oULocationval.ToString();
                string loc = _oULocation.Split(',').ElementAt(2);
                loc = loc.Split('=').ElementAt(1);
                return loc;
            }
            catch (Exception e)
            {
                try
                {
                    objDal.LogErrors(e.InnerException + e.StackTrace);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                return "";
            }
        }

        /// <summary>
        /// Method to extract the device type of machine
        /// laptop or desktop
        /// </summary>
        /// <returns>device type</returns>
        public string GetDevicType()
        {
            try
            {
                //if the machine name is not valid, else use D
                string machineName = Environment.MachineName;
            string deviceType = machineName.Last().ToString();
                if( deviceType.ToUpper().Equals("L") || deviceType.ToUpper().Equals("D"))
                {
                    return deviceType;
                }
                return "D";

            }
            catch (Exception e)
            {
                try
                {
                    objDal.LogErrors(e.InnerException + e.StackTrace);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                return "D";
            }
        }

        /// <summary>
        /// Method to get the last reboot date time of system
        /// </summary>
        /// <returns>last reboot</returns>
        public DateTime GetLastRebootDateTime()
        {
            DateTime dtBootTime = new DateTime();
            SelectQuery query = new SelectQuery("SELECT LastBootUpTime FROM Win32_OperatingSystem WHERE Primary = 'true'");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

            // get the datetime value and set the local boot
            // time variable to contain that value
            foreach (ManagementObject mo in searcher.Get())
            {
                dtBootTime =ManagementDateTimeConverter.ToDateTime( mo.Properties["LastBootUpTime"].Value.ToString());
            }
            return dtBootTime.ToUniversalTime();
        }

        /// <summary>
        /// Method to get the installation date time of operating system
        /// </summary>
        /// <returns>installation date</returns>
        public DateTime GetWindowsInstallationDateTime()
        {
            DateTime dtinstallDate = new DateTime();
            SelectQuery query =new SelectQuery("SELECT installDate FROM Win32_OperatingSystem WHERE Primary = 'true'");
            ManagementObjectSearcher searcher =  new ManagementObjectSearcher(query);
            foreach (ManagementObject mo in searcher.Get())
            {
                dtinstallDate =
                    ManagementDateTimeConverter.ToDateTime(
                        mo.Properties["installDate"].Value.ToString());
            }
            return dtinstallDate.ToUniversalTime();
        }
        /// <summary>
        /// Method to get the installation date time of operating system
        /// </summary>
        /// <returns>installation date</returns>
        public string GetUserName()
        {
            string userName = string.Empty;
            ManagementScope ms = new ManagementScope("\\\\.\\root\\cimv2");
            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_ComputerSystem");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(ms, query);
            foreach (ManagementObject mo in  searcher.Get())
            {
                userName=mo["UserName"].ToString();
            }
            return userName;
        }
        /// <summary>
        /// Method to fetch the IP Addresses of the system
        /// IP Addresses will be saved with comma separation
        /// </summary>
        /// <returns>IP Addresses</returns>
        public string GetIpAddress()
        {
            //Retrive the Name of HOST 
            string hostName = Dns.GetHostName();
            // Get the IP  
            string myIp = "";
            foreach (var ipAddress in Dns.GetHostByName(hostName).AddressList)
            {
                if (myIp != "")
                    myIp = myIp + ", " + ipAddress;
                else
                {
                    myIp = ipAddress.ToString();
                }
            }
            return myIp;
        }
        public  string GetOSFriendlyName()
        {
            string result = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            foreach (ManagementObject os in searcher.Get())
            {
                result = os["Caption"].ToString();
                break;
            }
            return result;
        }

        public string GetMACAddress()
        {
            string mac = string.Empty;
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up && (!nic.Description.Contains("Virtual") && !nic.Description.Contains("Pseudo")))
                {
                    if (nic.GetPhysicalAddress().ToString() != "")
                    {
                        mac = nic.Description+" | "+ nic.GetPhysicalAddress().ToString() ;
                        foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                mac = mac + " | " + ip.Address.ToString();
                                break;
                            }
                        }
                    }
                    //IPInterfaceProperties properties = adapter.GetIPProperties();
                    //sMacAddress = adapter.GetPhysicalAddress().ToString();
                }
               
            }
            return mac;

        }

        /// <summary>
        /// The centralized method to get all the system params
        /// </summary>
        /// <param name="objStatusTrack"></param>
        /// <returns>StatusTrack object with system params in it</returns>
        public StatusTrack GetSystemParams(ref StatusTrack objStatusTrack)
        {
            try
            {
            GetLocation();
            string domain = System.Environment.UserDomainName;
            objStatusTrack.InstallationDate = GetWindowsInstallationDateTime();
            objStatusTrack.OperatingSystem = GetOSFriendlyName();
            objStatusTrack.LoggedInUser = GetUserName();
            objStatusTrack.OULocation = _oULocation;
            objStatusTrack.MachineName = Environment.MachineName;
            objStatusTrack.LastReboot = GetLastRebootDateTime();
            objStatusTrack.IPAdress = GetIpAddress();
            objStatusTrack.MachineBuild = System.IO.File.ReadAllText(@"C:\Windows\EZI.txt");
            objStatusTrack.MACAddress = GetMACAddress();
            objStatusTrack.CapturedDate = DateTime.UtcNow;
            return objStatusTrack;
            }
            catch (Exception e)
            {
                try
                {
                    objDal.LogErrors(e.InnerException+e.StackTrace);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                return objStatusTrack;
            }
        }
    }
}
