using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace EcommerceImport.Data
{
    public partial class SlateDetailGLNumber
    {
        //public string type { get; }
        //public string accountNumber { get; }

        public SlateDetailGLNumber(string type, string accountNumber)
        {
            this.type = type;
            this.accountNumber = FormatAccount(accountNumber);
        }

        private string FormatAccount(string account)
        {
            return new Regex(@"[^\d]").Replace(account, "");
        }

        public bool AccountIsValid()
        {
           return WhitTools.Validator.GLAccountExists(accountNumber);
        }

        public string GetFormattedAccountNumber()
        {
            return (accountNumber ?? "").Trim() == "" ? "No account number provided" : accountNumber;
        }
    }

    
}
