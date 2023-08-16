namespace AddOn_API.DTOs.SaleOrder
{
    public class ReadExcelFile
    {
        public int LineNum {get; set;}
        public string ItemCode { get; set; }
        public decimal Quantity { get; set; }
        public string ShipToCode { get; set; }
        public string ShipToDesc { get; set; }
        public string PoNumber { get; set; }
        public string Width { get; set; }
    }
}