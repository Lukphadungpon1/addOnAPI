using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AddOn_API.DTOs.Picking;

public class IssueMaterialSearch
{
     public long Id { get; set; }
     public string? IssueNumber { get; set; }
    public string? Location { get; set; }
    public string? Lotlist { get; set; }
    public string? PickingBy { get; set; }
    public DateTime? PickingDate { get; set; }
    public DateTime? PrintDate { get; set; }
    public string? IssueBy { get; set; }
    public DateTime? IssueDate { get; set; }
    public string? CreateBy { get; set; }
}
