namespace AddOn_API.DTOs.AllocateLot
{
    public class AllocateLotExcelLotList
    {
        public long Id { get; set; }
        public string Lot { get; set; } = null!;
        public int? Total { get; set; }
        public int? S035 { get; set; }
        public int? S040 { get; set; }
        public int? S050 { get; set; }
        public int? S055 { get; set; }
        public int? S060 { get; set; }
        public int? S065 { get; set; }
        public int? S070 { get; set; }
        public int? S075 { get; set; }
        public int? S080 { get; set; }
        public int? S085 { get; set; }
        public int? S090 { get; set; }
        public int? S095 { get; set; }
        public int? S100 { get; set; }
        public int? S105 { get; set; }
        public int? S110 { get; set; }
        public int? S115 { get; set; }
        public int? S120 { get; set; }
        public int? S130 { get; set; }
        public int? S140 { get; set; }
        public int? S150 { get; set; }
        public int? S160 { get; set; }
        public int? S170 { get; set; }

        public virtual ICollection<AllocateLotSizeExcelLotList> AllocateLotSizes { get; set; }
    }
}