using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GCTestApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App 
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            ShellWindow shellWindow = new ShellWindow();
            shellWindow.Show();
        }


        /// <summary>
        /// Личный кабинет руководителя - точка входа.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            using (Mutex mutex = new Mutex(true, "GCTestApp", out var createdNew))
            {
                if (createdNew)
                {
                    var application = new App();
                    //ApplitaionContext.SystemApp = SystemApp.ManagerDashboard;
                    application.InitializeComponent();
                    application.Run();
                }
                else
                {
                    //Process current = Process.GetCurrentProcess();
                    //foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                    //{
                    //    if (process.Id != current.Id)
                    //    {
                    //        SetForegroundWindow(process.MainWindowHandle);
                    //        ShowWindowAsync(process.MainWindowHandle, 9);
                    //        break;
                    //    }
                    //}
                }
            }
        }
    }
}
