using System.Windows.Controls;
using GCTestApp.Infrastructure;
using GCTestApp.Module.ViewModel;
using Microsoft.Practices.ServiceLocation;

namespace GCTestApp
{
    /// <summary>
    /// Interaction logic for ShellWindow.xaml
    /// </summary>
    public partial class ShellWindow :  IShellWindow
    {
        public ShellWindow()
        {
            InitializeComponent();

            var vm = new TestViewModel();

           ServiceLocator.Current.GetInstance<WindowsManager>().ShowViewModel(ShellFrame, vm);
        }
        /// <summary>
        /// Контейнер для размещения представлений АРМ.
        /// </summary>
        public ContentControl ShellFrame => FrameMain;

    }
}
