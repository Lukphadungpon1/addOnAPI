using System;
using System.ComponentModel.Design.Serialization;
using System.ComponentModel.DataAnnotations;
using AddOn_API.Data;
using AddOn_API.Entities;
using AddOn_API.Interfaces;
using Microsoft.EntityFrameworkCore;
using AddOn_API.DTOs.ReqIssueMT;
using AddOn_API.DTOs.MailService;

namespace AddOn_API.Services
{
    public class ReqIssueMTService : IReqIssueMTService
    {
        private readonly DatabaseContext databaseContext;
        private readonly IConfiguration configuration;

        public ReqIssueMTService(DatabaseContext databaseContext, IConfiguration configuration)
        {
            this.configuration = configuration;
            this.databaseContext = databaseContext;

        }

        public async Task<(string errorMessage, List<ReqIssueMaterialD> reqIssueMaterialD)> CheckReqIssueMTDetail(List<ReqIssueMaterialD> reqIssueMaterialDs)
        {

            string errorMessage = string.Empty;
            List<ReqIssueMaterialD> _detail = new List<ReqIssueMaterialD>();

            var checkdup = await databaseContext.ReqIssueMaterialDs.Where(w => reqIssueMaterialDs.Select(s => s.Pddid).ToList().Contains(w.Pddid) && w.Status == "A").ToListAsync();

            if (checkdup.Count > 0)
            {
                errorMessage = "Request item dupicate in system.";
                _detail.AddRange(checkdup);
            }
            return (errorMessage, _detail);

        }

        public async Task Create(ReqIssueMaterialH reqIssueMaterialH)
        {

            using var transaction = databaseContext.Database.BeginTransaction();
            try
            {
                // var _lot = await databaseContext.AllocateLots.Where(w=> w.Lot == reqIssueMaterialH.Lot && w.Status == "A").FirstOrDefaultAsync();

                // _lot.StatusIssueMat = "Request Issue";
                // _lot.UpdateBy = reqIssueMaterialH.CreateBy;
                // _lot.UpdateDate = System.DateTime.Now;

                databaseContext.ReqIssueMaterialHs.Add(reqIssueMaterialH);


                await databaseContext.SaveChangesAsync();

                transaction.Commit();

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }

        }

        public async Task Update(ReqIssueMaterialH reqIssueMaterialH)
        {
            using var transaction = databaseContext.Database.BeginTransaction();
            try
            {
                databaseContext.ReqIssueMaterialDs.UpdateRange(reqIssueMaterialH.ReqIssueMaterialDs);
                databaseContext.ReqIssueMaterialHs.Update(reqIssueMaterialH);

                await databaseContext.SaveChangesAsync();

                transaction.Commit();

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }
        }

        public async Task DeleteDraftReqIssueMT(ReqIssueMaterialH reqIssueMaterialH)
        {
            using var transaction = databaseContext.Database.BeginTransaction();
            try
            {
                databaseContext.ReqIssueMaterialDs.RemoveRange(reqIssueMaterialH.ReqIssueMaterialDs);
                databaseContext.ReqIssueMaterialHs.Remove(reqIssueMaterialH);

                await databaseContext.SaveChangesAsync();

                transaction.Commit();

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }

        }

        public async Task<ReqIssueMaterialH> FindById(long id)
        {

           
            return await databaseContext.ReqIssueMaterialHs
                        .Include(s => s.ReqIssueMaterialDs.Where(w => w.Status == "A"))
                        .Include(s => s.ReqIssueMaterialLogs)
                        .Where(w => w.Id == id && w.Status != "Delete").FirstOrDefaultAsync();

        }

        public async Task<IEnumerable<ReqIssueMaterialH>> FindByRequest(ReqIssueMaterialH reqIssueMaterialH)
        {
            return await databaseContext.ReqIssueMaterialHs
                        .Include(s => s.ReqIssueMaterialDs).Where(w => w.Status == "A")
                        .Include(s => s.ReqIssueMaterialLogs)
                        .Where(w => w.RequestBy == reqIssueMaterialH.RequestBy).ToListAsync();
        }

        public async Task<IEnumerable<AllocateLot>> FindLotRequestMT(AllocateLot allocateLot)
        {
            return await databaseContext.AllocateLots
                        .Include(s => s.AllocateLotSizes).Where(w => w.Status == "A")
                        // .Include(s => s.AllocateMcs.Where( w=> w.StatusMc == "1"))
                        .Where(w => (string.IsNullOrEmpty(allocateLot.Buy) ? 1 == 1 : w.Buy == allocateLot.Buy)
                                        && (string.IsNullOrEmpty(allocateLot.SoNumber) ? 1 == 1 : w.SoNumber == allocateLot.SoNumber)
                                        && (string.IsNullOrEmpty(allocateLot.Lot) ? 1 == 1 : w.Lot == allocateLot.Lot)
                                        && (string.IsNullOrEmpty(allocateLot.PurOrder) ? 1 == 1 : w.PurOrder == allocateLot.PurOrder)
                                        && w.Status != "I"
                                        && w.StatusProduction == "Released")
                        .ToListAsync();
        }

        public async Task<string> GetReqNumber()
        {
            var ReqNumber = "";

            ReqNumber = await databaseContext.ReqIssueMaterialHs.MaxAsync(m => m.ReqNumber);
            if (string.IsNullOrEmpty(ReqNumber))
            {
                var month = DateTime.Today.Month.ToString();

                if (month.Length == 1)
                    month = "0" + month;

                var _default = DateTime.Today.Year.ToString() + month + "001";
                ReqNumber = $"Req{_default}";
            }
            else
            {
                var splitSO = Convert.ToInt32(ReqNumber.Substring(9, 3).ToString()) + 1;
                var runspllitSO = "000" + splitSO.ToString();

                ReqNumber = ReqNumber.Substring(0, 9) + runspllitSO.Substring(runspllitSO.Length - 3, 3);
            }


            return ReqNumber;


        }

        public async Task InsertLogReqIssueMT(ReqIssueMaterialLog reqIssueMaterialLog)
        {
            using var transaction = databaseContext.Database.BeginTransaction();
            try
            {

                databaseContext.ReqIssueMaterialLogs.Add(reqIssueMaterialLog);

                await databaseContext.SaveChangesAsync();

                transaction.Commit();

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }
        }

        public async Task<IEnumerable<ReqIssueMaterialH>> Search(ReqIssueMaterialH reqIssueMaterialH)
        {
            return await databaseContext.ReqIssueMaterialHs
                        .Include(s => s.ReqIssueMaterialDs).Where(w => w.Status == "A")
                        .Include(s => s.ReqIssueMaterialLogs)
                        .Where(w => w.Status != "Reject"
                            && (string.IsNullOrEmpty(reqIssueMaterialH.ReqNumber) ? 1 == 1 : w.ReqNumber == reqIssueMaterialH.ReqNumber)
                            && (string.IsNullOrEmpty(reqIssueMaterialH.Lot) ? 1 == 1 : w.Lot == reqIssueMaterialH.Lot)
                            
                            
                            && (reqIssueMaterialH.RequestDate == null ? 1 == 1 : w.RequestDate == reqIssueMaterialH.RequestDate)

                        ).ToListAsync();

        }

        public async Task<IEnumerable<ReqIssueMTItemList>> GetPDItemFromLot(AllocateLot allocateLot)
        {

            var items = await (databaseContext.ProductionOrderHs
                .GroupJoin(databaseContext.ProductionOrderDs.Where(w => w.Status == "A"),
                h => new { p1 = h.Id, p2 = h.AllocateLotSizeId },
                d => new { p1 = d.Pdhid, p2 = d.AllocateLotSizeId },
                (h, d) => new { ph = h, pd = d })
                .SelectMany(x => x.pd.DefaultIfEmpty(),
                    (h, d) => new { ph1 = h, pd1 = d })

                .GroupJoin(databaseContext.ReqIssueMaterialDs.Where(w => w.Status == "A"),
                pdd => new { p1 = pdd.pd1.Id, p2 = pdd.ph1.ph.Id },
                rd => new { p1 = rd.Pddid, p2 = rd.Pdhid },
                (pd2, rd2) => new { pdg = pd2, rd = rd2 })
                .SelectMany(x => x.rd.DefaultIfEmpty(),
                    (t1, t2) => new { t1 = t1, t2 = t2 })

                .Where(w => w.t1.pdg.ph1.ph.Lot == allocateLot.Lot && w.t1.pdg.ph1.ph.Status == "R" &&  w.t2 == null)
                .Select(s => new ReqIssueMTItemList
                {

                    Id = s.t1.pdg.pd1.Id,
                    Pdhid = s.t1.pdg.pd1.Pdhid,
                    AllocateLotSizeId = s.t1.pdg.pd1.AllocateLotSizeId,
                    LineNum = s.t1.pdg.pd1.LineNum,
                    ItemType = s.t1.pdg.pd1.ItemType,
                    ItemCode = s.t1.pdg.pd1.ItemCode,
                    ItemName = s.t1.pdg.pd1.ItemName,
                    BaseQty = s.t1.pdg.pd1.BaseQty,
                    PlandQty = s.t1.pdg.pd1.PlandQty,
                    UomName = s.t1.pdg.pd1.UomName,
                    Department = s.t1.pdg.pd1.Department,
                    Request = s.t2.ItemCode

                })
                .ToListAsync());


             


                // var items1 = await (from ph in databaseContext.ProductionOrderHs
                //             join pd in databaseContext.ProductionOrderDs
                //             on new {p1 = ph.Id} equals new {p1 = pd.Pdhid}
                //             into pdg
                //             from pdt in pdg.DefaultIfEmpty()
                //             where pdt.Status == "A"
                //             join isd in databaseContext.ReqIssueMaterialDs 
                //             on new {p1 = ph.Id ,p2 = pdt.Id ,p3="A" } equals new {p1 = isd.Pdhid,p2 = isd.Pddid,p3 = isd.Status }
                //             into isd
                //             from isdt in isd.DefaultIfEmpty()

                //             where ph.Lot == allocateLot.Lot && ph.Status == "R" && isdt == null 
                //             select (new ReqIssueMTItemList
                //                 {
                //                     Id = pdt.Id,
                //                     Pdhid = pdt.Pdhid,
                //                     AllocateLotSizeId = pdt.AllocateLotSizeId,
                //                     LineNum = pdt.LineNum,
                //                     ItemType = pdt.ItemType,
                //                     ItemCode = pdt.ItemCode,
                //                     ItemName = pdt.ItemName,
                //                     BaseQty =  pdt.BaseQty,
                //                     PlandQty = pdt.PlandQty,
                //                     UomName = pdt.UomName,
                //                     Department = pdt.Department,
                //                     Request = isdt.CreateBy,
                //                 })).ToListAsync();

                    
                          
                           
                             
           


            var sizedetail = await databaseContext.AllocateLotSizes.Where(w => w.Lot == allocateLot.Lot && w.Status == "A").ToListAsync();



            foreach (ReqIssueMTItemList item in items)
            {
                var _size = sizedetail.Where(w => w.RowId == item.AllocateLotSizeId).FirstOrDefault();

                item.Lot = _size!.Lot;
                item.ItemCodeS = _size.ItemCode;
                item.SizeNo = _size.SizeNo;
            }


            return items;


            // .Where( w=> w.pd ph.Lot == allocateLot.Lot && w.ph.Status == "R" && w.pd.Status == "A")
            // .Select( s=> new ProductionOrderD{})

            // var _item = databaseContext.ProductionOrderHs.Include( i => i.ProductionOrderDs).Where(w=> w.Status == "A")
            //     .Where( w=> w.Lot == allocateLot.Lot && w.Status == "R").Select( s=> new ProductionOrderD { Id=s.ProductionOrderDs }).ToListAsync();

        }

         public async Task<IEnumerable<TpstyleWithLocation>> GetTPWithLocation(List<TpstyleWithLocation> tpstyleWithLocation)
        {
            var _wharedata = tpstyleWithLocation.Select(s=> s.ArticleCode+s.GroupItem).ToList();

            return await databaseContext.TpstyleWithLocations.Where(w=> _wharedata.Contains(w.ArticleCode+w.GroupItem) && w.Status == "1" ).ToListAsync();

        }

        public async Task<(string errorMessage, ReqIssueMaterialH reqIssueMaterialH)> VerifyDataCreate(ReqIssueMaterialH reqIssueMaterialH)
        {
            string errorMessage = string.Empty;

            if (reqIssueMaterialH.ReqIssueMaterialDs.Count == 0)
            {
                errorMessage = "Item list is not Found.";
                return (errorMessage, reqIssueMaterialH);
            }

            return (errorMessage, reqIssueMaterialH);
        }


        public async Task<(string errorMessage, ReqIssueMaterialH reqIssueMaterialH)> VerifyDataUpdate(long id)
        {
            string errorMessage = string.Empty;

            var _reqIssue = await databaseContext.ReqIssueMaterialHs.Where(w => w.Id == id).FirstOrDefaultAsync();


            if (_reqIssue == null)
            {
                errorMessage = "Id is not Found..";
            }

            if (_reqIssue.Status != "Draft" || _reqIssue.Status != "Request Information")
            {
                errorMessage = $"Status is not Update. ({_reqIssue.Status} ).";
            }

            return (errorMessage, _reqIssue);
        }

        public async Task<(string errorMessage, ReqIssueMaterialH ReqIssueMaterialH)> VerifyDataDelete(long id)
        {
            string errorMessage = string.Empty;

            var _reqIssue = await databaseContext.ReqIssueMaterialHs.Where(w => w.Id == id).FirstOrDefaultAsync();

            if (_reqIssue == null)
            {
                errorMessage = "Id is not Found..";
            }

            if (_reqIssue.Status != "Draft")
            {
                errorMessage = $"Status is not delete. ({_reqIssue.Status} ).";

            }

            return (errorMessage, _reqIssue);
        }

        public async Task<(string errorMessage, ReqIssueMaterialH reqIssueMaterialH,ReqIssueMaterialLog reqIssueMTLog)> VerifyDataApproveprocess(ReqIssueMaterialH reqIssueMaterialH, ReqIssueMaterialLog reqIssueMaterialLog)
        {
            string errorMessage = string.Empty;

            List<ReqIssueMaterialLog> _log = reqIssueMaterialH.ReqIssueMaterialLogs.ToList();


            List<VwWebTpapproval> _tpapr = (await databaseContext.VwWebTpapprovals.Where(w => w.Program == "ADDON" && w.Department == reqIssueMaterialH.ReqDept && w.Site == (reqIssueMaterialH.Site == "KTH" ? "KTH1" : reqIssueMaterialH.Site)).ToListAsync());



            if ((_log.Count > 0 && reqIssueMaterialLog.Status == "Request" && reqIssueMaterialH.Status != "Request Information") || (_log.Count == 0 && reqIssueMaterialLog.Status != "Request"))
            {
                errorMessage = $"Can't action this process ({reqIssueMaterialLog.Status})";

            }


            /// approve verify permission
            if (reqIssueMaterialLog.Status != "Request")
            {
                var maxpermisapr = _tpapr.OrderByDescending(o=> o.Levels).FirstOrDefault();


                var chkpermisapr = _tpapr.Where(w => w.Name!.ToUpper() == reqIssueMaterialLog.Users!.ToUpper()).FirstOrDefault();
                if (chkpermisapr == null)
                {
                    errorMessage = $"Don't have permisstion to action this process ({reqIssueMaterialLog.Status})";
                }

                var chklevelapr = reqIssueMaterialH.ReqIssueMaterialLogs.MaxBy(m => m.LogDate);

                if (chklevelapr.Levels > chkpermisapr.Levels || (chklevelapr.Levels == chkpermisapr.Levels && reqIssueMaterialH.Status != "Request Discuss"))
                {
                    errorMessage = $"Don't have permisstion to action this process ({reqIssueMaterialLog.Status})";
                }

                //// check finish apr
                if (chkpermisapr.Levels == maxpermisapr.Levels && string.IsNullOrEmpty(errorMessage) && reqIssueMaterialLog.Status == "Approved"){
                    reqIssueMaterialH.Status = "Finish";
                }


                reqIssueMaterialLog.Levels = chkpermisapr.Levels;
                reqIssueMaterialLog.LogDate = System.DateTime.Now;

            }


            return (errorMessage, reqIssueMaterialH,reqIssueMaterialLog);


        }

        public async Task<MailData> PrepareDataApproveprocessSendEmail(ReqIssueMaterialH reqIssueMaterialH, ReqIssueMaterialLog reqIssueMaterialLog)
        {
            MailData _mailData = new MailData();

            string WHEmailList = string.Empty;
            configuration.Bind(nameof(WHEmailList), WHEmailList);

            var emailto = await databaseContext.VwWebTpapprovals.Where(w => w.Department == reqIssueMaterialH.ReqDept && w.Program == "ADDON" && w.Levels == (reqIssueMaterialLog.Levels + 1) && w.Site == reqIssueMaterialH.Site).ToListAsync();

            var emailuser = await databaseContext.VwWebUsers.Where(w => w.EmpUsername!.ToUpper() == reqIssueMaterialH.RequestBy!.ToUpper() && w.Site == reqIssueMaterialH.Site).FirstOrDefaultAsync();


            if (reqIssueMaterialLog.Status == "Request Information" || reqIssueMaterialLog.Status == "Request Discuss" || reqIssueMaterialLog.Status == "Reject")
                _mailData.EmailTo = emailuser.EmpEmail!;
            else if (reqIssueMaterialLog.Status == "Approved")
            {
                if (emailto.Count() == 0)
                    _mailData.EmailTo = WHEmailList;
                else
                    _mailData.EmailTo = String.Join(",", emailto.Select(s => s.Email).ToList());
            }


            string tabcur = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
            string tabcur1 = "&nbsp;&nbsp;&nbsp;";

            string detailbody = "<table style='width:100%; text-align:left;'> ";
            foreach (ReqIssueMaterialD item in reqIssueMaterialH.ReqIssueMaterialDs)
            {
                detailbody += "<tr> " +
                                    "<td>" + item.ItemCode + tabcur1 + item.ItemName + tabcur1 + "</td> " +
                                "</tr> ";
            }
            detailbody += "</table> ";

            string body = "<p><b>[ Notification ] This job has request. Please see as below. </b></p>" +
                     "<style> " +
"table, th, td { " +
"border: 1px solid black; " +
"border-collapse: collapse; " +
"} " +
    "<table style='width:100%; text-align:left;'> " +
     "<tr> " +
     "<td><b>Request Number  </b><br />" + tabcur + reqIssueMaterialH.ReqNumber + " <br /></td> " +
     "<td><b>Site  </b><br />" + tabcur + reqIssueMaterialH.Site + " <br /></td> " +
     "<td><b>Lot  </b><br />" + reqIssueMaterialH.Lot + " <br /> </td> " +
     "<td><b>Status  </b><br />" + tabcur + reqIssueMaterialH.Status + " <br /></td> " +
     "</tr> " +
     "<tr> " +
     "<td><b>Request By  </b><br />" + tabcur + reqIssueMaterialH.RequestBy + " <br /></td> " +
     "<td><b>Request Date  </b><br />" + tabcur + reqIssueMaterialH.RequestDate + " <br /></td> " +
     "<td><b>Require Date  </b><br />" + tabcur + reqIssueMaterialH.RequireDate + " <br /></td> " +
     "<td><b>Req Dept  </b><br />" + tabcur + reqIssueMaterialH.ReqDept + " <br /></td> " +
     "</tr> " +
     "<tr style='text-align:left; '> " +
     "<td colspan='4' ><b>Remark  </b><br />" + tabcur + reqIssueMaterialH.Remark + " <br /></td> " +
     "</tr> " +
     "<tr style='text-align:left; '> " +
     "<td colspan='4' >" + detailbody + "</td> " +
     "</tr> " +
     "<tr style='text-align:left; '> " +
     "<td colspan='3'><b>Comment  </b><br />" + tabcur + reqIssueMaterialLog.Comment + "<br /></td> " +
     "<td><b>Action By  </b><br />" + tabcur + reqIssueMaterialLog.Status + " [" + reqIssueMaterialLog.LogDate + "] <br /></td> " +
     "</tr> " +
     "</table> " +
     "<p><b>Please check the information and update status at the link attached. <br /><br /><br />" +
       "Link For access  <br />" +
       "http://kth-server-02/ADDON" +
       " <br /><br />" +
       "This is automatic sending email. Do not reply. <br />" +
       " <br /><br /><br />" +
       "Best Regards, <br />" +
       "ADD ON System <br />" +
       " " +
       "ROFU (Thailand) LIMITED,</b></p> ";


            _mailData.EmailForm = "";
            _mailData.EmailCC = emailuser.EmpEmail!;
            _mailData.EmailSubject = $"[ADD ON System] Notification  Request Issue Material [{System.DateTime.Now.ToString("dd/MM/yyyy HH:mm")}] ";
            _mailData.EmailBody = body;



            return _mailData;



        }



        public async Task<IEnumerable<AllocateLot>>  GetLotForRequest(AllocateLot allocateLot){


            
           return await databaseContext.AllocateLots
                        .Include(s => s.AllocateLotSizes).Where( w=> w.Status == "A")
                        // .Include(s => s.AllocateMcs.Where( w=> w.StatusMc == "1"))
                        .Where(w => (string.IsNullOrEmpty(allocateLot.Buy) ? 1 == 1 : w.Buy == allocateLot.Buy)
                                        && (string.IsNullOrEmpty(allocateLot.SoNumber) ? 1 == 1 : w.SoNumber == allocateLot.SoNumber)
                                        && (string.IsNullOrEmpty(allocateLot.Lot) ? 1 == 1 : w.Lot == allocateLot.Lot)
                                        && (string.IsNullOrEmpty(allocateLot.PurOrder) ? 1 == 1 : w.PurOrder == allocateLot.PurOrder)
                                        && w.Status != "I" 
                                        && w.GeneratePd == 5
                                        && (string.IsNullOrEmpty(w.StatusIssueMat) ||  w.StatusIssueMat == "Request Issue"))
                        .OrderBy(o => o.Lot)
                        .ToListAsync();

        }

        public async Task<(string errorMessage, ReqIssueMaterialH reqIssueMaterialHres)> VerifyLotReleasedtoPD(ReqIssueMaterialH reqIssueMaterialH)
        {
            string errorMessage = string.Empty;
            ReqIssueMaterialH _data = new ReqIssueMaterialH();

            var chkreleasedpd = await databaseContext.AllocateLots.Where(w=> w.Status == "A" && w.StatusProduction != "Released" && w.Lot == reqIssueMaterialH.Lot ).ToListAsync();

            if (chkreleasedpd.Count() > 0){
                errorMessage = "Lot can't released to production.";
                _data = reqIssueMaterialH;
            }

            return (errorMessage,_data);


        }

        public async Task<IEnumerable<TplocationGroup>> GetLocationIssue()
        {
            return  ( await databaseContext.TplocationGroups.ToListAsync());
        }

        

        public async Task<(string errorMessage, List<ReqIssueMaterialH> reqIssueMaterialH)> GetRequestIssueByApr(ReqIssueSearch reqIssueSearch, VwWebUser vwWebUser)
        {

            string errorMessage = string.Empty;
            List<ReqIssueMaterialH> reqIssueMaterialH = new List<ReqIssueMaterialH>();

            List<String> _Status = new List<string>(new string[] {"Request","Approved","Request Discuss"});



            var _aprdt = await databaseContext.VwWebTpapprovals.Where(w=> w.Program == "ADDON" && w.Types == "Request Mat" && w.Name.ToUpper() == vwWebUser.EmpUsername!.ToUpper()).FirstOrDefaultAsync();

            if (_aprdt == null){
                errorMessage = "Can't Find Approval in Template (DB)";
                return (errorMessage,reqIssueMaterialH);

            }

            var grouplogId = await (databaseContext.ReqIssueMaterialLogs.GroupJoin(databaseContext.ReqIssueMaterialHs,
                        l => new {p1 = l.ReqHid},
                         h => new {p1 = h.Id},
                        (l,h) => new { il = l, ih =h})
                        .SelectMany(sm => sm.ih.DefaultIfEmpty(),
                            (l,h) => new {  il = l,ih =h})
                        .Where(w => _Status.Contains(w.ih.Status)
                           && (string.IsNullOrEmpty(reqIssueSearch.Lot) ? 1==1 :  w.ih.Lot == reqIssueSearch.Lot)
                                     && (string.IsNullOrEmpty(reqIssueSearch.RequestBy) ? 1==1 :  w.ih.RequestBy.ToLower() == reqIssueSearch.RequestBy.ToLower())
                                     && (string.IsNullOrEmpty(reqIssueSearch.ReqDept) ? 1==1 : w.ih.ReqDept.ToLower() == reqIssueSearch.ReqDept.ToLower())
                                    && (string.IsNullOrEmpty(reqIssueSearch.RequestDate) ? 1==1 :  w.ih.RequestDate == Convert.ToDateTime(reqIssueSearch.RequestDate)))
                        .GroupBy( g=> g.il.il.ReqHid)
                        .Select(s=> new {
                            ReqHid = s.Key,
                            Id = s.Max(m => m.il.il.Id)
                        }).ToListAsync()
                            );



            // var grouplogId = await (databaseContext.ReqIssueMaterialLogs
            //     .Where(w=> _Status.Contains(w.Status!))
            //     .GroupBy(g=> g.ReqHid)
            //     .Select(s=> new {
            //     ReqHid = s.Key,
            //     Id = s.Max(m => m.Id)
            // }).ToListAsync());

            List<int> _whlog = grouplogId.Select(s=> s.Id).ToList();



            var _reqIssue = await (databaseContext.ReqIssueMaterialHs.GroupJoin(databaseContext.ReqIssueMaterialLogs.Where(w=> _whlog.Contains(w.Id)),
                                h => new {p1 = h.Id},
                                l => new {p1 = l.ReqHid},
                                (h,l) => new { ih = h, il =l})
                                .SelectMany(sm => sm.il.DefaultIfEmpty(),
                                    (h,l) => new { ih =h, il = l})
                                .Where(w=> _Status.Contains(w.ih.ih.Status!) && w.ih.ih.ReqDept == _aprdt.Department && w.il!.Levels <= _aprdt.Levels )
                                .Select(s=> new { issueHId = s.ih.ih.Id})
                                .ToListAsync()
                                );

            List<long>  _whiss = _reqIssue.Select(s=> s.issueHId).ToList();

            
            reqIssueMaterialH = await (databaseContext.ReqIssueMaterialHs
                                    .Include(d => d.ReqIssueMaterialDs)
                                    .Include(log => log.ReqIssueMaterialLogs)
                                    .Where(w=> _whiss.Contains(w.Id)
                                     )
                                    .ToListAsync()
                                        );


            if (reqIssueMaterialH.Count == 0){
                errorMessage = "Data is not Found.";
            }




             return (errorMessage,reqIssueMaterialH);

            
        }

        public async Task<(string errorMessage, List<ReqIssueMaterialH> reqIssueMaterialH)> GetRequestIssueByUser(ReqIssueSearch reqIssueSearch, VwWebUser vwWebUser)
        {
             string errorMessage = string.Empty;
            List<ReqIssueMaterialH> reqIssueMaterialH = new List<ReqIssueMaterialH>();


            var _aprdt = await databaseContext.VwWebTpapprovals.Where(w=> w.Program == "ADDON" && w.Types == "Request Mat" && w.Name.ToUpper() == vwWebUser.EmpUsername!.ToUpper()).FirstOrDefaultAsync();


            reqIssueMaterialH = await (databaseContext.ReqIssueMaterialHs
                                    .Include(d => d.ReqIssueMaterialDs.Where(ww=>ww.Status == "A"))
                                    .Include(log => log.ReqIssueMaterialLogs)
                                    .Where(w=> w.Status != "Delete" && 
                                    (string.IsNullOrEmpty(_aprdt.Department) ? 1== 1 : w.ReqDept == _aprdt.Department ) &&
                                    (w.CreateBy == vwWebUser.EmpUsername) 
                                     )
                                    .ToListAsync()
                                        );


            if (reqIssueMaterialH.Count == 0){
                errorMessage = "Data is not Found.";
            }




             return (errorMessage,reqIssueMaterialH);
        }
    }
}