namespace AddOn_API.DTOs.ReqIssueMT
{
    public class ReqIssueMTResponse
    {
         public string errorMessage { get; set; }

        public string referenceNumber { get; set; }

        public List<ReqIssueMTResponseItem> itemdetail { get; set; }
    }
}