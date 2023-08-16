using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class SaleOrderD
    {
        public long Id { get; set; }
        public long Sohid { get; set; }
        public int LineNum { get; set; }
        public string? ItemCode { get; set; }
        public string? BillOfMaterial { get; set; }
        public string? Dscription { get; set; }
        public decimal? Quantity { get; set; }
        public string? Width { get; set; }
        public string? UomCode { get; set; }
        public string? ShipToCode { get; set; }
        public string? ShipToDesc { get; set; }
        public string? PoNumber { get; set; }
        public string? Buy { get; set; }
        public string? ItemNo { get; set; }
        public decimal? SizeNo { get; set; }
        public string? Colors { get; set; }
        public string? Category { get; set; }
        public string? Gender { get; set; }
        public string? Style { get; set; }
        public string? LineStatus { get; set; }
        public long? GenerateLot { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? Updateby { get; set; }
        public DateTime? UpdateDate { get; set; }

        public virtual SaleOrderH Soh { get; set; } = null!;
    }
}
