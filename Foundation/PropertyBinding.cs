using System;
using System.ComponentModel;

namespace Foundation
{
    public class PropertyBinding
    {
        public PropertyBinding(string propertyName)
        {
            PropertyName = propertyName;
        }

        public event PropertyChangingEventHandler PropertyChanging = (sender, args) => { };
        public event PropertyChangedEventHandler PropertyChanged = (sender, args) => { };
        public event Func<string> Validation = () => null;

        public string PropertyName { get; private set; }

        public void InvokePropertyChanging(object sender, PropertyChangingEventArgs args)
        {
            PropertyChanging(sender, new PropertyChangingEventArgs(PropertyName));
        }

        public void InvokePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            PropertyChanged(sender, new PropertyChangedEventArgs(PropertyName));
        }

        public string InvokeValidation()
        {
            return Validation();
        }
    }
}