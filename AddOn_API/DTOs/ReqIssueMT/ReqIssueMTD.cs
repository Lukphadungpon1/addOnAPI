namespace AddOn_API.DTOs.ReqIssueMT
{
    public class ReqIssueMTD
    {
        public long Id { get; set; }
        public long ReqHid { get; set; }
        public long Pdhid { get; set; }
        public long Pddid { get; set; }
        public int LineNum { get; set; }
        public string ItemCode { get; set; }
        public string? ItemName { get; set; }
         public string? Location { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Status { get; set; }
    }
}