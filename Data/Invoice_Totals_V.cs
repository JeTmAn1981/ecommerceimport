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
    
    public partial class Invoice_Totals_V
    {
        public Nullable<int> Invoice { get; set; }
        public decimal Total_Paid { get; set; }
        public decimal Item_Total { get; set; }
        public decimal Total_Refunds { get; set; }
        public decimal Total_Unpaid_With_Conf { get; set; }
        public decimal Total_Unpaid_Without_Conf { get; set; }
    }
}
