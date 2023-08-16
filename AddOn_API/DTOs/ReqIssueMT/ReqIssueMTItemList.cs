namespace AddOn_API.DTOs.ReqIssueMT
{
    public class ReqIssueMTItemList
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
        public string? Department { get; set; }
        
        public string? Request { get; set; } 
        public bool chkReqIss { get; set; }      
        public decimal? Onhand { get; set; }

        public string? WhsCode {get; set; }
        public decimal? OnhandDFwh { get; set; }

       public string? Lot { get; set; }
        public string? ItemCodeS { get; set; }
        public string? SizeNo { get; set; }


    }
}