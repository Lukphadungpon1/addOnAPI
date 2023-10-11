using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AddOn_API.DTOs.Picking;

public class PickingItemD
{
       public long? Id { get; set; }
        public int IssueHid { get; set; }
        public string? Buy { get; set; }
        public string? Lot { get; set; }
        public long? ReqHid { get; set; }
        public long? ReqDid { get; set; }
        public long? Pdhid { get; set; }
        public long? Pddid { get; set; }
        public int? LineNum { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? UomName { get; set; }
        public string? Warehouse { get; set; }
        public decimal? BaseQty { get; set; }
        public decimal? PlandQty { get; set; }
        public decimal? PickQty { get; set; }
        public int IssueQty { get; set; }
        public int ConfirmQty { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Status { get; set; }
        public string? Location { get; set; }
        public string? SizeNo { get; set; }

}
