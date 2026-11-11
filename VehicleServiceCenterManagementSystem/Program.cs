using System;
using System.IO;
using System.Windows.Forms;
using VehicleServiceCenterManagementSystem.Forms;
namespace VehicleServiceCenterManagementSystem
{
    internal static class Program
    {
        private static void Log(Exception error)
        {
            try {string folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"AutoCare-Pro");Directory.CreateDirectory(folder);File.AppendAllText(Path.Combine(folder,"errors.log"),DateTime.UtcNow.ToString("O")+" "+error+Environment.NewLine);}catch { }
        }
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException+=(s,e)=>{Log(e.Exception);MessageBox.Show("An unexpected error occurred. Close this screen and try again. Details were saved in the AutoCare-Pro error log.","AutoCare-Pro",MessageBoxButtons.OK,MessageBoxIcon.Error);};
            AppDomain.CurrentDomain.UnhandledException+=(s,e)=>{if(e.ExceptionObject is Exception ex)Log(ex);};
            ApplicationConfiguration.Initialize();
            try {Application.Run(new HomeForm());}catch(Exception ex){Log(ex);MessageBox.Show("AutoCare-Pro could not start. Check the error log in your local application data folder.","AutoCare-Pro");}
        }
    }
}
