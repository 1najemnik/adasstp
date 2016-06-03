using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using BooleanConverter = Foundation.Converters.BooleanConverter;

namespace Foundation.MarkupExtensions
{
    public class ViewModelExtension : MarkupExtension
    {
        private static readonly BooleanConverter BooleanToVisibilityConverter = new BooleanConverter
        {
            OnTrue = Visibility.Visible,
            OnFalse = Visibility.Collapsed,
        };

        private FrameworkElement _targetObject;
        private DependencyProperty _targetProperty;

        public ViewModelExtension()
        {
        }

        public ViewModelExtension(string key)
        {
            Key = key;
        }

        public ViewModelExtension(string key, object defaultValue)
        {
            Key = key;
            DefaultValue = defaultValue;
        }

        public string Key { get; set; }
        public string StringFormat { get; set; }
        public string ElementName { get; set; }
        public object DefaultValue { get; set; }
        public object FallbackValue { get; set; }
        public object TargetNullValue { get; set; }
        public IValueConverter Converter { get; set; }
        public RelativeSource RelativeSource { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var service = (IProvideValueTarget) serviceProvider.GetService(typeof (IProvideValueTarget));
            _targetProperty = service.TargetProperty as DependencyProperty;
            _targetObject = service.TargetObject as FrameworkElement;
            if (_targetObject == null || _targetProperty == null) return this;

            var key = Key;
            if (_targetProperty == UIElement.VisibilityProperty && string.IsNullOrWhiteSpace(key))
                key = string.Format("Show{0}",
                                    string.IsNullOrWhiteSpace(_targetObject.Name)
                                        ? _targetObject.Tag
                                        : _targetObject.Name);

            key = string.IsNullOrWhiteSpace(key) ? _targetProperty.Name : key;
            if (!string.IsNullOrWhiteSpace(StringFormat)) Key = string.Format(StringFormat, _targetObject.Tag);

            var index = DefaultValue == null ? key : key + "," + DefaultValue;
            var path = string.IsNullOrWhiteSpace(ElementName) && RelativeSource == null
                           ? "[" + index + "]"
                           : "DataContext[" + index + "]";

            if (_targetProperty == UIElement.VisibilityProperty && Converter == null)
                Converter = BooleanToVisibilityConverter;

            var binding = new Binding(path) {Mode = BindingMode.TwoWay, Converter = Converter};
            if (ElementName != null) binding.ElementName = ElementName;
            if (FallbackValue != null) binding.FallbackValue = FallbackValue;
            if (TargetNullValue != null) binding.TargetNullValue = TargetNullValue; 
            if (RelativeSource != null) binding.RelativeSource = RelativeSource;

            _targetObject.SetBinding(_targetProperty, binding);
            //if (DefaultValue != null && _targetProperty.PropertyType.IsValueType && _targetProperty != UIElement.VisibilityProperty)
            //{
            //    if (DefaultValue is string)
            //        DefaultValue =
            //            TypeDescriptor.GetConverter(_targetProperty.PropertyType)
            //                          .ConvertFromString(DefaultValue.ToString());
            //    return DefaultValue;
            //}

            return binding.ProvideValue(serviceProvider);
        }
    }
}
