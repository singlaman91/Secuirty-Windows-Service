using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

namespace EZLC
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {

        public ProjectInstaller()
        {
            InitializeComponent();
        }
        //private void serviceInstaller1_BeforeInstall(object sender, InstallEventArgs e)
        //{
        //    //serviceInstaller1.Uninstall(null);
        //    //ServiceController sc = new ServiceController("FrameworkService(LCWS)");
        //    //sc.Stop();
        //    //serviceInstaller1.Uninstall(null);

        //}
        private void serviceInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {
            ServiceController sc = new ServiceController("FrameworkService(LCWS)");
            sc.Start();
            SetRecoveryOptions("FrameworkService(LCWS)");
        }
         void SetRecoveryOptions(string serviceName)
        {
            int exitCode;
            using (var process = new Process())
            {
                var startInfo = process.StartInfo;
                startInfo.FileName = "sc";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                // tell Windows that the service should restart if it fails
                startInfo.Arguments = string.Format("failure \"{0}\" reset= 0 actions=restart/60000/restart/60000// reset= 240", serviceName);
                process.Start();
                process.WaitForExit();
                exitCode = process.ExitCode;
            }
        }

        private void ProjectInstaller_BeforeInstall(object sender, InstallEventArgs e)
        {
            //serviceInstaller1.Uninstall(null);
            //serviceProcessInstaller1.Uninstall(null);
            // serviceInstaller1.Uninstall(null);
            //ServiceController sc = new ServiceController("FrameworkService(LCWS)");
            //if (sc.Status.Equals("Running"))
            //{
            //    sc.Stop();
            //    Thread.Sleep( 1000 * 10);
            //    //serviceInstaller1.Uninstall(null);
            //    //serviceProcessInstaller1.Uninstall(null);
            //}
        }

     

    }
}
