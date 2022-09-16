using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json;
using static MyAutocount.Utils;

namespace MyAutocount.Doctypes
{
    public class StockItem : NancyModule
    {
        const string DoctypeName = "StockItem";
        const string DatabaseTable = "Item";
        const string UOMTable = "ItemUOM";
        AutoCount.Data.DBSetting dbSetting;
        AutoCount.Authentication.UserSession userSession;

        public StockItem()
        {
            //this.dbSetting = new AutoCount.Data.DBSetting(AutoCount.Data.DBServerType.SQL2000, ".\\A2006", "test", "test123456", "AED_Shrdc");
            //this.userSession = new AutoCount.Authentication.UserSession(dbSetting);
            dbSetting = Auth.dbSetting;
            userSession = Auth.userSession;
            Run();
        }

        public static void Test()
        {
            StockItem stockItem = new StockItem();

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

            Get($"/{DoctypeName}/getSingle/{{itemCode}}", args =>
            {
                try
                {
                    Response response = GetSingle(args.itemCode);
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

                string itemCode = jsonData.itemCode;
                string description = jsonData.description;
                string uom = jsonData.uom;

                string unitCost = jsonData.unitCost;
                string price = jsonData.price;
                string costingMethod = jsonData.costingMethod;
                string itemGroup = jsonData.itemGroup;
                string leadTime = jsonData.leadTime;
                string dutyRate = jsonData.dutyRate;
                string taxType = jsonData.taxType;
                string purchaseTaxType = jsonData.purchaseTaxType;

                try
                {
                    Response response = Add(itemCode, description, uom, unitCost, price, costingMethod, itemGroup,
                        leadTime, dutyRate, taxType, purchaseTaxType);
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

                string itemCode = jsonData.itemCode;
                string description = jsonData.description;
                string uom = jsonData.uom;

                string unitCost = jsonData.unitCost;
                string price = jsonData.price;
                string costingMethod = jsonData.costingMethod;
                string itemGroup = jsonData.itemGroup;
                string leadTime = jsonData.leadTime;
                string dutyRate = jsonData.dutyRate;
                string taxType = jsonData.taxType;
                string purchaseTaxType = jsonData.purchaseTaxType;

                try
                {
                    Response response = Edit(itemCode, description, uom, unitCost, price, costingMethod, itemGroup,
                        leadTime, dutyRate, taxType, purchaseTaxType);
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
            string dbName = userSession.DBSetting.DBName;

            string query = $"SELECT tableItem.*, tableItemUOM.Price, tableItemUOM.Cost " +
                $"FROM [{dbName}].[dbo].[{UOMTable}] tableItemUOM " +
                $"INNER JOIN[{dbName}].[dbo].[{DatabaseTable}] tableItem " +
                $"ON tableItemUOM.ItemCode = tableItem.ItemCode";

            object[] paramsList = new object[] {};

            return Sql.RunSqlQuery(userSession, query, paramsList);


        }
        private string GetSingle(string itemCode)
        {

            string dbName = userSession.DBSetting.DBName;

            string query = $"SELECT tableItem.*, tableItemUOM.Price, tableItemUOM.Cost " +
                $"FROM [{dbName}].[dbo].[{UOMTable}] tableItemUOM " +
                $"INNER JOIN[{dbName}].[dbo].[{DatabaseTable}] tableItem " +
                $"ON tableItemUOM.ItemCode = tableItem.ItemCode " +
                $"WHERE tableItem.ItemCode = @itemCode" ;

            object[] paramsList = new object[]
            {
                new SqlParameter("itemCode", itemCode)
            };

            return Sql.RunSqlQuery(userSession, query, paramsList);
        }

        private string Add(string itemCode, string description, string uom, string unitCost, string price, 
            string costingMethod, string itemGroup, string leadTime, string dutyRate, string taxType, 
            string purchaseTaxType)
        {
            if (Auth.Login(userSession))
            {
                bool recalculate = false;
                Utils.Log("Creating ItemDataAccess cmd...");
                AutoCount.Stock.Item.ItemDataAccess cmd =
                    AutoCount.Stock.Item.ItemDataAccess.Create(userSession, userSession.DBSetting);
                Utils.Log("Created ItemDataAccess cmd done.");
                Utils.Log("Creating NewItem... *Very slow*");
                AutoCount.Stock.Item.ItemEntity itemEntity = cmd.NewItem(); // ** Takes very long time !!!
                Utils.Log("Created NewItem done.");

                Utils.Log("Adding stuff into NewItem entity...");
                itemEntity.ItemCode = itemCode;

                itemEntity.Description = description;
                itemEntity.BaseUomRecord.Uom = uom;

                if (!string.IsNullOrEmpty(unitCost)) 
                {
                    decimal x = decimal.Parse(unitCost.ToString());
                    itemEntity.BaseUomRecord.StandardCost = x;
                }

                if (!string.IsNullOrEmpty(price))
                {
                    decimal x = decimal.Parse(price.ToString());
                    itemEntity.BaseUomRecord.StandardSellingPrice = x;
                }

                if (!string.IsNullOrEmpty(unitCost) && !string.IsNullOrEmpty(price))
                {
                    if (decimal.Parse(unitCost.ToString()) > decimal.Parse(price.ToString()))
                    {
                        throw new Exception("Price must be greater or equal to cost.");
                    }
                }


                //0 : Fixed Cost
                //1 : Weighted Average
                //2 : FIFO
                //3 : LIFO

                if (!string.IsNullOrEmpty(costingMethod))
                {
                    int x = int.Parse(costingMethod.ToString());
                    itemEntity.CostingMethod = x;
                }

                if (!string.IsNullOrEmpty(dutyRate))
                {
                    decimal x = decimal.Parse(dutyRate.ToString());
                    itemEntity.DutyRate = x;
                }

                itemEntity.ItemGroup = itemGroup;
                itemEntity.LeadTime = leadTime;
                itemEntity.TaxType = taxType;
                itemEntity.PurchaseTaxType = purchaseTaxType;

                Utils.Log("Added stuff into NewItem entity done.");
                Utils.Log("Saving data... (Sometimes slow)");
                cmd.SaveData(itemEntity, ref recalculate);
                Utils.Log("Saved data done.");

                Utils.Log($"Saved {itemCode} successfully");
                return $"{DoctypeName} added: {itemCode}";

            }
            return $"{DoctypeName} add error: Login failed";
        }

        private string Edit(string itemCode, string description, string uom, string unitCost, string price,
            string costingMethod, string itemGroup, string leadTime, string dutyRate, string taxType,
            string purchaseTaxType)
        {
            if (Auth.Login(userSession))
            {
                bool recalculate = true;

                AutoCount.Stock.Item.ItemDataAccess cmd =
                    AutoCount.Stock.Item.ItemDataAccess.Create(userSession, userSession.DBSetting);
                AutoCount.Stock.Item.ItemEntity itemEntity;

                itemEntity = cmd.LoadItem(itemCode, AutoCount.Stock.Item.ItemEntryAction.Edit);
                itemEntity.Description = description;
                itemEntity.BaseUomRecord.Uom = uom;

                if (!string.IsNullOrEmpty(unitCost))
                {
                    decimal x = decimal.Parse(unitCost.ToString());
                    itemEntity.BaseUomRecord.StandardCost = x;
                } 

                if (!string.IsNullOrEmpty(price))
                {
                    decimal x = decimal.Parse(price.ToString());
                    itemEntity.BaseUomRecord.StandardSellingPrice = x;
                } 

                if (!string.IsNullOrEmpty(costingMethod))
                {
                    int x = int.Parse(costingMethod.ToString());
                    itemEntity.CostingMethod = x;
                }

                if (!string.IsNullOrEmpty(dutyRate))
                {
                    decimal x = decimal.Parse(dutyRate.ToString());
                    itemEntity.DutyRate = x;
                }

                if (!string.IsNullOrEmpty(unitCost) && !string.IsNullOrEmpty(price))
                {
                    if (decimal.Parse(unitCost.ToString()) > decimal.Parse(price.ToString()))
                    {
                        throw new Exception("Price must be greater or equal to cost.");
                    }
                }

                itemEntity.ItemGroup = itemGroup;
                itemEntity.LeadTime = leadTime;
                itemEntity.TaxType = taxType;
                itemEntity.PurchaseTaxType = purchaseTaxType;

                cmd.SaveData(itemEntity, ref recalculate);
                Utils.Log($"Edited {itemCode} successfully");
                return $"{DoctypeName} edited: {itemCode}";
            }
            return $"{DoctypeName} edit error: Login failed";

        }

        private string Delete(string itemCode)
        {
            if (Auth.Login(userSession))
            {
                AutoCount.Stock.Item.ItemDataAccess cmd = 
                    AutoCount.Stock.Item.ItemDataAccess.Create(userSession, userSession.DBSetting);
                cmd.Delete(itemCode);
                Utils.Log($"Deleted {itemCode} successfully");
                return $"{DoctypeName} deleted: {itemCode}";
            }
            return $"{DoctypeName} delete error: Login failed";
        }




    }
}
