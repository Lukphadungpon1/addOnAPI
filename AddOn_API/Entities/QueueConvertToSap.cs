using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class QueueConvertToSap
    {
        public long Id { get; set; }
        public string? TypeDc { get; set; }
        public long? DocumentId { get; set; }
        public long? DocEntry { get; set; }
        public string? DocNum { get; set; }
        public string? ErrorMessage { get; set; }
        public int? Complete { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Status { get; set; }
        public string? Lot { get; set; }
    }
}
