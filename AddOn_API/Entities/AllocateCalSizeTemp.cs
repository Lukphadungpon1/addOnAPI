using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class AllocateCalSizeTemp
    {
        public long RowId { get; set; }
        public string? Types { get; set; }
        public string? ItemCode { get; set; }
        public long? SalesOrderEntry { get; set; }
        public string? SalesOrder { get; set; }
        public string? PurOrder { get; set; }
        public string? ItemNo { get; set; }
        public string? ItemName { get; set; }
        public string? Width { get; set; }
        public string? ShipToCode { get; set; }
        public string? SizeNo { get; set; }
        public int? Total { get; set; }
        public int? Qty { get; set; }
        public int? Used { get; set; }
        public string? Buy { get; set; }
        public int? RowNo { get; set; }
        public string? TypeSale { get; set; }
    }
}
