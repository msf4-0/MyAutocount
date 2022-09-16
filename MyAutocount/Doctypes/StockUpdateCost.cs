using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json;
using static MyAutocount.Utils;

namespace MyAutocount.Doctypes
{
    // Use to update item cost
    // Delete StockUpdateCost doc will not recover to old cost
    // Old cost just for reference
    public class StockUpdateCost : NancyModule
    {
        const string DoctypeName = "StockUpdateCost";
        const string DatabaseTable = "vStockUpdateCost";
        const string DetailTable = "vStockUpdateCostDetail";
        AutoCount.Data.DBSetting dbSetting;
        AutoCount.Authentication.UserSession userSession;

        public StockUpdateCost()
        {
            //this.dbSetting = new AutoCount.Data.DBSetting(AutoCount.Data.DBServerType.SQL2000, ".\\A2006", "test", "test123456", "AED_Shrdc");
            //this.userSession = new AutoCount.Authentication.UserSession(dbSetting);
            dbSetting = Auth.dbSetting;
            userSession = Auth.userSession;
            Run();
        }

        public static void Test()
        {
            StockUpdateCost stockUpdateCost = new StockUpdateCost();
            Utils.print(stockUpdateCost.GetAll());

        }

        private void Run()
        {
            Get($"/{DoctypeName}/getAll", _ =>
            {
                try
                {
                    Response response = GetAll();
                    return response;
                }
                catch (Exception ex)
                {
                    Log(ex.ToString());
                    Response response = ex.Message;
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    return response;
                }

            });

            Get($"/{DoctypeName}/getSingle/{{docNo}}", args =>
            {
                try
                {
                    Response response = GetSingle(args.docNo);
                    return response;
                }
                catch (Exception ex)
                {
                    Log(ex.ToString());
                    Response response = ex.Message;
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    return response;
                }
            });

            Get($"/{DoctypeName}/getDetail/{{docNo}}", args => {
                try
                {
                    Response response = GetSingleDetail(args.docNo);
                    return response;
                }
                catch (Exception ex)
                {
                    Log(ex.ToString());
                    Response response = ex.Message;
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    return response;
                }
            });

            Post($"/{DoctypeName}/add", args =>
            {
                string jsonString = Nancy.IO.RequestStream.FromStream(this.Request.Body).AsString();
                dynamic jsonData = JsonConvert.DeserializeObject(jsonString);

                string docNo = jsonData.docNo;
                string dateString = jsonData.dateString;
                string description = jsonData.description;
                
                dynamic detailList = jsonData.detailList;
                List<StockUpdateCostDetailObject> detailObjectsList = new List<StockUpdateCostDetailObject>();

                foreach (dynamic detailObject in detailList)
                {
                    detailObjectsList.Add(new StockUpdateCostDetailObject(
                        detailObject.itemCode.ToString(),
                        detailObject.uom.ToString(),
                        decimal.Parse(detailObject.newCost.ToString())
                        ));
                }

                try
                {
                    Response response = Add(docNo, dateString, description, detailObjectsList);
                    return response;
                }
                catch (Exception ex)
                {
                    Log(ex.ToString());
                    Response response = ex.Message;
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    return response;
                }

            });

            Put($"/{DoctypeName}/edit", args =>
            {
                string jsonString = Nancy.IO.RequestStream.FromStream(this.Request.Body).AsString();
                dynamic jsonData = JsonConvert.DeserializeObject(jsonString);

                string docNo = jsonData.docNo;
                string dateString = jsonData.dateString;
                string description = jsonData.description;
                
                dynamic detailList = jsonData.detailList;
                List<StockUpdateCostDetailObject> detailObjectsList = new List<StockUpdateCostDetailObject>();

                foreach (dynamic detailObject in detailList)
                {
                    detailObjectsList.Add(new StockUpdateCostDetailObject(
                        detailObject.itemCode.ToString(),
                        detailObject.uom.ToString(),
                        decimal.Parse(detailObject.newCost.ToString())
                        ));
                }

                try
                {
                    Response response = Edit(docNo, dateString, description, detailObjectsList);
                    return response;
                }
                catch (Exception ex)
                {
                    Log(ex.ToString());
                    Response response = ex.Message;
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    return response;
                }

            });

            Delete($"/{DoctypeName}/delete/{{docNo}}", args => {
                try
                {
                    Response response = Delete(args.docNo);
                    return response;
                }
                catch (Exception ex)
                {
                    Log(ex.ToString());
                    Response response = ex.Message;
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    return response;
                }
            });

        }

        private string GetAll()
        {
            return Sql.GetAllFromSql(userSession, DatabaseTable);

        }
        private string GetSingle(string DocNo)
        {
            return Sql.GetSingleFromSql(userSession, DatabaseTable, "DocNo", DocNo);
        }
        private string GetSingleDetail(string docNo)
        {
            return Sql.GetSingleDetailFromSql(userSession, DatabaseTable, DetailTable, docNo);
        }

        private string Add(string docNo, string dateString, string description, List<StockUpdateCostDetailObject> detailObjectsList)
        {
            if (Auth.Login(userSession))
            {
                AutoCount.Stock.StockUpdateCost.StockUpdateCostCommand cmd =
                AutoCount.Stock.StockUpdateCost.StockUpdateCostCommand.Create(userSession, userSession.DBSetting);
                AutoCount.Stock.StockUpdateCost.StockUpdateCost doc = cmd.AddNew();

                // Input: dateString = 22/7/2022
                // Output: date = 22/7/2022 12:00:00 AM
                dateString = $"{dateString} 12:00:00 AM";
                DateTime date = DateTime.ParseExact(dateString, "dd-MM-yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                doc.DocNo = docNo;                   //mandatory
                doc.DocDate = date;
                doc.Description = description;

                foreach (StockUpdateCostDetailObject detailObject in detailObjectsList)
                {
                    AutoCount.Stock.StockUpdateCost.StockUpdateCostDetail detail = doc.AddDetail();
                    detail.ItemCode = detailObject.ItemCode;
                    detail.UOM = detailObject.UOM;
                    detail.NewCost = detailObject.NewCost;
                }

                doc.Save();
                Utils.Log("Saved successfully");
                return $"{DoctypeName} added: {docNo}";

            }
            return $"{DoctypeName} add error: Login failed";

        }

        private string Edit(string docNo, string dateString, string description, List<StockUpdateCostDetailObject> detailObjectsList)
        {
            if (Auth.Login(userSession))
            {
                AutoCount.Stock.StockUpdateCost.StockUpdateCostCommand cmd =
                AutoCount.Stock.StockUpdateCost.StockUpdateCostCommand.Create(userSession, userSession.DBSetting);
                AutoCount.Stock.StockUpdateCost.StockUpdateCost doc = cmd.Edit(docNo);

                // Input: dateString = 22/7/2022
                // Output: date = 22/7/2022 12:00:00 AM
                dateString = $"{dateString} 12:00:00 AM";
                DateTime date = DateTime.ParseExact(dateString, "dd-MM-yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                doc.DocDate = date;
                doc.Description = description;
                
                doc.ClearDetails();

                foreach (StockUpdateCostDetailObject detailObject in detailObjectsList)
                {
                    AutoCount.Stock.StockUpdateCost.StockUpdateCostDetail detail = doc.AddDetail();
                    detail.ItemCode = detailObject.ItemCode;
                    detail.UOM = detailObject.UOM;
                    detail.NewCost = detailObject.NewCost;
                }

                doc.Save();
                Utils.Log("Edited successfully");
                return $"{DoctypeName} edited: {docNo}";

            }
            return $"{DoctypeName} edit error: Login failed";

        }

        private string Delete(string docNo)
        {
            if (Auth.Login(userSession))
            {
                AutoCount.Stock.StockUpdateCost.StockUpdateCostCommand cmd =
                AutoCount.Stock.StockUpdateCost.StockUpdateCostCommand.Create(userSession, userSession.DBSetting);

                cmd.Delete(docNo);
                Utils.Log("Deleted successfully");
                return $"{DoctypeName} deleted: {docNo}";
            }
            return $"{DoctypeName} delete error: Login failed";

        }
    }

    internal class StockUpdateCostDetailObject
    {
        public string ItemCode { get; set; }    // mandatory
        public string UOM { get; set; }         // mandatory, must be the same as in StockItem
        public decimal NewCost { get; set; }

        public StockUpdateCostDetailObject(string itemCode, string uom, decimal newCost)
        {
            this.ItemCode = itemCode;
            this.UOM = uom;
            this.NewCost = newCost;
        }
    }

}
