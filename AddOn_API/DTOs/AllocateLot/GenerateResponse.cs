namespace AddOn_API.DTOs.AllocateLot
{
    public class GenerateResponse
    {

        public string errorMessage { get; set; }

        public string referenceNumber { get; set; }

        public List<GeneratePDResponse> generatePD { get; set; }
        

    }
}