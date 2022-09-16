using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MyAutocount
{
    public class DatabaseInfo
    {
        public string companyName { get; set; }
        public string remark { get; set; }
        public string databaseName { get; set; }
        public string serverName { get; set; }

        public DatabaseInfo(DataTable table, int row)
        {
            companyName = table.Rows[row].Field<string>("CompanyName");
            remark = table.Rows[row].Field<string>("Remark");
            databaseName = table.Rows[row].Field<string>("DatabaseName");
            serverName = table.Rows[row].Field<string>("ServerName");

        }

        public static DataTable DatabaseManagementFileToDataTable(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            DataTable table = new DataTable();

            table.Columns.Add("CompanyName", typeof(String));
            table.Columns.Add("Remark", typeof(String));
            table.Columns.Add("DatabaseName", typeof(String));
            table.Columns.Add("ServerName", typeof(String));

            foreach (XmlNode node in doc.DocumentElement.SelectNodes("/AutoCountDatabase/DatabaseInfo"))
            {
                string companyName = node.SelectSingleNode("CompanyName").InnerText;
                string remark = node.SelectSingleNode("Remark").InnerText;
                string databaseName = node.SelectSingleNode("DatabaseName").InnerText;
                string serverName = node.SelectSingleNode("ServerName").InnerText;

                DataRow row = table.NewRow();
                row["CompanyName"] = companyName;
                row["Remark"] = remark;
                row["DatabaseName"] = databaseName;
                row["ServerName"] = serverName;
                table.Rows.Add(row);

            }

            return table;

        }
    }
}
