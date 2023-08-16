using AddOn_API.DTOs.SAPQuery;
using AddOn_API.Entities;

namespace AddOn_API.Interfaces
{
    public interface IGeneratePDandMCService
    {
         Task<IEnumerable<ProductionOrderH>> FindAllPD();
        Task<IEnumerable<AllocateMc>> FindAllMC();

         Task<ProductionOrderH> FindPDById(long id);
         Task<AllocateMc> FindMCById(string barcodeId);

         Task CreatePD(ProductionOrderH productionOrderH);
         Task UpdatePD(ProductionOrderH productionOrderH);
         
        Task CreateMC(List<AllocateMc> allocateMc);
        Task UpdateMC(List<AllocateMc> allocateMc);


        Task<IEnumerable<ProductionOrderH>> SearchPD(AllocateLot allocateLot);
        Task<IEnumerable<AllocateMc>> SearchMC(AllocateLot allocateLot);


        Task<(string errorMessage,string Lot)> VerifyDataProductionOrder(AllocateLot allocateLot);

         Task<(string errorMessage,string Lot)> VerifyDataProductionOrderDel(AllocateLot allocateLot);
        

        Task<(string errorMessage,string Lot)> VerifyDataConvertPDtoSAP(AllocateLot allocateLot);
        Task<(string errorMessage,string Lot )> VerifyDataMainCard(AllocateLot AllocateLot);

        Task<(string errorMessage,string Lot )> VerifyDataMainCardDel(AllocateLot AllocateLot);
        
        Task<(string errorMessage,string Lot)> VerifyDataReleaseProductionToSAP(AllocateLot allocateLot);

         Task<(string errorMessage,string Lot)> VerifyDataCloseProductionToSAP(AllocateLot allocateLot);

         Task<(string errorMessage,string Lot)> VerifyDataCalcelProductionToSAP(AllocateLot allocateLot);

        Task<(string errorMessage,string SO)> VerifyDataSOwithAllocate(AllocateLot allocateLot);

        Task<List<ProductionOrderH>> PreparedatafromLottoPD(AllocateLot allocateLot,VwWebUser account);
        Task<List<ProductionOrderH>> PreparedataConvertPDToSAP(AllocateLot allocateLot);

        Task<List<ProductionOrderH>> PreparedataReleasePDToSAP(AllocateLot allocateLot);
        Task<List<ProductionOrderH>> PreparedataCalcelPDToSAP(AllocateLot allocateLot);

        Task<(string errorMessage,string Lot)> VerifyDataReleasedToPD(AllocateLot allocateLot);


        Task<List<AllocateMc>> PreparedatafromLottoMC(AllocateLot allocateLot,VwWebUser account);

        Task<string> GenRunningMC(AllocateLot allocateLot);
        
    }
}