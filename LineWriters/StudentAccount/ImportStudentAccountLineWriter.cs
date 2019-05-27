using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EcommerceImport.Utilities;

using EcommerceImport.ImportFiles;
using EcommerceImport.Data;

namespace EcommerceImport
{
   public abstract class ImportStudentAccountLineWriter : ImportLineWriter
    {
        protected string commonFields;
        
        public ImportStudentAccountLineWriter(WEB_Invoice_Items item, PaymentSourceProcessor processor) : base(item, processor)
        {
            
        }

        public override void WriteCharges()
        {
            SetCommonFields();
            WriteImportLines(commonFields);
        }

        private void WriteImportLines(string commonFields)
        {
            if (item.itemType.creditGLs.Count() > 0)
            {
                foreach (var creditGL in item.itemType.creditGLs)
                {
                    WriteImportLineToString(commonFields + "," + GetAmountToChargeGL(creditGL) + "," + item.Invoice);
                }
            }
        }

        private void SetCommonFields()
        {
            List<string> importFields = new List<string>();

            importFields.Add(item.customer.Whit_ID);
            importFields.Add(FormatName(item.customer.FIRST_NAME));
            importFields.Add(FormatName(item.customer.LAST_NAME));
            importFields.Add(item.itemType.Item_AR_Code);
            importFields.Add(item.itemType.Item_Account_Type);
            importFields.Add((item.Process_Date ?? DateTime.Now).ToString("M/d/yyyy"));

            commonFields = String.Join(",", importFields);
        }

        private string FormatName(string name)
        {
            name = name.Trim();
            
            if (name.Length > 0)
                name = (name.Substring(0, 1).ToUpper() + name.Substring(1, name.Length - 1).ToLower());
            
            return name;
        }
    }
}
