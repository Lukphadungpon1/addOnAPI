namespace AddOn_API.DTOs.SaleOrder
{
    public class DetailExcelFile
    {
          public int Id {get; set;}
        public string ItemCode { get; set; }
        public decimal Quantity { get; set; }
        public string ShipToCode { get; set; }
        public string ShipToDesc { get; set; }
        public string PoNumber { get; set; }
        public string Width { get; set; }
        public string Style { get; set; }
        public string Status { get; set; }
         public string Colors { get; set; }
        public string Category { get; set; }
        public string Gender { get; set; }

    }
}