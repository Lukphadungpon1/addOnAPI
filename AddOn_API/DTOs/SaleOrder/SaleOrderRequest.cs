using System.ComponentModel.DataAnnotations;

namespace AddOn_API.DTOs.SaleOrder
{
    public class SaleOrderRequest
    {
        [Required]
          public int? DocEntry { get; set; }
        [Required]
        public string? SoNumber { get; set; }
        [Required]
        public string? CardCode { get; set; }
          [Required]
        public string? CardName { get; set; }
        [Required]
        public string? Currency { get; set; }
     
     
        [Required]
        public string? Buy { get; set; }
        [Required]
        public string? DocStatus { get; set; }
        [Required]
        public string? SaleTypes { get; set; }
        [Required]
        public DateTime? DeliveryDate { get; set; }
        public string? Remark { get; set; }

        public List<IFormFile>? FormFiles {get; set;}




    }
}