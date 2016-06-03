using System;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Threading;
using AlfaDirectAutomation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UIBase.ViewModels;
using NLog;

namespace SeedWPF.Tests
{
    [TestClass]
    public class UIBaseTests
    {
        public static class DispatcherUtil
        {
            [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            public static void DoEvents()
            {
                DispatcherFrame frame = new DispatcherFrame();
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                    new DispatcherOperationCallback(ExitFrame), frame);
                Dispatcher.PushFrame(frame);
            }

            private static object ExitFrame(object frame)
            {
                ((DispatcherFrame)frame).Continue = false;
                return null;
            }
        }

        MainViewModel model = new MainViewModel();

        [TestInitialize]
        public void Setup()
        {
          //  DispatcherUtil.DoEvents();
          //  model.Open();
        }

       [TestMethod]
        public void SoundPlay_Test()
        {
            SoundUtility.Play(@"E:\Current\alfadirectautomationsoftstp\UIBase\bin\Debug\Sounds\Windows Pop-up Blocked.wav"); 
        }

       [TestMethod]
       public void Logs_Test()
       {  
           LogManager.GetCurrentClassLogger().Log(LogLevel.Info, new Exception("ssss"));
           model.SendLogs();
       }


        [TestCleanup]
        public void Cleanup()
        {
            DispatcherUtil.DoEvents();
            model.Stop();
        }

        [TestMethod]
        public void Test_Log()
        { 
            model.Log(LogLevel.Debug, false, "test", null);
            model.Log(LogLevel.Error, false, "test", null); 
            model.Log(LogLevel.Info, false, "test", new Exception("ssss")); 
        }

        [TestMethod]
        public void orderCheck_Test()
        {
            model.orderCheck(null);
        }

        [TestMethod]
        public void MakeSTPOrder_Test()
        {
            model.MakeSTPOrder(null, null);
        }

        [TestMethod]
        public void MakeSTPOrderInverted_Test()
        { 
            model.MakeSTPOrder(2.2, "B"); 
        } 
    }
}
