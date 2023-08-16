namespace AddOn_API.DTOs.SaleOrder
{
    public class SaleTypesDTO
    {
                public int Id { get; set; }
                public string? Name { get; set; }
                public string? Value { get; set; }
                public string? CreateBy { get; set; }
                public DateTime? CreateDate { get; set; }
                public int? Status { get; set; }
    }
}