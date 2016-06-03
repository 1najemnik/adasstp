using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows;

namespace Foundation
{
    public static class ResourceHelper
    {
        public static bool ResourceExists(string resourcePath)
        {
            var assembly = Application.ResourceAssembly;
            return ResourceExists(assembly, resourcePath);
        }

        public static bool ResourceExists(Assembly assembly, string resourceKey)
        {
            var contains = GetResourcePaths(assembly).Contains(resourceKey.ToLower());
            return contains;
        }

        public static IEnumerable<object> GetResourcePaths(Assembly assembly)
        {
            var culture = CultureInfo.CurrentCulture;
            var resourceName = assembly.GetName().Name + ".g";
            var resourceManager = new ResourceManager(resourceName, assembly);

            try
            {
                var resourceSet = resourceManager.GetResourceSet(culture, true, true);
                foreach (DictionaryEntry resource in resourceSet)
                {
                    yield return resource.Key;
                }
            }
            finally
            {
                resourceManager.ReleaseAllResources();
            }
        }
    }
}
