using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using Foundation;
using Microsoft.Win32;
using RadWindowAsMainWindow.Properties;
using UIBase.Properties;

namespace UIBase.ViewModels
{
    [DataContract]
    internal class SettingsViewModel : ViewModel
    { 
        public SettingsViewModel()
        {
            Initialize();
        }

        public int OrderQuantity
        {
            get { return Get(() => OrderQuantity); }
            set { Set(() => OrderQuantity, value); }
        }

        public string SoundFileName
        {
            get { return Get(() => SoundFileName); }
            set { Set(() => SoundFileName, value); }
        }

        public double StopPrice
        {
            get { return Get(() => StopPrice); }
            set { Set(() => StopPrice, value); }
        }

        public double Slippage
        {
            get { return Get(() => Slippage); }
            set { Set(() => Slippage, value); }
        }
          
        [OnDeserialized]
        private void Initialize(StreamingContext streamingContext = default(StreamingContext))
        {
            OrderQuantity = Settings.Default.OrderQuantity;
            Slippage = Settings.Default.Slippage;
            SoundFileName = Path.GetFileName(Settings.Default.SoundFileName); 
             
            StopPrice = Settings.Default.StopPrice;
            this[ApplicationCommands.Save].Executed += (sender, args) => Save();
            this[ApplicationCommands.Open].Executed += (sender, args) => Browse();
        }

        private void Browse()
        { 
            var d = new OpenFileDialog();
            d.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory + "Sounds";

            if (d.ShowDialog() == true)
            {
                SoundFileName = Path.GetFileName(d.FileName);
                Settings.Default.SoundFileName = d.FileName;
                Settings.Default.Save();
            }
        }

        private void Save()
        {
            try
            {
                Settings.Default.StopPrice = StopPrice;
                Settings.Default.OrderQuantity = OrderQuantity;
                Settings.Default.Slippage = Slippage;
                Settings.Default.Save();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
         
    }
}