using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcommerceImport;
using System.Text.RegularExpressions;
using EcommerceImport.ImportFiles;
using EcommerceImport.Data;
using System.Net.Mail;

namespace EcommerceImport
{

    public class Utilities
    {
        public static eCommerceEntities db = new eCommerceEntities();
        public static bool test = false;
        public static bool includeBank = false;
        public static bool includeSlate = true;
        public static bool includeDebit = false;
        public static bool importDetails = true;
        public static bool returnedpaymentsallowed = false;
        public const string LogFilePath = @"\\web3\e$\imports\eCommerceImport\Logs";
        public const string LogFileTestPath = @"\\web3\e$\imports\eCommerceImport\Logs\Test";

        public static List<PaymentSourceProcessor> fileProcessors;
        public static string CurrentLogFilePath = LogFilePath;
        public const string SLATE_BANK_GL_ACCOUNT = "100000117040";
        public const string PAYMENT_SENT_STATUS = "SENT";
        public const string PAYMENT_RETURNED_STATUS = "RETN";
        public const string PROGRAMMER_EMAIL = "redacted@whitworth.edu";
        protected const int ROCKSLINGORDER_INVOICETYPE = 84;
        public const int SLATE_PAYMENTS_INVOICE_TYPE = 141;
        public const int GL_ACCOUNT_CHARGETYPE = 1;
        public const int STUDENT_ACCOUNT_CREDIT_CHARGETYPE = 2;
        public const int STUDENT_ACCOUNT_DEBIT_CHARGETYPE = 3;

        public const int ENROLLMENT_DEPOSIT_MAIN_ITEM_TYPE_ID = 52;
        public const int ENROLLMENT_DEPOSIT_MAIN_AMOUNT = 100;
        public const int ENROLLMENT_DEPOSIT_HOUSING_ITEM_TYPE_ID = 51;
        public const int ENROLLMENT_DEPOSIT_HOUSING_AMOUNT = 100;
        public const int ENROLLMENT_DEPOSIT_MATRICULATION_ITEM_TYPE_ID = 46;
        public const int ENROLLMENT_DEPOSIT_MATRICULATION_AMOUNT = 150;

                public static List<Notification> notifications = new List<Notification>();
       
        public static string GetCurrentDateFormatted()
        {
            return DateTime.Now.ToString("MM_dd_yyyy_hh_mm_ss"); 
        }

        public static EcommerceUpdater GetEcommerceUpdater(WEB_Invoices invoice, RemittanceDetail remittanceDetail = null)
        {
            if ((invoice.Inv_TYPE ?? 0) == ROCKSLINGORDER_INVOICETYPE)
                return new RockSlingEcommerceUpdater(invoice, remittanceDetail);
            else
                return new EcommerceUpdater(invoice,remittanceDetail);
        }

        public static int GetChargeType(string description)
        {
            return IsEnrollmentDeposit(description) && !description.ToLower().Contains("fee") ? STUDENT_ACCOUNT_CREDIT_CHARGETYPE : GL_ACCOUNT_CHARGETYPE;
        }

        public static bool IsEnrollmentDeposit(string description)
        {
            return description.ToLower().Contains("whitworth deposit");
        }

        public   static  string WrapWithParagraph(string content)
        {
            return $"<p>{content}</p>";
        }
        
        public static void AddIrregularPaymentStatusAdministrators(MailMessage msg)
        {
            var irregularPaymentStatusAdministrators = db.Administrators.Where(a => a.NotifyIrregularStatus ?? true).Select(a => a.Email).ToList();

            if (irregularPaymentStatusAdministrators.Count > 0)
            {
                irregularPaymentStatusAdministrators.ForEach(administrator => msg.To.Add(administrator));
            }
            else
            {
                msg.To.Add(PROGRAMMER_EMAIL);
            }
        }

        public static void AddSummaryAdministratorRecipients(MailMessage msg)
        {
            var summaryAdministrators = db.Administrators.Where(a => a.NotifySummary ?? true).Select(a => a.Email).ToList();

            if (summaryAdministrators.Count > 0)
            {
                summaryAdministrators.ForEach(administrator => msg.To.Add(administrator));
            }
            else
            {
                msg.To.Add(PROGRAMMER_EMAIL);
            }
        }

    }
}
