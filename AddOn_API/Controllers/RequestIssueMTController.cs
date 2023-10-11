using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AddOn_API.DTOs.ReqIssueMT;
using AddOn_API.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Mapster;
using AddOn_API.Entities;
using AddOn_API.DTOs.MailService;
using AddOn_API.DTOs.AllocateLot;
using System.Text.Json;
//using AddOn_API.Models;

namespace AddOn_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestIssueMTController : ControllerBase
    {
        private readonly IAccountService accountService;
        private readonly IReqIssueMTService reqIssueMTService;
        private readonly IMailService mailService;
        private readonly ISapSDKService sapSDKService;
        public RequestIssueMTController(IAccountService accountService, IReqIssueMTService reqIssueMTService, IMailService mailService, ISapSDKService sapSDKService)
        {
            this.sapSDKService = sapSDKService;
            this.mailService = mailService;
            this.reqIssueMTService = reqIssueMTService;
            this.accountService = accountService;
        }

        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<ReqIssueMTH>>> GetAll()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                return Unauthorized();
            }
            var account = accountService.GetInfo(accessToken);

            ReqIssueMaterialH _data = new ReqIssueMaterialH();

            _data.RequestBy = account.EmpUsername;

            var reqList = await reqIssueMTService.FindByRequest(_data);

            if (reqList.Count() == 0)
            {
                return NotFound();
            }

            return StatusCode((int)HttpStatusCode.OK, reqList.Adapt<List<ReqIssueMTH>>());


        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReqIssueMTH>> GetById(long id)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                return Unauthorized();
            }

            var account = accountService.GetInfo(accessToken);

            ReqIssueMaterialH _data = new ReqIssueMaterialH();

            _data.RequestBy = account.EmpUsername;

            var reqList = await reqIssueMTService.FindById(id);

            if (reqList == null)
            {
                return NotFound();
            }

            return StatusCode((int)HttpStatusCode.OK, reqList.Adapt<ReqIssueMTH>());
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<IEnumerable<ReqIssueMTH>>> Search(ReqIssueMTH model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                return Unauthorized();
            }
            var account = accountService.GetInfo(accessToken);

            var reqList = await reqIssueMTService.Search(model.Adapt<ReqIssueMaterialH>());

            if (reqList.Count() == 0)
            {
                return NotFound();
            }

            return StatusCode((int)HttpStatusCode.OK, reqList.Adapt<List<ReqIssueMTH>>());
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<IEnumerable<ReqIssueMTH>>> GetRequestIssueByApr(ReqIssueSearch model)
        {
            // TODO: Your code here
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                return Unauthorized();
            }
            var account = accountService.GetInfo(accessToken);

           
           (string errorMessage, List<ReqIssueMaterialH> reqIssueMaterialH) = await reqIssueMTService.GetRequestIssueByApr(model,account);

           if (!string.IsNullOrEmpty(errorMessage)){
                return StatusCode((int)HttpStatusCode.NotFound,errorMessage);
           }


            return StatusCode((int)HttpStatusCode.OK, reqIssueMaterialH.Adapt<List<ReqIssueMTH>>());
            
        
          
        }
        
        [HttpPost("[action]")]
        public async Task<ActionResult<IEnumerable<ReqIssueMTH>>> GetRequestIssueByUser(ReqIssueSearch model)
        {
          // TODO: Your code here
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                return Unauthorized();
            }
            var account = accountService.GetInfo(accessToken);
 
           (string errorMessage, List<ReqIssueMaterialH> reqIssueMaterialH) = await reqIssueMTService.GetRequestIssueByUser(model,account);

           if (!string.IsNullOrEmpty(errorMessage)){
                return StatusCode((int)HttpStatusCode.NotFound,errorMessage);
           }


            return StatusCode((int)HttpStatusCode.OK, reqIssueMaterialH.Adapt<List<ReqIssueMTH>>());
        }
        



        [HttpPost("[action]")]
        public async Task<ActionResult<IEnumerable<AllocateLotResponse>>> GetLotForRequest(AllocateLotRequest model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                return Unauthorized();
            }
            var account = accountService.GetInfo(accessToken);

            var _allocate = (await reqIssueMTService.GetLotForRequest(model.Adapt<AllocateLot>()));

            var _result = new List<AllocateLotResponse>();
            _result = _allocate.Adapt<List<AllocateLotResponse>>();


            if (_allocate == null)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "Data is not Found..");
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, _result);
            }

        }

[HttpGet("[action]")]
public async Task<ActionResult<IEnumerable<LocationIssue>>> GetLocationIssue()
{
    var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                return Unauthorized();
            }
            var account = accountService.GetInfo(accessToken);

    
    var _data = (await reqIssueMTService.GetLocationIssue());
    
    if (_data.Count() == 0 )
            {
                return StatusCode((int)HttpStatusCode.NotFound, "Data is not Found..");
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, _data.Adapt<List<LocationIssue>>());
            }

}

        


        [HttpPost("[action]")]
        public async Task<ActionResult<IEnumerable<RequestIssueMTItemListH>>> GetItemPDByLot(AllocateLotRequest model)
        {
            try
            {
                var itemD = await reqIssueMTService.GetPDItemFromLot(model.Adapt<AllocateLot>());

                if (itemD.Count() == 0)
                {
                    return StatusCode((int)HttpStatusCode.NotFound, "Data is not Found..");
                }

                List<string> _itemCodeList = itemD.GroupBy(g => g.ItemCode).Select(s => s.Key).ToList();
                var onhandSap = await sapSDKService.GetItemOnhand(_itemCodeList);

                var _stdp = itemD.GroupBy(g => new { ArticleCode = g.ItemCodeS.Substring(0,g.ItemCodeS.IndexOf("_")), GroupItem = g.ItemCode.Substring(0, 4) }).Select(s => new TpstyleWithLocation
                {
                    ArticleCode = s.Key.ArticleCode,
                    GroupItem = s.Key.GroupItem
                }).ToList();

                var dpitem = await reqIssueMTService.GetTPWithLocation(_stdp);


                foreach (ReqIssueMTItemList item in itemD)
                {

                    var onhand = onhandSap.Where(w => w.ItemCode == item.ItemCode).FirstOrDefault();

                    var dept = dpitem.Where(w => w.ArticleCode == item.ItemCodeS.Substring(0,item.ItemCodeS.IndexOf("_")) && w.GroupItem == item.ItemCode.Substring(0, 4)).FirstOrDefault();

                    item.Department = (dept == null ? "" : dept.Location);
                    item.Onhand = onhand.OnHand;
                    item.WhsCode = onhand.WhsCode;
                    item.OnhandDFwh = onhand.OnHandDFwh;
                }

                List<RequestIssueMTItemListH> _itemh = itemD.GroupBy(g=> new {g.ItemCode,g.ItemName,g.UomName,g.Department,g.Onhand,g.WhsCode,g.OnhandDFwh}).Select(s=> new RequestIssueMTItemListH{
                    ItemCode = s.Key.ItemCode,
                    ItemName = s.Key.ItemName,
                    PlandQty = s.Sum( m=> m.PlandQty),
                    UomName = s.Key.UomName,
                    Department = s.Key.Department,
                    Onhand = s.Key.Onhand,
                    WhsCode = s.Key.WhsCode,
                    OnhandDFwh = s.Key.OnhandDFwh,
                }).ToList();

                int i = 0;
                foreach(RequestIssueMTItemListH item in _itemh){

                    List<ReqIssueMTItemList> _itemd = itemD.Where(w=> w.ItemCode== item.ItemCode && w.UomName == item.UomName && w.Department == item.Department && w.WhsCode == item.WhsCode).ToList();

                    i++;
                    item.Id =i;
                    item.ItemD = _itemd;
                }

                return StatusCode((int)HttpStatusCode.OK, _itemh);
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }
        }






        [HttpPost("[action]")]
        public async Task<ActionResult<ReqIssueMTH>> Create(ReqIssueMTH model)
        {

            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                return Unauthorized();
            }

            var account = accountService.GetInfo(accessToken);

            var reqIssueList = model.Adapt<ReqIssueMaterialH>();


            List<ReqIssueMTResponse> respon = new List<ReqIssueMTResponse>();

            try
            {

                (string errorMessagechkrel, ReqIssueMaterialH reqIssueMaterialHchkrel) = await reqIssueMTService.VerifyLotReleasedtoPD(reqIssueList);
                if (!string.IsNullOrEmpty(errorMessagechkrel))
                {
            
                        respon.Add(new ReqIssueMTResponse
                        {
                            referenceNumber = reqIssueMaterialHchkrel.Lot!,
                            errorMessage = errorMessagechkrel
                        });
                    
                    return StatusCode((int)HttpStatusCode.BadRequest, JsonSerializer.Serialize(respon));

                }
                    List<ReqIssueMaterialD> _detail = model.ReqIssueMaterialDs.Adapt<List<ReqIssueMaterialD>>();

                    string ReqNumber = await reqIssueMTService.GetReqNumber();



                    //// check item with item sap
                    (string errorMessagechkitem, List<ReqIssueMaterialD> reqIssueMaterialDchk) = await reqIssueMTService.CheckReqIssueMTDetail(_detail);

                    if (string.IsNullOrEmpty(errorMessagechkitem))
                    {

                        reqIssueList.ReqNumber = ReqNumber;
                        reqIssueList.CreateBy = account.EmpUsername;
                        reqIssueList.CreateDate = System.DateTime.Now;
                        reqIssueList.Status = "Request";

                        reqIssueList.ReqIssueMaterialDs = model.ReqIssueMaterialDs.Adapt<List<ReqIssueMaterialD>>();

                       

                        await reqIssueMTService.Create(reqIssueList);

                        respon.Add(new ReqIssueMTResponse
                        {
                            referenceNumber = reqIssueList.Lot!,
                            errorMessage = "Complete"
                        });

                    }
                    else
                    {

                        var _itemdetailerror = reqIssueMaterialDchk.Select(s => new ReqIssueMTResponseItem
                        {
                            errorMessage = "Dupicate item.",
                            referenceNumber = s.ItemCode,
                            id = s.Id
                        }).ToList();

                        respon.Add(new ReqIssueMTResponse
                        {
                            referenceNumber = reqIssueList.Lot!,
                            errorMessage = errorMessagechkitem,
                            itemdetail = _itemdetailerror
                        });

                    }


                

                return StatusCode((int)HttpStatusCode.Created, respon);
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }




        }

        [HttpPost("[action]")]
        public async Task<ActionResult<ReqIssueMTH>> RequestApprove(ReqIssueMTH model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (accessToken == null)
            {
                return Unauthorized();
            }
            var account = accountService.GetInfo(accessToken);

            try
            {
                var _reqIssueH = await reqIssueMTService.FindById(model.Id);

                
                ReqIssueMTLog _log = new ReqIssueMTLog{
                    Id = 0,
                    ReqHid = Convert.ToInt64(2),
                    Users = "",
                    LogDate = System.DateTime.Now,  
                    Status = "",
                    Levels = 0,
                    Comment = "",
                    Action = "",
                    ClientName = "",
                };

               
                foreach(ReqIssueMTLog ilog in model.ReqIssueMaterialLogs){
                    _log = ilog;
                }
                
               


                (string errorMessage, ReqIssueMaterialH reqIssueMaterialH,ReqIssueMaterialLog reqIssueMTLog) = await reqIssueMTService.VerifyDataApproveprocess(_reqIssueH, _log.Adapt<ReqIssueMaterialLog>());

                if (!string.IsNullOrEmpty(errorMessage))
                {

                    return StatusCode((int)HttpStatusCode.BadRequest, errorMessage);

                }
                else
                {
                    _reqIssueH.Status = _log.Status;
                    _reqIssueH.UpdateBy = account.EmpUsername;
                    _reqIssueH.UpdateDate = System.DateTime.Now;

                    await reqIssueMTService.Update(_reqIssueH);
                    await reqIssueMTService.InsertLogReqIssueMT(_log.Adapt<ReqIssueMaterialLog>());

                    MailData mailData = await reqIssueMTService.PrepareDataApproveprocessSendEmail(_reqIssueH, _log.Adapt<ReqIssueMaterialLog>());

                    mailService.SendMail(mailData);

                    return StatusCode((int)HttpStatusCode.OK, _reqIssueH);


                }
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }
        }


        [HttpPost("[action]")]
        public async Task<ActionResult<GenerateResponse>> Approvedprocess(List<ReqIssueMTH> model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (accessToken == null)
            {
                return Unauthorized();
            }
            var account = accountService.GetInfo(accessToken);

            List<GenerateResponse> generateResponses = new List<GenerateResponse>();

            try
            {
                foreach(ReqIssueMTH item in model){
                    var _reqIssueH = (await reqIssueMTService.FindById(item.Id));

                     ReqIssueMTLog _log = new ReqIssueMTLog{
                        Id = 0,
                        ReqHid = Convert.ToInt64(2),
                        Users = "",
                        LogDate = System.DateTime.Now,  
                        Status = "",
                        Levels = 0,
                        Comment = "",
                        Action = "",
                        ClientName = "",
                    };

                    foreach(ReqIssueMTLog ilog in item.ReqIssueMaterialLogs){
                        _log = ilog;
                    }
                

                (string errorMessage, ReqIssueMaterialH reqIssueMaterialH,ReqIssueMaterialLog reqIssueMTLog) = await reqIssueMTService.VerifyDataApproveprocess(_reqIssueH, _log.Adapt<ReqIssueMaterialLog>());

                if (!string.IsNullOrEmpty(errorMessage))
                {
                     generateResponses.Add(new GenerateResponse
                        {
                            errorMessage = errorMessage,
                            referenceNumber = _reqIssueH.ReqNumber +"("+_reqIssueH.Lot+")",
                        });
                    
                   // return StatusCode((int)HttpStatusCode.BadRequest, errorMessage);

                }
                else
                {
                    _reqIssueH.Status = (reqIssueMaterialH.Status == "Finish" ? reqIssueMaterialH.Status : _log.Status );
                    _reqIssueH.UpdateBy = account.EmpUsername;
                    _reqIssueH.UpdateDate = System.DateTime.Now;

                    await reqIssueMTService.Update(_reqIssueH);
                    await reqIssueMTService.InsertLogReqIssueMT(reqIssueMTLog);

                    MailData mailData = await reqIssueMTService.PrepareDataApproveprocessSendEmail(_reqIssueH, reqIssueMTLog);

                    mailService.SendMail(mailData);

                   
                    generateResponses.Add(new GenerateResponse
                        {
                            errorMessage = "Complete",
                            referenceNumber = _reqIssueH.ReqNumber +"("+_reqIssueH.Lot+")",
                        });

                }
                }

                 return StatusCode((int)HttpStatusCode.OK, generateResponses);
                
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, ReqIssueMTH model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                return Unauthorized();
            }
            var account = accountService.GetInfo(accessToken);

            var reqIssue = model.Adapt<ReqIssueMaterialH>();

            try
            {

                (string errorMessage, ReqIssueMaterialH reqIssueMaterialH) = await reqIssueMTService.VerifyDataUpdate(id);

                if (string.IsNullOrEmpty(errorMessage))
                {

                    List<ReqIssueMaterialD> _detail = reqIssue.ReqIssueMaterialDs.Adapt<List<ReqIssueMaterialD>>();

                    (string errorMessagechkitem, List<ReqIssueMaterialD> reqIssueMaterialDchk) = await reqIssueMTService.CheckReqIssueMTDetail(_detail);

                    if (string.IsNullOrEmpty(errorMessagechkitem))
                    {
                        reqIssue.UpdateBy = account.EmpUsername;
                        reqIssue.UpdateDate = System.DateTime.Now;

                        await reqIssueMTService.Update(reqIssue);
                    }


                }
                return StatusCode((int)HttpStatusCode.OK, reqIssue.ReqNumber);
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }



        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ReqIssueMTH>> Delete(long id)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                return Unauthorized();
            }
            var account = accountService.GetInfo(accessToken);


            try
            {

                (string errorMessage, ReqIssueMaterialH reqIssueMaterialH) = await reqIssueMTService.VerifyDataDelete(id);

                if (string.IsNullOrEmpty(errorMessage))
                {

                    var _reqIssuedel = await reqIssueMTService.FindById(id);

                    await reqIssueMTService.DeleteDraftReqIssueMT(_reqIssuedel);

                }
                return StatusCode((int)HttpStatusCode.OK, reqIssueMaterialH.ReqNumber);
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }


        }
    }
}