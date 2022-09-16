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
    public class CreditNote : NancyModule
    {
        const string DoctypeName = "CreditNote";
        const string PrimaryKey = "DocNo";

        const string DatabaseTable = "vCreditNote";
        const string DetailTable = "vCreditNoteDetail";

        AutoCount.Data.DBSetting dbSetting;
        AutoCount.Authentication.UserSession userSession;

        public CreditNote()
        {
            dbSetting = Auth.dbSetting;
            userSession = Auth.userSession;
            Run();
        }

        public static void Test()
        {
            CreditNote creditNote = new CreditNote();

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
                string docNo = data[CreditNoteConstants.DocNo];

                AutoCount.Invoicing.Sales.CreditNote.CreditNoteCommand cmd =
                    AutoCount.Invoicing.Sales.CreditNote.CreditNoteCommand.Create(userSession, userSession.DBSetting);
                AutoCount.Invoicing.Sales.CreditNote.CreditNote doc = cmd.AddNew();

                doc.DocNo = docNo;
                doc.DebtorCode = data[CreditNoteConstants.DebtorCode];
                doc.DocDate = DateStringToDateTime(data[CreditNoteConstants.Date].ToString());
                doc.Reason = data[CreditNoteConstants.Reason];
                doc.OurInvoiceNo = data[CreditNoteConstants.OurInvoiceNo];
                doc.CNType = data[CreditNoteConstants.CNType];

                dynamic detailList = data[CreditNoteConstants.DetailList];

                foreach (dynamic detailObject in detailList)
                {
                    AutoCount.Invoicing.Sales.CreditNote.CreditNoteDetail detail = doc.AddDetail();
                    detail.ItemCode = detailObject[CreditNoteConstants.ItemCode].ToString();
                    detail.UOM = detailObject[CreditNoteConstants.Uom].ToString();
                    detail.Qty = decimal.Parse(detailObject[CreditNoteConstants.Quantity].ToString());
                    detail.UnitPrice = decimal.Parse(detailObject[CreditNoteConstants.UnitPrice].ToString());
                    detail.Discount = detailObject[CreditNoteConstants.Discount].ToString();
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
                string docNo = data[CreditNoteConstants.DocNo];

                AutoCount.Invoicing.Sales.CreditNote.CreditNoteCommand cmd =
                    AutoCount.Invoicing.Sales.CreditNote.CreditNoteCommand.Create(userSession, userSession.DBSetting);
                AutoCount.Invoicing.Sales.CreditNote.CreditNote doc = cmd.Edit(docNo);

                doc.DocNo = docNo;
                doc.DebtorCode = data[CreditNoteConstants.DebtorCode];
                doc.DocDate = DateStringToDateTime(data[CreditNoteConstants.Date].ToString());
                doc.Reason = data[CreditNoteConstants.Reason];
                doc.OurInvoiceNo = data[CreditNoteConstants.OurInvoiceNo];
                doc.CNType = data[CreditNoteConstants.CNType];

                dynamic detailList = data[CreditNoteConstants.DetailList];

                doc.ClearDetails();

                foreach (dynamic detailObject in detailList)
                {
                    AutoCount.Invoicing.Sales.CreditNote.CreditNoteDetail detail = doc.AddDetail();
                    detail.ItemCode = detailObject[CreditNoteConstants.ItemCode].ToString();
                    detail.UOM = detailObject[CreditNoteConstants.Uom].ToString();
                    detail.Qty = decimal.Parse(detailObject[CreditNoteConstants.Quantity].ToString());
                    detail.UnitPrice = decimal.Parse(detailObject[CreditNoteConstants.UnitPrice].ToString());
                    detail.Discount = detailObject[CreditNoteConstants.Discount].ToString();
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
                AutoCount.Invoicing.Sales.CreditNote.CreditNoteCommand cmd =
                    AutoCount.Invoicing.Sales.CreditNote.CreditNoteCommand.Create(userSession, userSession.DBSetting);

                cmd.Delete(docNo);
                Log($"{DoctypeName} deleted: {docNo}");
                return $"{DoctypeName} deleted: {docNo}";
            }
            Log($"{DoctypeName} delete error: Login failed");
            return $"{DoctypeName} delete error: Login failed";

        }


    }

    internal static class CreditNoteConstants
    {
        // Key of dynamic data fetched from API.
        internal static string DocNo { get; } = "docNo";
        internal static string DebtorCode { get; } = "debtorCode";
        internal static string Date { get; } = "date";
        internal static string Reason { get; } = "reason";
        internal static string OurInvoiceNo { get; } = "ourInvoiceNo";
        internal static string CNType { get; } = "cnType";

        internal static string DetailList { get; } = "detailList";
        internal static string ItemCode { get; } = "itemCode";
        internal static string Uom { get; } = "uom";
        internal static string Quantity { get; } = "quantity";
        internal static string UnitPrice { get; } = "unitPrice";
        internal static string Discount { get; } = "discount";

    }

}
