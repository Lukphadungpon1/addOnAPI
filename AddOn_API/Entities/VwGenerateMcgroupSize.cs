using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class VwGenerateMcgroupSize
    {
        public long SaleOrderId { get; set; }
        public string? SoNumber { get; set; }
        public string? Buy { get; set; }
        public string? SaleTypes { get; set; }
        public string? ItemCode { get; set; }
        public string? Dscription { get; set; }
        public decimal? QtySaleOrderGsize { get; set; }
        public string? Width { get; set; }
        public string? ShipToCode { get; set; }
        public string? ShipToDesc { get; set; }
        public string? PoNumber { get; set; }
        public string? Colors { get; set; }
        public string? Category { get; set; }
        public string? Gender { get; set; }
        public long AllocateLotid { get; set; }
        public string Lot { get; set; } = null!;
        public string? ItemNo { get; set; }
        public string? ItemName { get; set; }
        public int? QtyLot { get; set; }
        public long AllocateSizeId { get; set; }
        public string? SizeNo { get; set; }
        public decimal? QtyLotGsize { get; set; }
        public int? QtyGsize { get; set; }
        public int? ProductionOrderId { get; set; }
        public int? ItemCodePd { get; set; }
        public int? ItemNamePd { get; set; }
        public string Project { get; set; } = null!;
        public string PlantCode { get; set; } = null!;
    }
}
