using System.Collections;
using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AddOn_API;
using Microsoft.AspNetCore.Mvc;
using AddOn_API.DTOs.SaleOrder;
using AddOn_API.Interfaces;
using Mapster;
using AddOn_API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using AddOn_API.DTOs.SAPQuery;

//using SaleOrder.Models;

namespace AddOn_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "IT")]

    public class SaleOrderController : ControllerBase
    {
        private readonly ISaleOrderService saleOrderService;
        private readonly IAccountService accountService;
        private readonly ISapSDKService sapSDKService;
        private readonly IUploadFileService uploadFileService;

        public SaleOrderController(ISaleOrderService saleOrderService, IAccountService accountService, ISapSDKService sapSDKService, IUploadFileService uploadFileService)
        {
            this.uploadFileService = uploadFileService;
            this.sapSDKService = sapSDKService;
            this.accountService = accountService;
            this.saleOrderService = saleOrderService;
        }

        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<SaleOrderResponse>>> FindAll()
        {
            List<SaleOrderResponse> itemH = new List<SaleOrderResponse>();
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                return Unauthorized();
            }

            var _result = (await saleOrderService.FindAll());

            if (_result == null)
            {
                return NotFound();
            }

            else
            {
                foreach (var item in _result)
                {
                    SaleOrderResponse _header = item.Adapt<SaleOrderResponse>();
                    List<SaleOrderDResponse> itemDetail = new List<SaleOrderDResponse>();

                    itemDetail = item.SaleOrderDs.Adapt<List<SaleOrderDResponse>>();
                    _header.SaleOrderD = itemDetail;

                    itemH.Add(_header);

                }



                return StatusCode((int)HttpStatusCode.OK, itemH);
            }


        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SaleOrderResponse>> FindById(long id)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            SaleOrderResponse itemH = new SaleOrderResponse();
            List<SaleOrderDResponse> itemDetail = new List<SaleOrderDResponse>();
            if (accessToken == null)
            {
                return Unauthorized();
            }

            var result = (await saleOrderService.FindById(id));

            if (result == null)
            {

                return NotFound();
            }
            else
            {

                itemH = result.Adapt<SaleOrderResponse>();
                itemDetail = result.SaleOrderDs.Adapt<List<SaleOrderDResponse>>();

                itemH.SaleOrderD = itemDetail;

            }
            return StatusCode((int)HttpStatusCode.OK, itemH);

        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<BusinessPartner>>> GetCustomer()
        {
           
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                return Unauthorized();
            }

            // TODO: Your code here
            var result = (await sapSDKService.GetBusinessPartner("Customer"));

            return StatusCode((int)HttpStatusCode.OK, result);

        }


        [HttpPost("[action]")]
        public async Task<ActionResult<List<SaleOrderResponse>>> Search([FromBody] SaleOrderSearch model)
        {
            List<SaleOrderResponse> itemH = new List<SaleOrderResponse>();



            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                return Unauthorized();
            }

            var result = (await saleOrderService.Search(model.Adapt<SaleOrderH>()));

            if (result == null)
            {

                return NotFound();
            }
            else
            {
                foreach (var item in result)
                {
                    SaleOrderResponse _header = item.Adapt<SaleOrderResponse>();
                    List<SaleOrderDResponse> itemDetail = new List<SaleOrderDResponse>();

                    itemDetail = item.SaleOrderDs.Adapt<List<SaleOrderDResponse>>();
                    _header.SaleOrderD = itemDetail;

                    itemH.Add(_header);

                }
            }
            return StatusCode((int)HttpStatusCode.OK, itemH);
        }


        [HttpPost("[action]")]
        public async Task<ActionResult<SaleOrderH>> AddSaleOrder([FromForm] SaleOrderRequest model)
        {

            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                return Unauthorized();
            }

            if (model.FormFiles == null){
                return StatusCode((int)HttpStatusCode.NotFound, "File Item List is not Found.");
            }

            var saleOrder = model.Adapt<SaleOrderH>();

            (string errorMessage, List<string> fileName) = await saleOrderService.UploadFile(model.FormFiles);
            if (!String.IsNullOrEmpty(errorMessage))
            {
                return StatusCode((int)HttpStatusCode.NotFound, "File Item List is not Found.");
            }

            //// get data from Excel File
            var strfileName = String.Join(",", fileName);

            var dataExcel = await saleOrderService.GetdatafromFile(strfileName);
            List<SaleOrderD> _saleOrderD = new List<SaleOrderD>();
            if (dataExcel == null)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "Item detail is not Found.");
            }
            else
            {
                _saleOrderD = dataExcel.Adapt<List<SaleOrderD>>();
            }

            //// get detail item from SAP
            var _detailitemIE = await sapSDKService.GetItemSale(_saleOrderD);


            //// check item with item sap
            (string errorMessagechkitem, string _chkitemsap) = await saleOrderService.CheckItemDetail(_saleOrderD, _detailitemIE.Adapt<List<DetailItem>>());

            if (!String.IsNullOrEmpty(errorMessagechkitem))
            {
                return StatusCode((int)HttpStatusCode.NotFound, $"{errorMessagechkitem} ({_chkitemsap})");
            }

            var account = accountService.GetInfo(accessToken);

            //// add data to Detail
            foreach (SaleOrderD item in _saleOrderD)
            {
                DetailItem _detail = _detailitemIE.FirstOrDefault(f => f.ItemCode == item.ItemCode);

                var _itemCode = item.ItemCode;
                decimal _itemSize = 0;
                if (item.ItemCode.Contains('_'))
                {
                    var _index = item.ItemCode.IndexOf('_');
                    _itemCode = item.ItemCode.Substring(0, _index);
                    //var aaa = item.ItemCodeS.Substring(_index+1,3);
                    _itemSize = Convert.ToDecimal(item.ItemCode.Substring(_index + 1, 3)) / 10;

                }
                item.Category = _detail.Category;
                item.Colors = _detail.Colors;
                item.Gender = _detail.Gender;
                item.Dscription = _detail.Style;
                item.Category = _detail.Category;
                item.Style = _detail.Style;
                item.ItemCode = _detail.ItemCode;
                item.ItemNo = _itemCode;
                item.LineStatus = "A";
                item.Buy = saleOrder.Buy;
                item.SizeNo = _itemSize;
                item.UomCode = _detail.UomCode;
                item.GenerateLot = 0;
                item.CreateBy = account.EmpUsername;
                item.CreateDate = DateTime.Now;

            }


            /// add data to Header
            saleOrder.SoNumber = await saleOrderService.GetSoNumber();
            saleOrder.SaleOrderDs = _saleOrderD;

            saleOrder.UploadFile = strfileName;

            
            saleOrder.CreateDate = DateTime.Now;
            saleOrder.CreateBy = account.EmpUsername;
            saleOrder.ConvertSap = 0;
            saleOrder.GenerateLot = 0;



            try
            {
                await saleOrderService.Create(saleOrder);
                return StatusCode((int)HttpStatusCode.Created, saleOrder.SoNumber);
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }

        }

        [HttpPut("{id}")]
        public async Task<ActionResult<SaleOrderResponse>> UpdateSaleorder(long id, [FromForm] SaleOrderRequest model)
        {

            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (accessToken == null)
            {
                return Unauthorized();
            }
            var saleOrderchk = await saleOrderService.FindById(id);

            if (saleOrderchk == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Id is not Found.");
            }
            if (saleOrderchk.DocStatus == "0")
            {
                return StatusCode((int)HttpStatusCode.BadRequest, $"Status is not Update. ({saleOrderchk.DocStatus})");
            }

            if (saleOrderchk.GenerateLot != 0)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, $"This Sale Order has Generate Lot");
            }


            var saleOrder = model.Adapt<SaleOrderH>();

            var strfileName = saleOrderchk.UploadFile;
            List<SaleOrderD> _saleOrderD = new List<SaleOrderD>();

            var account = accountService.GetInfo(accessToken);


            ///check file is not null
            if (model.FormFiles != null){
                 //// get data from Excel File
                (string errorMessage, List<string> fileName) = await saleOrderService.UploadFile(model.FormFiles);
                if (!String.IsNullOrEmpty(errorMessage))
                {
                    return StatusCode((int)HttpStatusCode.NotFound, "File Item List is not Found.");
                }
                strfileName = String.Join(",", fileName);
                 var dataExcel = await saleOrderService.GetdatafromFile(strfileName);
                 
                
                if (dataExcel == null)
                {
                    return StatusCode((int)HttpStatusCode.NotFound, "Item detail is not Found.");
                }
                else
                {
                    _saleOrderD = dataExcel.Adapt<List<SaleOrderD>>();
                }

                //// get detail item from SAP
                var _detailitemIE = await sapSDKService.GetItemSale(_saleOrderD);


                //// check item with item sap
                (string errorMessagechkitem, string _chkitemsap) = await saleOrderService.CheckItemDetail(_saleOrderD, _detailitemIE.Adapt<List<DetailItem>>());

                if (!String.IsNullOrEmpty(errorMessagechkitem))
                {
                    return StatusCode((int)HttpStatusCode.NotFound, errorMessagechkitem);
                }

                //// update data to Detail
                foreach (SaleOrderD item in _saleOrderD)
                {
                    DetailItem _detail = _detailitemIE.FirstOrDefault(f => f.ItemCode == item.ItemCode);

                    var _itemCode = item.ItemCode;
                    decimal _itemSize = 0;
                    if (item.ItemCode.Contains('_'))
                    {
                        var _index = item.ItemCode.IndexOf('_');
                        _itemCode = item.ItemCode.Substring(0, _index);
                        //var aaa = item.ItemCodeS.Substring(_index+1,3);
                        _itemSize = Convert.ToDecimal(item.ItemCode.Substring(_index + 1, 3)) / 10;

                    }
                    item.Sohid = saleOrderchk.Id;
                    item.Category = _detail.Category;
                    item.Colors = _detail.Colors;
                    item.Gender = _detail.Gender;
                    item.Dscription = _detail.Style;
                    item.Style = _detail.Style;
                    item.ItemCode = _detail.ItemCode;
                    item.ItemNo = _itemCode;
                    item.LineStatus = "A";
                    item.Buy = saleOrder.Buy;
                    item.SizeNo = _itemSize;
                    item.UomCode = _detail.UomCode;
                    item.GenerateLot = 0;
                    item.CreateBy = account.EmpUsername;
                    item.CreateDate = DateTime.Now;
                }
            }

            //update saleorder D
            if (_saleOrderD.Count() > 0){
                foreach(SaleOrderD item in saleOrderchk.SaleOrderDs){
                SaleOrderD _dataup = _saleOrderD.Where(w => w.ItemCode == item.ItemCode && w.ShipToCode == item.ShipToCode && w.Width == item.Width && w.PoNumber == item.PoNumber).FirstOrDefault();
                if (_dataup != null){
                    item.Quantity = _dataup.Quantity;
                    item.LineStatus = "A";
                    item.Updateby = account.EmpUsername;
                    item.UpdateDate = DateTime.Now;
                    item.Category = _dataup.Category;
                    item.Colors = _dataup.Colors;
                    item.Gender = _dataup.Gender;
                    item.Dscription = _dataup.Style;
                    item.Style = _dataup.Style;
                    item.ItemCode = _dataup.ItemCode;
                    item.ItemNo = _dataup.ItemNo;
                    item.Buy = _dataup.Buy;
                    item.SizeNo = _dataup.SizeNo;
                    item.UomCode = _dataup.UomCode;
                    
                    
                }else{
                    item.LineStatus = "I";
                    item.Updateby = account.EmpUsername;
                    item.UpdateDate = DateTime.Now;
                }
            }
            }
            
            List<SaleOrderD> _saleOrderDNew = new List<SaleOrderD>();
                foreach(SaleOrderD item in _saleOrderD){
                    SaleOrderD _dataup = saleOrderchk.SaleOrderDs.Where(w => w.ItemCode == item.ItemCode).FirstOrDefault();
                    if (_dataup == null){
                        _saleOrderDNew.Add(item);
                    }
                }
           
            ////update header
            saleOrderchk.CardCode = model.CardCode;
            saleOrderchk.CardName = model.CardName;
            saleOrderchk.Currency = model.Currency;
            saleOrderchk.DocStatus = model.DocStatus;
            saleOrderchk.Buy = model.Buy;
            saleOrderchk.SaleTypes = model.SaleTypes;
            saleOrderchk.DeliveryDate = model.DeliveryDate;
            saleOrderchk.Remark = model.Remark;
            saleOrderchk.UploadFile = strfileName;
            saleOrderchk.UpdateBy = account.EmpUsername;
            saleOrderchk.UpdateDate = DateTime.Now;
           
            

            try
            {

                if (saleOrderchk.ConvertSap == 1){
                   (string errorMessage,SaleOrderH saleorderHSAP ) =  await sapSDKService.UpdateSaleOrder(saleOrderchk,_saleOrderDNew);

                    if (errorMessage == ""){
                        await saleOrderService.Update(saleOrderchk,_saleOrderDNew);

                    }else{
                        return StatusCode((int)HttpStatusCode.BadRequest, "Can't Update into SAP (error : "+ errorMessage+")");
                    }

                }else{
                    await saleOrderService.Update(saleOrderchk,_saleOrderDNew);
                }

                
                return StatusCode((int)HttpStatusCode.OK, saleOrderchk.SoNumber);
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }
        }



        [HttpDelete("{id}")]
        public async Task<ActionResult<SaleOrderResponse>> DeleteDraftSaleorder(long id)
        {
            try
            {

                SaleOrderH _data = await saleOrderService.FindById(id);

                if (_data.DocStatus != "D")
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, _data.DocStatus);
                }

                await saleOrderService.DeleteDraftSaleorder(_data);



                // await saleOrderService.Update(saleOrder);
                return StatusCode((int)HttpStatusCode.OK, id);
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }
        }


        [HttpPost("[action]")]
        public async Task<ActionResult<SaleOrderResponse>> ConvertSaleOrderToSAP([FromBody] RequestCVSAP mdata)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (accessToken == null)
            {
                return Unauthorized();
            }

            SaleOrderH itemH = new SaleOrderH();
            List<SaleOrderD> itemDetail = new List<SaleOrderD>();

            SaleOrderH result = (await saleOrderService.FindById(mdata.id));

            string errorMessage = string.Empty;
            SaleOrderH saleOrderH = new SaleOrderH();


            if (result == null)
                return StatusCode((int)HttpStatusCode.NotFound, $"Can not Find Sale Order {mdata.id}");

            if (result.ConvertSap == 0){
                   if (result.DocStatus != "D")
                        return StatusCode((int)HttpStatusCode.BadRequest, "Can not Convert Sale Order");

                    if (result.DeliveryDate < System.DateTime.Now){
                        return StatusCode((int)HttpStatusCode.BadRequest, "Delivery Date is not match Current Date");
                    }
                    (string errorMessagecv, SaleOrderH saleOrderHcv) = await sapSDKService.ConvertSaleOrder(result);

                    errorMessage = errorMessagecv;
                    saleOrderH = saleOrderHcv;

            }
            else{
                    List<SaleOrderD> _saleOrderDNew = new List<SaleOrderD>();

                    (string errorMessageSAPupdate, SaleOrderH saleorderHSAP) = await sapSDKService.UpdateSaleOrder(result, _saleOrderDNew);

                     errorMessage = errorMessageSAPupdate;
                    saleOrderH = saleorderHSAP;
            }
            

            if (errorMessage != "")
            {
                return StatusCode((int)HttpStatusCode.BadRequest, errorMessage);
            }
            else
            {


                List<SaleOrderD> _saleOrderDNew = new List<SaleOrderD>();


                await saleOrderService.Update(saleOrderH,_saleOrderDNew);
                return StatusCode((int)HttpStatusCode.OK, $"Convert Sale Order Complete. {mdata.id}");
            }

        }


        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<SaleTypesDTO>>> GetSaleType()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (accessToken == null)
            {
                return Unauthorized();
            }

            List<SaleTypesDTO> SaletypeList = new List<SaleTypesDTO>();
            var result = (await saleOrderService.SaleType());

            if (result == null)
            {

                return NotFound();
            }
            else
            {
                SaletypeList = result.Adapt<List<SaleTypesDTO>>();
            }
            return StatusCode((int)HttpStatusCode.OK, SaletypeList);

        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<SaleTypesDTO>>> GetBuyMonth()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (accessToken == null)
            {
                return Unauthorized();
            }

            List<SaleTypesDTO> SaletypeList = new List<SaleTypesDTO>();

            var result = (await saleOrderService.BuyMonth());

            if (result == null)
            {

                return NotFound();
            }
            else
            {
                SaletypeList = result.Adapt<List<SaleTypesDTO>>();
            }
            return StatusCode((int)HttpStatusCode.OK, SaletypeList);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<SaleTypesDTO>>> GetTBuyYear()
        {
            // TODO: Your code here
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (accessToken == null)
            {
                return Unauthorized();
            }

            List<SaleTypesDTO> SaletypeList = new List<SaleTypesDTO>();

            var result = (await saleOrderService.BuyYear());

            if (result == null)
            {

                return NotFound();
            }
            else
            {
                SaletypeList = result.Adapt<List<SaleTypesDTO>>();
            }
            return StatusCode((int)HttpStatusCode.OK, SaletypeList);
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<DetailExcelFile>> GetSODetailFExcel([FromBody] List<DetailExcelFile> model)
        {

            List<SaleOrderD> _saleOrderD = new List<SaleOrderD>();

            _saleOrderD = model.Adapt<List<SaleOrderD>>();


            //// get detail item from SAP
            var _detailitemIE = await sapSDKService.GetItemSale(_saleOrderD);


            foreach (DetailExcelFile item in model)
            {
                DetailItem _detail = _detailitemIE.FirstOrDefault(f => f.ItemCode == item.ItemCode);



                if (_detail != null)
                {
                    item.Style = _detail.Style;
                    item.Colors = _detail.Colors;
                    item.Category = _detail.Category;
                    item.Gender = _detail.Gender;
                    item.Status = "A";


                }
                else
                {
                    item.Status = "D";
                }

            }



            return StatusCode((int)HttpStatusCode.OK, model);



        }


        [HttpGet("[action]")]
        public ActionResult DownloadSoDetailFile(string filename,string sonumber)
        {

            string path = "UploadFile/SaleOrder/";

            var memory = uploadFileService.DownloadFile(path,filename);

            return File(memory.ToArray(),"application/vnd.ms-excel",sonumber+".xlsx");
        }

        [HttpGet("[action]")]
        public ActionResult DownloadTemplateFile()
        {
            string filename = "TemplateSaleOrderD.xlsx";
             string path = "UploadFile/SaleOrder/";

            var memory = uploadFileService.DownloadFile(path,filename);

            return File(memory.ToArray(),"application/vnd.ms-excel",filename+".xlsx");
        }
        

        [HttpPost("[action]")]
        public async Task<ActionResult<SaleOrderResponse>> DeletoAllocateLot([FromBody] SaleOrderSearch model)
        {
              var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (accessToken == null)
            {
                return Unauthorized();
            }

            var account = accountService.GetInfo(accessToken);

            try{
            var saleOrderHS = await saleOrderService.Search(model.Adapt<SaleOrderH>());

            
            if (saleOrderHS.Count() == 0){
                 return StatusCode((int)HttpStatusCode.BadRequest, "Can't find Sale Order in system.");
            }

            var _saleOrder = saleOrderHS.First();


           (string errorMessage, SaleOrderH saleOrderH) = await saleOrderService.VerifyDataDeletoAllocateLot(_saleOrder);

           if (!string.IsNullOrEmpty(errorMessage)){
                return StatusCode((int)HttpStatusCode.BadRequest, errorMessage);
           }

            _saleOrder.UpdateBy = account.EmpUsername;
            _saleOrder.UpdateDate = System.DateTime.Now;
            _saleOrder.GenerateLot = 0;
            _saleOrder.GenerateLotBy = account.EmpUsername;

            await saleOrderService.DeletoAllocateLot(_saleOrder);


            return StatusCode((int)HttpStatusCode.Created, _saleOrder.Adapt<SaleOrderResponse>());

            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
            }
       
        }
        



        // [HttpPost("[action]")]
        // public async Task<ActionResult<IEnumerable<SaleOrderResponse>>> GetSaleOrder(SaleOrderRequest model)
        // {
        //     // TODO: Your code here
        //     await Task.Yield();

        //     var result = "";

        //     return StatusCode((int)HttpStatusCode.Created);
        // }

        // [HttpPost("[action]")]

        // public async ActionResult<SaleOrderResponse> AddSaleOrder([FromForm] SaleOrderRequest model)
        // {
        //     // TODO: Your code here

        //     (string errorMessage, List<string> fileName) = await saleOrderService.UploadFile(model.FormFiles);
        //     if (!String.IsNullOrEmpty(errorMessage)){
        //         return BadRequest();
        //     }

        //     var strfileName = String.Join(",",fileName);
        //     var saleOrder = model.Adapt<AosaleOrderH>();
        //     saleOrder.UploadFile = strfileName;

        //     var _result = await saleOrderService.Create(saleOrder);


        //     return StatusCode((int)HttpStatusCode.Created);
        // }




    }
}