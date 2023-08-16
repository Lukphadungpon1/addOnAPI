namespace AddOn_API.DTOs.AllocateLot
{
    public class AllocateLotSizeResponse
    {
        public long RowId { get; set; }
       
        public string Lot { get; set; } = null!;
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
        public string? BomVersion { get; set; }
    }
}