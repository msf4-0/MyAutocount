using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAutocount
{
    class Sql
    {
        public static string RunSqlQuery(AutoCount.Authentication.UserSession userSession, string query, object[] paramsArray)
        {
            DataTable table = userSession.DBSetting.GetDataTable(query, false, paramsArray);
            return Utils.DataTableToJsonString(table);
        }
        public static string GetAllFromSql(AutoCount.Authentication.UserSession userSession, string tableName)
        {
            string dbName = userSession.DBSetting.DBName;
            string query = $"SELECT * FROM [{dbName}].[dbo].[{tableName}]";
            DataTable table = userSession.DBSetting.GetDataTable(query, false);
            return Utils.DataTableToJsonString(table);
        }

        public static string GetSingleFromSql(AutoCount.Authentication.UserSession userSession, string tableName, string keyName, string key)
        {
            string dbName = userSession.DBSetting.DBName;
            string query = $"SELECT * FROM [{dbName}].[dbo].{tableName} WHERE {keyName} = @{keyName}";
            DataTable table = userSession.DBSetting.GetDataTable(query, false, new object[] {
                new SqlParameter(keyName, key),
            });
            return Utils.DataTableToJsonString(table);
        }

        public static string GetSingleDetailFromSql(AutoCount.Authentication.UserSession userSession, string table, string detailTable, string docNo)
        {
            string dbName = userSession.DBSetting.DBName;
            string query = $"SELECT * FROM [{dbName}].[dbo].[{detailTable}] " +
                $"WHERE DocKey IN (SELECT DocKey FROM [{dbName}].[dbo].[{table}] " +
                $"WHERE DocNo = @docNo)";
            object[] paramsList = new object[]
            {
                new SqlParameter("docNo", docNo)
            };
            return RunSqlQuery(userSession, query, paramsList);
        }

        public static string TestSql(AutoCount.Authentication.UserSession userSession, string query)
        {
            DataTable table = userSession.DBSetting.GetDataTable(query, false, new object[] { });
            return Utils.DataTableToJsonString(table);
        }



    }
}
