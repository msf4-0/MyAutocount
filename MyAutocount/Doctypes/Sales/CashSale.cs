using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json;
using static MyAutocount.Utils;

namespace MyAutocount.Doctypes.Sales
{
    public class CashSale : NancyModule
    {
        const string DoctypeName = "CashSale";
        const string PrimaryKey = "DocNo";

        const string DatabaseTable = "vCashSale";
        const string DetailTable = "vCashSaleDetail";

        AutoCount.Data.DBSetting dbSetting;
        AutoCount.Authentication.UserSession userSession;

        public CashSale()
        {
            dbSetting = Auth.dbSetting;
            userSession = Auth.userSession;
            Run();
        }

        public static void Test()
        {
            CashSale cashSale = new CashSale();

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
                try
                {
                    string jsonString = Nancy.IO.RequestStream.FromStream(this.Request.Body).AsString();
                    dynamic jsonData = JsonConvert.DeserializeObject(jsonString);

                    Response response = Add(jsonData);
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
                try
                {
                    string jsonString = Nancy.IO.RequestStream.FromStream(this.Request.Body).AsString();
                    dynamic jsonData = JsonConvert.DeserializeObject(jsonString);

                    Response response = Edit(jsonData);
                    response.StatusCode = HttpStatusCode.OK;
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
                string docNo = data[CashSaleConstants.DocNo];

                AutoCount.Invoicing.Sales.CashSale.CashSaleCommand cmd =
                    AutoCount.Invoicing.Sales.CashSale.CashSaleCommand.Create(userSession, userSession.DBSetting);
                AutoCount.Invoicing.Sales.CashSale.CashSale doc = cmd.AddNew();

                doc.DocNo = docNo;
                doc.DebtorCode = data[CashSaleConstants.DebtorCode];
                doc.DocDate = DateStringToDateTime(data[CashSaleConstants.Date].ToString());
                doc.ShipInfo = data[CashSaleConstants.ShipInfo];

                doc.PaymentMode = data[CashSaleConstants.PaymentMode];
                //doc.CashPayment = data[CashSaleConstants.CashPayment];  // Optional if Payment mode is credit sale (4)

                short paymentMode = short.Parse(data[CashSaleConstants.PaymentMode].ToString());
                decimal cashAmount = data[CashSaleConstants.CashPayment];
                MakePayment(doc, paymentMode, cashAmount);

                dynamic detailList = data[CashSaleConstants.DetailList];

                foreach (dynamic detailObject in detailList)
                {
                    AutoCount.Invoicing.Sales.CashSale.CashSaleDetail detail = doc.AddDetail();
                    detail.ItemCode = detailObject[CashSaleConstants.ItemCode].ToString();
                    detail.UOM = detailObject[CashSaleConstants.Uom].ToString();
                    detail.Qty = decimal.Parse(detailObject[CashSaleConstants.Quantity].ToString());
                    detail.UnitPrice = decimal.Parse(detailObject[CashSaleConstants.UnitPrice].ToString());
                    detail.Discount = detailObject[CashSaleConstants.Discount].ToString();
                }


                doc.Save();
                Log($"{DoctypeName} added: {docNo}");
                return $"{DoctypeName} added: {docNo}";

            }
            Log($"{DoctypeName} add error: Login failed");
            return $"{DoctypeName} add error: Login failed";
        }

        
        private void MakePayment(AutoCount.Invoicing.Sales.CashSale.CashSale cs, short paymentMode, decimal amount)
        {
            if (paymentMode != 1 && paymentMode != 4)
            {
                throw new Exception("Payment mode should be 1 or 4.");
            }

            cs.CashPayment = paymentMode == 1 ? amount : 0M;
            cs.CashSalePayment.ARPayment.ClearDetails();

            cs.CashSalePayment = AutoCount.Invoicing.Sales.SalesPayment.Create(
                cs.ReferPaymentDocKey,
                cs.DocKey,
                AutoCount.Document.DocumentType.CashSale,
                userSession,
                dbSetting
                );

            cs.CashSalePayment.DebtorCode = cs.DebtorCode;
            cs.CashSalePayment.CurrencyCode = cs.CurrencyCode;
            cs.CashSalePayment.DocDate = (DateTime) cs.DocDate;

            AutoCount.ARAP.ARPayment.ARPaymentDTLEntity paymentDetail = cs.CashSalePayment.ARPayment.NewDetail();
            paymentDetail.PaymentMethod = "CASH";
            paymentDetail.PaymentAmt = paymentMode == 1 ? amount : (decimal) cs.Total;

            if (cs.CashSalePayment.PaymentAmt > 0)
            {
                cs.ReferPaymentDocKey = cs.CashSalePayment.DocKey;
            }

        }


        private string Edit(dynamic data)
        {
            if (Auth.Login(userSession))
            {
                string docNo = data[CashSaleConstants.DocNo];

                AutoCount.Invoicing.Sales.CashSale.CashSaleCommand cmd =
                    AutoCount.Invoicing.Sales.CashSale.CashSaleCommand.Create(userSession, userSession.DBSetting);
                AutoCount.Invoicing.Sales.CashSale.CashSale doc = cmd.Edit(docNo);

                // If previous Payment Mode is cash (1), cannot changed to credit sale (4).
                if (doc.PaymentMode == 1)   // Previous mode
                {
                    if (data[CashSaleConstants.PaymentMode] == 4)   // New mode
                    {
                        throw new Exception("The cash sale was previously saved using cash payment mode, cannot change to credit sales.");
                    }

                }

                doc.DocNo = docNo;
                doc.DebtorCode = data[CashSaleConstants.DebtorCode];
                doc.DocDate = DateStringToDateTime(data[CashSaleConstants.Date].ToString());
                doc.ShipInfo = data[CashSaleConstants.ShipInfo];

                doc.PaymentMode = data[CashSaleConstants.PaymentMode];

                short paymentMode = short.Parse(data[CashSaleConstants.PaymentMode].ToString());
                decimal cashAmount = data[CashSaleConstants.CashPayment];
                MakePayment(doc, paymentMode, cashAmount);
                
                dynamic detailList = data[CashSaleConstants.DetailList];

                doc.ClearDetails();

                foreach (dynamic detailObject in detailList)
                {
                    AutoCount.Invoicing.Sales.CashSale.CashSaleDetail detail = doc.AddDetail();
                    detail.ItemCode = detailObject[CashSaleConstants.ItemCode].ToString();
                    detail.UOM = detailObject[CashSaleConstants.Uom].ToString();
                    detail.Qty = decimal.Parse(detailObject[CashSaleConstants.Quantity].ToString());
                    detail.UnitPrice = decimal.Parse(detailObject[CashSaleConstants.UnitPrice].ToString());
                    detail.Discount = detailObject[CashSaleConstants.Discount].ToString();
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
                AutoCount.Invoicing.Sales.CashSale.CashSaleCommand cmd =
                    AutoCount.Invoicing.Sales.CashSale.CashSaleCommand.Create(userSession, userSession.DBSetting);

                cmd.Delete(docNo);
                Log($"{DoctypeName} deleted: {docNo}");
                return $"{DoctypeName} deleted: {docNo}";
            }
            Log($"{DoctypeName} delete error: Login failed");
            return $"{DoctypeName} delete error: Login failed";

        }
    }

    internal static class CashSaleConstants
    {
        // Key of dynamic data fetched from API.
        internal static string DocNo { get; } = "docNo";
        internal static string DebtorCode { get; } = "debtorCode";
        internal static string Date { get; } = "date";
        internal static string ShipInfo { get; } = "shipInfo";

        internal static string PaymentMode { get; } = "paymentMode";
        internal static string CashPayment { get; } = "cashPayment";

        internal static string DetailList { get; } = "detailList";
        internal static string ItemCode { get; } = "itemCode";
        internal static string Uom { get; } = "uom";
        internal static string Quantity { get; } = "quantity";
        internal static string UnitPrice { get; } = "unitPrice";
        internal static string Discount { get; } = "discount";

    }
}
