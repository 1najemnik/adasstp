using System;
using ADLite;

namespace AlfaDirectAutomation
{
    public abstract class AlfaDirectConnection
    {
        /// <summary>
        /// Объект Альфа-Директ
        /// </summary>
        public static AlfaDirect adObj { get; set; }
        private static int connectTimeoutCount = 2;
        private static string ResultMessage;

        private static string Password { get; set; }
        private static string Login { get; set; }

        private static int connectTimeoutCurrent { get; set; }
         
        public static void Initialize(string password, string login)
        {
            Login = login;
            Password = password;
            adObj = new ADLite.AlfaDirectClass();
        }

        public static void Connect()
        {
            if (AlfaDirectConnection.adObj.Connected)
                return;

            AlfaDirectConnection.adObj.Password = Password;
            AlfaDirectConnection.adObj.UserName = Login;
            AlfaDirectConnection.adObj.Connected = true;
        }
        
        public static void Shutdown()
        {  
            AlfaDirectConnection.adObj = null;
            GC.Collect();
        }
    }
}
