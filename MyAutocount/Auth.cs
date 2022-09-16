using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MyAutocount
{
    class Auth
    {
        private static AutoCount.Data.DBSetting _dbSetting;
        private static AutoCount.Authentication.UserSession _userSession;
        public static string ipAddress;
        public static string port;
        private static string databaseName;
        private static string serverName;
        private static string saPassword;

        public static AutoCount.Data.DBSetting dbSetting
        {
            get
            {
                if (_dbSetting == null)
                {
                    InitDbSetting();
                    return _dbSetting;
                }

                return _dbSetting;
            }
            set
            {
                _dbSetting = dbSetting;
            }
        }

        public static AutoCount.Authentication.UserSession userSession
        {
            get
            {
                if (_userSession == null)
                {
                    InitUserSession();
                    return _userSession;
                }
                
                return _userSession;
            }
            set
            {
                _userSession = userSession;
            }
        }

        public static void InitDbSetting()
        {
            // For testing
            //_dbSetting = new AutoCount.Data.DBSetting(AutoCount.Data.DBServerType.SQL2000, "(local)\\A2006", "sa", "oCt2005-ShenZhou6_A2006", "AED_Shrdc");

            // For release
            _dbSetting = new AutoCount.Data.DBSetting(AutoCount.Data.DBServerType.SQL2000, serverName, "sa", saPassword, databaseName);

        }

        public static void InitUserSession()
        {
            _userSession = new AutoCount.Authentication.UserSession(_dbSetting);
        }


        public static bool Login(AutoCount.Authentication.UserSession userSession, string username = "ADMIN", string password = "admin")
        {
            if (!userSession.IsLogin)
            {
                if (userSession.Login(username, password))
                {
                    Utils.Log("Login successfully");
                    return true;
                }
                Utils.Log("Login failed");
                return false;
            }
            Utils.Log("Already login.");
            return true;
        }

        public static void InitCrudentials()
        {
            MySettings settings = MySettings.GetMySettings();
            ipAddress = settings.ipAddress;
            port = settings.port;
            databaseName = settings.databaseName;
            serverName = settings.serverName;
            saPassword = settings.saPassword;
        }

        public static void Init()
        {
            InitCrudentials();
            InitDbSetting();
            InitUserSession();
            Login(_userSession);
        }


    }
}
