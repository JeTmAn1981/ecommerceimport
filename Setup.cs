using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static EcommerceImport.Utilities;
using static EcommerceImport.Logging;
using EcommerceImport;
using EcommerceImport.ImportFiles;

namespace EcommerceImport
{
    public class Setup
    {
        public static void DoSetup(string[] args)
        {
            CheckCommandLineArguments(args);
            SetupLogger();
            SetupFileProcessors();
        }

        private static void SetupFileProcessors()
        {
            if (test)
            {
                fileProcessors = new List<PaymentSourceProcessor>();

                if (includeBank)
                {
                    fileProcessors.Add(new BankFileProcessor(logger));
                }

                if (includeSlate)
                {
                    fileProcessors.Add(new SlateFileProcessor(logger));
                }

                if (includeDebit)
                {
                    fileProcessors.Add(new DebitInvoiceProcessor(logger));
                }
            }
            else
            {
                fileProcessors = new List<PaymentSourceProcessor>()
                {
                    new BankFileProcessor(logger),
                    new SlateFileProcessor(logger),
                    new DebitInvoiceProcessor(logger)
                };
            }
        }

        private static void SetupLogger()
        {
            if (test)
            {
                logger = new Logger("EcommerceImport",LogFileTestPath, ref notifications);
            }
            else
            {
                logger = new Logger("EcommerceImport", LogFilePath, ref notifications);
            }
        }

        private static void CheckCommandLineArguments(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg == "test")
                {
                    test = true;
                }
                else if (arg == "returnedpaymentsallowed")
                {
                    returnedpaymentsallowed = true;
                }
                else if (arg == "includebank")
                {
                   includeBank = true;
                }
                else if (arg == "includeslate")
                {
                    includeSlate = true;
                }
                else if (arg == "includedebit")
                {
                    includeDebit = true;
                }
            }
        }
    }
}
