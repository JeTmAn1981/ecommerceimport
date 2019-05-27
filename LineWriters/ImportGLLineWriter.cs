using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EcommerceImport.Utilities;
using static EcommerceImport.Logging;
using EcommerceImport.ImportFiles;
using EcommerceImport.Data;


namespace EcommerceImport
{
    public class ImportGLLineWriter : ImportLineWriter
    {
        private string importFirstHalf;
        private WEB_ItemTaxes_V tax;
        private string chargeAmount, charge;
        private string description;
        private string gl, currentDebitGL, currentCreditGL;
      
        public ImportGLLineWriter(WEB_Invoice_Items item,PaymentSourceProcessor processor) : base(item,processor)
        {
            SetupImportFirstHalf();
        }
                
        private int GetInvoiceType()
        {
            return db.WEB_Invoices.First(i => i.Inv_NO == item.Invoice).Inv_TYPE ?? 0;
        }

        public override void WriteCharges()
        {
            WriteNormalCharges();
            WriteTaxCharges();
        }

        private void SetupImportFirstHalf()
        {
            importFirstHalf = FormatChargebackInvoice(item.Invoice);
            importFirstHalf += (item.Process_Date ?? DateTime.Now).ToString("MMddyyyy");
        }

        private void CreateAndWriteImportLine()
        {
            string importLine = importFirstHalf + gl;
            importLine += charge;
            importLine += description;

            WriteImportLineToString(importLine);
        }
               
        private static string FormatChargebackInvoice(int? invoice)
        {
            return "EC" + new string('0', 8 - invoice.ToString().Length) + invoice.ToString();
        }

        private string FormatChargebackAmount(decimal amount)
        {
            string returnAmount = "";

            if (amount <= 0)
                logger.LogNotification(new InvalidItemAmountError(item));

            returnAmount = Math.Round(amount, 2).ToString("#.00").Replace(".", "");

            if (returnAmount.ToString().Length < 1)
                throw new Exception($"Invalid amount:" + Environment.NewLine + "Amount: {amount}");

            if (returnAmount.ToString().Length < 7)
                returnAmount = new string('0', 7 - returnAmount.ToString().Length) + returnAmount;

            return returnAmount;
        }
        
        private void WriteNormalCharges()
        {
            if (item.itemType.creditGLs.Count() > 0)
            {
                foreach (var creditGL in item.itemType.creditGLs)
                {
                    chargeAmount = FormatChargebackAmount(GetAmountToChargeGL(creditGL));
                    description = item.description;

                    currentDebitGL = item.itemType.Item_GL_Debit;
                    currentCreditGL = creditGL.CreditGL;

                    WriteImportLines();
                }
            }
            else
            {
                logger.LogMessage("No credit GL's found for " + item.itemType.Description);
            }
        }

        private void WriteTaxCharges()
        {
            tax = db.WEB_ItemTaxes_V.Where(t => t.ItemType == item.Item_Type.Value).DefaultIfEmpty(null).First();

            WriteSalesTaxCharges();
            WriteBOCharges();
        }

        private void WriteSalesTaxCharges()
        {
            if (item.SalesTax > 0)
            {
                chargeAmount = FormatChargebackAmount(item.SalesTax.Value);
                                description = TruncateDescription("STax:" + item.description);

                currentDebitGL = tax.SalesDebitGL;
                currentCreditGL = tax.SalesCreditGL;

                WriteImportLines();
            }
        }

        private void WriteBOCharges()
        {
            WriteBOGoodsTaxCharges();
            WriteBOServicesTaxCharges();
        }

        private void WriteBOGoodsTaxCharges()
        {
            if (item.BOGoodsTax > 0)
            {
                chargeAmount = FormatChargebackAmount(item.BOGoodsTax.Value);
                description = TruncateDescription("BOGTax:" + item.description);

                currentDebitGL = tax.BOGoodsDebitGL;
                currentCreditGL = tax.BOGoodsCreditGL;

                WriteImportLines();
            }
        }

        private void WriteBOServicesTaxCharges()
        {
            if (item.BOServicesTax > 0)
            {
                chargeAmount = FormatChargebackAmount(item.BOServicesTax.Value);
                description = TruncateDescription("BOSTax:" + item.description);

                currentDebitGL = tax.BOServicesDebitGL;
                currentCreditGL = tax.BOServicesCreditGL;

                WriteImportLines();
            }
        }
        
        private void WriteImportLines()
        {
            if (ReversalRequired())
            {
                WriteCreditLine();
                WriteDebitLine();
            }
            else
            {
                WriteDebitLine();
                WriteCreditLine();
            }
        }

        private bool ReversalRequired()
        {
            return InvoicePaymentCancelled() || (InvoiceHasReturnedStatus() && ((!db.WEB_InvoiceType.First(it => it.Id == item.itemType.InvoiceType).PreviousPaymentRequiredForRefund ?? true) || InvoicePreviouslyProcessedAsPaid()));
        }

        private bool InvoicePaymentCancelled()
        {
            return db.WEB_Payments.First(p => p.Invoice == item.Invoice).Cancel_Date != null;
        }

        private bool InvoiceHasReturnedStatus()
        {
            return db.RemittanceDetails.Where(rd => rd.TransactionKey == item.Invoice && rd.PaymentStatus == PAYMENT_RETURNED_STATUS).Any();
        }

        private bool InvoicePreviouslyProcessedAsPaid()
        {
            return db.RemittanceDetails.Where(rd => rd.TransactionKey == item.Invoice && rd.PaymentStatus == PAYMENT_SENT_STATUS).Any();
        }

        private void WriteCreditLine()
        {
            gl = currentCreditGL;
            charge = GetPaddedCreditCharge();
            CreateAndWriteImportLine();
        }

        private string GetPaddedCreditCharge()
        {
            return ReversalRequired() ? chargeAmount + "0000000" : "0000000" + chargeAmount;
        }

        private void WriteDebitLine()
        {
            gl = currentDebitGL;
            charge = GetPaddedDebitCharge();
            CreateAndWriteImportLine();
        }

        private string GetPaddedDebitCharge()
        {
            return ReversalRequired() ? "0000000" + chargeAmount : chargeAmount + "0000000";
        }

        private string TruncateDescription(string description)
        {
            return description.Length > 80 ? description.Substring(0, 80) : description;
        }

        protected override void WriteImportLineToString(string importLine)
        {
            logger.LogMessage($"Writing {importLine}");

            processor.ecommerceImportData += importLine + Environment.NewLine;
        }

    }
}
