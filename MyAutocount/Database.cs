using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;

namespace MyAutocount
{
    public class Database : NancyModule
    {
        AutoCount.Data.DBSetting dbSetting;
        AutoCount.Authentication.UserSession userSession;
        public Database()
        {
            dbSetting = Auth.dbSetting;
            userSession = Auth.userSession;
            Run();

        }

        public static void Test()
        {
            Database dtb = new Database();
            Utils.print(dtb.GetTId());

        }

        private void Run() 
        {
            Get($"/getTId", _ => {
                try
                {
                    return GetTId();
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }

            });

        }

        public string GetTId()
        {
            string tid = Sql.RunSqlQuery(userSession, "SELECT TOP(1) [Transaction ID] FROM sys.fn_dblog(null,null) ORDER BY [Begin Time] DESC;", 
                new object[] { });
            return tid;
        }

    }
}
