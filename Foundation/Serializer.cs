using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Windows.Media;

namespace Foundation
{
    public static class Serializer
    {
        public const string JsonExtension = ".json";

        public static readonly List<Type> KnownTypes = new List<Type>
        {
            typeof (Type),
            typeof (Dictionary<string, string>),
            typeof (SolidColorBrush),
            typeof (MatrixTransform),
        };

        public static void SerializeDataContract(this object item, string file = null, Type type = null)
        {
            try
            {
                type = type ?? item.GetType();
                if (string.IsNullOrEmpty(file))
                    file = type.Name + JsonExtension;
                var serializer = new DataContractJsonSerializer(type, KnownTypes);
                using (var stream = File.Create(file))
                {
                    var currentCulture = Thread.CurrentThread.CurrentCulture;
                    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                    serializer.WriteObject(stream, item);
                    Thread.CurrentThread.CurrentCulture = currentCulture;
                }
            }
            catch (Exception exception)
            {
                Trace.WriteLine("Can not serialize json data contract");
                Trace.WriteLine(exception.StackTrace);
            }
        }

        public static TItem DeserializeDataContract<TItem>(string file = null)
        {
            try
            {
                if (string.IsNullOrEmpty(file))
                    file = typeof (TItem).Name + JsonExtension;
                var serializer = new DataContractJsonSerializer(typeof (TItem), KnownTypes);
                using (var stream = File.OpenRead(file))
                {
                    var currentCulture = Thread.CurrentThread.CurrentCulture;
                    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                    var item = (TItem) serializer.ReadObject(stream);
                    Thread.CurrentThread.CurrentCulture = currentCulture;
                    return item;
                }
            }
            catch
            {
                return default(TItem);
            }
        }
    }
}
