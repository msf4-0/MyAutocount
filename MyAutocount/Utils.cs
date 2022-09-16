using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MyAutocount
{
    class Utils
    {
        public static void print(object str)
        {
            Console.WriteLine(str.ToString());
        }

        public static string PrintTime()
        {
            string time = DateTime.Now.ToString("HH:mm:ss.ffffff", System.Globalization.DateTimeFormatInfo.InvariantInfo);
            return time;
        }

        public static void Log(string str)
        {
            print($"{PrintTime()} : {str}");
        }

        public static void Output(string str)
        {
            print($"Output: {str}");
        }
        public static string DataTableToJsonString(DataTable dataTable)
        {
            return JsonConvert.SerializeObject(dataTable, Formatting.Indented);
        }

        /// <summary>
        /// Input: dd-MM-yyyy , output: dd-MM-yyyy 12:00:00 AM
        /// </summary>
        /// <param name="dateString"></param>
        /// <returns></returns>
        public static DateTime DateStringToDateTime(string dateString)
        {
            dateString = $"{dateString} 12:00:00 AM";
            DateTime date = DateTime.ParseExact(dateString, "dd-MM-yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
            return date;
        }


    }

}