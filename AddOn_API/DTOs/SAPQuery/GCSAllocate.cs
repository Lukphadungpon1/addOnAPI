namespace AddOn_API.DTOs.SAPQuery
{
    public class GCSAllocate
    {
        public int Id { get; set; }
        public string SoNumber {get; set;}
        public int GCS_SalesOrderEntry { get; set; }
        public int GCS_SalesOrder { get; set; }
        public DateTime GCS_DocDate { get; set; }
        public string GCS_PurOrder { get; set; }
        public string GCS_ItemCode { get; set; }
        public string GCS_ItemName { get; set; }
        public string GCS_Width { get; set; }
        public DateTime GCS_StartDate { get; set; }
        public int GCS_Total { get; set; }
        public int GCS_060 { get; set; }
        public int GCS_065 { get; set; }
        public int GCS_070 { get; set; }
        public int GCS_075 { get; set; }
        public int GCS_080 { get; set; }
        public int GCS_085 { get; set; }
        public int GCS_090 { get; set; }
        public int GCS_095 { get; set; }
        public int GCS_100 { get; set; }
        public int GCS_105 { get; set; }
        public int GCS_110 { get; set; }
        public int GCS_115 { get; set; }
        public int GCS_120 { get; set; }
        public int GCS_130 { get; set; }
        public int GCS_140 { get; set; }
        public int GCS_150 { get; set; }
        public int GCS_160 { get; set; }
        public int GCS_170 { get; set; }
        public string GCS_Generate { get; set; }
        public int GCS_050 { get; set; }
        public int GCS_055 { get; set; }
        public string RF_PRD_LINE { get; set; }
        public int GCS_035 { get; set; }
        public int GCS_040 { get; set; }
        public int GCS_045 { get; set; }

        public string? message { get; set; }
        
    }
}