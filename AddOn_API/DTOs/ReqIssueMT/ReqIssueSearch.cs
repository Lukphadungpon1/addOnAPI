using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AddOn_API.DTOs.ReqIssueMT;

public class ReqIssueSearch
{
    public int Id { get; set; }
    public string? ReqNumber { get; set; }
    public string? Lot { get; set; }
    public string? Location { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public string? ReqDept { get; set; }
    public string? RequestBy { get; set; }
    public string? RequestDate { get; set; }


}
