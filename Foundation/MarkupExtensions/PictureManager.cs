using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Foundation.MarkupExtensions
{
    public class PictureManager : ResourceManager
    {
        public PictureManager(string basePath, Assembly resourceAssembly = null)
        {
            BasePath = basePath.Replace('\\', '/');
            BasePath = BasePath.StartsWith("/") ? BasePath.Substring(1) : BasePath;
            ResourceAssemblyName = (resourceAssembly ?? Assembly.GetCallingAssembly()).GetName().Name;
            ResourceAssembly = resourceAssembly;
        }

        public string BasePath { get; private set; }
        public Assembly ResourceAssembly { get; private set; }
        private string ResourceAssemblyName { get; set; }

        public override object GetObject(string key)
        {
            try
            {
                key = Path.Combine(BasePath, key).Replace('\\', '/');
                if (!ResourceHelper.ResourceExists(ResourceAssembly, key)) return null;
                key = key.StartsWith("/") || key.StartsWith("component") ? key : "/" + key;
                key = key.StartsWith("component") ? ";" + key : ";component" + key;
                var uri = new Uri("pack://application:,,,/" + ResourceAssemblyName + key);
                var image = new BitmapImage(uri);
                return new Image {Source = image};
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
