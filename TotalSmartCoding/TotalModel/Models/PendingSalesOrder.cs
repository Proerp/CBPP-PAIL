//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TotalModel.Models
{
    using System;
    
    public partial class PendingSalesOrder
    {
        public int SalesOrderID { get; set; }
        public string SalesOrderReference { get; set; }
        public System.DateTime SalesOrderEntryDate { get; set; }
        public int CustomerID { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public string VoucherCode { get; set; }
        public string ContactInfo { get; set; }
        public string ShippingAddress { get; set; }
        public int SalespersonID { get; set; }
        public int ReceiverID { get; set; }
        public string ReceiverCode { get; set; }
        public string ReceiverName { get; set; }
        public int TeamID { get; set; }
    }
}
