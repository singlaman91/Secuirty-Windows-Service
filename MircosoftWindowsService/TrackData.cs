using System;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using Microsoft.Win32;

namespace EZLC
{
    class TrackData
    {
        DAL objDal=new DAL();
        /// <summary>
        /// Method to track the status of service if it is running
        /// </summary>
        /// <param name="service">service to be tracked</param>
        /// <returns>status of service</returns>
        public string GetServiceStatus(string service)
        {
            ServiceController[] serviceControllers = ServiceController.GetServices();
            string status = Constants.NotInstalled;
            try
            {
            foreach (ServiceController serviceControllersObj in serviceControllers)
            {

                if (serviceControllersObj.DisplayName.ToLower().Contains(service.ToLower()))
                    status = serviceControllersObj.Status.ToString();
            }
            return status;
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
                return Constants.Error;
            }
        }

        /// <summary>
        /// Method to track the status of software and its versions installed
        /// </summary>
        /// <param name="software">software to be tracked</param>
        /// <returns>versions of softwares installed</returns>
        public string GetSoftwareStatus(string software)
        {
            try
            {
            string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            string softwareVersions = null;
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registry_key))
            {//handle casing in check
                foreach (string subkey_name in key.GetSubKeyNames())
                {
                    using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                    {
                        object line;
                        line = subkey.GetValue("DisplayName");
                        //Console.WriteLine(line);
                        if (line != null && line.ToString().ToLower().Contains(software.ToLower()))
                        {
                            if (softwareVersions != null)
                            {
                                softwareVersions = softwareVersions + ", " + line.ToString();
                            }
                            else
                            {
                                softwareVersions = line.ToString();
                            }
                        }
                    }
                }

                if (softwareVersions != null)
                {
                    return softwareVersions;
                }
            }
            return Constants.NotInstalled;
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

                return Constants.Error;
            }
        }

        /// <summary>
        ///  Method to track the status of file if it exists
        /// </summary>
        /// <param name="file">file to be tracked</param>
        /// <returns>status of file</returns>
        public string GetFileStatus(string file)
        {
            try
            {
            if (File.Exists(file))
                return Constants.Exists;
            return Constants.DoesNotExists;
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

                return Constants.Error;
            }
        }

        #region registry solutions
        /// <summary>
        /// Method to track the status of registry 
        /// </summary>
        /// <param name="registry"></param>
        /// <returns>status of registry</returns>
        /// http://www.c-sharpcorner.com/uploadfile/puranindia/the-windows-registry-in-C-Sharp/
        public string GetRegistryStatus(string registry)
        {
            try
            {
                if (String.IsNullOrEmpty(registry))
                {
                    return Constants.KeyNotFound;
                }
                
                    string[] lines = @registry.Split(Path.DirectorySeparatorChar);
                    string registryCategory = lines[1];

                    //method to save the get the reigstry category from the supplied string
                    //here the format of HKEY_LOCAL_MACHINE is changed to LocalMachine
                    string[] registryRoot = registryCategory.Split('_');
                    string registryRootValue = string.Empty;
                    for (int i = 1; i < registryRoot.Length; i++)
                    {
                        registryRootValue = registryRootValue + registryRoot[i];
                    }

                    string keyName = lines.LastOrDefault();

                    string path = string.Empty;

                    for (int i = 2; i < lines.Length - 1; i++)
                    {
                        if (path != string.Empty)
                            path = path + Path.DirectorySeparatorChar + lines[i];
                        else
                        {
                            path = lines[i];
                        }
                    }

                    string value = GetRegistryCategory(registryRootValue, path, keyName);
                    return value;
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
            
                return Constants.Error ;
            }
        }

        /// <summary>
        /// Method to fetch registry key value
        /// </summary>
        /// <param name="registryCategory">localmachine, users etc</param>
        /// <param name="path">path of the registry</param>
        /// <param name="keyName">the key from which value needs to be fetched</param>
        /// <returns>registry key value</returns>
        private string GetRegistryCategory(string registryCategory, string path, string keyName)
        {
            RegistryKey rk = null;
            switch (registryCategory.ToLower())
            {
                case "localmachine":
                    rk = Registry.LocalMachine.OpenSubKey(path);
                    break;

                case "classesroot":
                    rk = Registry.ClassesRoot.OpenSubKey(path);
                    break;

                case "currentconfig":
                    rk = Registry.CurrentConfig.OpenSubKey(path);
                    break;

                case "currentuser":
                    rk = Registry.CurrentUser.OpenSubKey(path);
                    break;

                case "users":
                    rk = Registry.Users.OpenSubKey(path);
                    break;
                default:
                    return Constants.KeyNotFound;

            }
            Object val = rk.GetValue(keyName);
            return val.ToString();
        }

        /// <summary>
        /// Method to track the status of registry 
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns>status of registry</returns>
        public string GetRegistryValue(string fullPath)
        {
            string[] lines = @fullPath.Split(Path.DirectorySeparatorChar);

            string defaultValue = lines.LastOrDefault();

            string path = string.Empty;

            for (int i = 2; i < lines.Length - 1; i++)
            {
                if (path != string.Empty)
                    path = path + Path.DirectorySeparatorChar + lines[i];
                else
                {
                    path = lines[i];
                }
            }
            string keyName = Path.GetDirectoryName(path);
            string valueName = Path.GetFileName(path);
            return Registry.GetValue(keyName, valueName, defaultValue).ToString();
        }
        #endregion
    }
}
