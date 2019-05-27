using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EcommerceImport.Utilities;
using static EcommerceImport.Logging;
using EcommerceImport.Data;


namespace EcommerceImport
{
    class IrregularPaymentStatusError : Notification
    {
        private RemittanceDetail remittanceDetail;
        
        public IrregularPaymentStatusError(RemittanceDetail remittanceDetail)
        {
            this.remittanceDetail = remittanceDetail;
            invoice = remittanceDetail.TransactionKey ?? 0;

            administratorEmail = PROGRAMMER_EMAIL; //AddIrregularPaymentStatusAdministrators(); 

            notifyAdministrators = true;

            SetMessage();
            logger.LogMessage(this);
        }

                public override void SetMessage()
        {
            message = @"ERROR: A remittance detail record with an irregular payment status (not ""SENT"") was encountered.  The remittance record is as follows:" + Environment.NewLine + Environment.NewLine;
            message += remittanceDetail.ToString() + Environment.NewLine + Environment.NewLine;

            message += $"Payment Status: {remittanceDetail.PaymentStatus}, ACH Return Code: {remittanceDetail.ACHReturnCode}, ";
            message += $"Reason Description: {remittanceDetail.ReasonDescription}, Return Date: {remittanceDetail.ReturnDate}";
         }
    }
}
