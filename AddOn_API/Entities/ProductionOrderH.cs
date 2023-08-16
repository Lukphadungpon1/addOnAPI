using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class ProductionOrderH
    {
        public ProductionOrderH()
        {
            ProductionOrderDs = new HashSet<ProductionOrderD>();
        }

        public long Id { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int? PlanQty { get; set; }
        public string? UomCode { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public string? Warehouse { get; set; }
        public string? Priority { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Project { get; set; }
        public string? Remark { get; set; }
        public long AllocateLotSizeId { get; set; }
        public int? ConvertSap { get; set; }
        public string? DocNum { get; set; }
        public int? DocEntry { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Lot { get; set; }
        public int? SodocEntry { get; set; }

        public virtual ICollection<ProductionOrderD> ProductionOrderDs { get; set; }
    }
}
