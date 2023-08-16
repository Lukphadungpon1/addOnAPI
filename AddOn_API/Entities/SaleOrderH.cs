using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class SaleOrderH
    {
        public SaleOrderH()
        {
            AllocateLots = new HashSet<AllocateLot>();
            SaleOrderDs = new HashSet<SaleOrderD>();
        }

        public long Id { get; set; }
        public string? SoNumber { get; set; }
        public int? DocEntry { get; set; }
        public string? DocNum { get; set; }
        public string? CardCode { get; set; }
        public string? CardName { get; set; }
        public string? Currency { get; set; }
        public string? Buy { get; set; }
        /// <summary>
        /// D = Draft, A = Active,C = Close,CN = Cancel
        /// </summary>
        public string? DocStatus { get; set; }
        public string? SaleTypes { get; set; }
        public string? Remark { get; set; }
        public string? UploadFile { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? ConvertSap { get; set; }
        public long? GenerateLot { get; set; }
        public string? GenerateLotBy { get; set; }

        public virtual ICollection<AllocateLot> AllocateLots { get; set; }
        public virtual ICollection<SaleOrderD> SaleOrderDs { get; set; }
    }
}
