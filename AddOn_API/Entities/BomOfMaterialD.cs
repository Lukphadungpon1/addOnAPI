using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class BomOfMaterialD
    {
        public string ItemCodeH { get; set; } = null!;
        public string Version { get; set; } = null!;
        public int LineNum { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public decimal? Quantity { get; set; }
        public string? UomName { get; set; }
        public string? Warehouse { get; set; }
        public string? Comment { get; set; }
        public string? Comment2 { get; set; }
        public string? Department { get; set; }
        public string? Status { get; set; }

        public virtual BomOfMaterialH BomOfMaterialH { get; set; } = null!;
    }
}
