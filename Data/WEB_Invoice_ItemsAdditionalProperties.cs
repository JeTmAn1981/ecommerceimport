using EcommerceImport.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EcommerceImport.Utilities;

namespace EcommerceImport.Data
{
    public partial class WEB_Invoice_Items
    {
        public WEB_ItemType itemType;
        public string description;
        public WEB_Customer customer;

        public void SetupItemInfo()
        {
            if (db.WEB_ItemType.Where(it => it.ID == Item_Type.Value).Any())
            {
                itemType = db.WEB_ItemType.Where(it => it.ID == Item_Type.Value).First();
                itemType.creditGLs = db.WEB_ItemType_CreditGLs.Where(cgl => cgl.ItemType == itemType.ID).ToList();
                try
                {
                    customer = db.WEB_Customer.Where(c => db.WEB_Invoices.Where(i => i.Inv_Customer_ID == c.Customer_ID && i.Inv_NO == Invoice.Value).Any()).First();
                }
                    catch(Exception ex)
                {

                }
                
                description = GetDescription();
            }
            else
            {
                throw new Exception($"Item Type Not Found For Item With:" +
                                    $"" + Environment.NewLine + "Invoice Number:{ Invoice }" + Environment.NewLine + "" +
                                    $"Item ID:{ ID }" + Environment.NewLine + "" +
                                    $"Item Type: { Item_Type.ToString() ?? "None"}");
            }
        }


        public void WriteToString(ImportLineWriter lineWriter)
        {
            SetupItemInfo();

            lineWriter.WriteCharges();
        }

        public string GetDescription()
        {
            string description = customer.LAST_NAME.Replace(" ", "") + "_" + customer.FIRST_NAME.Replace(" ", "");

            description += GetWhitID(customer);
            description += "_" + itemType.ShortDescription;

            return description;
        }

        public override string ToString()
        {
            return $"ID: {ID}, Item Type: {Item_Type}, Cost: {Cost}, Active: {Active}, Imported: {Imported}, Processed Date: {Process_Date}, Purchase Date: {Purchase_Date} ";
        }

        private static string GetWhitID(WEB_Customer customer)
        {
            return customer.Whit_ID.Trim().Length >= 7 ? "_" + customer.Whit_ID : "";
        }
    }
}
