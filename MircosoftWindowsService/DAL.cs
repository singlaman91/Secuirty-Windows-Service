using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace EZLC
{
    class DAL
    {
        /// <summary>
        /// This method interacts with web api to get the 
        /// config params (i.e. services, softwares, files to be tracked) 
        /// </summary>
        /// <param name="deviceType">laptop or desktop</param>
        /// <param name="Loc">location of the device</param>
        /// <returns>List of config params</returns>
        public async Task<List<ConfigParams>> GetConfigParams(string deviceType, string Loc)
        {
            HttpClient client = new HttpClient();
            List<ConfigParams> objConfigParams = null;
            client.BaseAddress = new Uri(ConfigurationSettings.AppSettings["RestServiceUri"]);
            
            //var header = new AuthenticationHeaderValue("Basic", Authentication.GenerateToken());
            //client.DefaultRequestHeaders.Authorization = header;
            // Usage
            HttpResponseMessage response = client.GetAsync("api/values?loc=" + Loc.Trim(' ') + "&deviceType=" + deviceType).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                objConfigParams = GetConfigParamFromJson(data);
                if(objConfigParams.Count>0)
                SaveDatainFile(data);
            }
            else
            {
                await LogErrors(response.StatusCode + ": " + response.ReasonPhrase);
            }

            return objConfigParams;
        }

        /// <summary>
        /// This method interacts with web api to save the tracked status  
        /// of softwares, services, files, registries
        /// </summary>
        /// <param name="obStatusTracks">the table object containing the list of tracked statuses</param>
        /// <returns>no of rows saved</returns>
        public async Task<string> SaveStatusTrack(List<StatusTrack> obStatusTracks,string IssuerName,string CertTemplateName,string CertTemplateGuidName)
        {
            //an HttpClient instance
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(ConfigurationSettings.AppSettings["RestServiceUri"])
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var header = new AuthenticationHeaderValue("Basic", Authentication.GenerateToken(IssuerName, CertTemplateName, CertTemplateGuidName));
            client.DefaultRequestHeaders.Authorization = header;
            StringContent sc = new StringContent(JsonConvert.SerializeObject(obStatusTracks), Encoding.Unicode, "application/json");

            //Usage
            HttpResponseMessage response = client.PostAsync("api/values", sc).Result;
            string content = null;
            if (response.IsSuccessStatusCode)
            {
                content = await response.Content.ReadAsStringAsync();
            }
            else
            {
              await  LogErrors(response.StatusCode+": "+response.ReasonPhrase);
                
            }
            return content;
        }

        /// <summary>
        /// Method to log errors in database via web api
        /// </summary>
        /// <param name="Exception"></param>
        /// <returns></returns>
        public async Task<string> LogErrors(string Exception)
        {
            SystemParams systemParams = new SystemParams();
            Exception ="Error: "+ Exception +" IP Address:"+ systemParams.GetIpAddress() + " and Machine Name: " + Environment.MachineName;
            //an HttpClient instance
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(ConfigurationSettings.AppSettings["RestServiceUri"])
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            StringContent sc = new StringContent(JsonConvert.SerializeObject(Exception), Encoding.Unicode, "application/json");

            //Usage
            HttpResponseMessage response = client.PostAsync("api/PostUserLogs", sc).Result;
            string content = null;
            if (response.IsSuccessStatusCode)
            {
                content = await response.Content.ReadAsStringAsync();
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }
            return content;
        }


        /// <summary>
        /// Method to save data in text file
        /// </summary>
        /// <param name="data">data to be saved</param>
        private void SaveDatainFile(string data)
        {
            EncryptionEngine encryptionEngine = new EncryptionEngine();

           string encryptedData= encryptionEngine.Encrypt(data);

            using (TextWriter tw = new StreamWriter(Constants.Filename))
            {
                tw.WriteLine(encryptedData);
                tw.Close();
            }
        }
        
        /// <summary>
        /// Method to convert json to config params entity listing
        /// </summary>
        /// <param name="json">json data</param>
        /// <returns>List of ConfigParams</returns>
        public List<ConfigParams> GetConfigParamFromJson(string json)
        {
            List<ConfigParams> objConfigParams = JsonConvert.DeserializeObject<List<ConfigParams>>(json);
            return objConfigParams;
        }
    }
}
