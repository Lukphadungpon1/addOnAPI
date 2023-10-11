using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AddOn_API.DTOs.SAPQuery;

public class BatchNumber
{
     public int Id { get; set; }
     public string ItemCode { get; set; }
     public string WhsCode { get; set; }
     public decimal Quantity { get; set; }
     public decimal UsedQty { get; set; }
     public string DistNumber { get; set; }
     public DateTime InDate { get; set; }
}
