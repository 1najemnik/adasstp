using System;
using System.Windows;
using System.Windows.Markup;

namespace Foundation.MarkupExtensions
{
    public class StoreExtension : MarkupExtension
    {
        public StoreExtension(Type itemType)
        {
            ItemType = itemType;
        }

        [ConstructorArgument("ItemType")]
        public Type ItemType { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var service = (IProvideValueTarget) serviceProvider.GetService(typeof (IProvideValueTarget));
            var frameworkElement = service.TargetObject as FrameworkElement;
            var dependancyProperty = service.TargetProperty as DependencyProperty;
            var methodInfo = typeof(Store).GetMethod("OfType").MakeGenericMethod(ItemType);
            var item = methodInfo.Invoke(null, new object[] { new object[0] });
            if (frameworkElement != null &&
                dependancyProperty == FrameworkElement.DataContextProperty &&
                item is ViewModel)
            {
                var viewModel = (ViewModel) item;
                frameworkElement.CommandBindings.AddRange(viewModel.CommandBindings.Values);
                var window = frameworkElement as Window;
                if (window != null)
                    viewModel.OnClosing += (o, e) => { if (!e.Cancel) window.Close(); };
                frameworkElement.Initialized += (sender, args) => frameworkElement.DataContext = viewModel;
                return null;
            }

            return item;
        }
    }
}
