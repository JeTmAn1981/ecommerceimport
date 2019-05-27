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
    class MissingStudentIDError : Notification
    {
        private WEB_Invoice_Items item;
        
        public MissingStudentIDError(WEB_Invoice_Items item)
        {
            this.item = item;
            this.invoice = item.Invoice ?? 0;

            notifyAdministrators = true;

            SetMessage();
            logger.LogMessage(this);
        }

        public override void SetMessage()
        {
            message = "ERROR: A student debit invoice item without a valid Whitworth student ID was encountered. Item information is as follows:" + Environment.NewLine + Environment.NewLine;
            message += item.ToString() + Environment.NewLine + Environment.NewLine;
         }
    }
}
