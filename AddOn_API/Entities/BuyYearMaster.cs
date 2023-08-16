using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class BuyYearMaster
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Value { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? Status { get; set; }
    }
}
