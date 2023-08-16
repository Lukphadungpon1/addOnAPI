using System;
using System.Security.Cryptography;
using System.Linq;
using System.Data;
using AddOn_API.Data;
using AddOn_API.DTOs.SaleOrder;
using AddOn_API.Entities;
using AddOn_API.Interfaces;
using Microsoft.EntityFrameworkCore;
using ExcelDataReader;


namespace AddOn_API.Services
{
    public class SaleOrderService : ISaleOrderService
    {
        private readonly DatabaseContext databaseContext;
        private readonly IUploadFileService uploadFileService;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly ISapSDKService sapSDKService;
        public SaleOrderService(DatabaseContext databaseContext, IUploadFileService uploadFileService, IWebHostEnvironment webHostEnvironment, ISapSDKService sapSDKService)
        {
            this.sapSDKService = sapSDKService;
            this.webHostEnvironment = webHostEnvironment;
            this.uploadFileService = uploadFileService;
            this.databaseContext = databaseContext;
        }

        public async Task Create(SaleOrderH aosaleOrderH)
        {
            using var transaction = databaseContext.Database.BeginTransaction();
            try{
                databaseContext.SaleOrderHs.Add(aosaleOrderH);
                await databaseContext.SaveChangesAsync();

                transaction.Commit();

            }catch(Exception ex){
                transaction.Rollback();
                throw ex;
            }
        }
        public async Task Update(SaleOrderH aosaleOrderH,List<SaleOrderD> saleOrderDNew)
        {
   

            using var transaction = databaseContext.Database.BeginTransaction();
            try{
               
                // var itemcodel = _itemd.Where(w => w.Id == aosaleOrderH.Id).Select(s => s.ItemCode).ToList();
                // var _detail = aosaleOrderH.SaleOrderDs.Where(w => !itemcodel.Contains(w.ItemCode) ).ToList();

                // var _header = databaseContext.SaleOrderHs.Where(w => w.Id == aosaleOrderH.Id).FirstOrDefault();

                // _header.CardCode = aosaleOrderH.CardCode;
                // _header.CardName = aosaleOrderH.CardName;
                // _header.Currency = aosaleOrderH.Currency;
                // _header.Remark = aosaleOrderH.Remark;
                // _header.Buy = aosaleOrderH.Buy;
                // _header.UpdateBy = aosaleOrderH.UpdateBy;
                // _header.UpdateDate = System.DateTime.Now;
                // _header.UploadFile = aosaleOrderH.UploadFile;
                // _header.DeliveryDate = aosaleOrderH.DeliveryDate;
                // _header.DocNum = aosaleOrderH.DocNum;
                // _header.DocStatus = aosaleOrderH.DocStatus;
                // _header.ConvertSap = aosaleOrderH.ConvertSap;
                // _header.DocEntry = aosaleOrderH.DocEntry;

                


                databaseContext.SaleOrderDs.UpdateRange(aosaleOrderH.SaleOrderDs);
                databaseContext.SaleOrderHs.Update(aosaleOrderH);





                await databaseContext.SaveChangesAsync();

                if (saleOrderDNew.Count > 0)  
                    databaseContext.SaleOrderDs.AddRange(saleOrderDNew);

                await databaseContext.SaveChangesAsync();
                transaction.Commit();
            }catch(Exception ex){
                transaction.Rollback();
                throw ex;
            }
        }


        public async Task DeleteDraftSaleorder(SaleOrderH aosaleOrderH)
        {
            using var transaction = databaseContext.Database.BeginTransaction();
            try{
                List<SaleOrderD> _itemd = databaseContext.SaleOrderDs.Where(w => w.Id == aosaleOrderH.Id).ToList();

                var _header = databaseContext.SaleOrderHs.Where(w => w.Id == aosaleOrderH.Id).FirstOrDefault();
                databaseContext.SaleOrderDs.RemoveRange(_itemd);

                databaseContext.SaleOrderHs.Remove(_header);
                await databaseContext.SaveChangesAsync();
                
                transaction.Commit();
            }catch(Exception ex){
                transaction.Rollback();
                throw ex;
            }
        }

        public async Task<IEnumerable<SaleOrderH>> FindAll()
        {
            return await databaseContext.SaleOrderHs.Include(d => d.SaleOrderDs.Where( w => w.LineStatus != "CN"))
            .OrderByDescending(d => d.SoNumber)
            .ToListAsync();
        }

        public async Task<SaleOrderH> FindById(long id)
        {
            return await databaseContext.SaleOrderHs.Include(d => d.SaleOrderDs.Where( w => w.LineStatus == "A"))
            .FirstOrDefaultAsync(p => p.Id == id);

        }

        public async Task<IEnumerable<SaleOrderH>> Search(SaleOrderH aosaleOrderH)
        {

            var aaa = string.IsNullOrEmpty(aosaleOrderH.DocNum) ? "_" : aosaleOrderH.DocNum;

            return await databaseContext.SaleOrderHs.Include(d => d.SaleOrderDs.Where( w => w.LineStatus == "A"))
            .Where(p => ( string.IsNullOrEmpty(aosaleOrderH.Buy) ? 1==1 : p.Buy == aosaleOrderH.Buy) &&
                        ( string.IsNullOrEmpty(aosaleOrderH.SoNumber) ? 1==1 : p.SoNumber.ToLower() == aosaleOrderH.SoNumber.ToLower()) &&
                        ( string.IsNullOrEmpty(aosaleOrderH.DocNum) ? 1== 1 : p.DocNum.ToLower() == aosaleOrderH.DocNum.ToLower() ) &&
                        ( string.IsNullOrEmpty(aosaleOrderH.CardCode) ? 1==1 : p.CardCode == aosaleOrderH.CardCode) &&
                        ( string.IsNullOrEmpty(aosaleOrderH.CardName) ? 1==1 : p.CardName.Contains(aosaleOrderH.CardName))
                        && p.DocStatus != "CN"
                    )
            .ToListAsync();

        }


        public async Task<(string errorMessage, List<string> fileName)> UploadFile(List<IFormFile> formFiles)
        {
            string path = "UploadFile/SaleOrder/";

            string errorMessage = string.Empty;
            List<string> fileName = new List<string>();
            if (uploadFileService.IsUpload(formFiles))
            {
                errorMessage = uploadFileService.Validation(formFiles);
                if (string.IsNullOrEmpty(errorMessage))
                {
                    fileName = await uploadFileService.UploadFile(formFiles, path);
                }
            }
            return (errorMessage, fileName);
        }

        public async Task<IEnumerable<ReadExcelFile>> GetdatafromFile(string fileName)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            string path = "UploadFile/SaleOrder/";

            var dataExcel = new List<ReadExcelFile>();

            var fullpart = $"{webHostEnvironment.WebRootPath}/{path}/{fileName}";

            using (var stream = System.IO.File.Open(fullpart, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var crow = 0;
                    while (reader.Read()) //Each row of the file
                    {

                        var _itemNo = reader.IsDBNull(0) ? string.Empty : reader.GetValue(0).ToString();
                        var _Quanlity = reader.IsDBNull(1) ? string.Empty : reader.GetValue(1).ToString();
                        var _ShipToCode = reader.IsDBNull(2) ? string.Empty : reader.GetValue(2).ToString();
                        var _ShitToName = reader.IsDBNull(3) ? string.Empty : reader.GetValue(3).ToString();
                        var _PoNumber = reader.IsDBNull(4) ? string.Empty : reader.GetValue(4).ToString();
                        var _Width = reader.IsDBNull(5) ? string.Empty : reader.GetValue(5).ToString();

                        if (!string.IsNullOrEmpty(_itemNo) && crow != 0)
                        {
                            dataExcel.Add(new ReadExcelFile
                            {
                                LineNum = crow,
                                ItemCode = _itemNo.ToString(),
                                Quantity = Convert.ToDecimal(_Quanlity),
                                ShipToCode = _ShipToCode.ToString(),
                                ShipToDesc = _ShitToName.ToString(),
                                PoNumber = _PoNumber.ToString(),
                                Width = _Width.ToString()
                            });
                        }
                        crow++;

                    }
                }
            }

            return dataExcel;
        }

        public async Task<string> GetSoNumber()
        {
            var soNumber = "";

            soNumber = await databaseContext.SaleOrderHs.MaxAsync(m => m.SoNumber);
            if (string.IsNullOrEmpty(soNumber))
            {
                var month =  DateTime.Today.Month.ToString();

                if (month.Length == 1)
                    month = "0"+ month;

                var _default = DateTime.Today.Year.ToString() + month + "001";
                soNumber = $"SO{_default}";
            }
            else
            {
                var splitSO = Convert.ToInt32(soNumber.Substring(8, 3).ToString()) + 1;
                var runspllitSO = "000" + splitSO.ToString();

                soNumber = soNumber.Substring(0, 8) + runspllitSO.Substring(runspllitSO.Length - 3, 3);
            }


            return soNumber;
        }

        public async Task<(string errorMessage, string saleOrderDs)> CheckItemDetail(List<SaleOrderD> saleOrderD, List<DetailItem> detailItem)
        {

            string errorMessage = string.Empty;
            string _result = string.Empty;

            int aaa = saleOrderD.Count();
            int bbb = detailItem.Count();

            var itemarr = detailItem.Select(x => x.ItemCode).ToList();
        
            List<string> _query = saleOrderD.Where(s => !itemarr.Contains(s.ItemCode))
                                .Select( s=> s.ItemCode).ToList();

            // var arr = saleOrderD.Select( s=> s.ItemCode).ToList();

           // var aaaa = detailItem.Where( s=> !arr.Contains( s.ItemCode)).ToList();
            var datadup = saleOrderD.GroupBy( g=> new {g.ItemCode,g.ShipToCode,g.PoNumber,g.Width}).Select(s => new {ItemCode = s.Key.ItemCode,ShipToCode = s.Key.ShipToCode,PoNumber = s.Key.PoNumber,Width = s.Key.Width, cnt = s.Count() }).ToList();

            List<string> chkdup = new List<string>();
            if (datadup.Count() > 0 ){
                chkdup = datadup.Where(s => s.cnt > 1 ).Select( s => s.ItemCode ).ToList();
            }
             

            if ( chkdup.Count() > 0 ){
              
                _result = string.Join(",",chkdup);
            }

            if (_query.Count() > 0)
                _result = string.Join(",",_query);



            if (_query.Count > 0)
            {
                errorMessage = "item is not Active.";
                _result = _result.Substring(0, _result.Length - 1);
            }

            if (chkdup.Count > 0){
                errorMessage = "item is duplicate.";
                 _result = _result.Substring(0, _result.Length - 1);
            }


            return (errorMessage, _result);
        }

        public async Task<IEnumerable<SaleType>> SaleType()
        {
            List<SaleType> _query = await databaseContext.SaleTypes.Where(w=> w.Status == 1).ToListAsync();

            return _query;
        }

        public async Task<IEnumerable<BuyMonthMaster>> BuyMonth()
        {
           
            List<BuyMonthMaster> _query = await databaseContext.BuyMonthMasters.Where(w=> w.Status == 1).ToListAsync();

            return _query;
        }

        public async Task<IEnumerable<BuyYearMaster>> BuyYear()
        {
             List<BuyYearMaster> _query = await databaseContext.BuyYearMasters.Where(w=> w.Status == 1).ToListAsync();

            return _query;
        }

        public async Task<List<long>> GetDocEntryFLot(List<AllocateLot> allocateLots)
        {
            List<long> _docentry = new List<long>();

          
            var _so = await databaseContext.SaleOrderHs.Where( w=>  allocateLots.Select(s => s.SaleOrderId).ToList().Contains(w.Id) && w.DocStatus == "A" ).ToListAsync();

            _docentry = _so.Select(s=> Convert.ToInt64(s.DocEntry)).ToList();

            return _docentry;
        }

        public async Task<(string errorMessage, SaleOrderH saleOrderH)> VerifyDataDeletoAllocateLot(SaleOrderH saleOrderH)
        {
            string errorMessage = string.Empty;

            var chk = await databaseContext.SaleOrderHs.Where(w=> w.SoNumber == saleOrderH.SoNumber).FirstOrDefaultAsync();

            if (chk == null){
                errorMessage = "Sale Order is not Found.";
                return (errorMessage,saleOrderH);
            }
            
            if (chk.GenerateLot == 0){
                errorMessage = "Sale Order can't generate Lot.";
                 return (errorMessage,saleOrderH);
            }

            var chkallocatelot = await databaseContext.AllocateLots.Where(w=> w.SoNumber == saleOrderH.SoNumber && w.Status == "A" && (w.GenerateMc != 0 || w.GeneratePd != 0)).ToListAsync();

            if (chkallocatelot.Count > 0){
                errorMessage = "Lot has generate MC or PD.";
                return (errorMessage,saleOrderH);
            }

            return(errorMessage,saleOrderH);

        }

        public async Task DeletoAllocateLot(SaleOrderH saleOrderH)
        {
            using var transaction = databaseContext.Database.BeginTransaction();
            try{
               
               var _lotsize = await databaseContext.AllocateLotSizes.Where( w=> w.SaleOrderId == saleOrderH.Id).ToListAsync();
               var _lot = await databaseContext.AllocateLots.Where(w=> w.SaleOrderId == saleOrderH.Id).ToListAsync();

                foreach(AllocateLotSize its in _lotsize){
                    its.UpdateBy = saleOrderH.UpdateBy;
                    its.UpdateDate = System.DateTime.Now;
                    its.Status = "I";
                }

                foreach(AllocateLot itl in _lot){
                     itl.UpdateBy = saleOrderH.UpdateBy;
                    itl.UpdateDate = System.DateTime.Now;
                    itl.Status = "I";
                }

                databaseContext.AllocateLotSizes.UpdateRange(_lotsize);
                databaseContext.AllocateLots.UpdateRange(_lot);

                databaseContext.SaleOrderHs.Update(saleOrderH);

                await databaseContext.SaveChangesAsync();



                transaction.Commit();
            }catch(Exception ex){
                transaction.Rollback();
                throw ex;
            }
        }
    }
}