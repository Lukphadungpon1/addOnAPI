using AddOn_API.DTOs.SaleOrder;
using AddOn_API.DTOs.SAPQuery;
using AddOn_API.Entities;

namespace AddOn_API.Interfaces
{
    public interface ISapSDKService
    {
            Task<IEnumerable<DetailItem>> GetItemSale(List<SaleOrderD> aosaleOrderD);

            Task<IEnumerable<SalePersonSAP>> GetSalePerson();

            Task<IEnumerable<BusinessPartner>> GetBusinessPartner(string type);

            Task<IEnumerable<BomofMaterialSAPH>> GetBomofMaterial(string Code);

            Task<IEnumerable<SaleOrderQuery>> GetSaleOrder(List<long> DocEntry);

            Task<IEnumerable<ItemOnhand>> GetItemOnhand(List<string> ItemCodeList);

            Task<IEnumerable<BatchNumber>> GetBatNumber(List<string> ItemCodeList);

            Task<(string errorMessage,SaleOrderH saleOrderH )> ConvertSaleOrder(SaleOrderH saleOrderHs);
            Task<(string errorMessage,SaleOrderH saleOrderH)> UpdateSaleOrder(SaleOrderH saleOrderHs,List<SaleOrderD> saleOrderDNew);

            Task<(string errorMessage,ProductionOrderH productionOrderH)> ConvertProductionOrder(ProductionOrderH productionOrderH);

            Task<(string errorMessage,ProductionOrderH productionOrderH)> UpdateStatusdProductionOrder(ProductionOrderH productionOrderH,string type);
           
            Task<(string errorMessage,IEnumerable<GCSProdtype>)> InsertGCSProdType(List<GCSProdtype> gCSProdtypes);
                         
            Task<(string errorMessage, IEnumerable<GCSAllocate>)> InsertGCSAllocate(List<GCSAllocate> gCSAllocates);

            Task<(string errorMessage, IEnumerable<GCSAllocateMC>)> InsertGCSAllocateMC(List<GCSAllocateMC> gCSAllocateMC); 
    

            Task<(string errorMessage,BomOfMaterialH bomOfMaterialH)> ConvertBomOfMaterial(BomOfMaterialH bomOfMaterialH );

            Task<(string errorMessage,IssueMaterialH issueMaterialH,List<BatchNumber> batchNumbersRT)> ConvertIssueMaterial(IssueMaterialH issueMaterialH,List<ProductionOrderH> productionOrderHs,List<BatchNumber> batchNumbers);

            Task<(string errorMessage,IssueMaterialH issueMaterialH)> ConvertIssueMaterialML(IssueMaterialH issueMaterialH,List<BatchNumber> batchNumbers);
    }
}