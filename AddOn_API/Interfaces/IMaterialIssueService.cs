using AddOn_API.Entities;

namespace AddOn_API.Interfaces
{
    public interface IMaterialIssueService
    {
    
         Task<IssueMaterialH> FindById(long id);

         Task<(string errorMessage,string IssueMaterialD )> CheckItemDetail(List<IssueMaterialD> issueMaterialDs);

         Task Create(IssueMaterialH issueMaterialH);

         Task Update(IssueMaterialH issueMaterialH);

         Task DeleteDraftSaleorder(IssueMaterialH issueMaterialH);

        Task<IEnumerable<IssueMaterialH>> Search(IssueMaterialH issueMaterialH);


    }
}