using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class IssueMaterialTranBarcode
    {
        public long Id { get; set; }
        public long IssueDid { get; set; }
        public long IssueHid { get; set; }
        public string BarcodeId { get; set; } = null!;
        public decimal? Qty { get; set; }
        public decimal? IssueQty { get; set; }
        public string? ScanBy { get; set; }
        public DateTime? ScanDate { get; set; }
        public string? Status { get; set; }

        public virtual IssueMaterialD Issue { get; set; } = null!;
    }
}
