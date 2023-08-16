using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class ReqIssueMaterialLog
    {
        public int Id { get; set; }
        public long ReqHid { get; set; }
        public string? Users { get; set; }
        public DateTime? LogDate { get; set; }
        public string? Status { get; set; }
        public int? Levels { get; set; }
        public string? Comment { get; set; }
        public string? Action { get; set; }
        public string? ClientName { get; set; }

        public virtual ReqIssueMaterialH ReqH { get; set; } = null!;
    }
}
