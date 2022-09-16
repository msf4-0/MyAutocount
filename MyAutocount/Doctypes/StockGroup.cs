using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json;
using static MyAutocount.Utils;

namespace MyAutocount.Doctypes
{
    public class StockGroup : NancyModule
    {
        const string DoctypeName = "StockGroup";
        const string DatabaseTable = "vStockGroup";
        AutoCount.Data.DBSetting dbSetting;
        AutoCount.Authentication.UserSession userSession;

        public StockGroup()
        {
            //this.dbSetting = new AutoCount.Data.DBSetting(AutoCount.Data.DBServerType.SQL2000, ".\\A2006", "test", "test123456", "AED_Shrdc");
            //this.userSession = new AutoCount.Authentication.UserSession(dbSetting);
            dbSetting = Auth.dbSetting;
            userSession = Auth.userSession;
            Run();
        }

        public static void Test()
        {
            StockGroup stockGroup = new StockGroup();
            Utils.print(stockGroup.GetAll());
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

            Get($"/{DoctypeName}/getSingle/{{itemGroup}}", args =>
            {
                try
                {
                    Response response = GetSingle(args.itemGroup);
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

            // GetGeneralAccountCodes
            Get($"/{DoctypeName}/getCodes", _ => {
                try
                {
                    Response response = GetGeneralAccountCodes();
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

                string itemGroup = jsonData.itemGroup;
                string description = jsonData.description;
                dynamic stockCodes = jsonData.stockCodes;

                StockGroupCodes stockGroupCodes = new StockGroupCodes(
                        stockCodes.SalesCode.ToString(),
                        stockCodes.CashSalesCode.ToString(),
                        stockCodes.SalesReturnCode.ToString(),
                        stockCodes.SalesDiscountCode.ToString(),
                        stockCodes.PurchaseCode.ToString(),
                        stockCodes.CashPurchaseCode.ToString(),
                        stockCodes.PurchaseReturnCode.ToString(),
                        stockCodes.PurchaseDiscountCode.ToString(),
                        stockCodes.BalanceStockCode.ToString()			// 330-0000
                    );

                try
                {
                    Response response = Add(itemGroup, description, stockGroupCodes);
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

                string itemGroup = jsonData.itemGroup;
                string description = jsonData.description;

                dynamic stockCodes = jsonData.stockCodes;

                StockGroupCodes stockGroupCodes = new StockGroupCodes(
                        stockCodes.SalesCode.ToString(),
                        stockCodes.CashSalesCode.ToString(),
                        stockCodes.SalesReturnCode.ToString(),
                        stockCodes.SalesDiscountCode.ToString(),
                        stockCodes.PurchaseCode.ToString(),
                        stockCodes.CashPurchaseCode.ToString(),
                        stockCodes.PurchaseReturnCode.ToString(),
                        stockCodes.PurchaseDiscountCode.ToString(),
                        stockCodes.BalanceStockCode.ToString()
                    );

                try
                {
                    Response response = Edit(itemGroup, description, stockGroupCodes);
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
        private string GetSingle(string itemGroup)
        {
            return Sql.GetSingleFromSql(userSession, DatabaseTable, "itemGroup", itemGroup);
        }

        private string Add(string itemGroup, string description, StockGroupCodes stockGroupCodes)
        {
            if (Auth.Login(userSession))
            {
                AutoCount.Stock.ItemGroup.ItemGroupCommand cmd =
                AutoCount.Stock.ItemGroup.ItemGroupCommand.Create(userSession, userSession.DBSetting);
                AutoCount.Stock.ItemGroup.ItemGroupEntity itemGroupEntity = cmd.NewItemGroup();

                itemGroupEntity.ItemGroup = itemGroup;
                itemGroupEntity.Description = description;

                itemGroupEntity.SalesCode = stockGroupCodes.SalesCode;
                itemGroupEntity.CashSalesCode = stockGroupCodes.CashSalesCode;
                itemGroupEntity.SalesReturnCode = stockGroupCodes.SalesReturnCode;
                itemGroupEntity.SalesDiscountCode = stockGroupCodes.SalesDiscountCode;

                itemGroupEntity.PurchaseCode = stockGroupCodes.PurchaseCode;
                itemGroupEntity.CashPurchaseCode = stockGroupCodes.CashPurchaseCode;
                itemGroupEntity.PurchaseReturnCode = stockGroupCodes.PurchaseReturnCode;
                itemGroupEntity.PurchaseDiscountCode = stockGroupCodes.PurchaseDiscountCode;
                itemGroupEntity.BalanceStockCode = stockGroupCodes.BalanceStockCode;

                cmd.SaveItemGroup(itemGroupEntity);
                Utils.Log("Saved successfully");
                return $"{DoctypeName} added: {itemGroup}";
            }
            return $"{DoctypeName} add error: Login failed";
        }

        private string Edit(string itemGroup, string description, StockGroupCodes stockGroupCodes)
        {
            if (Auth.Login(userSession))
            {
                AutoCount.Stock.ItemGroup.ItemGroupCommand cmd =
                    AutoCount.Stock.ItemGroup.ItemGroupCommand.Create(userSession, userSession.DBSetting);
                AutoCount.Stock.ItemGroup.ItemGroupEntity itemGroupEntity = cmd.GetItemGroup(itemGroup);

                itemGroupEntity.Description = description;

                itemGroupEntity.SalesCode = stockGroupCodes.SalesCode;
                itemGroupEntity.CashSalesCode = stockGroupCodes.CashSalesCode;
                itemGroupEntity.SalesReturnCode = stockGroupCodes.SalesReturnCode;
                itemGroupEntity.SalesDiscountCode = stockGroupCodes.SalesDiscountCode;

                itemGroupEntity.PurchaseCode = stockGroupCodes.PurchaseCode;
                itemGroupEntity.CashPurchaseCode = stockGroupCodes.CashPurchaseCode;
                itemGroupEntity.PurchaseReturnCode = stockGroupCodes.PurchaseReturnCode;
                itemGroupEntity.PurchaseDiscountCode = stockGroupCodes.PurchaseDiscountCode;
                itemGroupEntity.BalanceStockCode = stockGroupCodes.BalanceStockCode;

                cmd.SaveItemGroup(itemGroupEntity);
                Utils.Log("Edited successfully");
                return $"{DoctypeName} edited: {itemGroup}";
            }
            return $"{DoctypeName} edit error: Login failed";
        }

        private string Delete(string itemGroup)
        {
            if (Auth.Login(userSession))
            {
                AutoCount.Stock.ItemGroup.ItemGroupCommand cmd =
                    AutoCount.Stock.ItemGroup.ItemGroupCommand.Create(userSession, userSession.DBSetting);

                cmd.DeleteItemGroup(itemGroup);
                Utils.Log("Deleted successfully");
                return $"{DoctypeName} deleted: {itemGroup}";
            }
            return $"{DoctypeName} delete error: Login failed";
        }

        public static string GetGeneralAccountCodes()
        {
            AutoCount.Data.DBSetting dbSetting = new AutoCount.Data.DBSetting(AutoCount.Data.DBServerType.SQL2000, ".\\A2006", "test", "test123456", "AED_Shrdc");
            AutoCount.Authentication.UserSession userSession = new AutoCount.Authentication.UserSession(dbSetting);
            string dbName = userSession.DBSetting.DBName;

            string query = $"SELECT AccNo, AccDescription FROM [{dbName}].[dbo].[vAccNo] " +
                $"WHERE AccNo IN (SELECT AccNo FROM [{dbName}].[dbo].[vAccNo] EXCEPT " +
                $"SELECT AccNo FROM [{dbName}].[dbo].[Debtor] EXCEPT " +
                $"SELECT AccNo FROM [{dbName}].[dbo].[Creditor])";

            object[] paramsList = { };

            return Sql.RunSqlQuery(userSession, query, paramsList);

        }




    }


    internal class StockGroupCodes
    {
        public string SalesCode { get; set; }    
        public string CashSalesCode { get; set; }
        public string SalesReturnCode { get; set; }
        public string SalesDiscountCode { get; set; }
        public string PurchaseCode { get; set; }
        public string CashPurchaseCode { get; set; }
        public string PurchaseReturnCode { get; set; }
        public string PurchaseDiscountCode { get; set; }
        public string BalanceStockCode { get; set; }       

        public StockGroupCodes(string SalesCode, string CashSalesCode, string SalesReturnCode, string SalesDiscountCode,
            string PurchaseCode, string CashPurchaseCode, string PurchaseReturnCode, string PurchaseDiscountCode, string BalanceStockCode)
        {
            this.SalesCode = SalesCode;
            this.CashSalesCode = CashSalesCode;
            this.SalesReturnCode = SalesReturnCode;
            this.SalesDiscountCode = SalesDiscountCode;
            this.PurchaseCode = PurchaseCode;
            this.CashPurchaseCode = CashPurchaseCode;
            this.PurchaseReturnCode = PurchaseReturnCode;
            this.PurchaseDiscountCode = PurchaseDiscountCode;
            this.BalanceStockCode = BalanceStockCode;
        }

    }

}
