using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Foundation
{
    public static class Store
    {
        private static readonly Dictionary<Type, object> StoredItemsDictionary = new Dictionary<Type, object>();

        public static TItem OfType<TItem>(params object[] args) where TItem : class
        {
            var itemType = typeof (TItem);
            if (StoredItemsDictionary.ContainsKey(itemType))
                return (TItem) StoredItemsDictionary[itemType];

            var hasDataContract = Attribute.IsDefined(itemType, typeof (DataContractAttribute));
            var item = hasDataContract
                ? Serializer.DeserializeDataContract<TItem>() ?? (TItem) Activator.CreateInstance(itemType, args)
                : (TItem) Activator.CreateInstance(itemType, args);

            StoredItemsDictionary.Add(itemType, item);
            return (TItem) StoredItemsDictionary[itemType];
        }

        public static void Snapshot()
        {
            StoredItemsDictionary
                .Where(p => Attribute.IsDefined(p.Key, typeof (DataContractAttribute)))
                .Select(p => p.Value).ToList()
                .ForEach(i => i.SerializeDataContract());
        }
    }
}
