namespace AddOn_API.DTOs.QueueCVSAP
{
    public class QueueCVSAPRequest
    {
          public long Id { get; set; }
        public string? TypeDc { get; set; }
        public long? DocumentId { get; set; }
        public string? CreateBy { get; set; }

    }
}