using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using AddOn_API.DTOs.AllocateLot;
using AddOn_API.DTOs.SaleOrder;
using AddOn_API.Entities;
using AddOn_API.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace AddOn_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]


    public class AllocateController : ControllerBase
    {
        private readonly IAllocateService allocateService;
        private readonly IAccountService accountService;
        private readonly ISapSDKService sapSDKService;
        private readonly ISaleOrderService saleOrderService;
        private readonly IGeneratePDandMCService generatePDandMCService;

        public AllocateController(IAllocateService allocateService, IAccountService accountService, IGeneratePDandMCService generatePDandMCService, ISapSDKService sapSDKService, ISaleOrderService saleOrderService)
        {
            this.generatePDandMCService = generatePDandMCService;


            this.sapSDKService = sapSDKService;
            this.saleOrderService = saleOrderService;
            this.accountService = accountService;
            this.allocateService = allocateService;
        }

        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<AllocateLotResponse>>> FindAll()
        {

            var accessToken = await HttpContext.GetTokenAsync("access_token");
            string access = (await accountService.CheckPermissionAccess(accessToken, "Allocate"));
            if (access == "Null")
                return Unauthorized();


            var _allocate = (await allocateService.FindAll());
            var _result = new List<AllocateLotResponse>();
            _result = _allocate.Adapt<List<AllocateLotResponse>>();

            if (_allocate.Count() == 0)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "Data is not Found..");
            }
            else
            {

                foreach (var item in _allocate)
                {
                    AllocateLotResponse _header = item.Adapt<AllocateLotResponse>();
                    List<AllocateLotSizeResponse> Size = new List<AllocateLotSizeResponse>();
                    List<AllocateMcResponse> mc = new List<AllocateMcResponse>();

                    Size = item.AllocateLotSizes.Adapt<List<AllocateLotSizeResponse>>();
                    _header.AllocateLotSizes = Size;

                    mc = item.AllocateMcs.Adapt<List<AllocateMcResponse>>();
                    _header.AllocateMcs = mc;

                    _result.Add(_header);

                }


                return StatusCode((int)HttpStatusCode.OK, _result);
            }

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AllocateLotResponse>> FindById(long id)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            string access = (await accountService.CheckPermissionAccess(accessToken, "Allocate"));
            if (access == "Null")
                return Unauthorized();


            var _allocate = (await allocateService.FindById(id));
            var _result = new List<AllocateLotResponse>();
            _result = _allocate.Adapt<List<AllocateLotResponse>>();

            if (_allocate == null)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "Data is not Found..");
            }
            else
            {

                AllocateLotResponse _header = _allocate.Adapt<AllocateLotResponse>();
                List<AllocateLotSizeResponse> Size = new List<AllocateLotSizeResponse>();
                List<AllocateMcResponse> mc = new List<AllocateMcResponse>();

                Size = _allocate.AllocateLotSizes.Adapt<List<AllocateLotSizeResponse>>();
                _header.AllocateLotSizes = Size;

                mc = _allocate.AllocateMcs.Adapt<List<AllocateMcResponse>>();
                _header.AllocateMcs = mc;

                _result.Add(_header);

                return StatusCode((int)HttpStatusCode.OK, _result);
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<AllocateLotResponse>> GenerateLot([FromBody] AllocateLotRequest model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            string access = (await accountService.CheckPermissionAccess(accessToken, "Allocate"));
            if (access == "Null")
                return Unauthorized();


            var account = accountService.GetInfo(accessToken);
            var allocateLot = model.Adapt<AllocateLot>();
            allocateLot.CreateBy = account.EmpUsername;





            (string errorMessage, string SoNumber) = await allocateService.VerifyData(allocateLot);
            if (errorMessage != "")
            {
                return StatusCode((int)HttpStatusCode.BadRequest, $"{errorMessage} ({SoNumber})");
            }
            try
            {
                await allocateService.Create(allocateLot);

                var resultlot = await allocateService.Search(allocateLot);

                return StatusCode((int)HttpStatusCode.Created, resultlot.Adapt<List<AllocateLotResponse>>());
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }


        }



        [HttpPost("[action]")]
        public async Task<ActionResult<AllocateLotResponse>> SearchLot([FromBody] AllocateLotRequest model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            string access = (await accountService.CheckPermissionAccess(accessToken, "Allocate"));
            if (access == "Null")
                return Unauthorized();

            AllocateLot _data = model.Adapt<AllocateLot>();
            var _allocate = (await allocateService.Search(_data));
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

        [HttpPut("{id}")]
        public async Task<ActionResult<AllocateLotResponse>> UpdateLot(int id, AllocateLotResponse model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (accessToken == null)
            {
                return Unauthorized();
            }

            var account = accountService.GetInfo(accessToken);



            AllocateLot _datah = model.Adapt<AllocateLot>();

            var allocateLotchk = await allocateService.FindById(id);

            if (allocateLotchk == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Id is not Found.");
            }

            (string errorMessage, string Lot) = await allocateService.VerifyDataLot(_datah);

            if (!String.IsNullOrEmpty(errorMessage))
            {
                return StatusCode((int)HttpStatusCode.NotFound, errorMessage + Lot);
            }

            var saleorder = await saleOrderService.FindById(model.SaleOrderId);



            try
            {

                  var detSize1 = allocateLotchk.AllocateLotSizes.Join(model.AllocateLotSizes
               , s => new { id = s.RowId }
               , e => new { id = e.RowId }
               , (s, e) => new { size = s, sedit = e })
               .Where(w => w.size.Lot == model.Lot)
                 .Select(sn => new
               {
                   sn.size.RowId,
                   sn.size.Lot,
                   sn.size.SizeNo,
                   sn.size.SaleOrderId,
                   sn.size.SaleOrderLineNum,
                   sn.sedit.Qty,
                   qtytotal = (sn.sedit.Qty > sn.size.Qty ? (sn.sedit.Qty - sn.size.Qty) : (sn.size.Qty - sn.sedit.Qty)),

               }).ToList();

                var detSize = detSize1.Join(saleorder.SaleOrderDs.Where(w => w.Sohid == model.SaleOrderId && w.LineStatus == "A")
               , sg => new { id = sg.SaleOrderId, linenum = sg.SaleOrderLineNum }
               , sd => new { id = sd.Sohid, linenum = sd.LineNum }
               , (sg, sd) => new { size = sg, sdetail = sd })
               .Where(w=> w.size.Lot == model.Lot)
               .Select(sn => new
               {
                   sn.size.RowId,
                   sn.size.Lot,
                   sn.size.SizeNo,
                   sn.size.SaleOrderId,
                   sn.size.SaleOrderLineNum,
                   sn.size.Qty,
                   qtytotal = (sn.size.Qty > sn.sdetail.Quantity ? sn.size.Qty : (sn.sdetail.Quantity - sn.size.qtytotal)),

               }).ToList();



            //     var detSize = allocateLotchk.AllocateLotSizes.Join(model.AllocateLotSizes
            //    , s => new { id = s.RowId }
            //    , e => new { id = e.RowId }
            //    , (s, e) => new { size = s, sedit = e })
            //    .Where(w => w.size.Lot == model.Lot)
            //    .Join(saleorder.SaleOrderDs.Where(w => w.Id == model.SaleOrderId && w.LineStatus == "A")
            //    , sg => new { id = sg.size.SaleOrderId, linenum = sg.size.SaleOrderLineNum }
            //    , sd => new { id = sd.Sohid, linenum = sd.LineNum }
            //    , (sg, sd) => new { size = sg.size, sedit = sg.sedit, sdetail = sd })
            //    .Select(sn => new
            //    {
            //        sn.size.RowId,
            //        sn.size.Lot,
            //        sn.size.SizeNo,
            //        sn.size.SaleOrderId,
            //        sn.size.SaleOrderLineNum,
            //        sn.sedit.Qty,
            //        qtytotal = sn.sdetail.Quantity - (sn.size.Qty - sn.sedit.Qty),

            //    }).ToList();


                var saleorderD = saleorder.SaleOrderDs.Where(w => detSize.Select(s => s.SaleOrderLineNum).ToList().Contains(w.LineNum)).ToList();


                ////update total into sale order D
                foreach (SaleOrderD item in saleorderD)
                {

                    var totalqty = detSize.Where(w => w.SaleOrderId == item.Sohid & w.SaleOrderLineNum == item.LineNum).FirstOrDefault();
                    if (totalqty != null)
                    {
                        item.Quantity = totalqty.qtytotal;
                        item.Updateby = account.EmpUsername;
                        item.UpdateDate = DateTime.Now;
                        item.LineStatus = (totalqty.qtytotal == 0) ? "I" : "A";

                    }

                }

                //// update qty to Lot
                allocateLotchk.S035 = detSize.Where(w => w.SizeNo == "3.5").Select(s => s.Qty).FirstOrDefault();
                allocateLotchk.S040 = detSize.Where(w => w.SizeNo == "4.0").Select(s => s.Qty).FirstOrDefault();
                allocateLotchk.S050 = detSize.Where(w => w.SizeNo == "5.0").Select(s => s.Qty).FirstOrDefault();
                allocateLotchk.S055 = detSize.Where(w => w.SizeNo == "5.5").Select(s => s.Qty).FirstOrDefault();
                allocateLotchk.S060 = detSize.Where(w => w.SizeNo == "6.0").Select(s => s.Qty).FirstOrDefault();
                allocateLotchk.S070 = detSize.Where(w => w.SizeNo == "7.0").Select(s => s.Qty).FirstOrDefault();
                allocateLotchk.S075 = detSize.Where(w => w.SizeNo == "7.5").Select(s => s.Qty).FirstOrDefault();

                allocateLotchk.S080 = detSize.Where(w => w.SizeNo == "8.0").Select(s => s.Qty).FirstOrDefault();
                allocateLotchk.S085 = detSize.Where(w => w.SizeNo == "8.5").Select(s => s.Qty).FirstOrDefault();
                allocateLotchk.S090 = detSize.Where(w => w.SizeNo == "9.0").Select(s => s.Qty).FirstOrDefault();
                allocateLotchk.S095 = detSize.Where(w => w.SizeNo == "9.5").Select(s => s.Qty).FirstOrDefault();
                allocateLotchk.S100 = detSize.Where(w => w.SizeNo == "10.0").Select(s => s.Qty).FirstOrDefault();
                allocateLotchk.S105 = detSize.Where(w => w.SizeNo == "10.5").Select(s => s.Qty).FirstOrDefault();
                allocateLotchk.S110 = detSize.Where(w => w.SizeNo == "11.0").Select(s => s.Qty).FirstOrDefault();

                allocateLotchk.S115 = detSize.Where(w => w.SizeNo == "11.5").Select(s => s.Qty).FirstOrDefault();
                allocateLotchk.S120 = detSize.Where(w => w.SizeNo == "12.0").Select(s => s.Qty).FirstOrDefault();
                allocateLotchk.S130 = detSize.Where(w => w.SizeNo == "13.0").Select(s => s.Qty).FirstOrDefault();
                allocateLotchk.S140 = detSize.Where(w => w.SizeNo == "14.0").Select(s => s.Qty).FirstOrDefault();
                allocateLotchk.S150 = detSize.Where(w => w.SizeNo == "15.0").Select(s => s.Qty).FirstOrDefault();
                allocateLotchk.S160 = detSize.Where(w => w.SizeNo == "16.0").Select(s => s.Qty).FirstOrDefault();
                allocateLotchk.S170 = detSize.Where(w => w.SizeNo == "17.0").Select(s => s.Qty).FirstOrDefault();

                allocateLotchk.Total = model.AllocateLotSizes.Sum(s => s.Qty);
                allocateLotchk.UpdateBy = account.EmpUsername;
                allocateLotchk.UpdateDate = DateTime.Now;

                foreach (AllocateLotSize item in allocateLotchk.AllocateLotSizes)
                {
                    item.Qty = model.AllocateLotSizes.Where(w => w.RowId == item.RowId).Select(s => s.Qty).FirstOrDefault();
                    item.UpdateBy = account.EmpUsername;
                    item.UpdateDate = DateTime.Now;
                }




                List<SaleOrderD> _saleOrderDNew = new List<SaleOrderD>();

               

                await allocateService.UpdateGenerateLot(allocateLotchk, saleorder.SaleOrderDs);





                // update GenerateLot into SaleOrderH
                (string errorMessageChkSTDSO, SaleOrderH saleOrderHChkSTDSO) = await allocateService.VerifyAllocateLotStatusSO(allocateLotchk.SaleOrderId);

                if (errorMessageChkSTDSO == "")
                    await allocateService.UpdateAllocateLotStatusSO(saleOrderHChkSTDSO);




                var _dataresult = await allocateService.FindById(id);

                return StatusCode((int)HttpStatusCode.OK, _dataresult.Adapt<AllocateLotResponse>());
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }

        }




        [HttpDelete("{id}")]
        public async Task<ActionResult<AllocateLotResponse>> DeleteLot(int id)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (accessToken == null)
            {
                return Unauthorized();
            }

            var account = accountService.GetInfo(accessToken);


            var allocateLotchk = await allocateService.FindById(id);

            if (allocateLotchk == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Id is not Found.");
            }

            (string errorMessage, string Lot) = await allocateService.VerifyDataLot(allocateLotchk);

            if (!String.IsNullOrEmpty(errorMessage))
            {
                return StatusCode((int)HttpStatusCode.NotFound, errorMessage + Lot);
            }

            var saleorder = await saleOrderService.FindById(allocateLotchk.SaleOrderId);

            try
            {

                var detSize = allocateLotchk.AllocateLotSizes
               .Join(saleorder.SaleOrderDs.Where(w => w.Id == allocateLotchk.SaleOrderId && w.LineStatus == "A")
               , sg => new { id = sg.SaleOrderId, linenum = sg.SaleOrderLineNum }
               , sd => new { id = sd.Id, linenum = sd.LineNum }
               , (sg, sd) => new { size = sg, sdetail = sd })
               .Select(sn => new
               {
                   sn.size.RowId,
                   sn.size.Lot,
                   sn.size.SizeNo,
                   sn.size.SaleOrderId,
                   sn.size.SaleOrderLineNum,
                   sn.size.Qty,
                   qtytotal = sn.sdetail.Quantity - sn.size.Qty,

               }).ToList();


                var saleorderD = saleorder.SaleOrderDs.Where(w => detSize.Select(s => s.SaleOrderLineNum).ToList().Contains(w.LineNum)).ToList();


                ////update total into sale order D
                foreach (SaleOrderD item in saleorderD)
                {

                    var totalqty = detSize.Where(w => w.SaleOrderId == item.Id & w.SaleOrderLineNum == item.LineNum).FirstOrDefault();
                    if (totalqty != null)
                    {
                        item.Quantity = totalqty.qtytotal;
                        item.Updateby = account.EmpUsername;
                        item.UpdateDate = DateTime.Now;
                        item.LineStatus = (totalqty.qtytotal == 0) ? "I" : "A";

                    }
                }

                foreach (AllocateLotSize item in allocateLotchk.AllocateLotSizes)
                {
                    item.Status = "I";
                    item.UpdateBy = account.EmpUsername;
                    item.UpdateDate = DateTime.Now;
                }

                allocateLotchk.Status = "I";
                allocateLotchk.UpdateBy = account.EmpUsername;
                allocateLotchk.UpdateDate = DateTime.Now;

                await allocateService.UpdateGenerateLot(allocateLotchk, saleorder.SaleOrderDs);


                // update GenerateLot into SaleOrderH
                (string errorMessageChkSTDSO, SaleOrderH saleOrderHChkSTDSO) = await allocateService.VerifyAllocateLotStatusSO(allocateLotchk.SaleOrderId);

                if (errorMessageChkSTDSO == "")
                    await allocateService.UpdateAllocateLotStatusSO(saleOrderHChkSTDSO);





                return StatusCode((int)HttpStatusCode.OK, id);
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }


        }

        [HttpPost("[action]")]
        public async Task<ActionResult<List<GenerateResponse>>> DeleteLotList([FromBody] RequestCVSAP model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (accessToken == null)
            {
                return Unauthorized();
            }


            var account = accountService.GetInfo(accessToken);

            var _saleOrder = await saleOrderService.FindById(model.id);

            (string errorMessage, List<GenerateResponse> generateResponses) = await allocateService.VerifyDataDeleteLotList(model.id);

            AllocateLot lotrequest = new AllocateLot();
            lotrequest.SoNumber = _saleOrder.SoNumber!;

            var _lotlist = await allocateService.Search(lotrequest);


            if (!string.IsNullOrEmpty(errorMessage))
            {

                return StatusCode((int)HttpStatusCode.BadRequest, JsonSerializer.Serialize(generateResponses));
            }

            try
            {

                foreach (AllocateLot item in _lotlist)
                {
                    item.Status = "I";
                    item.UpdateBy = account.EmpUsername;
                    item.UpdateDate = System.DateTime.Now;

                    foreach (AllocateLotSize itemd in item.AllocateLotSizes)
                    {
                        itemd.Status = "I";
                        itemd.UpdateBy = account.EmpUsername;
                        itemd.UpdateDate = DateTime.Now;
                    }
                }

                _saleOrder.GenerateLot = 0;
                _saleOrder.GenerateLotBy = account.EmpUsername;
                _saleOrder.UpdateBy = account.EmpUsername;
                _saleOrder.UpdateDate = System.DateTime.Now;


                await allocateService.Delete(_lotlist.ToList(), _saleOrder);

                return StatusCode((int)HttpStatusCode.OK, model.id);

            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());

            }




        }

        [HttpPost("[action]")]
        public async Task<ActionResult<List<GenerateResponse>>> UpdateMultiLot([FromForm] AllocateLotUpdateLotList model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (accessToken == null)
            {
                return Unauthorized();
            }

            var account = accountService.GetInfo(accessToken);

            if (model.FormFiles == null)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "File Item List is not Found.");
            }

            (string errorMessage, List<string> fileName) = await allocateService.UploadFile(model.FormFiles);
            if (!String.IsNullOrEmpty(errorMessage))
            {
                return StatusCode((int)HttpStatusCode.NotFound, "File Item List is not Found.");
            }


            List<AllocateLot> _lotrequest = new List<AllocateLot>();


            //// get data from Excel File
            var strfileName = String.Join(",", fileName);

            var dataExcel = await allocateService.GetdatafromFile(strfileName);
            if (dataExcel == null)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "Item detail is not Found.");
            }


            foreach (AllocateLotExcelLotList item in dataExcel)
            {
                var _allocatelotDB = await allocateService.Search(item.Adapt<AllocateLot>());

                if (_allocatelotDB == null)
                {
                    _lotrequest.Add(_allocatelotDB.Where(w => w.Lot == item.Lot).FirstOrDefault()!);
                }

            }

            List<GenerateResponse> _lotresult = new List<GenerateResponse>();

            if (_lotrequest.Count == 0 || _lotrequest.Count != dataExcel.Count())
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Can't find Lot in list");
            }


            (string errorMessageverifydata, List<GenerateResponse> generateResponses) = await allocateService.VerifyDataUpdateLotList(_lotrequest);
            if (!string.IsNullOrEmpty(errorMessageverifydata))
            {

                return StatusCode((int)HttpStatusCode.BadRequest, JsonSerializer.Serialize(generateResponses));
            }


            (string errorMessageverifydataStyle, List<GenerateResponse> generateResponsesStyle) = await allocateService.VerifyStyleWithSaleOrderD(dataExcel.Adapt<List<AllocateLotExcelLotList>>());
            if (!string.IsNullOrEmpty(errorMessageverifydataStyle))
            {

                return StatusCode((int)HttpStatusCode.BadRequest, JsonSerializer.Serialize(generateResponsesStyle));
            }

            try
            {

                foreach (AllocateLot item in _lotrequest)
                {

                    AllocateLotExcelLotList _lotexcel = dataExcel.Where(w => w.Lot == item.Lot).FirstOrDefault().Adapt<AllocateLotExcelLotList>();

                    (AllocateLot _allocateLotprepare, List<AllocateLotSize> _newAllocateLotSize) = await allocateService.PreparedatarequestUpdateLotList(_lotexcel, item, account);

                    var saleorder = await saleOrderService.FindById(_allocateLotprepare.SaleOrderId);

                   _allocateLotprepare.UploadFile = strfileName;

                    var allocateLotchk = await allocateService.FindById(_allocateLotprepare.Id);

                  

                    var detSize = allocateLotchk.AllocateLotSizes.Join(_allocateLotprepare.AllocateLotSizes
                                    , s => new { id = s.RowId }
                                    , e => new { id = e.RowId }
                                    , (s, e) => new { size = s, sedit = e })
                                    .Where(w => w.size.Lot == _allocateLotprepare.Lot)
                                    .Join(saleorder.SaleOrderDs.Where(w => w.Id == _allocateLotprepare.SaleOrderId && w.LineStatus == "A")
                                    , sg => new { id = sg.size.SaleOrderId, linenum = sg.size.SaleOrderLineNum }
                                    , sd => new { id = sd.Id, linenum = sd.LineNum }
                                    , (sg, sd) => new { size = sg.size, sedit = sg.sedit, sdetail = sd })
                                    .Select(sn => new
                                    {
                                        sn.size.RowId,
                                        sn.size.Lot,
                                        sn.size.SizeNo,
                                        sn.size.SaleOrderId,
                                        sn.size.SaleOrderLineNum,
                                        sn.sedit.Qty,
                                        qtytotal = sn.sdetail.Quantity - (sn.size.Qty - sn.sedit.Qty),

                                    }).ToList();

                    var saleorderD = saleorder.SaleOrderDs.Where(w => detSize.Select(s => s.SaleOrderLineNum).ToList().Contains(w.LineNum)).ToList();

                    ////update total into sale order D
                    foreach (SaleOrderD items in saleorderD)
                    {

                        var totalqty = detSize.Where(w => w.SaleOrderId == item.Id & w.SaleOrderLineNum == items.LineNum).FirstOrDefault();
                        if (totalqty != null)
                        {
                            items.Quantity = totalqty.qtytotal;
                            items.Updateby = account.EmpUsername;
                            items.UpdateDate = DateTime.Now;
                            items.LineStatus = (totalqty.qtytotal == 0) ? "I" : "A";
                        }                      

                    }

                    //// update total into sale order D  case new size in lot
                    foreach (SaleOrderD items in saleorder.SaleOrderDs){
                        var newqty = _newAllocateLotSize.Where( w=> w.SizeNo == items.ItemCode).FirstOrDefault();
                         if (newqty != null){
                            items.Quantity = items.Quantity + newqty.Qty;
                            items.Updateby = account.EmpUsername;
                            items.UpdateDate = DateTime.Now;
                            items.LineStatus = "A";
                         }

                    }

                    List<SaleOrderD> _saleOrderDNew = new List<SaleOrderD>();

                    await allocateService.UpdateGenerateLot(_allocateLotprepare, saleorder.SaleOrderDs);

                    _lotresult.Add( new GenerateResponse{
                        errorMessage = "Complete",
                        referenceNumber = _allocateLotprepare.Lot
                    });
                }


                return StatusCode((int)HttpStatusCode.OK, JsonSerializer.Serialize(_lotresult));

            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());

            }

        }


        [HttpPost("[action]")]
        public async Task<ActionResult<String>> ChangeLotNumber([FromBody] ChangeLotNumber model)
        {

              var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (accessToken == null)
            {
                return Unauthorized();
            }

            List<AllocateLot> _lotOld = new List<AllocateLot>();
            

            var account = accountService.GetInfo(accessToken);
           
            AllocateLot alLotFrom = new AllocateLot {Lot = model.LotFrom};  
            AllocateLot alLotTo = new AllocateLot {Lot = model.LotTo};  

            var _lotFrom = await allocateService.Search(alLotFrom);  
            var _lotTo = await allocateService.Search(alLotTo);


            if (_lotFrom.Count() == 0 || _lotTo.Count() == 0 ){
                return StatusCode((int)HttpStatusCode.NotFound, "Lot Number is not Found.");
            }

            _lotOld.AddRange(_lotFrom);
            _lotOld.AddRange(_lotTo);

           

            (string errorMessage, ChangeLotNumber changeLotNumber) = await allocateService.VerifyChangeLotNumber(alLotFrom,alLotTo );
            
            List<AllocateLot> _lotNew = await allocateService.PrepareDataChangeLot(_lotOld,model,account);


            if (!String.IsNullOrEmpty(errorMessage)){
                return StatusCode((int)HttpStatusCode.NotFound, errorMessage);
            }

            try
            {
                
                await allocateService.UpdateChangeLotNumber(_lotOld,_lotNew);     


                return StatusCode((int)HttpStatusCode.OK, JsonSerializer.Serialize(changeLotNumber));
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }


        }
        

        [HttpPost("[action]")]
        public async Task<ActionResult<List<GenerateResponse>>> LotReleasetoPD([FromBody] List<AllocateLotRequest> model)
        {
           var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (accessToken == null)
            {
                return Unauthorized();
            }

            var account = accountService.GetInfo(accessToken);
            List<GenerateResponse> _lotresult = new List<GenerateResponse>();

            try
            {
                
                (string errorMessage, List<GenerateResponse> generateResponses) = await allocateService.VerifyLotReleasetoPD(model.Adapt<List<AllocateLot>>());
                if (!string.IsNullOrEmpty(errorMessage)){
                    
                    return StatusCode((int)HttpStatusCode.BadRequest, JsonSerializer.Serialize(generateResponses));
                }

                foreach(AllocateLot item in model.Adapt<List<AllocateLot>>()){
                    var _allocatelot = await allocateService.Search(item);

                    foreach(AllocateLot iteml in _allocatelot){
                        iteml.StatusProduction = "Released";
                        iteml.UpdateBy = account.EmpUsername;
                        iteml.UpdateDate = System.DateTime.Now;

                        await allocateService.Update(iteml);

                        _lotresult.Add( new GenerateResponse{
                            errorMessage = "Complete",
                            referenceNumber = iteml.Lot
                        });
                    }

                }
                // Released
               return StatusCode((int)HttpStatusCode.OK, JsonSerializer.Serialize(_lotresult));

            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());

            }

        
            
        }
        
        [HttpPost("[action]")]
        public async Task<ActionResult<List<GenerateResponse>>> DelLotReleasetoPD([FromBody] List<AllocateLotRequest> model)
        {
           var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (accessToken == null)
            {
                return Unauthorized();
            }

            var account = accountService.GetInfo(accessToken);
            List<GenerateResponse> _lotresult = new List<GenerateResponse>();

            try
            {
                
                (string errorMessage, List<GenerateResponse> generateResponses) = await allocateService.VerifyDelLotReleasetoPD(model.Adapt<List<AllocateLot>>());
                if (!string.IsNullOrEmpty(errorMessage)){
                    
                    return StatusCode((int)HttpStatusCode.BadRequest, JsonSerializer.Serialize(generateResponses));
                }

                foreach(AllocateLot item in model.Adapt<List<AllocateLot>>()){
                    var _allocatelot = await allocateService.Search(item);

                    foreach(AllocateLot iteml in _allocatelot){
                        iteml.StatusProduction = null;
                        iteml.UpdateBy = account.EmpUsername;
                        iteml.UpdateDate = System.DateTime.Now;

                        await allocateService.Update(iteml);

                        _lotresult.Add( new GenerateResponse{
                            errorMessage = "Complete",
                            referenceNumber = iteml.Lot
                        });
                    }

                }
                // Released
               return StatusCode((int)HttpStatusCode.OK, JsonSerializer.Serialize(_lotresult));

            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());

            }
        }
        




        // [HttpGet("{id}")]
        // public async Task<ActionResult<AllocateLotResponse>> FindById(long id)
        // {
        //      var accessToken = await HttpContext.GetTokenAsync("access_token");
        //     string access = (await accountService.CheckPermissionAccess(accessToken,"Allocate"));
        //     if (access == "Null")
        //         return Unauthorized();

        //     var _allocate = (await allocateService.FindById(id));
        //     var _result = new AllocateLotResponse();

        //     if (_result == null){
        //       return StatusCode((int)HttpStatusCode.NotFound,_result);
        //     }
        //     else{
        //         return StatusCode((int)HttpStatusCode.OK,_result);
        //     }    
        // }
    }
}