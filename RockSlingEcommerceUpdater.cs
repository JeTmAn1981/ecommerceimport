﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EcommerceImport.Utilities;

using static WhitTools.SQL;
using EcommerceImport.Data;

namespace EcommerceImport
{
    public class RockSlingEcommerceUpdater:EcommerceUpdater
    {
        private nopCommerceEntities nopCommercedb = new nopCommerceEntities();

        public RockSlingEcommerceUpdater(WEB_Invoices invoice, RemittanceDetail remittanceDetail = null):base(invoice,remittanceDetail)
        {
        }

        protected override void UpdateSpecialDetails()
        {
            //Invoice was generated by Rock & Sling nopCommerce-powered storefront, so update that database 
            //to reflect paid status

            var rockSlingOrder = nopCommercedb.Orders.FirstOrDefault(o => o.Id == nopCommercedb.OrderUSBankTransactionKeys.FirstOrDefault(tk => tk.TransactionKey == invoice.Inv_NO).OrderID);

            if (rockSlingOrder != null) 
            {
                rockSlingOrder.PaymentStatusId = 30;
                nopCommercedb.SaveChanges();
            }
            
        }
        
    }
}