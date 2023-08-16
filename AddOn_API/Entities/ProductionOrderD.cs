using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class ProductionOrderD
    {
        public long Id { get; set; }
        public long Pdhid { get; set; }
        public long AllocateLotSizeId { get; set; }
        public int LineNum { get; set; }
        public string? ItemType { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public decimal? BaseQty { get; set; }
        public decimal? PlandQty { get; set; }
        public string? UomName { get; set; }
        public string? Warehouse { get; set; }
        public string? IssueMethod { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Department { get; set; }
        public string? BomItemCode { get; set; }
        public string? BomVersion { get; set; }
        public string? Status { get; set; }
        public int? BomLine { get; set; }

        public virtual ProductionOrderH ProductionOrderH { get; set; } = null!;
    }
}
