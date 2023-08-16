using AddOn_API.DTOs.SaleOrder;
using AddOn_API.DTOs.SAPQuery;
using AddOn_API.Entities;

namespace AddOn_API.Interfaces
{
    public interface ISaleOrderService
    {
         Task<IEnumerable<SaleOrderH>> FindAll();

         Task<SaleOrderH> FindById(long id);

         Task<String> GetSoNumber();

         Task<(string errorMessage,string saleOrderDs )> CheckItemDetail(List<SaleOrderD> saleOrderD,List<DetailItem> detailItem);

         Task Create(SaleOrderH aosaleOrderH);

         Task Update(SaleOrderH aosaleOrderH,List<SaleOrderD> saleOrderDNew);

         Task DeleteDraftSaleorder(SaleOrderH aosaleOrderH);

        Task<IEnumerable<SaleOrderH>> Search(SaleOrderH aosaleOrderH);

        Task<IEnumerable<ReadExcelFile>> GetdatafromFile(string fileName);

        Task<(string errorMessage,List<string> fileName)> UploadFile(List<IFormFile> formFiles);

        Task<List<long>> GetDocEntryFLot(List<AllocateLot> allocateLots);
        
        Task<IEnumerable<SaleType>> SaleType();

         Task<IEnumerable<BuyMonthMaster>> BuyMonth();

          Task<IEnumerable<BuyYearMaster>> BuyYear();

          Task<(string errorMessage,SaleOrderH saleOrderH)> VerifyDataDeletoAllocateLot(SaleOrderH saleOrderH);

          Task DeletoAllocateLot(SaleOrderH saleOrderH);



    }
}