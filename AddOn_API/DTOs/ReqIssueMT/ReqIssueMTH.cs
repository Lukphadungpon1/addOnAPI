namespace AddOn_API.DTOs.ReqIssueMT
{
    public class ReqIssueMTH
    {
        public long Id { get; set; }
        public string? ReqNumber { get; set; }
       
        public string? Lot { get; set; }
        public string? RequestBy { get; set; }
        public DateTime? RequestDate { get; set; }
        public string? ReqDept { get; set; }
        public DateTime? RequireDate { get; set; }
        public string? Remark { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Status { get; set; }

        public virtual ICollection<ReqIssueMTD> ReqIssueMaterialDs { get; set; }
        public virtual ICollection<ReqIssueMTLog> ReqIssueMaterialLogs { get; set; }
    }
}