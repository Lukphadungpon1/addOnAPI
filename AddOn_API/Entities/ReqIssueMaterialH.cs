using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class ReqIssueMaterialH
    {
        public ReqIssueMaterialH()
        {
            ReqIssueMaterialDs = new HashSet<ReqIssueMaterialD>();
            ReqIssueMaterialLogs = new HashSet<ReqIssueMaterialLog>();
        }

        public long Id { get; set; }
        public string? ReqNumber { get; set; }
        public string? Lot { get; set; }
        public string? RequestBy { get; set; }
        public DateTime? RequestDate { get; set; }
        public string? ReqDept { get; set; }
        public string? Site { get; set; }
        public DateTime? RequireDate { get; set; }
        public string? Remark { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Status { get; set; }

        public virtual ICollection<ReqIssueMaterialD> ReqIssueMaterialDs { get; set; }
        public virtual ICollection<ReqIssueMaterialLog> ReqIssueMaterialLogs { get; set; }
    }
}
