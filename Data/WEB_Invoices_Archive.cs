//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EcommerceImport.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class WEB_Invoices_Archive
    {
        public int Inv_NO { get; set; }
        public string Inv_Customer_ID { get; set; }
        public Nullable<int> Inv_TYPE { get; set; }
        public System.DateTime Inv_DATE { get; set; }
        public bool Inv_Posted_Flag { get; set; }
        public Nullable<System.DateTime> Inv_Completed { get; set; }
        public string Inv_Payment_Type { get; set; }
        public Nullable<bool> Inv_Active { get; set; }
        public string Inv_Confirmation_Number { get; set; }
        public Nullable<double> Inv_Amt_Paid { get; set; }
        public Nullable<int> Inv_Paid { get; set; }
        public Nullable<System.DateTime> Inv_Paid_Date { get; set; }
        public Nullable<int> Inv_Imported { get; set; }
    }
}
