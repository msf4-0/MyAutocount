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
    public class Creditor : NancyModule
    {
        const string DoctypeName = "Creditor";
        const string DatabaseTable = "vCreditor";
        const string PrimaryKey = "CreditorCode";

        AutoCount.Data.DBSetting dbSetting;
        AutoCount.Authentication.UserSession userSession;

        public Creditor()
        {
            dbSetting = Auth.dbSetting;
            userSession = Auth.userSession;
            Run();
        }

        public static void Test()
        {
            Creditor debtor = new Creditor();

        }

        private void Run()
        {
            Get($"/{DoctypeName}/getAll", _ => {
                try
                {
                    return GetAll();
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }

            });

            Get($"/{DoctypeName}/getSingle/{{creditorCode}}", args => {
                try
                {
                    return GetSingle(args.creditorCode);
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

            Delete($"/{DoctypeName}/delete/{{debtorCode}}", args => {
                try
                {
                    return Delete(args.debtorCode);
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
        private string GetSingle(string creditorCode)
        {
            return Sql.GetSingleFromSql(userSession, DatabaseTable, PrimaryKey, creditorCode);
        }

        private string Add(dynamic data)
        {
            if (Auth.Login(userSession))
            {
                string creditorCode = data[CreditorConstants.CreditorCode];

                AutoCount.ARAP.Creditor.CreditorDataAccess cmd =
                    AutoCount.ARAP.Creditor.CreditorDataAccess.Create(userSession, userSession.DBSetting);
                AutoCount.ARAP.Creditor.CreditorEntity creditorEntity = cmd.NewCreditor();

                creditorEntity.ControlAccount = "400-0000";

                creditorEntity.AccNo = creditorCode;
                creditorEntity.CompanyName = data[CreditorConstants.CompanyName];

                creditorEntity.Address1 = data[CreditorConstants.BillingAddress1];
                creditorEntity.Address2 = data[CreditorConstants.BillingAddress2];
                creditorEntity.Address3 = data[CreditorConstants.BillingAddress3];
                creditorEntity.Address4 = data[CreditorConstants.BillingAddress4];

                creditorEntity.Phone1 = data[CreditorConstants.Phone];
                creditorEntity.Mobile = data[CreditorConstants.Mobile];
                creditorEntity.Fax1 = data[CreditorConstants.Fax];
                creditorEntity.EmailAddress = data[CreditorConstants.EmailAddress];

                creditorEntity.Attention = data[CreditorConstants.Attention];
                creditorEntity.NatureOfBusiness = data[CreditorConstants.BusinessNature];

                creditorEntity.DisplayTerm = data[CreditorConstants.CreditTerm];
                creditorEntity.StatementType = data[CreditorConstants.StatementType];
                creditorEntity.AgingOn = data[CreditorConstants.AgingOn];

                creditorEntity.CreditLimit = data[CreditorConstants.CreditLimit];
                creditorEntity.OverdueLimit = data[CreditorConstants.OverdueLimit];

                cmd.SaveCreditor(creditorEntity, userSession.LoginUserID);
                Log($"{DoctypeName} added: {creditorCode}");
                return $"{DoctypeName} added: {creditorCode}";
            }
            Log($"{DoctypeName} add error: Login failed");
            return $"{DoctypeName} add error: Login failed";
        }

        private string Edit(dynamic data)
        {
            if (Auth.Login(userSession))
            {
                string creditorCode = data[CreditorConstants.CreditorCode];

                AutoCount.ARAP.Creditor.CreditorDataAccess cmd =
                    AutoCount.ARAP.Creditor.CreditorDataAccess.Create(userSession, userSession.DBSetting);
                AutoCount.ARAP.Creditor.CreditorEntity creditorEntity = cmd.GetCreditor(creditorCode);

                creditorEntity.ControlAccount = "400-0000";

                creditorEntity.CompanyName = data[CreditorConstants.CompanyName];

                creditorEntity.Address1 = data[CreditorConstants.BillingAddress1];
                creditorEntity.Address2 = data[CreditorConstants.BillingAddress2];
                creditorEntity.Address3 = data[CreditorConstants.BillingAddress3];
                creditorEntity.Address4 = data[CreditorConstants.BillingAddress4];

                creditorEntity.Phone1 = data[CreditorConstants.Phone];
                creditorEntity.Mobile = data[CreditorConstants.Mobile];
                creditorEntity.Fax1 = data[CreditorConstants.Fax];
                creditorEntity.EmailAddress = data[CreditorConstants.EmailAddress];

                creditorEntity.Attention = data[CreditorConstants.Attention];
                creditorEntity.NatureOfBusiness = data[CreditorConstants.BusinessNature];

                creditorEntity.DisplayTerm = data[CreditorConstants.CreditTerm];
                creditorEntity.StatementType = data[CreditorConstants.StatementType];
                creditorEntity.AgingOn = data[CreditorConstants.AgingOn];

                creditorEntity.CreditLimit = data[CreditorConstants.CreditLimit];
                creditorEntity.OverdueLimit = data[CreditorConstants.OverdueLimit];

                cmd.SaveCreditor(creditorEntity, userSession.LoginUserID);
                Log($"{DoctypeName} edited: {creditorCode}");
                return $"{DoctypeName} edited: {creditorCode}";
            }
            Log($"{DoctypeName} edit error: Login failed");
            return $"{DoctypeName} edit error: Login failed";

        }

        private string Delete(string creditorCode)
        {
            if (Auth.Login(userSession))
            {
                AutoCount.ARAP.Creditor.CreditorDataAccess cmd =
                    AutoCount.ARAP.Creditor.CreditorDataAccess.Create(userSession, userSession.DBSetting);
                cmd.DeleteCreditor(creditorCode);

                Log($"{DoctypeName} deleted: {creditorCode}");
                return $"{DoctypeName} deleted: {creditorCode}";
            }
            return $"{DoctypeName} delete error: Login failed";
        }


    }


    internal static class CreditorConstants
    {
        internal static string CreditorCode { get; } = "creditorCode";
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
