using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AddOn_API.DTOs.AllocateLot;
using AddOn_API.Entities;
using AddOn_API.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using AddOn_API.DTOs.SAPQuery;
using AddOn_API.DTOs.MailService;
//using AddOn_API.Models;

namespace AddOn_API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class GenerateMCandPDController : ControllerBase
    {
        private readonly IAccountService accountService;
        private readonly IGeneratePDandMCService generatePDandMCService;
        private readonly IAllocateService allocateService;
        private readonly ISapSDKService sapSDKService;
        private readonly IQueueCVSAPServices queueCVSAPServices;
        private readonly IMailService mailService;
        private readonly ISaleOrderService saleOrderService;
        public GenerateMCandPDController(IAccountService accountService, IGeneratePDandMCService generatePDandMCService, IAllocateService allocateService, ISapSDKService sapSDKService, IQueueCVSAPServices queueCVSAPServices, IMailService mailService, ISaleOrderService saleOrderService)
        {
            this.saleOrderService = saleOrderService;
            this.mailService = mailService;
            this.queueCVSAPServices = queueCVSAPServices;
            this.sapSDKService = sapSDKService;
            this.allocateService = allocateService;
            this.generatePDandMCService = generatePDandMCService;
            this.accountService = accountService;
        }

        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<AllocateMcResponse>>> GetMaincardAll()
        {
            // TODO: Your code here
            await Task.Yield();

            return new List<AllocateMcResponse> { };
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<GenerateResponse>> GenerateMainCard([FromBody] List<AllocateLotRequest> model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            string access = (await accountService.CheckPermissionAccess(accessToken, "Allocate"));
            if (access == "Null")
                return Unauthorized();


            var account = accountService.GetInfo(accessToken);
            var allocateLot = model.Adapt<List<AllocateLot>>();
            List<GenerateResponse> generateResponses = new List<GenerateResponse>();

            List<AllocateLot> _lotAFVerify = new List<AllocateLot>();




            foreach (AllocateLot al in allocateLot)
            {

                (string errorMessage, string Lot) = await generatePDandMCService.VerifyDataMainCard(al);

                if (errorMessage != "")
                    generateResponses.Add(new GenerateResponse
                    {
                        errorMessage = errorMessage,
                        referenceNumber = Lot
                    });
                else
                {
                    var _lot = await allocateService.Search(al);

                    _lotAFVerify.Add(_lot.First());
                }
            }

            if (_lotAFVerify.Count() == 0)
            {

                //     string error = "";
                //    foreach(GenerateResponse err in generateResponses){
                //     error = err.ReferenceNumber + " : " + err.errorMessage + ",";
                //    }


                return StatusCode((int)HttpStatusCode.BadRequest, JsonSerializer.Serialize(generateResponses));
            }




          //  List<long> soidlist = await saleOrderService.GetDocEntryFLot(_lotAFVerify);

           
          //  var soidList = string.Join(",", soidlist.ToString());

            //AllocateController.csvar saleordersap = await sapSDKService.GetSaleOrder(soidlist);

            foreach (AllocateLot iteml in _lotAFVerify)
            {
                SaleOrderH _saleH = await saleOrderService.FindById(iteml.SaleOrderId);

                (string errorMessagechkso, string itemCode) = await generatePDandMCService.VerifyDataSOwithAllocate(iteml);

                if (errorMessagechkso != "")
                {
                    generateResponses.Add(new GenerateResponse
                    {
                        errorMessage = errorMessagechkso,
                        referenceNumber = iteml.Lot + "(" + itemCode + ")"
                    });
                }

            }
            if (generateResponses.Count > 0)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, JsonSerializer.Serialize(generateResponses));
            }

            try
            {
                foreach (AllocateLot al in _lotAFVerify)
                {

                    List<AllocateMc> _almc = new List<AllocateMc>();

                    _almc = (await generatePDandMCService.PreparedatafromLottoMC(al, account));

                    if (_almc.Count > 0)
                    {
                        await generatePDandMCService.CreateMC(_almc);


                        al.GenerateMc = 1;
                        al.GenerateMcby = account.EmpUsername;
                        al.UpdateBy = account.EmpUsername;
                        al.UpdateDate = System.DateTime.Now;

                        await allocateService.Update(al);
            


                        generateResponses.Add(new GenerateResponse
                        {
                            errorMessage = "Complete",
                            referenceNumber = al.Lot
                        });
                    }
                    else
                    {
                        generateResponses.Add(new GenerateResponse
                        {
                            errorMessage = "Can not Generate Maincard.",
                            referenceNumber = al.Lot
                        });
                    }

                }


                return StatusCode((int)HttpStatusCode.Created, generateResponses);





            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }

        }


        [HttpPost("[action]")]
        public async Task<ActionResult<GenerateResponse>> DeleteMainCard([FromBody] List<AllocateLotRequest> model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            string access = (await accountService.CheckPermissionAccess(accessToken, "Allocate"));
            if (access == "Null")
                return Unauthorized();


            var account = accountService.GetInfo(accessToken);
            var allocateLot = model.Adapt<List<AllocateLot>>();
            List<GenerateResponse> generateResponses = new List<GenerateResponse>();

            List<AllocateLot> _lotAFVerify = new List<AllocateLot>();

            foreach (AllocateLot al in allocateLot)
            {

                (string errorMessage, string Lot) = await generatePDandMCService.VerifyDataMainCardDel(al);

                if (errorMessage != "")
                    generateResponses.Add(new GenerateResponse
                    {
                        errorMessage = errorMessage,
                        referenceNumber = Lot
                    });
                else
                {
                    _lotAFVerify.Add(al);
                }
            }

            if (_lotAFVerify.Count() == 0)
            {
                //      string error = "";
                //    foreach(GenerateResponse err in generateResponses){
                //     error = err.ReferenceNumber + " : " + err.errorMessage + ",";
                //    }
                return StatusCode((int)HttpStatusCode.BadRequest, JsonSerializer.Serialize(generateResponses));
            }

            try
            {
                foreach (AllocateLot al in _lotAFVerify)
                {



                    var _almc = (await generatePDandMCService.SearchMC(al));

                    if (_almc != null)
                    {

                        List<AllocateMc> _datamc = new List<AllocateMc>();

                        foreach (AllocateMc item in _almc)
                        {
                            item.StatusMc = "0";
                            item.UpdateBy = account.EmpUsername;
                            item.UpdateDate = System.DateTime.Now;


                            _datamc.Add(item);
                        }



                        await generatePDandMCService.UpdateMC(_datamc);

                        var _datalot = (await allocateService.Search(al));

                        foreach (AllocateLot alu in _datalot)
                        {
                            alu.GenerateMc = 0;
                            alu.GenerateMcby = "";
                            alu.UpdateBy = account.EmpUsername;
                            alu.UpdateDate = System.DateTime.Now;

                            await allocateService.Update(alu);
                        }

                        generateResponses.Add(new GenerateResponse
                        {
                            errorMessage = "Complete",
                            referenceNumber = al.Lot
                        });

                    }

                }
                return StatusCode((int)HttpStatusCode.Created, generateResponses);

            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }
        }


        [HttpPost("[action]")]
        public async Task<ActionResult<AllocateMcResponse>> SearchMC(AllocateLotRequest model)
        {
            List<AllocateMcResponse> itemMC = new List<AllocateMcResponse>();

            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                return Unauthorized();
            }

            var result = (await generatePDandMCService.SearchMC(model.Adapt<AllocateLot>()));

            if (result == null)
            {
                return NotFound();
            }
            else
            {
                itemMC = result.Adapt<List<AllocateMcResponse>>();
                Int64 i = 0;
                foreach (AllocateMcResponse item in itemMC)
                {
                    item.Id = i;

                    i++;
                }

            }
            return StatusCode((int)HttpStatusCode.OK, itemMC);
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<ProductionHResponse>> SearchPD(AllocateLotRequest model)
        {
            List<ProductionHResponse> itemMC = new List<ProductionHResponse>();

            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                return Unauthorized();
            }


            List<AllocateCalSize> size = new List<AllocateCalSize>();
            var datasize = (await allocateService.FindLotSize(model.Adapt<AllocateLot>()));

            if (datasize != null)
            {
                size = datasize.Adapt<List<AllocateCalSize>>();
            }


            var result = (await generatePDandMCService.SearchPD(model.Adapt<AllocateLot>()));

            if (result == null)
            {
                return NotFound();
            }
            else
            {
                itemMC = result.Adapt<List<ProductionHResponse>>();
            }

            foreach (ProductionHResponse pdh in itemMC)
            {
                var sizename = size.Where(w => w.RowId == pdh.AllocateLotSizeId).FirstOrDefault();

                pdh.AllocateLotSizeName = sizename.ItemCode;

                // List<ProductionOrderDResponse> _pddetail = pdh.ProductionOrderDs.Adapt<List<ProductionOrderDResponse>>();
                // var i = 0;
                // foreach (var item in _pddetail){
                //     item.Id = i;
                //     i++;
                // }

                //pdh.ProductionOrderDs = _pddetail;
            }

            return StatusCode((int)HttpStatusCode.OK, itemMC);
        }



        [HttpPost("[action]")]
        public async Task<ActionResult<GenerateResponse>> DeleteProductionOrder([FromBody] List<AllocateLotRequest> model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            string access = (await accountService.CheckPermissionAccess(accessToken, "Allocate"));
            if (access == "Null")
                return Unauthorized();


            var account = accountService.GetInfo(accessToken);
            var allocateLot = model.Adapt<List<AllocateLot>>();
            List<GenerateResponse> generateResponses = new List<GenerateResponse>();

            List<AllocateLot> _lotAFVerify = new List<AllocateLot>();

            foreach (AllocateLot al in allocateLot)
            {

                (string errorMessage, string Lot) = await generatePDandMCService.VerifyDataProductionOrderDel(al);

                if (errorMessage != "")
                    generateResponses.Add(new GenerateResponse
                    {
                        errorMessage = errorMessage,
                        referenceNumber = Lot
                    });
                else
                {
                    _lotAFVerify.Add(al);
                }
            }

            if (_lotAFVerify.Count() == 0)
            {
                //     string error = "";
                //    foreach(GenerateResponse err in generateResponses){
                //     error = err.ReferenceNumber + " : " + err.errorMessage + ",";
                //    }
                return StatusCode((int)HttpStatusCode.BadRequest, JsonSerializer.Serialize(generateResponses));
            }

            try
            {
                foreach (AllocateLot al in _lotAFVerify)
                {



                    var _almc = (await generatePDandMCService.SearchPD(al));

                    if (_almc != null)
                    {

                        List<ProductionOrderH> _datamc = new List<ProductionOrderH>();

                        foreach (ProductionOrderH item in _almc)
                        {
                            item.Status = "0";
                            item.UpdateBy = account.EmpUsername;
                            item.UpdateDate = System.DateTime.Now;


                            _datamc.Add(item);
                        }


                        foreach (ProductionOrderH item in _datamc)
                        {
                            await generatePDandMCService.UpdatePD(item);
                        }


                        var _datalot = (await allocateService.Search(al));

                        foreach (AllocateLot alu in _datalot)
                        {
                            alu.GeneratePd = 0;
                            alu.GeneratePdby = "";
                            alu.UpdateBy = account.EmpUsername;
                            alu.UpdateDate = System.DateTime.Now;

                            await allocateService.Update(alu);
                        }

                        generateResponses.Add(new GenerateResponse
                        {
                            errorMessage = "Complete",
                            referenceNumber = al.Lot
                        });

                    }

                }
                return StatusCode((int)HttpStatusCode.Created, generateResponses);

            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<GenerateResponse>> GenerateProductionOrder([FromBody] List<AllocateLotRequest> model)
        {

            var accessToken = await HttpContext.GetTokenAsync("access_token");
            string access = (await accountService.CheckPermissionAccess(accessToken, "Allocate"));
            if (access == "Null")
                return Unauthorized();


            var account = accountService.GetInfo(accessToken);
            var allocateLot = model.Adapt<List<AllocateLot>>();

            List<GenerateResponse> generateResponses = new List<GenerateResponse>();

            List<AllocateLot> _lotAFVerify = new List<AllocateLot>();


            MailData _mailData = new MailData{
                EmailForm = "it.helpdesk@rofuth.com",
                        EmailTo = "theerayut.l@rofuth.com",
                        EmailCC = account.EmpEmail!,
                        EmailSubject = "",
                        EmailBody = "",
            };

             string body = string.Empty;
            foreach (AllocateLot al in allocateLot)
            {

                (string errorMessage, string Lot) = await generatePDandMCService.VerifyDataProductionOrder(al);


                if (errorMessage == "There are the department is empty value")
                {
                    body += al.Lot + " [" + Lot + "] Error : " + errorMessage  + "<br/>";
                    _mailData.EmailSubject = errorMessage;
                    errorMessage = "";
                }


                if (errorMessage != "")
                    generateResponses.Add(new GenerateResponse
                    {
                        errorMessage = errorMessage,
                        referenceNumber = al.Lot + "("+ Lot +")"
                    });
                else
                {
                     var _lot = await allocateService.Search(al);

                    _lotAFVerify.Add(_lot.First());
                }
            }


            if (_mailData.EmailSubject != ""){
                 _mailData.EmailBody = body;
                 mailService.SendMail(_mailData);

            }


            if (_lotAFVerify.Count() == 0)
            {
                //     string error = "";
                //    foreach(GenerateResponse err in generateResponses){
                //     error = err.ReferenceNumber + " : " + err.errorMessage + ",";
                //    }
                return StatusCode((int)HttpStatusCode.BadRequest, JsonSerializer.Serialize(generateResponses));
            }



            
          
          //  List<long> soidlist = await saleOrderService.GetDocEntryFLot(_lotAFVerify);

           
          //  var soidList = string.Join(",", soidlist.ToString());

            //AllocateController.csvar saleordersap = await sapSDKService.GetSaleOrder(soidlist);

            foreach (AllocateLot iteml in _lotAFVerify)
            {
                SaleOrderH _saleH = await saleOrderService.FindById(iteml.SaleOrderId);

                (string errorMessagechkso, string itemCode) = await generatePDandMCService.VerifyDataSOwithAllocate(iteml);

                if (errorMessagechkso != "")
                {
                    generateResponses.Add(new GenerateResponse
                    {
                        errorMessage = errorMessagechkso,
                        referenceNumber = iteml.Lot + "(" + itemCode + ")"
                    });
                }

            }
            if (generateResponses.Count > 0)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, JsonSerializer.Serialize(generateResponses));
            }

            try
            {
                foreach (AllocateLot al in _lotAFVerify)
                {

                    List<ProductionOrderH> _pdh = new List<ProductionOrderH>();





                    _pdh = (await generatePDandMCService.PreparedatafromLottoPD(al, account));

                    if (_pdh.Count > 0)
                    {

                        foreach (ProductionOrderH item in _pdh)
                        {
                            await generatePDandMCService.CreatePD(item);
                        }



                        var _datalot = (await allocateService.Search(al));

                        foreach (AllocateLot alu in _datalot)
                        {
                            alu.GeneratePd = 1;
                            alu.GeneratePdby = account.EmpUsername;
                            alu.UpdateBy = account.EmpUsername;
                            alu.UpdateDate = System.DateTime.Now;

                            await allocateService.Update(alu);
                        }






                        generateResponses.Add(new GenerateResponse
                        {
                            errorMessage = "Complete",
                            referenceNumber = al.Lot
                        });
                    }

                }
                return StatusCode((int)HttpStatusCode.Created, generateResponses);

            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }

        }


        [HttpPost("[action]")]
        public async Task<ActionResult<GenerateResponse>> ConvertProductionToSAP([FromBody] List<AllocateLotRequest> model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            string access = (await accountService.CheckPermissionAccess(accessToken, "Allocate"));
            if (access == "Null")
                return Unauthorized();


            var account = accountService.GetInfo(accessToken);

            List<GenerateResponse> generateResponses = new List<GenerateResponse>();
            var allocateLot = model.Adapt<List<AllocateLot>>();

            List<AllocateLot> _lotAFVerify = new List<AllocateLot>();

            foreach (AllocateLot al in allocateLot)
            {
                (string errorMessage, string Lot) = await generatePDandMCService.VerifyDataConvertPDtoSAP(al);

                if (errorMessage != "")
                    generateResponses.Add(new GenerateResponse
                    {
                        errorMessage = errorMessage,
                        referenceNumber = Lot
                    });
                else
                {

                    _lotAFVerify.Add(al);
                }
            }
            if (_lotAFVerify.Count() == 0)
            {
                //     string error = "";
                //    foreach(GenerateResponse err in generateResponses){
                //     error = err.ReferenceNumber + " : " + err.errorMessage + ",";
                //    }


                return StatusCode((int)HttpStatusCode.BadRequest, JsonSerializer.Serialize(generateResponses));
            }

            try
            {
                foreach (AllocateLot al in _lotAFVerify)
                {

                    List<GeneratePDResponse> generatePDerroe = new List<GeneratePDResponse>();

                    List<ProductionOrderH> _pdh = new List<ProductionOrderH>();

                    _pdh = (await generatePDandMCService.PreparedataConvertPDToSAP(al));

                    List<QueueConvertToSap> _queuePD = new List<QueueConvertToSap>();

                    if (_pdh.Count > 0)
                    {

                        foreach (ProductionOrderH ph in _pdh)
                        {

                            _queuePD.Add(new QueueConvertToSap
                            {
                                Id = 0,
                                TypeDc = "PD_P",
                                DocumentId = ph.Id,
                                DocEntry = null,
                                DocNum = null,
                                ErrorMessage = null,
                                Complete = 0,
                                CreateBy = account.EmpUsername,
                                CreateDate = System.DateTime.Now,
                                StartDate = null,
                                EndDate = null,
                                Status = 1,
                                Lot = al.Lot
                                

                            });

                            // (string errorMessageCVProd, ProductionOrderH productionOrderH) = await sapSDKService.ConvertProductionOrder(ph);

                            // if (errorMessageCVProd == "")
                            // {

                            //     productionOrderH.UpdateBy = account.EmpUsername;
                            //     productionOrderH.UpdateDate = System.DateTime.Now;


                            //     await generatePDandMCService.UpdatePD(productionOrderH);


                            // }

                            generatePDerroe.Add(new GeneratePDResponse
                            {
                                errorMessage = "Add Queue CV",
                                itemCode = ph.ItemCode,
                                id = ph.Id
                            });

                        }

                        await queueCVSAPServices.Create(_queuePD);

                        //var result = await generatePDandMCService.SearchPD(al);

                        // if (result.Where(w => w.ConvertSap == 0).Count() == 0)
                        // {
                        await allocateService.UpdateGeneratePD(al, "QueueConvertTOSAP", account);

                        generateResponses.Add(new GenerateResponse
                        {
                            errorMessage = "Complete",
                            referenceNumber = al.Lot,
                            generatePD = generatePDerroe
                        });
                        // }
                        // else
                        // {
                        //     generateResponses.Add(new GenerateResponse
                        //     {
                        //         errorMessage = "Error : Convert Production Order To SAP",
                        //         referenceNumber = al.Lot,
                        //         generatePD = generatePDerroe
                        //     });
                        // }
                    }
                    else
                    {
                        generateResponses.Add(new GenerateResponse
                        {
                            errorMessage = "Error : Can't Find Production Order for convert",
                            referenceNumber = al.Lot,
                            generatePD = generatePDerroe
                        });
                    }

                }
                return StatusCode((int)HttpStatusCode.Created, generateResponses);
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<GenerateResponse>> ReleaseProductionToSAP([FromBody] List<AllocateLotRequest> model)
        {
            // TODO: Your code here
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            string access = (await accountService.CheckPermissionAccess(accessToken, "Allocate"));
            if (access == "Null")
                return Unauthorized();


            var account = accountService.GetInfo(accessToken);

            List<GenerateResponse> generateResponses = new List<GenerateResponse>();
            var allocateLot = model.Adapt<List<AllocateLot>>();


            List<AllocateLot> _lotAFVerify = new List<AllocateLot>();

            foreach (AllocateLot al in allocateLot)
            {
                (string errorMessage, string Lot) = await generatePDandMCService.VerifyDataReleaseProductionToSAP(al);

                if (errorMessage != "")
                    generateResponses.Add(new GenerateResponse
                    {
                        errorMessage = errorMessage,
                        referenceNumber = Lot
                    });
                else
                {

                    _lotAFVerify.Add(al);
                }
            }
            if (_lotAFVerify.Count() == 0)
            {
                //     string error = "";
                //    foreach(GenerateResponse err in generateResponses){
                //     error = err.ReferenceNumber + " : " + err.errorMessage + ",";
                //    }
                return StatusCode((int)HttpStatusCode.BadRequest, JsonSerializer.Serialize(generateResponses));
            }


            try
            {
                foreach (AllocateLot al in _lotAFVerify)
                {

                    List<GeneratePDResponse> generatePDerroe = new List<GeneratePDResponse>();

                    List<ProductionOrderH> _pdh = new List<ProductionOrderH>();

                    _pdh = (await generatePDandMCService.PreparedataReleasePDToSAP(al));

                    List<QueueConvertToSap> _queuePD = new List<QueueConvertToSap>();

                    if (_pdh.Count > 0)
                    {

                        foreach (ProductionOrderH ph in _pdh)
                        {
                            _queuePD.Add(new QueueConvertToSap
                            {
                                Id = 0,
                                TypeDc = "PD_R",
                                DocumentId = ph.Id,
                                DocEntry = null,
                                DocNum = null,
                                ErrorMessage = null,
                                Complete = 0,
                                CreateBy = account.EmpUsername,
                                CreateDate = System.DateTime.Now,
                                StartDate = null,
                                EndDate = null,
                                Status = 1,
                                Lot = al.Lot

                            });

                            // (string errorMessageCVProd, ProductionOrderH productionOrderH) = await sapSDKService.UpdateStatusdProductionOrder(ph, "R");

                            // if (errorMessageCVProd == "")
                            // {

                            //     productionOrderH.UpdateBy = account.EmpUsername;
                            //     productionOrderH.UpdateDate = System.DateTime.Now;


                            //     await generatePDandMCService.UpdatePD(productionOrderH);
                            // }

                            generatePDerroe.Add(new GeneratePDResponse
                            {
                                errorMessage = "Add Queue Released",
                                itemCode = ph.ItemCode,
                                id = ph.Id
                            });

                        }

                        await queueCVSAPServices.Create(_queuePD);


                        // var result = await generatePDandMCService.SearchPD(al);

                        // if (result.Where(w => w.ConvertSap == 1 && w.Status == "P").Count() == 0)
                        // {
                        await allocateService.UpdateGeneratePD(al, "QueueReleasedTOSAP", account);

                        generateResponses.Add(new GenerateResponse
                        {
                            errorMessage = "Complete",
                            referenceNumber = al.Lot,
                            generatePD = generatePDerroe
                        });
                        // }
                        // else
                        // {
                        //     generateResponses.Add(new GenerateResponse
                        //     {
                        //         errorMessage = "Error : Released Production Order To SAP",
                        //         referenceNumber = al.Lot,
                        //         generatePD = generatePDerroe
                        //     });
                        // }
                    }
                    else
                    {
                        generateResponses.Add(new GenerateResponse
                        {
                            errorMessage = "Error : Can't Find Production Order for release",
                            referenceNumber = al.Lot,
                            generatePD = generatePDerroe
                        });
                    }

                }
                return StatusCode((int)HttpStatusCode.Created, generateResponses);
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }

        }

        [HttpPost("[action]")]
        public async Task<ActionResult<GenerateResponse>> CancelProductionToSAP([FromBody] List<AllocateLotRequest> model)
        {
            // TODO: Your code here
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            string access = (await accountService.CheckPermissionAccess(accessToken, "Allocate"));
            if (access == "Null")
                return Unauthorized();


            var account = accountService.GetInfo(accessToken);

            List<GenerateResponse> generateResponses = new List<GenerateResponse>();
            var allocateLot = model.Adapt<List<AllocateLot>>();


            List<AllocateLot> _lotAFVerify = new List<AllocateLot>();

            foreach (AllocateLot al in allocateLot)
            {
                (string errorMessage, string Lot) = await generatePDandMCService.VerifyDataCalcelProductionToSAP(al);

                if (errorMessage != "")
                    generateResponses.Add(new GenerateResponse
                    {
                        errorMessage = errorMessage,
                        referenceNumber = Lot
                    });
                else
                {

                    _lotAFVerify.Add(al);
                }
            }
            if (_lotAFVerify.Count() == 0)
            {
                //     string error = "";
                //    foreach(GenerateResponse err in generateResponses){
                //     error = err.ReferenceNumber + " : " + err.errorMessage + ",";
                //    }
                return StatusCode((int)HttpStatusCode.BadRequest, JsonSerializer.Serialize(generateResponses));
            }

            try
            {
                foreach (AllocateLot al in _lotAFVerify)
                {

                    List<GeneratePDResponse> generatePDerroe = new List<GeneratePDResponse>();

                    List<ProductionOrderH> _pdh = new List<ProductionOrderH>();

                    _pdh = (await generatePDandMCService.PreparedataCalcelPDToSAP(al));

                    List<QueueConvertToSap> _queuePD = new List<QueueConvertToSap>();

                    if (_pdh.Count > 0)
                    {

                        foreach (ProductionOrderH ph in _pdh)
                        {
                            _queuePD.Add(new QueueConvertToSap
                            {
                                Id = 0,
                                TypeDc = "PD_C",
                                DocumentId = ph.Id,
                                DocEntry = null,
                                DocNum = null,
                                ErrorMessage = null,
                                Complete = 0,
                                CreateBy = account.EmpUsername,
                                CreateDate = System.DateTime.Now,
                                StartDate = null,
                                EndDate = null,
                                Status = 1

                            });

                            // (string errorMessageCVProd, ProductionOrderH productionOrderH) = await sapSDKService.UpdateStatusdProductionOrder(ph, "C");

                            // if (errorMessageCVProd == "")
                            // {
                            //     productionOrderH.Status = "I";
                            //     productionOrderH.UpdateBy = account.EmpUsername;
                            //     productionOrderH.UpdateDate = System.DateTime.Now;


                            //     await generatePDandMCService.UpdatePD(productionOrderH);
                            // }

                            generatePDerroe.Add(new GeneratePDResponse
                            {
                                errorMessage = "Add Queue Cancel",
                                itemCode = ph.ItemCode,
                                id = ph.Id
                            });

                        }

                        // var result = await generatePDandMCService.SearchPD(al);

                        // if (result.Where(w => w.ConvertSap == 1 && (w.Status == "P" || w.Status == "R")).Count() == 0)
                        // {
                        await allocateService.UpdateGeneratePD(al, "QueueCancelTOSAP", account);

                        generateResponses.Add(new GenerateResponse
                        {
                            errorMessage = "Complete",
                            referenceNumber = al.Lot,
                            generatePD = generatePDerroe
                        });
                        // }
                        // else
                        // {
                        //     generateResponses.Add(new GenerateResponse
                        //     {
                        //         errorMessage = "Error : Cancel Production Order To SAP",
                        //         referenceNumber = al.Lot,
                        //         generatePD = generatePDerroe
                        //     });
                        // }
                    }
                    else
                    {
                        generateResponses.Add(new GenerateResponse
                        {
                            errorMessage = "Error : Can't Find Production Order for cancel",
                            referenceNumber = al.Lot,
                            generatePD = generatePDerroe
                        });
                    }

                }
                return StatusCode((int)HttpStatusCode.Created, generateResponses);
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }



        }

        // [HttpPost("[action]")]
        // public async Task<ActionResult<GenerateResponse>> ReleaseLotToProduction([FromBody] List<AllocateLotRequest> model)
        // {
        //     var accessToken = await HttpContext.GetTokenAsync("access_token");
        //     string access = (await accountService.CheckPermissionAccess(accessToken, "Allocate"));
        //     if (access == "Null")
        //         return Unauthorized();


        //     var account = accountService.GetInfo(accessToken);

        //     List<GenerateResponse> generateResponses = new List<GenerateResponse>();
        //     var allocateLot = model.Adapt<List<AllocateLot>>();


        //     List<AllocateLot> _lotAFVerify = new List<AllocateLot>();

        //     foreach (AllocateLot al in allocateLot)
        //     {
        //         (string errorMessage, string Lot) = await generatePDandMCService.VerifyDataReleasedToPD(al);

        //         if (errorMessage != "")
        //             generateResponses.Add(new GenerateResponse
        //             {
        //                 errorMessage = errorMessage,
        //                 referenceNumber = Lot
        //             });
        //         else
        //         {

        //             _lotAFVerify.Add(al);
        //         }
        //     }
        //     if (_lotAFVerify.Count() == 0)
        //     {
        //         //     string error = "";
        //         //    foreach(GenerateResponse err in generateResponses){
        //         //     error = err.ReferenceNumber + " : " + err.errorMessage + ",";
        //         //    }
        //         return StatusCode((int)HttpStatusCode.BadRequest, JsonSerializer.Serialize(generateResponses));
        //     }


        //     try
        //     {
        //         foreach (AllocateLot al in _lotAFVerify)
        //         {
        //             var _allocateLot = await allocateService.Search(al);

        //             foreach (AllocateLot item in _allocateLot)
        //             {

        //                 item.StatusProduction = "Released";
        //                 item.UpdateBy = account.EmpUsername;
        //                 item.UpdateDate = System.DateTime.Now;

        //                 await allocateService.Update(item);

        //                 generateResponses.Add(new GenerateResponse
        //                 {
        //                     errorMessage = "Complete",
        //                     referenceNumber = item.Lot

        //                 });
        //             }

        //         }
        //         return StatusCode((int)HttpStatusCode.Created, generateResponses);
        //     }
        //     catch (Exception ex)
        //     {

        //         return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
        //     }

        // }


        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<GCSProdtype>>> InsertProdType()
        {
            List<GCSProdtype> _data = new List<GCSProdtype>();


            (string errorMessage, IEnumerable<GCSProdtype> listdata) = await sapSDKService.InsertGCSProdType(_data);


            return StatusCode((int)HttpStatusCode.Created, errorMessage);

            // TODO: Your code here

        }



    }
}