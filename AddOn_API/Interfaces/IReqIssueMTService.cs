using AddOn_API.DTOs.MailService;
using AddOn_API.DTOs.ReqIssueMT;
using AddOn_API.Entities;
using AddOn_API.DTOs.AllocateLot;

namespace AddOn_API.Interfaces
{
    public interface IReqIssueMTService
    {
        Task<IEnumerable<ReqIssueMaterialH>> FindByRequest(ReqIssueMaterialH reqIssueMaterialH);

        Task<ReqIssueMaterialH> FindById(long id);

        Task<String> GetReqNumber();

        Task<(string errorMessage,List<ReqIssueMaterialD> reqIssueMaterialD)> CheckReqIssueMTDetail(List<ReqIssueMaterialD> reqIssueMaterialDs);
         
        Task Create(ReqIssueMaterialH reqIssueMaterialH);

        Task Update(ReqIssueMaterialH reqIssueMaterialH);

        Task DeleteDraftReqIssueMT(ReqIssueMaterialH reqIssueMaterialH);

        Task<IEnumerable<ReqIssueMaterialH>> Search(ReqIssueMaterialH reqIssueMaterialH);

        Task InsertLogReqIssueMT(ReqIssueMaterialLog reqIssueMaterialLog);


        Task<IEnumerable<AllocateLot>> FindLotRequestMT(AllocateLot allocateLot);

        Task<IEnumerable<ReqIssueMTItemList>> GetPDItemFromLot(AllocateLot allocateLot);

        Task<(string errorMessage,ReqIssueMaterialH reqIssueMaterialH)> VerifyDataCreate(ReqIssueMaterialH reqIssueMaterialH);
        Task<(string errorMessage,ReqIssueMaterialH reqIssueMaterialH)> VerifyDataUpdate(long id);
        Task<(string errorMessage,ReqIssueMaterialH ReqIssueMaterialH)> VerifyDataDelete(long id);
        Task<(string errorMessage,ReqIssueMaterialH reqIssueMaterialH)> VerifyDataApproveprocess(ReqIssueMaterialH reqIssueMaterialH,ReqIssueMaterialLog reqIssueMaterialLog);

        Task<(string errorMessage,ReqIssueMaterialH reqIssueMaterialHres)> VerifyLotReleasedtoPD(ReqIssueMaterialH reqIssueMaterialH);


        Task<MailData> PrepareDataApproveprocessSendEmail(ReqIssueMaterialH reqIssueMaterialH,ReqIssueMaterialLog reqIssueMaterialLog);


        Task<IEnumerable<AllocateLot>> GetLotForRequest(AllocateLot allocateLot);

        Task<IEnumerable<TpstyleWithLocation>> GetTPWithLocation(List<TpstyleWithLocation> tpstyleWithLocation);

        Task<IEnumerable<TplocationGroup>> GetLocationIssue();


    }
}