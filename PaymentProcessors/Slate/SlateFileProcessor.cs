using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcommerceImport;
using static EcommerceImport.Utilities;
using System.Text.RegularExpressions;
using EcommerceImport.ImportFiles;

using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Data;
using System.Data.OleDb;
using static System.IO.File;
using EcommerceImport.Data;
using WinSCP;
using System.Web.UI.WebControls;



namespace EcommerceImport
{
    public class SlateFileProcessor : FileProcessor
    {
        private const string SLATE_ECOMMERCE_PATH = "/outgoing/ecommerce/*";
        List<SlateDetail> details;
     
        public SlateFileProcessor(Logger logger) : base(logger)
        {
            name = "Slate Payments";
        }

        protected override void SetFileVariables()
        {
            filePattern = "*.xlsx";

            SetFilePaths();
        }

        private void SetFilePaths()
        {
            if (test)
            {
                inputFilePath = @"\\web3\e$\Imports\eCommerceImport\test\Slate";
                archivePath = @"\\web3\e$\Imports\eCommerceImport\test\Slate\Archive\";

                ecommOutputFilePath = @"\\datatel\gl.interfaces\test\SLATEECOMMERCE.TXT"; ;
                ecommStuCreditOutputFilePath = @"\\datatel\gl.interfaces\test\SLATEECOMMSTU.TXT"; ;
            }
            else
            {
                inputFilePath = @"\\web3\e$\Imports\eCommerceImport\Slate";
                archivePath = @"\\web3\e$\Imports\eCommerceImport\Slate\Archive\";

                ecommOutputFilePath = @"\\datatel\gl.interfaces\SLATEECOMMERCE.TXT"; ;
                ecommStuCreditOutputFilePath = @"\\datatel\gl.interfaces\SLATEECOMMSTU.TXT";
            }
        }

        protected override void GetImportDetails(string file)
        {
            InitializeDetails();

            DataSet ds = new DataSet();
            FileInfo importFile = new FileInfo(file);

            string connectionString = GetConnectionString(file);

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = conn;

                // Get all Sheets in Excel File
                DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                // Loop through all Sheets to get data
                foreach (DataRow dr in dtSheet.Rows)
                {
                    string sheetName = dr["TABLE_NAME"].ToString();

                    if (!sheetName.EndsWith("$"))
                        continue;

                    // Get all rows from the Sheet
                    cmd.CommandText = "SELECT * FROM [" + sheetName + "]";

                    DataTable dt = new DataTable();
                    dt.TableName = sheetName;

                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    da.Fill(dt);

                    ds.Tables.Add(dt);
                }

                cmd = null;
                conn.Close();

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    AddSlateDetail(row, importFile.LastWriteTime);
                }

                SaveDetailsToDatabase();
            }
        }

        private void InitializeDetails()
        {
            details = new List<SlateDetail>();
        }

        private void SaveDetailsToDatabase()
        {
            db.SlateDetails.AddRange(details);
            db.SaveChanges();
        }

        private void AddSlateDetail(DataRow row, DateTime fileLastWriteTime)
        {
            SlateDetail detail = new SlateDetail();

            detail.firstName = row.ItemArray[0].ToString();
            detail.lastName = row.ItemArray[1].ToString();
            detail.whitworthID = row.ItemArray[2].ToString();
            detail.date = Convert.ToDateTime(row.ItemArray[3]);
            detail.netAmount = GetDouble(row.ItemArray[4].ToString());
            detail.SlateDetailGLNumber = new SlateDetailGLNumber("Revenue", row.ItemArray[5].ToString());
            detail.action = row.ItemArray[6].ToString();
            detail.fee = GetDouble(row.ItemArray[7].ToString());
            detail.SlateDetailGLNumber1 = new SlateDetailGLNumber("Fees", row.ItemArray[8].ToString());
            detail.paymentDescription = row.ItemArray[9].ToString();
            detail.dateProcessed = fileLastWriteTime;
            details.Add(detail);
        }
     

        private static double GetDouble(string item)
        {
            Double value = 0;
            
            try
            {
                value = Math.Abs(Convert.ToDouble(item));
            }
            catch
            {

            }

            return value;
        }

        protected override void ImportDetails()
        {
            if (!(test && !importDetails))
            {
                foreach (SlateDetail detail in details)
                {
                    detail.Import();
                }
            }
        }
        
        protected override void WriteImportDataToString()
        {
            logger.LogMessage("Writing Slate remittance details.");

            foreach (var detail in details)
            {
                var invoiceNumber = detail.GetInvoiceNumber();

                List<WEB_Invoice_Items> items = db.WEB_Invoice_Items.ToList().Where(i => i.Invoice == invoiceNumber).ToList();

                if (items.Count() > 0)
                {
                    logger.LogMessage("Writing items for " + detail.ToString());

                    items.ForEach(i => i.WriteToString(GetImportLineWriter(i)));

                    logger.LogMessage(Environment.NewLine);
                }
                else
                    logger.LogMessage("No items found for: " + detail.ToString());
            }
        }


        private string GetConnectionString(string file)
        {
            Dictionary<string, string> props = new Dictionary<string, string>();

            // XLSX - Excel 2007, 2010, 2012, 2013
            props["Provider"] = "Microsoft.ACE.OLEDB.12.0;";
            props["Extended Properties"] = "Excel 12.0 XML";
            props["Data Source"] = file;

            // XLS - Excel 2003 and Older
            //props["Provider"] = "Microsoft.Jet.OLEDB.4.0";
            //props["Extended Properties"] = "Excel 8.0";
            //props["Data Source"] = "C:\\MyExcel.xls";

            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, string> prop in props)
            {
                sb.Append(prop.Key);
                sb.Append('=');
                sb.Append(prop.Value);
                sb.Append(';');
            }

            return sb.ToString();
        }

        protected override void TransferFiles()
        {
            if (!test)
            {
                try
                {
                    // Setup session options
                    SessionOptions sessionOptions = new SessionOptions
                    {
                        Protocol = Protocol.Sftp,
                        HostName = "redacted",
                        UserName = "redacted",
                        Password = "redacted",
                        PortNumber = 22,
                        SshHostKeyFingerprint = "redacted"
                    };

                    using (Session session = new Session())
                    {
                        // Connect
                        session.Open(sessionOptions);

                        // Upload files
                        TransferOptions transferOptions = new TransferOptions();
                        transferOptions.TransferMode = TransferMode.Binary;

                        TransferOperationResult transferResult;
                        transferResult = session.GetFiles(SLATE_ECOMMERCE_PATH, @"\\web3\e$\Imports\eCommerceImport\Slate\", false, transferOptions);

                        // Throw on any error
                        transferResult.Check();

                        // Print results
                        foreach (TransferEventArgs transfer in transferResult.Transfers)
                        {

                            Console.WriteLine("Download of {0} succeeded", transfer.FileName);
                        }

                        session.RemoveFiles(SLATE_ECOMMERCE_PATH);
                    }
                }
                catch (Exception e)
                {

                    logger.LogMessage("Error: " + e.ToString());
                }
            }
        }

        public override string GetSummaryMessage()
        {
            string message = "";

            try
            {
                var importedItems = (from WEB_Invoice_Items item in db.WEB_Invoice_Items
                                     where
                                item.Imported == true
                                     && item.Active == true
                                     && item.Cancel != true
                                     select item).ToList().Where(item => details.Any(sd => sd.GetInvoiceNumber() == item.Invoice));

                var importedGLItems = importedItems.Where(item => item.itemType.ChargeType == GL_ACCOUNT_CHARGETYPE);
                var importedStudentCreditItems = importedItems.Where(item => item.itemType.ChargeType == STUDENT_ACCOUNT_CREDIT_CHARGETYPE);

                string glPaymentDesignation = "<strong>Slate GL</strong>";
                string studentCreditPaymentDesignation = "<strong>Slate Student Credit</strong>";

                message += "<h2>Slate</h2>";

                if (importedGLItems.Count() > 0)
                {
                    message += WrapWithParagraph( $"{glPaymentDesignation} import total: ${importedGLItems.Sum(i => i.Total)} from {importedGLItems.Select(ci => ci.Invoice).Distinct().Count()} transaction(s).");
                }
                else
                {
                    message += WrapWithParagraph($"No {glPaymentDesignation} transactions were found.");
                }

                if (importedStudentCreditItems.Count() > 0)
                {
                    message += WrapWithParagraph($"{studentCreditPaymentDesignation} import total: ${importedStudentCreditItems.Sum(i => i.Total)} from {importedStudentCreditItems.Select(ci => ci.Invoice).Distinct().Count()} transaction(s).");
                }
                else
                {
                    message += WrapWithParagraph($"No {studentCreditPaymentDesignation} import transactions were found.");
                }

            }
            catch (Exception ex)
            {
                message += ex.ToString();
            }
            
            return "\n" + message;
        }
    }


}
