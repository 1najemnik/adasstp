using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Foundation.MarkupExtensions
{
    public class Picture : MarkupExtension
    {
        public static readonly List<ResourceManager> Managers = new List<ResourceManager>();

        public Picture(string key)
        {
            Key = key.EndsWith(".png") ? key : key + ".png";
            Width = Height = 16;
        }

        public Picture(string key, int size)
        {
            Key = key.EndsWith(".png") ? key : key + ".png";
            Width = Height = size;
        }

        public string Key { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var value = Managers.Select(m => m.GetObject(Key)).LastOrDefault(v => v != null) as Image;
            if (value == null) return null;
            if (double.IsNaN(Height)) Height = value.Height;
            else value.Height = Height;
            if (double.IsNaN(Width)) Width = value.Width;
            else value.Width = Width;
            return value;
        }
    }
}
