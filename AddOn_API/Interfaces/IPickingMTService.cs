using AddOn_API.DTOs.AllocateLot;
using AddOn_API.DTOs.Picking;
using AddOn_API.Entities;

namespace AddOn_API.Interfaces
{
    public interface IPickingMTService
    {
        Task Create(IssueMaterialH issueMaterialH);

         Task Update(IssueMaterialH issueMaterialH);

        Task<String> GetIssueNumber();

        Task<(string errorMessage,List<IssueMaterialD> issueMaterialDs )> CheckPickingDetail(List<IssueMaterialD> issueMaterialD);


         Task<IEnumerable<IssueMaterialH>> Search(IssueMaterialSearch issueMaterialSearch);

          Task<IEnumerable<AllocateLot>> GetLotForPicking(AllocateLotRequest allocateLotRequest);

          Task<IEnumerable<PickingItemH>> GetItemForPickg(List<AllocateLotRequest> allocateLotRequest);


        Task<(string errorMessage,IssueMaterialH issueMaterialH)> VerifyDataItemDetail(IssueMaterialH issueMaterialH);


        Task<(string errorMessage, List<IssueMaterialManual> issueMaterialManual)> AddIssueManual(List<PickingItemH> pickingItemH);

    }
}