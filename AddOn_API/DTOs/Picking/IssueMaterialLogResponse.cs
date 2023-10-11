using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AddOn_API.DTOs.Picking;

public class IssueMaterialLogResponse
{
    public long Id { get; set; }
        public long? IssueHid { get; set; }
        public string? Users { get; set; }
        public DateTime? LogDate { get; set; }
        public string? Status { get; set; }
        public int? Levels { get; set; }
        public string? Comment { get; set; }
        public string? Action { get; set; }
        public string? ClientName { get; set; }
}
