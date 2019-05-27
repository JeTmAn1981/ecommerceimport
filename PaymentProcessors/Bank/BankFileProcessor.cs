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
    public class BankFileProcessor : FileProcessor
    {
        private List<RemittanceDetail> remittanceDetails = new List<RemittanceDetail>();

        public BankFileProcessor(Logger logger):base(logger)
        {
            name = "U.S. Bank Payments";
        }


        protected override void SetFileVariables()
        {
            filePattern = "*e510.wwcww*rm.x320*";

            SetFilePaths();
        }

        private void SetFilePaths()
        {
            if (test)
            {
                inputFilePath = @"\\web3\e$\Imports\eCommerceImport\test";
                archivePath = @"\\web3\e$\Imports\eCommerceImport\test\Archive\";

                ecommOutputFilePath = @"\\datatel\gl.interfaces\test\ECOMMERCE.TXT"; ;
                ecommStuCreditOutputFilePath = @"\\datatel\gl.interfaces\test\ECOMMSTU.TXT"; 
            }
            else
            {
                inputFilePath = @"\\web3\e$\Imports\eCommerceImport";
                archivePath = @"\\web3\e$\Imports\eCommerceImport\Archive\";

                ecommOutputFilePath = @"\\datatel\gl.interfaces\ECOMMERCE.TXT"; 
                ecommStuCreditOutputFilePath = @"\\datatel\gl.interfaces\ECOMMSTU.TXT";
            }
        }

        protected override void GetImportDetails(string file)
        {
            foreach (string line in File.ReadLines(file).Where(l => l.StartsWith((char)34 + "D" + (char)34)))
            {
                AddRemittanceDetail(line);
            }
        }

        protected override void ImportDetails()
        {
foreach (RemittanceDetail detail in remittanceDetails)
            {
                detail.Import();
            }
        }

        private void AddRemittanceDetail(string line)
        {
            RemittanceDetail detail = new RemittanceDetail();
            string[] fields = GetSplitFields(line);

            detail.DataDate = DateTime.Now;
            detail.Completed = 0;
            detail.RecordIndicator = fields[0];
            detail.TransactionConfirmationID = fields[1];
            detail.BillerProductCode = fields[2];
            detail.AmountDue = ConvertFieldToDecimal(fields[3]);
            detail.PaymentAmount = ConvertFieldToDecimal(fields[4]);
            detail.ConvenienceFee = ConvertFieldToDecimal(fields[5]);
            detail.TotalAmount = ConvertFieldToDecimal(fields[6]);
            detail.PaymentEffectiveDate = fields[7];
            detail.InitiationDate = fields[8];
            detail.PaymentType = fields[9];
            detail.PaymentAuthorization = fields[10];
            detail.PaymentStatus = fields[11];
            detail.ACHReturnCode = fields[12];
            detail.ReasonDescription = fields[13];
            detail.ReturnDate = fields[14];
            detail.AccessMethod = fields[15];
            detail.UserID = fields[16];
            detail.FirstName = fields[17];
            detail.LastName = fields[18];
            detail.CompanyName = fields[19];
            detail.Phone = fields[20];
            detail.StreetAddress1 = fields[21];
            detail.StreetAddress2 = fields[22];
            detail.City = fields[23];
            detail.StateCode = fields[24];
            detail.ZipCode5 = fields[25];
            detail.ZipCode4 = fields[26];
            detail.Email = fields[27];
            detail.DateSubmitted = Convert.ToDateTime(fields[29]);
            detail.ProductName = fields[31];
            detail.TransactionKey = Convert.ToInt32(fields[33]);

            remittanceDetails.Add(detail);
        }

        protected override void WriteImportDataToString()
        {
            logger.LogMessage("Writing bank remittance details.");

            foreach (var detail in remittanceDetails.Where(rd => rd.validForImport).OrderBy(rd => rd.TransactionKey))
            {
                List<WEB_Invoice_Items> items = db.WEB_Invoice_Items.Where(i => i.Invoice == detail.TransactionKey).ToList();

                if (items.Count() > 0)
                {
                    logger.LogMessage("Writing items for " + detail.ToString());

                    items.ForEach(i => i.WriteToString(GetImportLineWriter(i)));

                    logger.LogMessage(Environment.NewLine);
                }
                else
                    logger.LogMessage("No items found for: " + detail.ToString());
            }
        }

        
        private static decimal ConvertFieldToDecimal(string field)
        {
            decimal amount;

            if (Decimal.TryParse(field, out amount))
                return amount;

            return 0;
        }

        public static string[] GetSplitFields(string line)
        {
            var matches = Regex.Matches(line, @"""(.+?)""");
            string[] fields = new string[matches.Count];

            for (int x = 0; x < matches.Count; x++)
                fields[x] = matches[x].ToString().Replace(@"""", "");

            return fields;
        }

        protected override void TransferFiles()
        {
         //replace with FTP connection to U.S. Bank eventually
        }

        public override string GetSummaryMessage()
        {
            string message = "";

            var paidItems = (from WEB_Invoice_Items item in db.WEB_Invoice_Items
                             where  
                             item.Imported == true
                             && item.Active == true
                             && item.Cancel != true
                             select item).ToList().Where(item => remittanceDetails.Any(rd => rd.TransactionKey == item.Invoice));

            var paidGLItems = paidItems.Where(item => item.itemType.ChargeType == GL_ACCOUNT_CHARGETYPE);
            var paidStudentCreditItems = paidItems.Where(item => item.itemType.ChargeType == STUDENT_ACCOUNT_CREDIT_CHARGETYPE);

            string glPaymentDesignation = "<strong>U.S. Bank GL</strong>";
            string studentCreditPaymentDesignation = "<strong>U.S. Bank Student Credit</strong>";

            message += "<h2>U.S. Bank</h2>";

            if (paidGLItems.Count() > 0)
            {
                message += WrapWithParagraph($"{glPaymentDesignation} import total: ${paidGLItems.Sum(i => i.Total)}");
            }
            else
            {
                message += WrapWithParagraph($"No {glPaymentDesignation} import transactions were found.</p>");
            }

            if (paidStudentCreditItems.Count() > 0)
            {
                message += WrapWithParagraph($"{studentCreditPaymentDesignation} Credit import total: ${paidStudentCreditItems.Sum(i => i.Total)}");
            }
            else
            {
                message += WrapWithParagraph($"No {studentCreditPaymentDesignation} Credit import transactions were found.");
            }

            return "\n" + message;
        }

        
    }
}
