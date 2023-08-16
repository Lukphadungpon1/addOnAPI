using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class AllocateCal
    {
        public long RowId { get; set; }
        public string? Name { get; set; }
        public string? Canceled { get; set; }
        public string? Transfered { get; set; }
        public long? SalesOrderEntry { get; set; }
        public string? SalesOrder { get; set; }
        public DateTime? SaleDocDate { get; set; }
        public int? ProductionOrderEntry { get; set; }
        public int? ProductionOrder { get; set; }
        public string? PurOrder { get; set; }
        public string? Lot { get; set; }
        public string? ItemNo { get; set; }
        public string? ItemName { get; set; }
        public string? Width { get; set; }
        public string? ShipToCode { get; set; }
        public string? ShipToName { get; set; }
        public DateTime? StartDate { get; set; }
        public int? Total { get; set; }
        public int? S035 { get; set; }
        public int? S040 { get; set; }
        public int? S045 { get; set; }
        public int? S050 { get; set; }
        public int? S055 { get; set; }
        public int? S060 { get; set; }
        public int? S065 { get; set; }
        public int? S070 { get; set; }
        public int? S075 { get; set; }
        public int? S080 { get; set; }
        public int? S085 { get; set; }
        public int? S090 { get; set; }
        public int? S095 { get; set; }
        public int? S100 { get; set; }
        public int? S105 { get; set; }
        public int? S110 { get; set; }
        public int? S115 { get; set; }
        public int? S120 { get; set; }
        public int? S130 { get; set; }
        public int? S140 { get; set; }
        public int? S150 { get; set; }
        public int? S160 { get; set; }
        public int? S170 { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Freeze { get; set; }
        public string? Buy { get; set; }
    }
}
