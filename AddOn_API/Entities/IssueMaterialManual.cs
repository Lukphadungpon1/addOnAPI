using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class IssueMaterialManual
    {
        public long Id { get; set; }
        public long? IssueHid { get; set; }
        public string? Buy { get; set; }
        public string? Lot { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
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
        public int? ConvertSap { get; set; }
        public int? DocEntry { get; set; }
        public string? DocNum { get; set; }
        public string? Location { get; set; }

        public virtual IssueMaterialH? IssueH { get; set; }
    }
}
