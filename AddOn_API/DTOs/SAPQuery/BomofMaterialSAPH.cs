namespace AddOn_API.DTOs.SAPQuery
{
    public class BomofMaterialSAPH
    {
      
       public string ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Type { get; set; }
        public int? Qauntity { get; set; }
        public string? ToWh { get; set; }
        public string? ProCode { get; set; }
        public string? Uom { get; set; }


        public virtual ICollection<BomofMaterialSAPD> BomOfMaterialD { get; set; }
       
    }
}