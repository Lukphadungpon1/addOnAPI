using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class TpstyleWithLocation
    {
        public long Id { get; set; }
        public string? ArticleCode { get; set; }
        public string? Location { get; set; }
        public string? GroupItem { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Status { get; set; }
    }
}
