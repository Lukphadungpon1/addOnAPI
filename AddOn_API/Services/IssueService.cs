using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AddOn_API.Data;
using AddOn_API.DTOs.Picking;
using AddOn_API.Entities;
using AddOn_API.Interfaces;
using Microsoft.EntityFrameworkCore;

using static AddOn_API.Installers.JWTInstaller;

namespace AddOn_API.Services;

public class IssueService : IIssueMTServices
{
        private readonly DatabaseContext databaseContext;
        private readonly JwtSettings jwtSettings;
        private readonly ISapSDKService sapSDKService;
       
    public IssueService(DatabaseContext databaseContext, JwtSettings jwtSettings,ISapSDKService sapSDKService)
    {
            this.sapSDKService = sapSDKService;
            
            this.jwtSettings = jwtSettings;
            this.databaseContext = databaseContext;
    }

    public async Task<IssueMaterialH> FindById(long id)
    {
        return await databaseContext.IssueMaterialHs
                        .Include(i => i.IssueMaterialDs)
                        .Include(i => i.IssueMaterialManuals)
                        .Include(i => i.IssueMaterialLogs)
                    .Where(w=> w.Status != "Delete" && w.Id == id).FirstOrDefaultAsync();


    }

    public async Task<IEnumerable<IssueMTGroupD>> GetissueMTListD(long id)
    {
       
        var _result = await (databaseContext.IssueMaterialDs.Where(w=> w.IssueHid == id && w.Status != "D").ToListAsync());

        List<string> _itemCodeList = _result.GroupBy(g => g.ItemCode).Select(s => s.Key).ToList();
        var onhandSap = await sapSDKService.GetItemOnhand(_itemCodeList);


        var manualpick =await (databaseContext.IssueMaterialManuals.Where(w=> w.IssueHid == id && w.Status != "D").ToListAsync());
 
        var _itemD = await (databaseContext.IssueMaterialDs.Where(w=> w.IssueHid == id).ToListAsync());


        List<IssueMTGroupD> data = _itemD
                            .Where(w=> w.IssueHid == id)
                            .GroupBy(g => new {g.Buy,g.Location,g.ItemCode,g.ItemName,g.Warehouse})
                            .Select((s,i) => new IssueMTGroupD {
                                Id = i+1,
                                Buy = s.Key.Buy,
                                Location = s.Key.Location,
                                ItemCode = s.Key.ItemCode,
                                ItemName = s.Key.ItemName,
                                Warehouse = s.Key.Warehouse,
                                Color = "",
                                BaseQty = s.Sum(s=> s.BaseQty.Value),
                                PlandQty = s.Sum(s=> s.PlandQty.Value),
                                PickQty = s.Sum(s=> s.PickQty.Value),
                                Onhand = 0,
                                OnhandWH = 0,
                                IssueQty =  s.Sum(s=> s.PickQty.Value),
                                ConfirmQty = 0
                            }).ToList();

       foreach(IssueMTGroupD item in data){
            var _ml = manualpick.Where(w=> w.ItemCode == item.ItemCode).FirstOrDefault();
            var _onhand = onhandSap.Where(w=> w.ItemCode == item.ItemCode).FirstOrDefault();

            if (_onhand != null){
                item.Onhand = _onhand.OnHand;
                item.OnhandWH = _onhand.OnHandDFwh;
                item.Color = _onhand.Color;
            }

            if (_ml != null){
                item.PickQty = item.PlandQty + _ml.PickQty.Value;
                item.IssueQty = item.PlandQty + _ml.PickQty.Value;
            }
           
       }

       return data;

    }

    public async Task<IEnumerable<ProductionOrderH>> GetProductionH(IssueMaterialH issueMaterialH)
    {
        List<long> _PdhId = issueMaterialH.IssueMaterialDs.GroupBy(g=> new {g.Pdhid}).Select(s=> s.Key.Pdhid).ToList();

        return await databaseContext.ProductionOrderHs.Include(i=> i.ProductionOrderDs).Where(w=> w.Status != "0" && _PdhId.Contains(w.Id) ).ToListAsync();
    }

    public async Task Update(IssueMaterialH issueMaterialH,IssueMaterialLog issueMaterialLogNew)
    {
        using var transaction = databaseContext.Database.BeginTransaction();
            try{
               
                databaseContext.IssueMaterialDs.UpdateRange(issueMaterialH.IssueMaterialDs);
                databaseContext.IssueMaterialManuals.UpdateRange(issueMaterialH.IssueMaterialManuals);

                databaseContext.IssueMaterialLogs.Add(issueMaterialLogNew);

                databaseContext.IssueMaterialHs.Update(issueMaterialH);

            

                await databaseContext.SaveChangesAsync();

              

                await databaseContext.SaveChangesAsync();
                transaction.Commit();
            }catch(Exception ex){
                transaction.Rollback();
                throw ex;
            }
    }

    public async Task UpdateStatusLot(IssueMaterialH issueMaterialH)
    {   
        List<long> _PdHList =  (issueMaterialH.IssueMaterialDs.Select(s=> s.Pdhid).Distinct().ToList());
        List<string> _lotList = issueMaterialH.Lotlist.Split(",").ToList();


        var lotlist = await (databaseContext.AllocateLots.Where(w=> _lotList.Contains(w.Lot)).ToListAsync());

        using var transaction = databaseContext.Database.BeginTransaction();
            try{

                var chkisspd = await (databaseContext.ProductionOrderDs.Where(w=> w.Status == "A")
                                .GroupJoin(databaseContext.ReqIssueMaterialDs.Where(w=> w.Status == "Issued"),
                                ih => new {p1 = ih.Pdhid, p2 = ih.Id,p3 = ih.LineNum},
                                isd => new {p1 = isd.Pddid, p2 = isd.Pddid,p3 =isd.LineNum },
                                (h,d) => new {ih =h,isd = d})
                .SelectMany(sm => sm.isd.DefaultIfEmpty(),
                    (t1,t2) => new {t1 = t1,t2=t2})
                .Where(w=> _PdHList.Contains(w.t1.ih.Pdhid) && w.t2 == null )
                    .ToListAsync());

                string statusissue = "Issued";

                if (chkisspd.Count > 0 ){
                    statusissue = "Issue";
                }
                
                foreach(AllocateLot item in lotlist){
                    item.StatusIssueMat = statusissue;
                }

                await databaseContext.SaveChangesAsync();

                transaction.Commit();
            }catch(Exception ex){
                transaction.Rollback();
                throw ex;
            }
    }

    public async Task<(string errorMessage, IssueMaterialH issueMaterialHs)> VerifyDataIssue(IssueMaterialH issueMaterialH)
    {
        string errorMessageres = string.Empty;

        if (issueMaterialH.Status == "Issued")
            errorMessageres = "This job has issued.";



        return (errorMessageres,issueMaterialH);
    }

    public async Task<(string errorMessage, IssueMaterialH issueMaterialHs)> VerifyDeleteIssue(IssueMaterialH issueMaterialH)
    {
        string errorMessageres = string.Empty;



        if (issueMaterialH.Status != "Draft")
            errorMessageres = "This job can't delete.";

        if (issueMaterialH.PrintDate != null ){
            errorMessageres = "This job has Print to warehouse.";
        }



        return (errorMessageres,issueMaterialH);
    }
}
