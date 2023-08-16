namespace AddOn_API.DTOs.SaleOrder
{
    public class SaleOrderResponse
    {
        public SaleOrderResponse()
        {
            SaleOrderD = new HashSet<SaleOrderDResponse>();
        }

        public long Id { get; set; }

         public string? SoNumber { get; set; }
        public int? DocEntry { get; set; }
        public string? DocNum { get; set; }
        public string? CardCode { get; set; }
        public string? CardName { get; set; }
        public decimal? DocRate { get; set; }
        public string? Currency { get; set; }
        public string? Buy { get; set; }
        public string? DocStatus { get; set; }
        public string? SaleTypes { get; set; }
        public string? Remark { get; set; }
        public string? UploadFile { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public long? ConvertSap { get; set; }

        public long? GenerateLot { get; set; }
        public string? GenerateLotBy { get; set; }

        public virtual ICollection<SaleOrderDResponse> SaleOrderD { get; set; }
    }
}