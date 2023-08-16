namespace AddOn_API.DTOs.SAPQuery;

public class ItemOnhand
{
    public int Id { get; set; }
    public int ItmsGrpCod { get; set; }
    public string ItmsGrpNam { get; set; }
    public string ItemCode { get; set; }
    public string ItemName { get; set; }
    public int IUoMEntry { get; set; }
    public string UomName { get; set; }
    public string InvntryUom { get; set; }
    public Decimal OnHand { get; set; }
    public string WhsCode { get; set; }
    public string WhsName { get; set; }
    public Decimal OnHandDFwh { get; set; }

    public string Color { get; set; }

}
