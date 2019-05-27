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
    class PaymentReceived : Notification
    {
        private RemittanceDetail remittanceDetail;

        public PaymentReceived(RemittanceDetail remittanceDetail, string administratorEmail)
        {
            this.remittanceDetail = remittanceDetail;
            this.invoice = remittanceDetail.TransactionKey ?? 0;
            this.administratorEmail = administratorEmail;
            notifyAdministrators = true;

            SetMessage();
            logger.LogMessage(this);
        }

        public override void SetMessage()
        {
            message = remittanceDetail.ToString() +  
                      Environment.NewLine + $"Payment has been successfully received for this invoice." + 
                      Environment.NewLine + "";
        }
    }
}
