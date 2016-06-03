using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlfaDirectAutomation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SeedWPF.Tests
{ 
    [TestClass]
    public class AlfaDirectAutomationTests
    {
        public static AlfaDirectProvider provider;

        [TestInitialize]
        public void SetupTests()
        {
 

            AlfaDirectConnection.Initialize("", "");
            AlfaDirectConnection.Connect();

            provider = new AlfaDirectProvider("103487", "GOLD-3.16", "52205", "FORTS");
        }

        [TestMethod]
        public void TestMethod1()
        {
            var stockItemResult = provider.GetOrdersStatusesCallResult();
            if (!stockItemResult.Success)
            {
                
            }
    
        }

        [TestCleanup]
        public void CleanupTests()
        {
            AlfaDirectConnection.Shutdown();
        }
    }
}
