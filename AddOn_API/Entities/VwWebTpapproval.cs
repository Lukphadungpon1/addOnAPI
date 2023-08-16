using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class VwWebTpapproval
    {
        public int RowId { get; set; }
        public string? Department { get; set; }
        public string? Types { get; set; }
        public string? Condition { get; set; }
        public string? Program { get; set; }
        public string? Site { get; set; }
        public int? Levels { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}
