using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EcommerceImport.Utilities;
using static EcommerceImport.Logging;
using EcommerceImport.Data;
using EcommerceImport;
using EcommerceImport.ImportFiles;


namespace EcommerceImport
{
    class InvalidGLAccountError : Notification
    {
        private SlateDetail slateDetail;
        private SlateDetailGLNumber account;

        public InvalidGLAccountError(SlateDetail slateDetail,SlateDetailGLNumber account)
        {
            this.slateDetail = slateDetail;
            this.account = account;
            administratorEmail = "redacted@whitworth.edu";
            notifyAdministrators = true;

            SetMessage();
            logger.LogMessage(this);
        }

        public override void SetMessage()
        {
            message = "ERROR: A Slate record was transmitted which contained an invalid GL account number.  This record will not be imported.  The Slate record is as follows:" + Environment.NewLine + Environment.NewLine;
            message += slateDetail.ToString() + Environment.NewLine + Environment.NewLine;
            message += $"The invalid GL Account is: {account.type} - {account.GetFormattedAccountNumber()}" + Environment.NewLine + Environment.NewLine;
        }
    }
}
