using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class AllocateLotSize
    {
        public long RowId { get; set; }
        public string? Lot { get; set; }
        public string? ItemCode { get; set; }
        public long? AllocateLotId { get; set; }
        public long SaleOrderId { get; set; }
        public int SaleOrderLineNum { get; set; }
        public string? Type { get; set; }
        public string? SizeNo { get; set; }
        public int? Qty { get; set; }
        public int? Receives { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Status { get; set; }
        public string? BomVersion { get; set; }

        public virtual AllocateLot? AllocateLot { get; set; }
        public virtual BomOfMaterialH? BomOfMaterialH { get; set; }
    }
}
