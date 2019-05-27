using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static System.IO.File;
using EcommerceImport.ImportFiles;
using static EcommerceImport.Utilities;

namespace EcommerceImport.ImportFiles
{
    public abstract class FileProcessor: PaymentSourceProcessor
    {
        protected string inputFilePath,archivePath;
        protected string filePattern;
       
        public FileProcessor(Logger logger):base(logger)
        {
            SetFileVariables();
        }

        public override void ProcessRecords()
        {
            TransferFiles();
            GetImportDetailsFromAllFiles();

            ImportDetails();

            if (writeOutputFile)
            {
                WriteImportDataToString();
                WriteImportStringDataToFile();
            }

            MoveToArchive();
        }

        private void GetImportDetailsFromAllFiles()
        {
            foreach (string file in Directory.EnumerateFiles(inputFilePath, filePattern, SearchOption.TopDirectoryOnly))
            {
                if (file.IndexOf('~') == -1)
                {
                    logger.LogMessage($"Processing file: {file}");
                    GetImportDetails(file);
                }
            }
        }

        private void MoveToArchive()
        {
            foreach (string file in Directory.EnumerateFiles(inputFilePath, filePattern, SearchOption.TopDirectoryOnly))
            {
                string newFilePath = archivePath + GetCurrentDateFormatted() + " " + Path.GetFileName(file);

                logger.LogMessage("Moving " + file + " to " + newFilePath);

                File.Copy(file, newFilePath, true);
                File.Delete(file);
            }
        }

        protected override void WriteImportStringDataToFile()
        {
            AppendAllText(ecommOutputFilePath, ecommerceImportData);
            AppendAllText(ecommStuCreditOutputFilePath, ecommerceStuCreditImportData);
        }


        protected abstract void ImportDetails();
        protected abstract void GetImportDetails(string file);

        protected abstract void TransferFiles();
    }
}
