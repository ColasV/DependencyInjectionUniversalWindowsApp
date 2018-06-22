using System;
using System.Linq;
using Windows.UI.Xaml.Controls;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjectionUniversalApp
{
    public class FrameWithDependencyInjection : Frame
    {
        private readonly ServiceProvider _serviceProvider;
        private const string ViewModel = "ViewModel";

        public FrameWithDependencyInjection([NotNull] ServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            _serviceProvider = serviceProvider;
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            var newContentType = newContent.GetType();

            Type viewModel;
            try
            {
                viewModel = newContentType.GetProperty(ViewModel).PropertyType;
            }
            catch (NullReferenceException)
            {
                throw new Exception("Property ViewModel need to exist on the page you try to navigate to.");
            }

            var constructors = viewModel.GetConstructors();
            if (constructors == null || constructors.Length != 1)
            {
                throw new Exception("The ViewModel should only have only one constructor to be correctly instanciated");
            }

            var constructor = constructors.First();
            var services = constructor.GetParameters().Select(p => _serviceProvider.GetService(p.ParameterType)).ToArray();

            var dependency = constructor.Invoke(services);

            newContentType.GetProperty(ViewModel).SetValue(newContent, dependency);

            base.OnContentChanged(oldContent, newContent);
        }
    }
}
