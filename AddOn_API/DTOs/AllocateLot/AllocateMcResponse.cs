namespace AddOn_API.DTOs.AllocateLot
{
    public class AllocateMcResponse
    {
        public long Id { get; set; }
        public string PlantCode { get; set; } = null!;
        public string TypeCode { get; set; } = null!;
        public string BarcodeId { get; set; } = null!;
        public int BasketSeq { get; set; }
        public int? BarcodeQty { get; set; }
        public long AllocateLotid { get; set; }
        public long SaleOrderid { get; set; }
        public string Lot { get; set; } = null!;
        public string? SizeNo { get; set; }
        public long AllocateSizeId { get; set; }
        public string? ItemNo { get; set; }
        public string? ItemName { get; set; }
        public string? StatusMc { get; set; }
        public long? ProductionOrderId { get; set; }
        public string? ItemCodePd { get; set; }
        public string? ItemNamePd { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Buy { get; set; }
        public string? Width { get; set; }
        public string? Ponumber { get; set; }
        public string? ShipToCode { get; set; }
        public string? ShipToDesc { get; set; }
        public string? Colors { get; set; }
        public string? Category { get; set; }
        public string? Gender { get; set; }

    }
}