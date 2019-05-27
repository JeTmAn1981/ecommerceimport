using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EcommerceImport.Utilities;
using static EcommerceImport.Logging;
using EcommerceImport.ImportFiles;
using EcommerceImport.Data;


namespace EcommerceImport
{
    class ImportStudentAccountCreditLineWriter : ImportStudentAccountLineWriter
    {
        public ImportStudentAccountCreditLineWriter(WEB_Invoice_Items item, PaymentSourceProcessor processor) : base(item, processor)
        {

        }

        protected override void WriteImportLineToString(string importLine)
        {
            logger.LogMessage($"Writing {importLine}");

            processor.ecommerceStuCreditImportData += importLine + Environment.NewLine;
        }


    }
}
