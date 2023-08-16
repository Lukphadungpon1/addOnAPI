namespace AddOn_API.DTOs.ReqIssueMT
{
    public class ReqIssueMTLog
    {
        public int Id { get; set; }
        public long ReqHid { get; set; }
        public string? Users { get; set; }
        public DateTime? LogDate { get; set; }
        public string? Status { get; set; }
        public int? Levels { get; set; }
        public string? Comment { get; set; }
        public string? Action { get; set; }
        public string? ClientName { get; set; }
    }
}