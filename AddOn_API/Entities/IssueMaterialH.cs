using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class IssueMaterialH
    {
        public IssueMaterialH()
        {
            IssueMaterialDs = new HashSet<IssueMaterialD>();
            IssueMaterialLogs = new HashSet<IssueMaterialLog>();
        }

        public long Id { get; set; }
        public string? IssueNumber { get; set; }
        public string? Location { get; set; }
        public string? PickingBy { get; set; }
        public DateTime? PickingDate { get; set; }
        public DateTime? PrintDate { get; set; }
        public string? IssueBy { get; set; }
        public DateTime? IssueDate { get; set; }
        public string CreateBy { get; set; } = null!;
        public DateTime? CreateDate { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Status { get; set; }
        public string? UploadFile { get; set; }
        public int? ConvertSap { get; set; }
        public int? DocEntry { get; set; }
        public string? DocNum { get; set; }

        public virtual ICollection<IssueMaterialD> IssueMaterialDs { get; set; }
        public virtual ICollection<IssueMaterialLog> IssueMaterialLogs { get; set; }
    }
}
