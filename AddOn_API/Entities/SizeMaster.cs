using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class SizeMaster
    {
        public int? RowNumber { get; set; }
        public string? Canceled { get; set; }
        public string? Object { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string SizeCode { get; set; } = null!;
        public int? QtyPerMc { get; set; }
        public int? QtyMin { get; set; }
        public int? QtyMax { get; set; }
        public int? QtyMc { get; set; }
    }
}
