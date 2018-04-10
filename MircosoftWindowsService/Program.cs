using System;
using System.ServiceProcess;

namespace EZLC
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
         
            var myService = new EZLC();

            try
            {

            if (Environment.UserInteractive)
            {
                myService.Start();
                myService.Stop();
            }
            else
            {
                    var servicesToRun = new ServiceBase[] { myService };
                    ServiceBase.Run(servicesToRun);
               
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        
    }
}
