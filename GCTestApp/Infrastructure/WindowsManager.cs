using System.Windows.Controls;

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
       public void ShowViewModel( ContentControl parentviewModel, object viewModel)
       {

        

            parentviewModel.Content = viewModel;
           //parentviewModel.DataContext = viewModel;
       }
        
    }
}