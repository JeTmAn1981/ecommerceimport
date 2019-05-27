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


namespace EcommerceImport.PaymentProcessors.Slate
{
    public class ItemTypeCreator
    {
        string description, creditGL, debitGL;
 
        public ItemTypeCreator(string description, string creditGL, string debitGL)
        {
            this.description = description;
            this.creditGL = creditGL;
            this.debitGL = debitGL;
      }

        
        public int GetItemType(int chargeType)
        {
            WEB_ItemType currentItemType = db.WEB_ItemType.FirstOrDefault(it => 
            it.InvoiceType == SLATE_PAYMENTS_INVOICE_TYPE &&
            it.Description == description && 
            it.ChargeType == chargeType && 
            it.Item_GL_Debit == debitGL && 
            it.WEB_ItemType_CreditGLs.Any(cg => cg.CreditGL == creditGL));

            if (currentItemType == null)
            {
                currentItemType = CreateNewItemType(chargeType);
            }

            return currentItemType.ID;
        }

        private WEB_ItemType CreateNewItemType(int chargeType)
        {
            WEB_ItemType currentItemType = new WEB_ItemType();

            currentItemType.InvoiceType = SLATE_PAYMENTS_INVOICE_TYPE;
            currentItemType.ShortDescription = description;
            currentItemType.Description = description;
            currentItemType.ChargeType = chargeType;
            currentItemType.Item_GL_Debit = debitGL;
            currentItemType.Item_Amt = 0;
            currentItemType.ControlEvaluationType = 1;
            currentItemType.ShowOnBackend = 1;

            if (chargeType == STUDENT_ACCOUNT_CREDIT_CHARGETYPE)
            {
                currentItemType.Item_AR_Code = "ENRS";
                currentItemType.Item_Account_Type = "01";
            }

            db.WEB_ItemType.Add(currentItemType);
            db.SaveChanges();

            AddCreditGL(currentItemType);

            return currentItemType;
        }

        private void AddCreditGL(WEB_ItemType currentItemType)
        {
            WEB_ItemType_CreditGLs newCreditGL = new WEB_ItemType_CreditGLs();

            newCreditGL.ItemType = currentItemType.ID;
            newCreditGL.ChargeAmount = 100;
            newCreditGL.DivisionType = "PERCENT";
            newCreditGL.CreditGL = creditGL;

            db.WEB_ItemType_CreditGLs.Add(newCreditGL);
            db.SaveChanges();
        }

    }

}
