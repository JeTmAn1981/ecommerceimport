#define DEBUG

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhitTools;
using System.IO;
using EcommerceImport;
using static System.IO.File;
using static EcommerceImport.Utilities;

using static EcommerceImport.BankFileProcessor;
using static EcommerceImport.SlateFileProcessor;
using static EcommerceImport.Setup;
using static WhitTools.Email;
using System.Net.Mail;
using WinSCP;

using static EcommerceImport.Logging;
using EcommerceImport.Data;

namespace EcommerceImport
{


    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //SaveTestSlateDetail();

                DoSetup(args);
                RunEcommerceImport();
                DoTeardown();
            }
            catch (Exception ex)
            {
                SendErrorMessage(ex, "E-Commerce Process Error");

                DoTeardown();
            }

        }

        private static void SaveTestSlateDetail()
        {

            EcommerceImport.Data.SlateDetail testDetail = new EcommerceImport.Data.SlateDetail();

            EcommerceImport.Data.SlateDetailGLNumber number1 = new EcommerceImport.Data.SlateDetailGLNumber();
            number1.accountNumber = "testnumber1";
            number1.type = "blah";
            testDetail.SlateDetailGLNumber = number1;

            EcommerceImport.Data.SlateDetailGLNumber number2 = new EcommerceImport.Data.SlateDetailGLNumber();
            number2.accountNumber = "testnumber2";
            number2.type = "blah2";
            testDetail.SlateDetailGLNumber1 = number2;

            var dbtest = new EcommerceImport.Data.eCommerceEntities();
            dbtest.SlateDetails.Add(testDetail);
            dbtest.SaveChanges();
        }

        private static void SendErrorMessage(Exception ex, string subject)
        {
            SmtpClient client = new SmtpClient(whitworthMailServerAddress);
            MailMessage msg;

            string errorEmail = PROGRAMMER_EMAIL;

            msg = new MailMessage("redacted@whitworth.edu", errorEmail, subject, ex.ToString());

            client.Send(msg);
        }

        static void RunEcommerceImport()
        {
            ProcessPayments();

#if (!DEBUG)
            {
                NotifyAdministrators();
            }

#endif            
        }

        private static void DoTeardown()
        {
            logger.WriteLogFiles();
            NotifyAdministrators();
        }
   
        private static void ProcessPayments()
        {
            fileProcessors.ForEach(fp => {
                try
                {
                    fp.ProcessRecords();
                }
                catch (Exception ex)
                {
                    SendErrorMessage(ex, "E-Commerce Error - Processing " + fp.name);
                }
                
            });
        }

          static void NotifyAdministrators()
        {
            var currentNotifications = notifications.Where(n => n.notifyAdministrators == true).Select(n => n.administratorEmail).Distinct();

            foreach (var administratorEmail in currentNotifications)
            {
                NotifyAdministrator(administratorEmail);
            }

            SendPaymentsSummary();
        }

        static void SendPaymentsSummary()
        {
            string summaryMessage = GetSummaryMessage();

            SmtpClient client = new SmtpClient(whitworthMailServerAddress);
            MailMessage msg = new MailMessage();
                        
            if (test)
            {
                msg.From = new MailAddress(PROGRAMMER_EMAIL);
                msg.To.Add(PROGRAMMER_EMAIL);
                msg.Subject = "E-Commerce Notifications Payments Summary";
                msg.Body = summaryMessage;
            }
            else
            {
                msg.From = new MailAddress("redacted@whitworth.edu");

                AddSummaryAdministratorRecipients(msg);
                msg.To.Add("redacted@whitworth.edu");
                msg.Subject = "E-Commerce Notifications Payments Summary";
                msg.Body = summaryMessage;
            }

            msg.IsBodyHtml = true;

            client.Send(msg);
        }

        private static string GetSummaryMessage()
        {
            string summaryMessage = $"<p>E-commerce processing was completed at {DateTime.Now.ToString()}.</p><br />";
            
            summaryMessage += string.Concat(fileProcessors.Select(fp => fp.GetSummaryMessage()));
                        
            return summaryMessage;
        }

        private static void NotifyAdministrator(string administratorEmail)
        {
            string emailMessage = "";

            AddNotificationsWithInvoices(administratorEmail, ref emailMessage);
            AddNotificationsWithoutInvoice(administratorEmail, ref emailMessage);

            if (emailMessage != "")
            {
                SendNotificationEmail(administratorEmail, emailMessage);
            }
        }

        private static void SendNotificationEmail(string administratorEmail, string emailMessage)
        {
                SmtpClient client = new SmtpClient(whitworthMailServerAddress);
                MailMessage msg;

                if (test)
                {
                    emailMessage += "Would be sent to " + administratorEmail;
                    msg = new MailMessage(PROGRAMMER_EMAIL, PROGRAMMER_EMAIL, "E-Commerce Notifications", emailMessage);

                }
                else
                {
                    msg = new MailMessage("redacted@whitworth.edu", administratorEmail, "E-Commerce Notifications", emailMessage);
                }

                client.Send(msg);
      }

        private static void AddNotificationsWithInvoices(string administratorEmail, ref string emailMessage)
        {
            List<int> invoices = notifications.Where(e => e.notifyAdministrators && e.administratorEmail == administratorEmail && e.invoice > 0).Select(e => e.invoice).Distinct().ToList();

            foreach (var invoice in invoices)
            {
                var customerID = db.WEB_Invoices.First(i => i.Inv_NO == invoice).Inv_Customer_ID;

                var currentCustomer = db.WEB_Customer.First(c => c.Customer_ID == customerID);

                emailMessage += "Invoice #" + invoice + Environment.NewLine + Environment.NewLine;
                emailMessage += currentCustomer.FULL_NAME + Environment.NewLine + Environment.NewLine;

                var currentNotifications = notifications.Where(e => e.notifyAdministrators && e.invoice == invoice && e.administratorEmail == administratorEmail);

                foreach (var notification in currentNotifications)
                {
                    emailMessage += notification.message + Environment.NewLine;
                }

                emailMessage += Environment.NewLine + Environment.NewLine;
            }
          }

        private static void AddNotificationsWithoutInvoice(string administratorEmail,ref string emailMessage)
        {
            foreach (var notification in notifications.Where(e => e.notifyAdministrators && e.invoice <= 0 && e.administratorEmail == administratorEmail))
            {
                emailMessage += notification.message + Environment.NewLine;
            }
        }
    }
}
