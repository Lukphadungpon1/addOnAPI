using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AddOn_API.DTOs.Picking;

public class PickingItemH
{
    public int Id { get; set; }
    public string Buy { get; set; }
    public string Location {get; set;}
    public string ItemCode { get; set; }
    public string ItemName { get; set; }
    public string UomName { get; set; }
    public string Color {get; set;}
    public string Warehouse { get; set; }
    public decimal BaseQty { get; set; }
    public decimal Onhand { get; set;}
    public decimal OnhandWH {get;set;}
    public decimal PlandQty { get; set; }
    public decimal PickQty { get; set; }
    public decimal IssueQty { get; set; }
    public decimal? ConfirmQty { get; set; }

     public virtual ICollection<PickingItemD> PickingItemD { get; set; }

}
