using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using NLog;

namespace RadWindowAsMainWindow
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{

		protected override void OnStartup(StartupEventArgs e)
		{
			new MainWindow().Show();
			base.OnStartup(e);
		}

        public App()
            : base()
        {
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        { 
            LogManager.GetCurrentClassLogger().Log(LogLevel.Error, e.Exception);

            e.Handled = true;
        }
	}
}
