namespace AddOn_API.DTOs.SAPQuery
{
    public class BomSAPQurry
    {
        public int Id {get;set;}
          public string ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Type { get; set; }
        public int? Qauntity { get; set; }
        public string? ToWh { get; set; }
        public string? ProCode { get; set; }
        public string? Uom { get; set; }
        public string Version { get; set; }
        public int LineNum { get; set; }
        public string? ItemCodeD { get; set; }
        public string? ItemNameD { get; set; }
        public decimal? QuantityD { get; set; }
        public string? UomName { get; set; }
        public string? Warehouse { get; set; }
        public string? Comment { get; set; }
        public string? Comment2 { get; set; }
        public string? DepartmentCode { get; set; }
        public string? DepartmentName { get; set; }
    }
}