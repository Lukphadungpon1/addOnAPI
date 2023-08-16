namespace AddOn_API.DTOs.AllocateLot
{
    public class AllocateLotSizeExcelLotList
    {
         public long Id { get; set; }
        public string Lot { get; set; } = null!;
        public string? SizeNo { get; set; }      
        public int? Qty { get; set; }
 
    }
}