using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AddOn_API.DTOs.Picking;

public class IssueMaterialHResponse
{
        public long Id { get; set; }
        public string? IssueNumber { get; set; }
        public string? Location { get; set; }
        public string Lotlist { get; set; } = null!;
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
        public string? Remark { get; set; }

        public virtual ICollection<IssueMaterialDResponse> IssueMaterialDs { get; set; }
        public virtual ICollection<IssueMaterialLogResponse> IssueMaterialLogs { get; set; }
        public virtual ICollection<IssueMaterialManualResponse> IssueMaterialManuals { get; set; }
}
