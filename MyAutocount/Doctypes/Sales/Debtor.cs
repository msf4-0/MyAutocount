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

namespace MyAutocount.Doctypes.Sales
{
    public class Debtor : NancyModule
    {
        const string DoctypeName = "Debtor";
        const string DatabaseTable = "vDebtor";
        const string PrimaryKey = "DebtorCode";

        AutoCount.Data.DBSetting dbSetting;
        AutoCount.Authentication.UserSession userSession;

        public Debtor()
        {
            dbSetting = Auth.dbSetting;
            userSession = Auth.userSession;
            Run();
        }

        public static void Test()
        {
            Debtor debtor = new Debtor();


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

            Get($"/{DoctypeName}/getSingle/{{debtorCode}}", args =>
            {
                try
                {
                    Response response = GetSingle(args.debtorCode);
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

            Delete($"/{DoctypeName}/delete/{{debtorCode}}", args => {
                try
                {
                    Response response = Delete(args.debtorCode);
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
        private string GetSingle(string debtorCode)
        {
            return Sql.GetSingleFromSql(userSession, DatabaseTable, PrimaryKey, debtorCode);
        }

        private string Add(dynamic data)
        {
            if (Auth.Login(userSession))
            {
                string debtorCode = data[DebtorConstants.DebtorCode];

                AutoCount.ARAP.Debtor.DebtorDataAccess cmd =
                    AutoCount.ARAP.Debtor.DebtorDataAccess.Create(userSession, userSession.DBSetting);
                AutoCount.ARAP.Debtor.DebtorEntity debtorEntity = cmd.NewDebtor();

                debtorEntity.ControlAccount = "300-0000";

                debtorEntity.AccNo = debtorCode;
                debtorEntity.CompanyName = data[DebtorConstants.CompanyName];

                debtorEntity.Address1 = data[DebtorConstants.BillingAddress1];
                debtorEntity.Address2 = data[DebtorConstants.BillingAddress2];
                debtorEntity.Address3 = data[DebtorConstants.BillingAddress3];
                debtorEntity.Address4 = data[DebtorConstants.BillingAddress4];

                debtorEntity.DeliverAddr1 = data[DebtorConstants.DeliveryAddress1];
                debtorEntity.DeliverAddr2 = data[DebtorConstants.DeliveryAddress2];
                debtorEntity.DeliverAddr3 = data[DebtorConstants.DeliveryAddress3];
                debtorEntity.DeliverAddr4 = data[DebtorConstants.DeliveryAddress4];

                debtorEntity.Phone1 = data[DebtorConstants.Phone];
                debtorEntity.Mobile = data[DebtorConstants.Mobile];
                debtorEntity.Fax1 = data[DebtorConstants.Fax];
                debtorEntity.EmailAddress = data[DebtorConstants.EmailAddress];

                debtorEntity.Attention = data[DebtorConstants.Attention];
                debtorEntity.NatureOfBusiness = data[DebtorConstants.BusinessNature];

                debtorEntity.DisplayTerm = data[DebtorConstants.CreditTerm];
                debtorEntity.StatementType = data[DebtorConstants.StatementType];
                debtorEntity.AgingOn = data[DebtorConstants.AgingOn];

                debtorEntity.CreditLimit = data[DebtorConstants.CreditLimit];
                debtorEntity.OverdueLimit = data[DebtorConstants.OverdueLimit];

                cmd.SaveDebtor(debtorEntity, userSession.LoginUserID);
                Log($"{DoctypeName} added: {debtorCode}");
                return $"{DoctypeName} added: {debtorCode}";
            }
            Log($"{DoctypeName} add error: Login failed");
            return $"{DoctypeName} add error: Login failed";

        }


        private string Edit(dynamic data)
        {
            if (Auth.Login(userSession))
            {
                string debtorCode = data[DebtorConstants.DebtorCode];

                AutoCount.ARAP.Debtor.DebtorDataAccess cmd =
                    AutoCount.ARAP.Debtor.DebtorDataAccess.Create(userSession, userSession.DBSetting);
                AutoCount.ARAP.Debtor.DebtorEntity debtorEntity = cmd.GetDebtor(debtorCode);

                debtorEntity.ControlAccount = "300-0000";

                debtorEntity.CompanyName = data[DebtorConstants.CompanyName];

                debtorEntity.Address1 = data[DebtorConstants.BillingAddress1];
                debtorEntity.Address2 = data[DebtorConstants.BillingAddress2];
                debtorEntity.Address3 = data[DebtorConstants.BillingAddress3];
                debtorEntity.Address4 = data[DebtorConstants.BillingAddress4];

                debtorEntity.DeliverAddr1 = data[DebtorConstants.DeliveryAddress1];
                debtorEntity.DeliverAddr2 = data[DebtorConstants.DeliveryAddress2];
                debtorEntity.DeliverAddr3 = data[DebtorConstants.DeliveryAddress3];
                debtorEntity.DeliverAddr4 = data[DebtorConstants.DeliveryAddress4];

                debtorEntity.Phone1 = data[DebtorConstants.Phone];
                debtorEntity.Mobile = data[DebtorConstants.Mobile];
                debtorEntity.Fax1 = data[DebtorConstants.Fax];
                debtorEntity.EmailAddress = data[DebtorConstants.EmailAddress];

                debtorEntity.Attention = data[DebtorConstants.Attention];
                debtorEntity.NatureOfBusiness = data[DebtorConstants.BusinessNature];

                debtorEntity.DisplayTerm = data[DebtorConstants.CreditTerm];
                debtorEntity.StatementType = data[DebtorConstants.StatementType];
                debtorEntity.AgingOn = data[DebtorConstants.AgingOn];

                debtorEntity.CreditLimit = data[DebtorConstants.CreditLimit];
                debtorEntity.OverdueLimit = data[DebtorConstants.OverdueLimit];

                cmd.SaveDebtor(debtorEntity, userSession.LoginUserID);
                Log($"{DoctypeName} edited: {debtorCode}");
                return $"{DoctypeName} edited: {debtorCode}";

            }
            Log($"{DoctypeName} edit error: Login failed");
            return $"{DoctypeName} edit error: Login failed";

        }

        private string Delete(string debtorCode)
        {
            if (Auth.Login(userSession))
            {
                AutoCount.ARAP.Debtor.DebtorDataAccess cmd =
                    AutoCount.ARAP.Debtor.DebtorDataAccess.Create(userSession, userSession.DBSetting);

                //AutoCount.ARAP.Debtor.DebtorEntity debtor = cmd.GetDebtor(debtorCode);
                //debtor.IsActive = false;

                cmd.DeleteDebtor(debtorCode);

                Log($"{DoctypeName} deleted: {debtorCode}");
                return $"{DoctypeName} deleted: {debtorCode}";
            }
            return $"{DoctypeName} delete error: Login failed";
        }



    }


    internal static class DebtorConstants
    {                    
        internal static string DebtorCode { get; } = "debtorCode";
        internal static string CompanyName { get; } = "companyName";
        internal static string BillingAddress1 { get; } = "billingAddress1";
        internal static string BillingAddress2 { get; } = "billingAddress2";
        internal static string BillingAddress3 { get; } = "billingAddress3";
        internal static string BillingAddress4 { get; } = "billingAddress4";
        internal static string DeliveryAddress1 { get; } = "deliveryAddress1";
        internal static string DeliveryAddress2 { get; } = "deliveryAddress2";
        internal static string DeliveryAddress3 { get; } = "deliveryAddress3";
        internal static string DeliveryAddress4 { get; } = "deliveryAddress4";
        internal static string Phone { get; } = "phone";
        internal static string Mobile { get; } = "mobile";
        internal static string Fax { get; } = "fax";
        internal static string EmailAddress { get; } = "emailAddress";
        internal static string Attention { get; } = "attention";
        internal static string BusinessNature { get; } = "businessNature";
        internal static string CreditTerm { get; } = "creditTerm";
        internal static string StatementType { get; } = "statementType";
        internal static string AgingOn { get; } = "agingOn";
        internal static string CreditLimit { get; } = "creditLimit";
        internal static string OverdueLimit { get; } = "overdueLimit";

    }




}
