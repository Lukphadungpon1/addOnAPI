using AddOn_API.DTOs.AllocateLot;
using AddOn_API.Entities;

namespace AddOn_API.Interfaces
{
    public interface IAllocateService
    {
        Task<IEnumerable<AllocateLot>> FindAll();

         Task<AllocateLot> FindById(long id);

         Task<(string errorMessage,string SoNumber )> VerifyData(AllocateLot AllocateLot);

         Task<(string errorMessage,string Lot)> VerifyDataLot(AllocateLot allocateLot);

         Task<(string errorMessage,List<GenerateResponse> generateResponses)> VerifyDataDeleteLotList(long saleOrderId);

        
         

         Task Create(AllocateLot allocateLot);

         Task Update(AllocateLot allocateLot);

         Task<(string errorMessage, SaleOrderH saleOrderH)> VerifyAllocateLotStatusSO(long saleOrderId);

         Task UpdateAllocateLotStatusSO(SaleOrderH saleOrderH);

         Task UpdateGenerateLot(AllocateLot allocateLot,IEnumerable<SaleOrderD> saleOrderDs);

        Task<IEnumerable<AllocateLotSize>> FindLotSize(AllocateLot allocateLot);
         
        Task<IEnumerable<AllocateLot>> Search(AllocateLot allocateLot);

        Task UpdateGeneratePD(AllocateLot allocateLot,string type,VwWebUser account);

        Task Delete(List<AllocateLot> allocateLots,SaleOrderH saleOrderH);


        Task<IEnumerable<AllocateLotExcelLotList>> GetdatafromFile(string fileName);

        Task<(string errorMessage,List<string> fileName)> UploadFile(List<IFormFile> formFiles);
         Task<(string errorMessage,List<GenerateResponse> generateResponse)> VerifyDataUpdateLotList(List<AllocateLot> allocateLots);

         Task<(string errorMessage,List<GenerateResponse> generateResponses)> VerifyStyleWithSaleOrderD(List<AllocateLotExcelLotList> allocateLotExcelLotLists);

         Task<(AllocateLot allocateLot,List<AllocateLotSize> newAllocateLotSizes)> PreparedatarequestUpdateLotList(AllocateLotExcelLotList allocateLot,AllocateLot allocateLotDB,VwWebUser account);


         Task<(string errorMessage, ChangeLotNumber changeLotNumber)> VerifyChangeLotNumber(AllocateLot allocateLotFrom, AllocateLot allocateLotTo);

        Task<List<AllocateLot>> PrepareDataChangeLot(List<AllocateLot> allocateLots,ChangeLotNumber changeLotNumber,VwWebUser account);
         Task UpdateChangeLotNumber(List<AllocateLot> allocateLotold,List<AllocateLot> allocateLotnew); 
         

        Task<(string errorMessage,List<GenerateResponse> generateResponses)> VerifyLotReleasetoPD(List<AllocateLot> allocateLots);

        Task<(string errorMessage,List<GenerateResponse> generateResponses)> VerifyDelLotReleasetoPD(List<AllocateLot> allocateLots);

    
    }
}