using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json;
using static MyAutocount.Utils;

namespace MyAutocount.Doctypes.Purchase
{
    public class PurchaseOrder : NancyModule
    {
        const string DoctypeName = "PurchaseOrder";
        const string PrimaryKey = "DocNo";

        const string DatabaseTable = "vPurchaseOrder";
        const string DetailTable = "vPurchaseOrderDetail";

        AutoCount.Data.DBSetting dbSetting;
        AutoCount.Authentication.UserSession userSession;

        public PurchaseOrder()
        {
            dbSetting = Auth.dbSetting;
            userSession = Auth.userSession;
            Run();
        }

        public static void Test()
        {
            PurchaseOrder order = new PurchaseOrder();

        }

        private void Run()
        {
            Get($"/{DoctypeName}/getAll", _ =>
            {
                try
                {
                    return GetAll();
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }

            });

            Get($"/{DoctypeName}/getSingle/{{docNo}}", args =>
            {
                try
                {
                    return GetSingle(args.docNo);
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            });

            Get($"/{DoctypeName}/getDetail/{{docNo}}", args => {
                try
                {
                    return GetSingleDetail(args.docNo);
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            });

            Post($"/{DoctypeName}/add", args =>
            {
                try
                {
                    string jsonString = Nancy.IO.RequestStream.FromStream(this.Request.Body).AsString();
                    dynamic jsonData = JsonConvert.DeserializeObject(jsonString);
                    return Add(jsonData);
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }

            });

            Put($"/{DoctypeName}/edit", args =>
            {
                try
                {
                    string jsonString = Nancy.IO.RequestStream.FromStream(this.Request.Body).AsString();
                    dynamic jsonData = JsonConvert.DeserializeObject(jsonString);
                    return Edit(jsonData);
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            });

            Delete($"/{DoctypeName}/delete/{{docNo}}", args => {
                try
                {
                    return Delete(args.docNo);
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            });


        }

        private string GetAll()
        {
            return Sql.GetAllFromSql(userSession, DatabaseTable);

        }
        private string GetSingle(string docNo)
        {
            return Sql.GetSingleFromSql(userSession, DatabaseTable, PrimaryKey, docNo);
        }

        private string GetSingleDetail(string docNo)
        {
            return Sql.GetSingleDetailFromSql(userSession, DatabaseTable, DetailTable, docNo);
        }

        private string Add(dynamic data)
        {
            if (Auth.Login(userSession))
            {
                string docNo = data[PurchaseOrderConstants.DocNo];

                AutoCount.Invoicing.Purchase.PurchaseOrder.PurchaseOrderCommand cmd =
                    AutoCount.Invoicing.Purchase.PurchaseOrder.PurchaseOrderCommand.Create(userSession, userSession.DBSetting);
                AutoCount.Invoicing.Purchase.PurchaseOrder.PurchaseOrder doc = cmd.AddNew();

                doc.DocNo = docNo;
                doc.CreditorCode = data[PurchaseOrderConstants.CreditorCode];
                doc.DocDate = DateStringToDateTime(data[PurchaseOrderConstants.Date].ToString());
                doc.ShipInfo = data[PurchaseOrderConstants.ShipInfo];

                dynamic detailList = data[PurchaseOrderConstants.DetailList];

                foreach (dynamic detailObject in detailList)
                {
                    AutoCount.Invoicing.Purchase.PurchaseOrder.PurchaseOrderDetail detail = doc.AddDetail();
                    detail.ItemCode = detailObject[PurchaseOrderConstants.ItemCode].ToString();
                    detail.UOM = detailObject[PurchaseOrderConstants.Uom].ToString();
                    detail.Qty = decimal.Parse(detailObject[PurchaseOrderConstants.Quantity].ToString());
                    detail.UnitPrice = decimal.Parse(detailObject[PurchaseOrderConstants.UnitPrice].ToString());
                    detail.Discount = detailObject[PurchaseOrderConstants.Discount].ToString();
                }

                doc.Save();
                Log($"{DoctypeName} added: {docNo}");
                return $"{DoctypeName} added: {docNo}";

            }
            Log($"{DoctypeName} add error: Login failed");
            return $"{DoctypeName} add error: Login failed";
        }

        private string Edit(dynamic data)
        {
            if (Auth.Login(userSession))
            {
                string docNo = data[PurchaseOrderConstants.DocNo];

                AutoCount.Invoicing.Purchase.PurchaseOrder.PurchaseOrderCommand cmd =
                    AutoCount.Invoicing.Purchase.PurchaseOrder.PurchaseOrderCommand.Create(userSession, userSession.DBSetting);
                AutoCount.Invoicing.Purchase.PurchaseOrder.PurchaseOrder doc = cmd.Edit(docNo);

                doc.CreditorCode = data[PurchaseOrderConstants.CreditorCode];
                doc.DocDate = DateStringToDateTime(data[PurchaseOrderConstants.Date].ToString());
                doc.ShipInfo = data[PurchaseOrderConstants.ShipInfo];

                dynamic detailList = data[PurchaseOrderConstants.DetailList];

                doc.ClearDetails();

                foreach (dynamic detailObject in detailList)
                {
                    AutoCount.Invoicing.Purchase.PurchaseOrder.PurchaseOrderDetail detail = doc.AddDetail();
                    detail.ItemCode = detailObject[PurchaseOrderConstants.ItemCode].ToString();
                    detail.UOM = detailObject[PurchaseOrderConstants.Uom].ToString();
                    detail.Qty = decimal.Parse(detailObject[PurchaseOrderConstants.Quantity].ToString());
                    detail.UnitPrice = decimal.Parse(detailObject[PurchaseOrderConstants.UnitPrice].ToString());
                    detail.Discount = detailObject[PurchaseOrderConstants.Discount].ToString();
                }

                doc.Save();
                Log($"{DoctypeName} edited: {docNo}");
                return $"{DoctypeName} edited: {docNo}";

            }
            Log($"{DoctypeName} edit error: Login failed");
            return $"{DoctypeName} edit error: Login failed";
        }

        private string Delete(string docNo)
        {
            if (Auth.Login(userSession))
            {
                AutoCount.Invoicing.Purchase.PurchaseOrder.PurchaseOrderCommand cmd =
                    AutoCount.Invoicing.Purchase.PurchaseOrder.PurchaseOrderCommand.Create(userSession, userSession.DBSetting);

                cmd.Delete(docNo);
                Log($"{DoctypeName} deleted: {docNo}");
                return $"{DoctypeName} deleted: {docNo}";
            }
            Log($"{DoctypeName} delete error: Login failed");
            return $"{DoctypeName} delete error: Login failed";

        }







    }

    internal static class PurchaseOrderConstants
    {
        // Key of dynamic data fetched from API.
        internal static string DocNo { get; } = "docNo";
        internal static string CreditorCode { get; } = "creditorCode";
        internal static string Date { get; } = "date";
        internal static string ShipInfo { get; } = "shipInfo";
        internal static string DetailList { get; } = "detailList";
        internal static string ItemCode { get; } = "itemCode";
        internal static string Uom { get; } = "uom";
        internal static string Quantity { get; } = "quantity";
        internal static string UnitPrice { get; } = "unitPrice";
        internal static string Discount { get; } = "discount";

    }

}
