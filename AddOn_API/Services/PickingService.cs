
using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AddOn_API.Data;
using AddOn_API.DTOs.AllocateLot;
using AddOn_API.Entities;
using AddOn_API.Interfaces;
using Microsoft.EntityFrameworkCore;
using static AddOn_API.Installers.JWTInstaller;
using AddOn_API.DTOs.Picking;
using Mapster;

namespace AddOn_API.Services;

public class PickingService : IPickingMTService
{
        private readonly DatabaseContext databaseContext;
        private readonly JwtSettings jwtSetting;
        private readonly ISapSDKService sapSDKService;
    public PickingService(DatabaseContext databaseContext, JwtSettings jwtSetting, ISapSDKService sapSDKService)
    {
            this.sapSDKService = sapSDKService;
            this.jwtSetting = jwtSetting;
            this.databaseContext = databaseContext;
    }

    public async Task<(string errorMessage, List<IssueMaterialManual> issueMaterialManual)> AddIssueManual(List<PickingItemH> pickingItemH)
    {
        string errorMessage = string.Empty;
        List<IssueMaterialManual> issueMaterialManuals = new List<IssueMaterialManual>();

        var _itemhm = pickingItemH.Where(w=> w.PickQty > w.PlandQty).Select(s=> new IssueMaterialManual {
            Id= 0,
            IssueHid = 0,
            Buy = s.Buy,
            Lot = ( string.Join(",",s.PickingItemD.Select(s=>s.Lot).ToList())),
            ItemCode = s.ItemCode,
            ItemName = s.ItemName,
            Warehouse = s.Warehouse,
            IssueMethod = "Manual",
            BaseQty = s.BaseQty,
            PlandQty = s.PlandQty,
            PickQty = s.PickQty - s.PlandQty,
            IssueQty = 0,
            ConfirmQty = 0,
            CreateBy = "",
            CreateDate = System.DateTime.Now,
            Status = "A"
        }).ToList();

        



       
        return (errorMessage,_itemhm);
    }

    public async Task<(string errorMessage, List<IssueMaterialD> issueMaterialDs)> CheckPickingDetail(List<IssueMaterialD> issueMaterialD)
    {
        string errorMessage = string.Empty;

        foreach(IssueMaterialD item in issueMaterialD){
            if (item.PickQty != item.PlandQty)
            errorMessage = "pickQty not match planqty.";

            if (item.Location == "")
            errorMessage = "Location is empty.";
        }
        

       return (errorMessage,issueMaterialD);
    }

    public async Task Create(IssueMaterialH issueMaterialH)
    {
         using var transaction = databaseContext.Database.BeginTransaction();
            try{
                databaseContext.IssueMaterialHs.Add(issueMaterialH);
                await databaseContext.SaveChangesAsync();

                transaction.Commit();

            }catch(Exception ex){
                transaction.Rollback();
                throw ex;
            }
    }

    public async Task<string> GetIssueNumber()
    {
         var IssNumber = "";

            IssNumber = await databaseContext.IssueMaterialHs.MaxAsync(m => m.IssueNumber);
            if (string.IsNullOrEmpty(IssNumber))
            {
                var month = DateTime.Today.Month.ToString();

                if (month.Length == 1)
                    month = "0" + month;

                var _default = DateTime.Today.Year.ToString() + month + "001";
                IssNumber = $"Iss{_default}";
            }
            else
            {
                var splitSO = Convert.ToInt32(IssNumber.Substring(9, 3).ToString()) + 1;
                var runspllitSO = "000" + splitSO.ToString();

                IssNumber = IssNumber.Substring(0, 9) + runspllitSO.Substring(runspllitSO.Length - 3, 3);
            }

        return IssNumber;
    }

    public async Task<IEnumerable<PickingItemH>> GetItemForPickg(List<AllocateLotRequest> allocateLotRequest)
    {
        List<string> lotwh = allocateLotRequest.Where(w=> !string.IsNullOrEmpty(w.Lot)).Select(s=> s.Lot!).ToList();

    
        var _itempick =  (await databaseContext.VwItemForPickings.Where(w=> ((lotwh.Count > 0) ? lotwh.Contains(w.Lot!) : 1 ==1) ).ToListAsync());
      

        List<PickingItemH> _result = _itempick.GroupBy(g=> new {g.Buy,g.ItemCode,g.ItemName,g.Warehouse,g.Location})
                                        .Select((s,i)=> new PickingItemH {
                                            Id=i+1,
                                            Buy = s.Key.Buy,
                                            Location = s.Key.Location,
                                            ItemCode = s.Key.ItemCode,
                                            ItemName = s.Key.ItemName,
                                            Warehouse = s.Key.Warehouse,
                                            Color = "",
                                            BaseQty = s.Sum(s=> s.BaseQty.Value),
                                            PlandQty = s.Sum(s=> s.PlandQty.Value),
                                            PickQty = s.Sum(s=> s.PlandQty.Value),
                                            Onhand = 0,
                                            OnhandWH = 0,
                                            IssueQty = 0,
                                            ConfirmQty = 0,
                                            PickingItemD = _itempick.Where(w=> w.Buy == s.Key.Buy && w.ItemCode == s.Key.ItemCode && w.Warehouse == s.Key.Warehouse).ToList().Adapt<List<PickingItemD>>()
                                        }).ToList();      


        List<string> _itemCodeList = _result.GroupBy(g => g.ItemCode).Select(s => s.Key).ToList();
        var onhandSap = await sapSDKService.GetItemOnhand(_itemCodeList);

        foreach(PickingItemH item in _result){
            var _onhand = onhandSap.Where(w=> w.ItemCode == item.ItemCode).FirstOrDefault();

            if (_onhand != null){
                item.Onhand = _onhand.OnHand;
                item.OnhandWH = _onhand.OnHandDFwh;
                item.Color = _onhand.Color;
            }
        }
        // foreach(PickingItemH item in _result){
        //     var detail = _itempick.Where(w=> w.Lot == item.Lot && w.ItemCode == item.ItemCode && w.Buy == item.Buy && w.Warehouse == item.Warehouse).ToList();
        //     item.PickingItemD = detail.Adapt<List<PickingItemD>>();
        // } 


        return _result;

            
    }

    public async Task<IEnumerable<AllocateLot>> GetLotForPicking(AllocateLotRequest allocateLotRequest)
    {



        



        var _lotwhere = await( databaseContext.AllocateLots
                            .Where(w=> w.Status != "I" 
                            && (string.IsNullOrEmpty(allocateLotRequest.Buy) ? 1 == 1 : w.Buy == allocateLotRequest.Buy)
                            && (string.IsNullOrEmpty(allocateLotRequest.Lot) ? 1==1 : w.Lot == allocateLotRequest.Lot)
                            && (string.IsNullOrEmpty(allocateLotRequest.PurOrder) ? 1 == 1 : w.PurOrder == allocateLotRequest.PurOrder)
                            ).ToListAsync());

        List<string> lotwh = _lotwhere.Select(s=> s.Lot).ToList();
        

        var _data =await(databaseContext.ReqIssueMaterialHs
        .GroupJoin(databaseContext.ReqIssueMaterialDs.Where(ww=>ww.Status == "A"),
        rh => new {p1 = rh.Id},
        rd => new {p1 = rd.ReqHid},
        (h,d) => new {h=h,d=d} )
        .SelectMany(sm => sm.d.DefaultIfEmpty(),
            (th,td) => new {th = th,td=td})
        .GroupJoin(databaseContext.IssueMaterialDs.Where(w=>w.Status == "A"),
            ih => new {p1 = ih.th.h.Id, p2 = ih.td.Id,p3 = ih.td.Pdhid,p4 = ih.td.Pddid},
            isd => new {p1 = isd.ReqHid, p2 = isd.ReqDid,p3 =isd.Pdhid ,p4 = isd.Pddid},
            (h,d) => new {ih =h,isd = d})
        .SelectMany(sm=> sm.isd.DefaultIfEmpty(),
            (t1,t2) => new {t1=t1,t2=t2})
        .Where(w => w.t1.ih.th.h.Status == "Finish" && w.t2 == null
                && ( lotwh.Count == 0 ? 1 ==1 : lotwh.Contains(w.t1.ih.th.h.Lot!) )
        )
        .Select(s => new {
            Lot = s.t1.ih.th.h.Lot,
            ReqNumber = s.t1.ih.th.h.ReqNumber
        }).ToListAsync());


        List<string> lotstr = _data.Select(s=> s.Lot!).Distinct().ToList();
        

        var _lotList = (await databaseContext.AllocateLots.Where(w=> lotstr.Contains(w.Lot) && w.Status != "I" ).ToListAsync());

        return _lotList;

       
    }

    public async Task<IEnumerable<IssueMaterialH>> Search(IssueMaterialSearch issueMaterialSearch)
    {
        
        // var _temp = await (databaseContext.AllocateLotSizes.Where(w=> w.Status == "A" && w.AllocateLot.Lot == "20230500002").Select(s=> new {
        //     buy = s.AllocateLot.Buy,
        //     lot = s.Lot,
        //     sizeno = s.SizeNo
        // }).ToListAsync());

    

        return (await databaseContext.IssueMaterialHs.Include(i=> i.IssueMaterialDs.Where(w=> w.Status == "A")).Include(i=> i.IssueMaterialLogs).Include(i=> i.IssueMaterialManuals.Where(w=> w.Status == "A"))
                    .Where(w=> w.Status != "Delete" &&
                                (string.IsNullOrEmpty(issueMaterialSearch.IssueNumber)) ? 1==1 : w.IssueNumber == issueMaterialSearch.IssueNumber &&
                                (string.IsNullOrEmpty(issueMaterialSearch.Location) ? 1==1 : w.Location == issueMaterialSearch.Location) &&
                                (string.IsNullOrEmpty(issueMaterialSearch.Lotlist)) ? 1==1 : issueMaterialSearch.Lotlist.Contains(w.Lotlist) &&
                                (string.IsNullOrEmpty(issueMaterialSearch.PickingBy)) ? 1==1 : w.PickingBy.ToLower() == issueMaterialSearch.PickingBy.ToLower() &&
                                (string.IsNullOrEmpty(issueMaterialSearch.CreateBy) ? 1==1 : w.CreateBy.ToLower() == issueMaterialSearch.CreateBy.ToLower())
                    ).ToListAsync());
      

    }

    public async Task Update(IssueMaterialH issueMaterialH)
    {
       using var transaction = databaseContext.Database.BeginTransaction();
            try{

                databaseContext.IssueMaterialDs.UpdateRange(issueMaterialH.IssueMaterialDs);
                databaseContext.IssueMaterialManuals.UpdateRange(issueMaterialH.IssueMaterialManuals);

                databaseContext.IssueMaterialHs.Update(issueMaterialH);

                await databaseContext.SaveChangesAsync();

                await databaseContext.SaveChangesAsync();
                transaction.Commit();
            }catch(Exception ex){
                transaction.Rollback();
                throw ex;
            }
    }

    public async Task<(string errorMessage, IssueMaterialH issueMaterialH)> VerifyDataItemDetail(IssueMaterialH issueMaterialH)
    {
        string errorMessage = string.Empty;

        var chkcdetail = issueMaterialH.IssueMaterialDs.Count();

        if (chkcdetail == 0){
            errorMessage = "Item is not Found";
            return (errorMessage,issueMaterialH);
        }

        var chkpickzero = issueMaterialH.IssueMaterialDs.Where(w=> w.PickQty == 0).ToList();
        
        if (chkpickzero.Count > 0){
            errorMessage = "Picking Qty is Zero.";
            return (errorMessage,issueMaterialH);
        }
        
        

        

        return (errorMessage,issueMaterialH);
    }

}
