namespace AddOn_API.DTOs.SAPQuery
{
    public class BomofMaterialSAPD
    {
         public string ItemCodeH { get; set; }
        public string Version { get; set; }
        public int LineNum { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public decimal? Quantity { get; set; }
        public string? UomName { get; set; }
        public string? Warehouse { get; set; }
        public string? Comment { get; set; }
        public string? Comment2 { get; set; }
        public string? DepartmentCode { get; set; }
        public string? DepartmentName { get; set; }
    }
}