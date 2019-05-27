using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using System.IO;
    using static System.IO.File;
using static EcommerceImport.Utilities;
    using global::EcommerceImport.Data;

    namespace EcommerceImport.ImportFiles
    {
        public abstract class PaymentSourceProcessor
        {
            public string ecommerceImportData;
            public string ecommerceStuCreditImportData, ecommerceStuDebitImportData;
            protected string ecommOutputFilePath, ecommStuCreditOutputFilePath, ecommStuDebitOutputFilePath;
        public string name = "Generic Payment Source Processor";
        protected bool writeOutputFile = true;
        protected Logger logger;

        public PaymentSourceProcessor(Logger logger)
            {
            this.logger = logger;
                ecommerceImportData = "";
                ecommerceStuCreditImportData = "";
                ecommerceStuDebitImportData = "";

                SetFileVariables();
            }

            
            protected ImportLineWriter GetImportLineWriter(WEB_Invoice_Items item)
            {
                item.SetupItemInfo();

                if (item.itemType.ChargeType == 1)
                    return new ImportGLLineWriter(item,  this);
                else if (item.itemType.ChargeType == STUDENT_ACCOUNT_CREDIT_CHARGETYPE)
                    return new ImportStudentAccountCreditLineWriter(item, this);
                else if (item.itemType.ChargeType == STUDENT_ACCOUNT_DEBIT_CHARGETYPE)
                    return new ImportStudentAccountDebitLineWriter(item, this);

                return null;
            }

            protected abstract void WriteImportStringDataToFile();
            
            public abstract void ProcessRecords();

            protected abstract void SetFileVariables();
       
        
            protected abstract void WriteImportDataToString();

            public abstract string GetSummaryMessage();
        }
    }
