using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static MyAutocount.Utils;

namespace MyAutocount
{
    public class MySettings
    {
        const string SettingsJsonFilename = "settings.json";
        public string ipAddress;
        public string port;
        public string databaseName;
        public string serverName;
        public string saPassword;

        public void PrintInfo()
        {
            Console.WriteLine($"IP Address: {ipAddress}\nPort: {port}\nDatabase Name: {databaseName}\n" +
                $"Server Name: {serverName}\nSa Password: {saPassword}");
        }

        public static MySettings GetMySettings()
        {
            if (File.Exists(SettingsJsonFilename))
            {
                using (StreamReader r = new StreamReader(SettingsJsonFilename))
                {
                    string json = r.ReadToEnd();
                    MySettings settings = JsonConvert.DeserializeObject<MySettings>(json);
                    return settings;
                }
            } else
            {
                MySettings settings = WriteMySettings();
                return settings;
            }
        }

        public static MySettings WriteMySettings()
        {
            string appDataPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string dmfPath = System.IO.Path.Combine(appDataPath + "\\AutoCount\\Accounting 2", "A2006.dmf");
           
            try
            {
                DataTable table = DatabaseInfo.DatabaseManagementFileToDataTable(dmfPath);
                DatabaseInfo databaseInfo = new DatabaseInfo(table, 0);

                MySettings settings = new MySettings()
                {
                    // Write default settings to settings.json if not exists
                    ipAddress = "localhost",
                    port = "8888",
                    databaseName = databaseInfo.databaseName,
                    serverName = databaseInfo.serverName,
                    saPassword = "oCt2005-ShenZhou6_A2006"
                };

                string json = JsonConvert.SerializeObject(settings);
                System.IO.File.WriteAllText(SettingsJsonFilename, json);
                return settings;

            } catch (System.IO.FileNotFoundException)
            {
                print("DMF File is not found. Check if \"A2006.dmf\" file is located at: C:\\ProgramData\\AutoCount\\Accounting 2\\ ");
                return null;
            }
            

            
        }

    }
}
