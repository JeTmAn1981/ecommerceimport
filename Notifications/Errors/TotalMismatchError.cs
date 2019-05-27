using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EcommerceImport.Utilities;
using static EcommerceImport.Logging;
using static EcommerceImport.Logging;


namespace EcommerceImport
{
    class TotalMismatchError : Notification
    {
        
        string total1Name;
        decimal total1;
        string total2Name;
        decimal total2;

        public TotalMismatchError(int invoice, string total1Name, decimal total1, string total2Name, decimal total2)
        {
            this.invoice = invoice;
            this.total1Name = total1Name;
            this.total1 = total1;
            this.total2Name = total2Name;
            this.total2 = total2;

            SetMessage();
            logger.LogMessage(this);
        }

        public override void SetMessage()
        {
            message = Environment.NewLine + $"ERROR for Invoice #{invoice}: {total1Name} total is ${total1}.  This does not match the {total2Name} total of ${total2}." + Environment.NewLine + "";
        }
    }
}
