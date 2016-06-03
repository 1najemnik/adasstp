using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Foundation
{
    [DataContract]
    public class ViewModelBase : PropertyNameProvider, INotifyPropertyChanging, INotifyPropertyChanged
    {
        protected Dictionary<string, object> Values = new Dictionary<string, object>();
        private const string IndexerName = System.Windows.Data.Binding.IndexerName; /* "Item[]" */
        public event PropertyChangingEventHandler PropertyChanging = (sender, args) => { };
        public event PropertyChangedEventHandler PropertyChanged = (sender, args) => { };

        public object this[string key]
        {
            get { return Values.ContainsKey(key) ? Values[key] : null; }
            set
            {
                RaisePropertyChanging(IndexerName);
                if (Values.ContainsKey(key)) Values[key] = value;
                else Values.Add(key, value);
                RaisePropertyChanged(IndexerName);
            }
        }

        public object this[string key, object defaultValue]
        {
            get
            {
                if (Values.ContainsKey(key)) return Values[key];
                Values.Add(key, defaultValue);
                return defaultValue;
            }
            set { this[key] = value; }
        }

        [DataMember]
        private Dictionary<string, object> ImplicitProperties
        {
            get
            {
                var explicitProperties = GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Static |
                                   BindingFlags.Public | BindingFlags.NonPublic)
                    .Select(p => p.Name).ToArray();

                return Values.Where(pair => !explicitProperties.Contains(pair.Key))
                    .ToDictionary(pair => pair.Key, pair => pair.Value is ValueType ? pair.Value.ToString() : pair.Value);
            }
            set { if (value != null) value.ToList().ForEach(pair => this[pair.Key] = pair.Value); }
        }

        public void RaisePropertyChanging(string propertyName)
        {
            PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        [OnDeserializing]
        private void Initialize(StreamingContext context = default(StreamingContext))
        {
            if (PropertyChanging == null) PropertyChanging = (sender, args) => { };
            if (PropertyChanged == null) PropertyChanged = (sender, args) => { };
            if (Values == null) Values = new Dictionary<string, object>();
        }
    }
}