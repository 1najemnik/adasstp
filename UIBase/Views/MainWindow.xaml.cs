using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AlfaDirectAutomation;
using Telerik.Windows.Controls;

namespace RadWindowAsMainWindow
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : RadWindow
	{ 
		public MainWindow()
		{
			InitializeComponent();
            this.Closed += new EventHandler<WindowClosedEventArgs>(MainWindow_Closed);
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            if (AlfaDirectConnection.adObj == null)
                return;

            string resultMessage;
            AlfaDirectConnection.adObj.UnSubscribeTable("orders", "", out resultMessage);

            AlfaDirectConnection.Shutdown();
        } 
	}
}
