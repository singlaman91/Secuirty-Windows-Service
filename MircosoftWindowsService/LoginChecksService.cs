using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace EZLC
{
    public partial class EZLC : ServiceBase
    {
        //DAL dalObj = new DAL();
        private Timer _createOrderTimer;
        private DateTime _lastRun = DateTime.Now;
        public static List<ConfigParams> objConfigParamsesCommon = null;
        //private Timer _createOrderTimer=new Timer();
        //private DateTime _lastRun = DateTime.Now;
        //public static List<ConfigParams> objConfigParamsesCommon = new List<ConfigParams>();
        public EZLC()
        {
            InitializeComponent();
          
        }

        public void Start()
        {
            OnStart(new string[0]);
        }
        protected override void OnStart(string[] args)
        {
            
          Task<string> obj=  ProcessData();
            int interval = 4;
            if (objConfigParamsesCommon != null)
            {
                 interval = Convert.ToInt32(objConfigParamsesCommon.ElementAt(0).FrequencyValue);
            }
            _createOrderTimer = new System.Timers.Timer();
            _createOrderTimer.Elapsed += new System.Timers.ElapsedEventHandler(createOrderTimer_Elapsed);
            //_createOrderTimer.Interval = interval*60*1000; // interval min
            _createOrderTimer.Interval = interval*60 * 10 * 1000; // interval evry 10 mins
            _createOrderTimer.Enabled = true;
            _createOrderTimer.AutoReset = true;
            _createOrderTimer.Start();
        }

        protected void createOrderTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs args)
        {
            Task<string> obj = ProcessData();
        }
        protected override void OnStop()
        {
           
        }
        /// <summary>
        /// This method gets the values of config params from the web api
        /// and then check the status of each the params to be tracked
        /// fetch the system level params from the system
        /// </summary>
        /// <returns>no of rows was saved</returns>
        public  async Task<string> ProcessData()
        {
            //GetCertificate();
            DAL dalObj = new DAL();
            SystemParams systemParamsObj = new SystemParams();
            TrackData trackDataObj = new TrackData();
            List<StatusTrack> objStatusTracks = new List<StatusTrack>();
            try
            {
                List<ConfigParams> obConfigParamsList = await GetConfigParams();

                foreach (ConfigParams obConfigParams in obConfigParamsList)
                {
                    string category = obConfigParams.Category.ToLower();
                    StatusTrack objStatusTrack = new StatusTrack
                    {
                        ConfigParamsId = obConfigParams.ID
                    };

                    switch (category)
                    {
                        case "service":
                            objStatusTrack.Status = trackDataObj.GetServiceStatus(obConfigParams.Name);
                            break;

                        case "software":
                            string softwares = trackDataObj.GetSoftwareStatus(obConfigParams.Name);
                            objStatusTrack.Status = softwares.Contains(Constants.NotInstalled) ? Constants.NotInstalled : Constants.Installed;
                            objStatusTrack.SoftwareVersion = softwares;
                            break;

                        case "file":
                            objStatusTrack.Status = trackDataObj.GetFileStatus(obConfigParams.Name);
                            break;

                        case "registry":
                            objStatusTrack.Status = trackDataObj.GetRegistryStatus(obConfigParams.Name) == null ? Constants.DoesNotExists : trackDataObj.GetRegistryStatus(obConfigParams.Name);
                            break;
                        default:
                            objStatusTrack.Status = String.Empty;
                            break;
                    }

                    systemParamsObj.GetSystemParams(ref objStatusTrack);
                    objStatusTracks.Add(objStatusTrack);
                }
                string response = string.Empty;
                if (obConfigParamsList.Count>0)
                 response = await dalObj.SaveStatusTrack(objStatusTracks, obConfigParamsList[0].IssuerName, obConfigParamsList[0].CertTemplateName, obConfigParamsList[0].CertTemplateGuidName);
                return response;
            }
            catch (Exception e)
            {
                try
                {
                    dalObj.LogErrors(e.InnerException + e.StackTrace);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                   // return "error" + e;
                }
                Console.WriteLine(e);

                return "error" + e;
            }
        }
  
        /// <summary>
        /// Method to get config params from db everyday once else get from file
        /// </summary>
        /// <returns>List of ConfigParams</returns>
        private async Task<List<ConfigParams>> GetConfigParams()
        {
            try
            {
                SystemParams systemParamsObj = new SystemParams();
                DAL dalObj = new DAL();
                string encryptedData = string.Empty;
                DateTime lastModified = DateTime.MinValue;
                    //= System.IO.File.GetLastWriteTimeUtc(@Constants.Filename);
               // if (lastModified.AddDays(5) > DateTime.UtcNow)
                    if(File.Exists( @Constants.Filename))
                { encryptedData = System.IO.File.ReadAllText(@Constants.Filename); }

                List<ConfigParams> obConfigParamsList = null;

              //  lastModified = DateTime.MinValue;

                if (encryptedData.Equals(String.Empty))// read from db if file does not exists or data does not exists
                {
                    obConfigParamsList = await dalObj.GetConfigParams(systemParamsObj.GetDevicType(), systemParamsObj.GetLocation());
                }
                else//read from file
                {
                    EncryptionEngine objEncryptionEngine = new EncryptionEngine();
                    string data = objEncryptionEngine.Decrypt(encryptedData);
                    obConfigParamsList = dalObj.GetConfigParamFromJson(data);
                    lastModified = obConfigParamsList.ElementAt(0).FileCreationDate;
                    if (lastModified.AddDays(1) < DateTime.UtcNow)
                    {
                        obConfigParamsList = null;
                        obConfigParamsList = await dalObj.GetConfigParams(systemParamsObj.GetDevicType(), systemParamsObj.GetLocation());
                    }
                }
                objConfigParamsesCommon = obConfigParamsList;
                return obConfigParamsList;
            }
            catch(Exception e)
            {
                throw e;
            }
        }

    
    }
}
