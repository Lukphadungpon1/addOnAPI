namespace AddOn_API.DTOs.SAPQuery
{
    public class SaleOrderQuery
    {
        public int Id { get; set; }
        public int DocEntry { get; set; }
        public string? DocNum { get; set; }
        public string? CardCode { get; set; }
        public string? CardName { get; set; }
        public string? Buy { get; set; }
         public string? ItemCode { get; set; }
         public int LineNum { get; set; }
        public string? Dscription { get; set; }
        public decimal? Quantity { get; set; }
        public string? PoNumber { get; set; }
        public string? Width { get; set; }
        public string? ShipToCode { get; set; }
        public string? ShipToDesc { get; set; }


    }
}