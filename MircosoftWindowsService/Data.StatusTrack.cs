using System;

namespace EZLC
{
    public class StatusTrack
    {
        public int ID { get; set; }

        public int MachineDataID { get; set; }

        public int FrequencyId { get; set; }

        public int ConfigParamsId { get; set; }
       
        public string LoggedInUser { get; set; }

        public string Status { get; set; }

        public string MachineName { get; set; }

        public string OULocation { get; set; }

        public DateTime InstallationDate { get; set; }

        public DateTime LastReboot { get; set; }

        public string OperatingSystem { get; set; }

        public string MachineBuild { get; set; }

        public string IPAdress { get; set; }
        public string MACAddress { get; set; }
        public DateTime CapturedDate { get; set; }
        public string SoftwareVersion { get; set; }
       
       

    }
}
