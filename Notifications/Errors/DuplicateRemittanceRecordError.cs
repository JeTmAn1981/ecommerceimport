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
    class DuplicateRemittanceRecordError : Notification
    {
        private RemittanceDetail remittanceDetail;
        
        public DuplicateRemittanceRecordError(RemittanceDetail remittanceDetail)
        {
            this.remittanceDetail = remittanceDetail;
            invoice = remittanceDetail.TransactionKey ?? 0;
            notifyAdministrators = true;

            SetMessage();
            logger.LogMessage(this);
        }

        public override void SetMessage()
        {
            message = "ERROR: A remittance record was transmitted for an invoice that was previously processed.  The remittance record is as follows:" + Environment.NewLine + Environment.NewLine;
            message += remittanceDetail.ToString() + Environment.NewLine + Environment.NewLine;
        }
    }
}
