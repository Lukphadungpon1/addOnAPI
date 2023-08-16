namespace AddOn_API.DTOs.AllocateLot
{
    public class ProductionHResponse
    {
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
        public string? Width { get; set; }
        public int? SodocEntry { get; set; }

         public string? AllocateLotSizeName { get; set; }

        public virtual ICollection<ProductionOrderDResponse> ProductionOrderDs { get; set; }
    }
}