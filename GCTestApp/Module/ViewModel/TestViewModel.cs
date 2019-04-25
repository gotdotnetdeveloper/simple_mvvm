using System;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GCTestApp.Infrastructure;
using GCTestApp.Module.Model;
using GCTestApp.Repository;
using Microsoft.Practices.ServiceLocation;

namespace GCTestApp.Module.ViewModel
{
    public class TestViewModel : ViewModelBase , ITestViewModel
    {
        private ICommand _ShowNewCommand;
        private DocumentRepository _documentRepository;
        public TestViewModel(DocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
            Model = new TestModel();
            Model.Name = "edfsdf";
            Model.Description = string.Join(", ", documentRepository.GetDocumentNumber());
        }
        public TestModel Model { get; set; }


        public ICommand ShowNewCommand => _ShowNewCommand ?? (_ShowNewCommand =
        new RelayCommand(ShowNewCommandExecute));

        private void ShowNewCommandExecute()
        {
            ServiceLocator.Current.GetInstance<WindowsManager>().ShowInNewWindow<ITestViewModel>();
        }
    }
}