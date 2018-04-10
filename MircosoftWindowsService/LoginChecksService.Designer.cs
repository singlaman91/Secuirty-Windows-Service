namespace EZLC
{
    partial class EZLC
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.myProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            // 
            // myProcessInstaller
            // 
            this.myProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.myProcessInstaller.Password = null;
            this.myProcessInstaller.Username = null;
            // 
            // EZLC
            // 
            this.ServiceName = "EZLC";

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller myProcessInstaller;
    }
}
