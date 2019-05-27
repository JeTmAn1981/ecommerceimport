using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EcommerceImport.Utilities;

using EcommerceImport.ImportFiles;
using EcommerceImport.Data;

namespace EcommerceImport
{
    public abstract class ImportLineWriter
    {
        protected WEB_Invoice_Items item;
        protected string data;
        protected PaymentSourceProcessor processor;

        protected ImportLineWriter(WEB_Invoice_Items item, PaymentSourceProcessor processor) 
        {
            this.item = item;
            this.processor = processor;
        }
                
        public abstract void WriteCharges();

        protected abstract void WriteImportLineToString(string importLine);
      
        
        protected decimal GetAmountToChargeGL(WEB_ItemType_CreditGLs creditGL)
        {
            decimal amount;
            decimal GLChargeAmount = Convert.ToDecimal(creditGL.ChargeAmount.Value);
            decimal itemValue = Convert.ToDecimal(item.Total.Value);

            if (creditGL.DivisionType == "ABSOLUTE")
                amount = GLChargeAmount;
            else if (creditGL.DivisionType == "PERCENT")
                amount = (GLChargeAmount / 100) * itemValue;
            else
                amount = itemValue;

            return item.itemType.ChargeType != 3 ? amount : -amount;
        }

    }
}
