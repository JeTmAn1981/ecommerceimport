using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EcommerceImport.Utilities;
using static EcommerceImport.Logging;
using EcommerceImport.Data;
using static EcommerceImport.Logging;

namespace EcommerceImport
{
    class RemittanceDetailProcessingError: Notification
    {
        private Func<WEB_Invoice_Items, bool> itemIsProcessed;
        private List<WEB_Invoice_Items> invoiceItems;
        private RemittanceDetail remittanceDetail;
        
        public RemittanceDetailProcessingError(List<WEB_Invoice_Items> invoiceItems, RemittanceDetail remittanceDetail)
        {
            this.invoiceItems = invoiceItems;
            this.invoice = remittanceDetail.TransactionKey ?? 0;
            this.remittanceDetail = remittanceDetail;
            notifyAdministrators = true;

            SetMessage();
            logger.LogMessage(this);
        }

        public override void SetMessage()
        {
            decimal itemTotal = invoiceItems.Sum(item => item.Total) ?? 0;

            message = "ERROR: A remittance detail record was not successfully processed.  The remittance record is as follows:" + Environment.NewLine + Environment.NewLine;
            message += remittanceDetail.ToString() + Environment.NewLine + Environment.NewLine;
            message += $"The item total of ${itemTotal} did not match the remittance detail amount of ${remittanceDetail.TotalAmount} for a difference of ${remittanceDetail.TotalAmount - itemTotal}." + Environment.NewLine + Environment.NewLine;
         }
    }
}
