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
    
    public partial class BatchIndex
    {
        public int BatchID { get; set; }
        public Nullable<System.DateTime> EntryDate { get; set; }
        public string Reference { get; set; }
        public int FillingLineID { get; set; }
        public int CommodityID { get; set; }
        public int CartonPerPallet { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime EditedDate { get; set; }
        public bool IsDefault { get; set; }
        public string CommodityCode { get; set; }
        public string CommodityName { get; set; }
        public int PackPerCarton { get; set; }
        public string CommodityOfficialCode { get; set; }
        public string NextPackNo { get; set; }
        public string NextCartonNo { get; set; }
        public string NextPalletNo { get; set; }
        public string BatchCode { get; set; }
        public bool InActive { get; set; }
        public decimal Volume { get; set; }
        public string CommodityAPICode { get; set; }
        public int Shelflife { get; set; }
        public bool AutoBarcode { get; set; }
        public string FinalCartonNo { get; set; }
        public decimal PackageVolume { get; set; }
        public bool AutoCarton { get; set; }
        public string BatchPackNo { get; set; }
        public string BatchCartonNo { get; set; }
        public string BatchPalletNo { get; set; }
        public int EntryMonthID { get; set; }
        public string SentPackNo { get; set; }
        public string SentCartonNo { get; set; }
        public string SentPalletNo { get; set; }
    }
}
