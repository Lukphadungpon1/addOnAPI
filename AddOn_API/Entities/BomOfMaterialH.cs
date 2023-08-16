using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class BomOfMaterialH
    {
        public BomOfMaterialH()
        {
            AllocateLotSizes = new HashSet<AllocateLotSize>();
            BomOfMaterialDs = new HashSet<BomOfMaterialD>();
        }

        public string ItemCode { get; set; } = null!;
        public string? ItemName { get; set; }
        public string? Type { get; set; }
        public int? Qauntity { get; set; }
        public string Version { get; set; } = null!;
        public string? DefaultBom { get; set; }
        public DateTime? DefaultBomDate { get; set; }
        public string? ToWh { get; set; }
        public short? PriceList { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Status { get; set; }
        public string? ProCode { get; set; }
        public int? ConvertSap { get; set; }
        public DateTime? CenvertSapdate { get; set; }

        public virtual ICollection<AllocateLotSize> AllocateLotSizes { get; set; }
        public virtual ICollection<BomOfMaterialD> BomOfMaterialDs { get; set; }
    }
}
