using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcommerceImport;
using static EcommerceImport.Utilities;
using System.Text.RegularExpressions;
using EcommerceImport.ImportFiles;

using static System.IO.File;
using EcommerceImport.Data;


namespace EcommerceImport
{
    public class DebitInvoiceProcessor:PaymentSourceProcessor
    {
        List<WEB_Invoice_Items> eligibleStudentDebitItems = new List<WEB_Invoice_Items>();

        public DebitInvoiceProcessor(Logger logger):base(logger)
        {
            name = "Debit Invoices";
            GetEligibleStudentDebitItems();
        }

        private void GetEligibleStudentDebitItems()
        {
            logger.LogMessage("Setting eligible student debit items");

            if (test)
            {
                eligibleStudentDebitItems = new List<WEB_Invoice_Items>();
                eligibleStudentDebitItems.Add(db.WEB_Invoice_Items.Where(i => i.ID == 119948).First());
            }
            else
            {
                eligibleStudentDebitItems = db.WEB_Invoice_Items.Where(i => i.Active.Value && i.Process_Date == null && !i.Imported.Value && i.Cost > 0 && db.WEB_ItemType.Where(it => it.ID == i.Item_Type.Value && it.ChargeType == 3).Any()).ToList();

                var invalidStudentDebitItems = (from WEB_Invoice_Items item in eligibleStudentDebitItems
                                                join invoice in db.WEB_Invoices on item.Invoice equals invoice.Inv_NO
                                                join customer in db.WEB_Customer on invoice.Inv_Customer_ID equals customer.Customer_ID
                                                where customer.Whit_ID.Trim().Length == 0
                                                select item).ToList();

                invalidStudentDebitItems.ForEach(item =>
                {
                    logger.LogNotification(new MissingStudentIDError(item));
                    eligibleStudentDebitItems.Remove(item);
                });
            }
        }

        protected override void SetFileVariables()
        {
            if (test)
            {
                ecommStuDebitOutputFilePath = @"\\datatel\gl.interfaces\test\ECOMMSTUDEBIT.TXT";
            }
            else
            {
                ecommStuDebitOutputFilePath = @"\\datatel\gl.interfaces\ECOMMSTUDEBIT.TXT";
            }
        }


        public override void ProcessRecords()
        {
            if (eligibleStudentDebitItems.Count > 0)
            {
                logger.LogMessage("Updating student debit invoice details");

                EcommerceUpdater ecommerceUpdater = GetEcommerceUpdater(new WEB_Invoices() { Inv_NO = 0 });

                foreach (var invoice in (from item in eligibleStudentDebitItems select item.Invoice).Distinct())
                {
                    ecommerceUpdater.invoice.Inv_NO = invoice.Value;
                    ecommerceUpdater.ResetInvoiceDetails();
                    ecommerceUpdater.UpdateInvoiceDetails();
                }

                WriteImportDataToString();
                WriteImportStringDataToFile();
            }
            else
            {
                logger.LogMessage("No student debit invoices found.");
            }
        }

        protected override void WriteImportStringDataToFile()
        {
            AppendAllText(ecommStuDebitOutputFilePath, ecommerceStuDebitImportData);
        }


        protected override  void WriteImportDataToString()
        {
            if (eligibleStudentDebitItems.Count > 0)
            {
                logger.LogMessage("Writing student debit details.");

                eligibleStudentDebitItems.ForEach(i => i.WriteToString(GetImportLineWriter(i)));
            }
            else
            {
                logger.LogMessage("No student debit charges available for import.");
            }
        }

                public override string GetSummaryMessage()
        {
            string message = "";
            string studentDebitPaymentDesignation = "<strong>Student Debit</strong>";

            message += "<h2>Student Debit</h2>";

            if (eligibleStudentDebitItems.Count() > 0)
            {
                message += WrapWithParagraph($"{studentDebitPaymentDesignation} import total: ${eligibleStudentDebitItems.Sum(i => i.Total)} from " + eligibleStudentDebitItems.Count() + " transaction(s).");
            }
            else
            {
                message += WrapWithParagraph($"No {studentDebitPaymentDesignation} import transactions were found.");
            }
            
            return "\n" + message;
        }
    }
}
