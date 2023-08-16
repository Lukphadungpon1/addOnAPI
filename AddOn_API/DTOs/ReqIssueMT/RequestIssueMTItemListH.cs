using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AddOn_API.DTOs.ReqIssueMT;

public class RequestIssueMTItemListH
{
     public long Id { get; set; }
     public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
        public decimal? PlandQty { get; set; }
    public string? UomName { get; set; }
    public string? Department { get; set; }
    public decimal? Onhand { get; set; }

    public string? WhsCode {get; set; }
    public decimal? OnhandDFwh { get; set; }

    public List<ReqIssueMTItemList> ItemD { get; set; }

}
