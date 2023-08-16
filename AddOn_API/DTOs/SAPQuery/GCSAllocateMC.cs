namespace AddOn_API.DTOs.SAPQuery
{
    public class GCSAllocateMC
    {
        public int Id { get; set; }
        public string SoNumber {get; set;}
        public string GCS_MainCardCode { get; set; }
        public string GCS_MainCardDesc { get; set; }
        public int GCS_JobOrderEntry { get; set; }
        public string GCS_JobOrder { get; set; }
        public string GCS_SalesOrderEntry { get; set; }
        public string GCS_SalesOrder { get; set; }
        public string GCS_PurOrder { get; set; }
        public string GCS_ItemCode { get; set; }
        public string GCS_ItemName { get; set; }
        public string GCS_SizeCode { get; set; }
        public int GCS_Basket { get; set; }
        public int GCS_Qty { get; set; }
        public DateTime GCS_IssueDate { get; set; }
        public DateTime GCS_ReceiptDate { get; set; }
        public string GCS_Status { get; set; }
        public string GCS_Width { get; set; }
        public string GCS_Lot { get; set; }
        public int RF_MC_QTY { get; set; }
        public int RF_Accum_Rc_Qty { get; set; }

         public string? message { get; set; }
    }
}