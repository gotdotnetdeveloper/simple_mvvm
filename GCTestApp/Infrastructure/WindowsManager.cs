using System.Windows.Controls;
using Microsoft.Practices.ServiceLocation;

namespace GCTestApp.Infrastructure
{
    /// <summary>
    /// MVVM - first
    /// </summary>
    public class WindowsManager
    {
        /// <summary>
        /// Отобразить вью-модель
        /// </summary>
        /// <param name="parentviewModel"></param>
        /// <param name="viewModel"></param>
       public void ShowInContentControl( ContentControl parentviewModel, object viewModel)
       {
            parentviewModel.Content = viewModel;
       }
        public void ShowInNewWindow<T>() where T : class
        {
           var vm =  ServiceLocator.Current.GetInstance<T>();

            ShellWindow shellWindow = new ShellWindow();
            shellWindow.ShellFrame.Content = vm;
            shellWindow.Show();
        }
    }
}