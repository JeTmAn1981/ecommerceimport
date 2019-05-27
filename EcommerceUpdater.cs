using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EcommerceImport.Utilities;
using static EcommerceImport.Logging;
using static WhitTools.SQL;


using EcommerceImport.Data;

namespace EcommerceImport
{
    public class EcommerceUpdater
    {
        public WEB_Invoices invoice { get; set; }

        protected RemittanceDetail remittanceDetail;
        protected decimal invoiceItemsTotal;
        protected decimal paymentTotal;

        public EcommerceUpdater(WEB_Invoices invoice, RemittanceDetail remittanceDetail = null)
        {
            this.invoice = invoice;
            this.remittanceDetail = remittanceDetail;
        }

        public void ResetInvoiceDetails()
        {
            ResetWebPayments();
            ResetInvoiceItems();
        }

        private void ResetWebPayments()
        {
            logger.LogMessage("Resetting web payments for Invoice #" + invoice);

            foreach (var wp in db.WEB_Payments.Where(payment => payment.Invoice == invoice.Inv_NO))
            {
                wp.Paid_Date = null;
                wp.Payment_Processed = null;
            }
        }

        private void ResetInvoiceItems()
        {
            logger.LogMessage("Resetting invoice items for Invoice #" + invoice);

            foreach (var ii in (from ii in db.WEB_Invoice_Items
                                where ii.Invoice == invoice.Inv_NO
                                select ii).ToList())
            {
                ii.Process_Date = null;
                ii.Imported = false;
            }
        }

        public void UpdateInvoiceDetails()
        {
            if (!FoundErrors())
            {
                SetInvoiceAmounts();

                ProcessWebPayments();
                ProcessInvoiceItems();
                UpdateSpecialDetails();

                db.SaveChanges();
            }
        }

        protected virtual void UpdateSpecialDetails()
        {
            
        }
        
        private bool FoundErrors()
        {
            bool foundErrors = false;

            if (remittanceDetail != null && remittanceDetail.PaymentStatus == PAYMENT_SENT_STATUS)
            {
                var invoiceItems = db.WEB_Invoice_Items.Where(i => i.Invoice == invoice.Inv_NO).ToList();
                
                if (invoiceItems.Where(i => i.Invoice == invoice.Inv_NO && i.Active == true).Sum(i => i.Cost) != remittanceDetail.PaymentAmount)
                {
                    logger.LogNotification(new RemittanceDetailProcessingError(invoiceItems, remittanceDetail));
                    remittanceDetail.validForImport = false;
                    foundErrors = true;
                }
            }

            return foundErrors;
        }

        private void SetInvoiceAmounts()
        {
            invoiceItemsTotal = db.WEB_Invoice_Items.Where(i => i.Invoice == invoice.Inv_NO).Sum(i => i.Cost) ?? 0;
            paymentTotal = db.WEB_Payments.Where(p => p.Invoice == invoice.Inv_NO).Sum(p => p.Total) ?? 0;
        }

        private void ProcessInvoiceItems()
        {
            CheckInvoiceItemsForErrors();
            UpdateInvoiceItems();
        }

        private void CheckInvoiceItemsForErrors()
        {
            if (remittanceDetail != null && invoiceItemsTotal != remittanceDetail.PaymentAmount)
            {
                logger.LogNotification(new TotalMismatchError(invoice.Inv_NO, "Invoice items", invoiceItemsTotal, "Remittance Detail", remittanceDetail.PaymentAmount ?? 0));
            }
        }

        private void ProcessWebPayments()
        {
            CheckWebPaymentForErrors();
            UpdateWebPayment();
        }

        private void CheckWebPaymentForErrors()
        {
            if (paymentTotal != invoiceItemsTotal)
            {
                logger.LogNotification(new TotalMismatchError(invoice.Inv_NO, "Web Payment", paymentTotal, "Invoice items", invoiceItemsTotal));
            }

            if (remittanceDetail != null && paymentTotal != remittanceDetail.PaymentAmount)
            {
                logger.LogNotification(new TotalMismatchError(invoice.Inv_NO, "Web Payment", paymentTotal, "Remittance Detail", remittanceDetail.PaymentAmount ?? 0));
            }
        }

        private void UpdateWebPayment()
        {
            logger.LogMessage("Updating Web Payment for Invoice #" + invoice);

            foreach (var payment in db.WEB_Payments.Where(wp => wp.Invoice == invoice.Inv_NO))
            {
                if (remittanceDetail != null)
                {
                    payment.Confirmation_Number = remittanceDetail.TransactionConfirmationID;

                    if (remittanceDetail.TotalAmount > 0 && remittanceDetail.PaymentStatus == PAYMENT_SENT_STATUS)
                        payment.Paid_Date = DateTime.Now;
        
                    if (remittanceDetail.TotalAmount < 0 || remittanceDetail.PaymentStatus != PAYMENT_SENT_STATUS)
                    {
                        ProcessCancellation(payment);
                    }
                }
                else
                {
                        payment.Paid_Date = DateTime.Now;
                }

                if (payment.Paid_Date != null && (payment.Cancel_Conf_Number ?? "").Length <= 0 && payment.Type.Value == 1)
                    payment.Payment_Processed = "1";
            }
        }

        private void UpdateInvoiceItems()
        {
            foreach (var invoiceItem in db.WEB_Invoice_Items.Where(i => i.Invoice == invoice.Inv_NO))
            {
                invoiceItem.Process_Date = DateTime.Now;
                invoiceItem.Imported = true;
            }
        }

        private void ProcessCancellation(WEB_Payments payment)
        {
            payment.Cancel_Conf_Number = remittanceDetail.TransactionConfirmationID;
            payment.Cancel_Date = remittanceDetail.DataDate;

            UpdateSetReversals(payment);
        }

        private void UpdateSetReversals(WEB_Payments payment)
        {
            foreach (var item in db.WEB_Invoice_Items.Where(ii => ii.Set_Number == payment.Set_Number))
            {
                if (item.Process_Date != null && item.Active.Value && item.Cancel.Value)
                {
                    RemoveExcessSetReversals();
                    db.SetReversals.Add(new SetReversal() { Set_Number = item.Set_Number.Value, Invoice = item.Invoice.Value });
                }
            }
        }

        private void RemoveExcessSetReversals()
        {
            int minSetNumber =
                db.SetReversals.Where(n => n.Invoice == invoice.Inv_NO).OrderBy(s => s.Set_Number).First().Set_Number;

            foreach (var sn in db.SetReversals.Where(n => n.Invoice == invoice.Inv_NO && n.Set_Number != minSetNumber))
            {
                db.SetReversals.Remove(sn);
            }
        }
    }
}
