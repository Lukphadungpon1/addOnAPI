using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class IssueMaterialD
    {
        public IssueMaterialD()
        {
            IssueMaterialTranBarcodes = new HashSet<IssueMaterialTranBarcode>();
        }

        public long Id { get; set; }
        public long IssueHid { get; set; }
        public string Buy { get; set; } = null!;
        public string Lot { get; set; } = null!;
        public long ReqHid { get; set; }
        public long ReqDid { get; set; }
        public long Pdhid { get; set; }
        public long Pddid { get; set; }
        public int LineNum { get; set; }
        public string ItemCode { get; set; } = null!;
        public string? ItemName { get; set; }
        public string? UomName { get; set; }
        public string? Warehouse { get; set; }
        public string? IssueMethod { get; set; }
        public decimal? BaseQty { get; set; }
        public decimal? PlandQty { get; set; }
        public decimal? PickQty { get; set; }
        public decimal? IssueQty { get; set; }
        public decimal? ConfirmQty { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Status { get; set; }

        public virtual IssueMaterialH IssueH { get; set; } = null!;
        public virtual ReqIssueMaterialH ReqH { get; set; } = null!;
        public virtual ICollection<IssueMaterialTranBarcode> IssueMaterialTranBarcodes { get; set; }
    }
}
