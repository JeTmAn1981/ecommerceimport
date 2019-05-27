using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EcommerceImport.Utilities;
using static EcommerceImport.Logging;

namespace EcommerceImport.Data
{
   public partial class RemittanceDetail
    {
        private EcommerceUpdater ecommerceUpdater { get; set; }
        public bool validForImport = true;

        public override string ToString()
        {
            List<string> displayProperties = new List<string>();

            displayProperties.Add(FirstName);
            displayProperties.Add(LastName);
            displayProperties.Add(ProductName);
            displayProperties.Add(TotalAmount.ToString());
            displayProperties.Add(TransactionKey.ToString());
            displayProperties.Add(PaymentStatus);
            displayProperties.Add(TransactionConfirmationID);

            return String.Join(", ", displayProperties);
        }

        public void Import()
        {
            ecommerceUpdater = GetEcommerceUpdater(db.WEB_Invoices.Where(i => i.Inv_NO == TransactionKey.Value).First(), this);

            RemovePreviousImportDetails();

            db.RemittanceDetails.Add(this);

            ecommerceUpdater.UpdateInvoiceDetails();

            db.SaveChanges();

            CheckForNotifications();
        }

        private void RemovePreviousImportDetails()
        {
            /*If remittance record has been processed in a previous batch, remove it first before processing 
            new record*/
            if (RemittanceDetailAlreadyExists())
            {
                if (!returnedpaymentsallowed)
                {
                    logger.LogNotification(new DuplicateRemittanceRecordError(this));
                }

                RemoveRemittanceDetail();
            }

            ecommerceUpdater.ResetInvoiceDetails();
        }

        private void RemoveRemittanceDetail()
        {
            db.RemittanceDetails.Remove(db.RemittanceDetails.Where(rd => rd.TransactionKey == TransactionKey).First());
        }

        private bool RemittanceDetailAlreadyExists()
        {
            return db.RemittanceDetails.Where(rd => rd.TransactionKey == TransactionKey).Any();
        }

        private void CheckForNotifications()
        {
            if (PaymentStatus != PAYMENT_SENT_STATUS)
            {
                logger.LogNotification(new IrregularPaymentStatusError(this));
            }

            //Needs to be updated to pull types and administrator emails for payment received 
            //messages from database

            var currentInvoiceType = db.WEB_InvoiceType.FirstOrDefault(it => it.Id == db.WEB_Invoices.FirstOrDefault(i => i.Inv_NO == TransactionKey).Inv_TYPE);

            if (currentInvoiceType.NotifyOnPayment ?? false)
            {
                var supervisors = (from supervisor in db.Web_InvoiceSupervisors
                                   where supervisor.InvoiceType == currentInvoiceType.Id
                                   select supervisor.Email).ToList();

                if (supervisors.Count > 0)
                {
                    logger.LogNotification(new PaymentReceived(this, String.Join(";", supervisors)));
                }
            }
        }
    }
}
