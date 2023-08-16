using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AddOn_API.DTOs.QueueCVSAP;
using Microsoft.AspNetCore.Mvc;
using AddOn_API.Interfaces;
using Microsoft.AspNetCore.Authentication;
using System.Net;
using AddOn_API.Entities;
using Mapster;
//using AddOn_API.Models;

namespace AddOn_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueueCVSAPController : ControllerBase
    {
        private readonly IQueueCVSAPServices queueCVSAPServices;
        private readonly IGeneratePDandMCService generatePDandMCService;
        private readonly ISapSDKService sapSDKService;
        private readonly IAccountService accountService;
        private readonly IAllocateService allocateService;
        public QueueCVSAPController(IQueueCVSAPServices queueCVSAPServices, IGeneratePDandMCService generatePDandMCService, ISapSDKService sapSDKService, IAccountService accountService, IAllocateService allocateService)
        {
            this.allocateService = allocateService;
            this.accountService = accountService;
            this.sapSDKService = sapSDKService;
            this.generatePDandMCService = generatePDandMCService;
            this.queueCVSAPServices = queueCVSAPServices;
        }

        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<QueueCVSAPResponse>>> GetQueueCVSAP()
        {

            List<QueueCVSAPResponse> itemH = new List<QueueCVSAPResponse>();
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                return Unauthorized();
            }

            var _result = await (queueCVSAPServices.FindAll());

            if (_result.Count() == 0)
            {
                return NotFound();
            }

            else
            {
                return StatusCode((int)HttpStatusCode.OK, _result.Adapt<List<QueueCVSAPResponse>>());
            }


        }

        [HttpGet("{id}")]
        public async Task<ActionResult<QueueCVSAPResponse>> GetQueueCVSAPById(long id)
        {
            List<QueueCVSAPResponse> itemH = new List<QueueCVSAPResponse>();
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                return Unauthorized();
            }

            var _result = (await queueCVSAPServices.FindAll());

            if (_result == null)
            {
                return NotFound();
            }

            else
            {
                return StatusCode((int)HttpStatusCode.OK, itemH);
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<QueueCVSAPResponse>> Search(QueueCVSAPResponse model)
        {
            List<QueueCVSAPResponse> itemH = new List<QueueCVSAPResponse>();

            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                return Unauthorized();
            }

            var _result = (await queueCVSAPServices.FindAll());

            if (_result == null)
            {
                return NotFound();
            }

            else
            {

                return StatusCode((int)HttpStatusCode.OK, itemH);
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<QueueCVSAPResponse>> DeleteQueue(List<QueueCVSAPResponse> model)
        {
            List<QueueCVSAPResponse> itemH = new List<QueueCVSAPResponse>();
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                return Unauthorized();
            }


            (string errorMessage, List<QueueConvertToSap> queueConvertToSapsRespon) = await queueCVSAPServices.VerifyDataDeleteQueue(model.Adapt<List<QueueConvertToSap>>());

            if (errorMessage != "")
            {
                return StatusCode((int)HttpStatusCode.BadRequest, queueConvertToSapsRespon.Adapt<List<QueueCVSAPResponse>>());
            }

            try
            {

                await queueCVSAPServices.Delete(model.Adapt<List<QueueConvertToSap>>());


            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }


            return StatusCode((int)HttpStatusCode.OK, itemH);
        }


        [HttpPost("[action]")]
        public async Task<ActionResult<QueueCVSAPResponse>> ConvertPDToSAP(List<QueueCVSAPResponse> model)
        {
            List<QueueCVSAPResponse> itemH = new List<QueueCVSAPResponse>();
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var account = accountService.GetInfo(accessToken);

            if (accessToken == null)
            {
                return Unauthorized();
            }

            try
            {

                foreach (QueueConvertToSap item in model.Adapt<List<QueueConvertToSap>>())
                {

                    var _ph = await generatePDandMCService.FindPDById(item.DocumentId!.Value);
                    DateTime _start = System.DateTime.Now;


                    if (_ph.DocEntry != null)
                    {
                        item.ErrorMessage = "Item has convert : " + _ph.DocNum;
                    }
                    else
                    {
                        (string errorMessageCVProd, ProductionOrderH productionOrderH) = await sapSDKService.ConvertProductionOrder(_ph);

                        if (errorMessageCVProd == "")
                        {

                            productionOrderH.UpdateBy = item.CreateBy;
                            productionOrderH.UpdateDate = System.DateTime.Now;


                            await generatePDandMCService.UpdatePD(productionOrderH);


                            item.DocNum = productionOrderH.DocNum;
                            item.DocEntry = productionOrderH.DocEntry;
                            item.ErrorMessage = "";
                            item.StartDate = _start;
                            item.EndDate = System.DateTime.Now;
                            item.Complete = 1;

                            AllocateLot al = new AllocateLot { Lot = _ph.Lot!};

                            var result = await generatePDandMCService.SearchPD(al);

                            if (result.Where(w => w.ConvertSap == 0).ToList().Count() == 0)
                            {
                                await allocateService.UpdateGeneratePD(al,"ConvertTOSAP",account);
                            }

                        }
                        else{
                            item.ErrorMessage = errorMessageCVProd;
                            item.StartDate = _start;
                            item.EndDate = System.DateTime.Now;
                        }

                    }

                    await queueCVSAPServices.Update(item);
                    itemH.Add(item.Adapt<QueueCVSAPResponse>());
                }

                return StatusCode((int)HttpStatusCode.Created, itemH);
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }
        }


        [HttpPost("[action]")]
        public async Task<ActionResult<QueueCVSAPResponse>> ReleaseProductionToSAP(List<QueueCVSAPResponse> model)
        {

            List<QueueCVSAPResponse> itemH = new List<QueueCVSAPResponse>();
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var account = accountService.GetInfo(accessToken);

            if (accessToken == null)
            {
                return Unauthorized();
            }

            try
            {

                foreach (QueueConvertToSap item in model.Adapt<List<QueueConvertToSap>>())
                {

                    var _ph = await generatePDandMCService.FindPDById(item.DocumentId!.Value);
                    DateTime _start = System.DateTime.Now;


                    if (_ph.Status == "R")
                    {
                        item.ErrorMessage = "Item has released : " + _ph.DocNum;
                    }
                    else
                    {
                        (string errorMessageCVProd, ProductionOrderH productionOrderH) = await sapSDKService.UpdateStatusdProductionOrder(_ph, "R");

                        if (errorMessageCVProd == "")
                        {

                            productionOrderH.UpdateBy = item.CreateBy;
                            productionOrderH.UpdateDate = System.DateTime.Now;


                            await generatePDandMCService.UpdatePD(productionOrderH);


                            item.DocNum = productionOrderH.DocNum;
                            item.DocEntry = productionOrderH.DocEntry;
                            item.ErrorMessage = "";
                            item.StartDate = _start;
                            item.EndDate = System.DateTime.Now;
                            item.Complete = 1;

                            AllocateLot al = new AllocateLot { Lot = _ph.Lot!};

                            var result = await generatePDandMCService.SearchPD(al);

                            if (result.Where(w => w.ConvertSap == 1 && w.Status == "P").ToList().Count() == 0)
                            {
                                await allocateService.UpdateGeneratePD(al,"ReleasedTOSAP",account);
                            }

                        }else{
                            item.ErrorMessage = errorMessageCVProd;
                            item.StartDate = _start;
                            item.EndDate = System.DateTime.Now;
                        }

                    }

                    await queueCVSAPServices.Update(item);
                    itemH.Add(item.Adapt<QueueCVSAPResponse>());
                }

                return StatusCode((int)HttpStatusCode.Created, itemH);
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }

        }


        [HttpPost("[action]")]
        public async Task<ActionResult<QueueCVSAPResponse>> CancelProductionToSAP(List<QueueCVSAPResponse> model)
        {
            List<QueueCVSAPResponse> itemH = new List<QueueCVSAPResponse>();
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var account = accountService.GetInfo(accessToken);

            if (accessToken == null)
            {
                return Unauthorized();
            }

            try
            {

                foreach (QueueConvertToSap item in model.Adapt<List<QueueConvertToSap>>())
                {

                    var _ph = await generatePDandMCService.FindPDById(item.DocumentId!.Value);
                    DateTime _start = System.DateTime.Now;


                    if (_ph.Status == "R")
                    {
                        item.ErrorMessage = "Item has released : " + _ph.DocNum;
                    }
                    else
                    {
                        (string errorMessageCVProd, ProductionOrderH productionOrderH) = await sapSDKService.UpdateStatusdProductionOrder(_ph, "C");

                        if (errorMessageCVProd == "")
                        {
                            productionOrderH.Status = "I";
                            productionOrderH.UpdateBy = item.CreateBy;
                            productionOrderH.UpdateDate = System.DateTime.Now;

                            await generatePDandMCService.UpdatePD(productionOrderH);


                            item.DocNum = productionOrderH.DocNum;
                            item.DocEntry = productionOrderH.DocEntry;
                            item.ErrorMessage = "";
                            item.StartDate = _start;
                            item.EndDate = System.DateTime.Now;
                            item.Complete = 1;


                            AllocateLot al = new AllocateLot { Lot = _ph.Lot!};

                            var result = await generatePDandMCService.SearchPD(al);

                            if (result.Where(w => w.ConvertSap == 1 && (w.Status == "P" || w.Status == "R")).ToList().Count() == 0)
                            {
                                await allocateService.UpdateGeneratePD(al, "CancelTOSAP", account);
                            }

                        }else{
                            item.ErrorMessage = errorMessageCVProd;
                            item.StartDate = _start;
                            item.EndDate = System.DateTime.Now;
                        }

                    }

                    await queueCVSAPServices.Update(item);
                    itemH.Add(item.Adapt<QueueCVSAPResponse>());
                }

                return StatusCode((int)HttpStatusCode.Created, itemH);
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }



        }



        [HttpPost("[action]")]
        public async Task<ActionResult<QueueCVSAPResponse>> convertGIToSAP(List<QueueCVSAPResponse> model)
        {
            // TODO: Your code here
            await Task.Yield();

            return null;
        }





    }
}