using Autofac;
using Autofac.Extras.CommonServiceLocator;
using GCTestApp.Module.ViewModel;
using GCTestApp.Repository;
using Microsoft.Practices.ServiceLocation;

namespace GCTestApp.Infrastructure
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<WindowsManager>().AsSelf().AutoActivate().SingleInstance();


            
            builder.RegisterType<WindowsManager>().AsSelf().AutoActivate().SingleInstance();
            builder.RegisterType<DocumentRepository>().AsSelf().AutoActivate().SingleInstance();


            builder.RegisterType<TestViewModel>().InstancePerDependency();


            var container = builder.Build();
            var csl = new AutofacServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => csl);
        }
    }
}