using System.Drawing;
using System.Resources;
using System.Data;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.ComponentModel;
using System.Net.Http.Headers;
using AddOn_API.Data;
using AddOn_API.DTOs.StoreProceduce;
using AddOn_API.Entities;
using AddOn_API.Interfaces;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static AddOn_API.Installers.JWTInstaller;
using AddOn_API.DTOs.AllocateLot;
using ExcelDataReader;

namespace AddOn_API.Services
{
    public class AllocateService : IAllocateService
    {
        private readonly DatabaseContext databaseContext;
        private readonly JwtSettings jwtSetting;
        private readonly IUploadFileService uploadFileService;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly DbStoreProceduce dbStoreProceduce;

        public AllocateService(DatabaseContext databaseContext, DbStoreProceduce dbStoreProceduce, JwtSettings jwtSetting, IUploadFileService uploadFileService,IWebHostEnvironment webHostEnvironment )
        {
            this.dbStoreProceduce = dbStoreProceduce;
            this.jwtSetting = jwtSetting;
            this.uploadFileService = uploadFileService;
            this.webHostEnvironment = webHostEnvironment;
            this.databaseContext = databaseContext;
        }

        public async Task<(string errorMessage, string SoNumber)> VerifyData(AllocateLot AllocateLot)
        {
            string errorMessage = string.Empty;
            string SoNumber = string.Empty;

            var saleOrder = await databaseContext.SaleOrderHs.Where(w => (string.IsNullOrEmpty(AllocateLot.Buy) ? 1 == 1 : w.Buy == AllocateLot.Buy) 
                                                                    && (string.IsNullOrEmpty(AllocateLot.SoNumber) ? 1 == 1 : w.SoNumber == AllocateLot.SoNumber)).ToListAsync();

            if (saleOrder == null)
            {
                errorMessage = "SaleOrder is not Found";
                SoNumber = AllocateLot.SoNumber;
            }
            else
            {
                if (saleOrder.Where(w => w.GenerateLot == 0).Count() == 0)
                {
                    errorMessage = "SaleOrder has generated Lot";

                    List<string> list = saleOrder.Where(w => w.GenerateLot == 1).Select(s => s.SoNumber).ToList();
                    var result = string.Join(",", list);
                    SoNumber = result;
                }
            }
            return (errorMessage, SoNumber);

        }

        public async Task Create(AllocateLot allocateLot)
        {
            using var transaction = databaseContext.Database.BeginTransaction();
            try
            {

                string Buy = string.IsNullOrEmpty(allocateLot.Buy) ? string.Empty : allocateLot.Buy;
                string SoNumber = string.IsNullOrEmpty(allocateLot.SoNumber) ? string.Empty : allocateLot.SoNumber;
                string CreateBy = string.IsNullOrEmpty(allocateLot.CreateBy) ? string.Empty : allocateLot.CreateBy;

                List<spGenerateLot> _generateLotTMP = (await dbStoreProceduce.spGenerateLot.FromSqlRaw($"exec [ao].[sp_GenerateLot] '{Buy}','{SoNumber}','{CreateBy}'").ToListAsync());

                List<AllocateLot> _resultallocateLot = new List<AllocateLot>();

                if (_generateLotTMP != null)
                {
                    foreach (spGenerateLot data in _generateLotTMP)
                    {
                        AllocateLot _data = data.Adapt<AllocateLot>();


                        var detsize = await databaseContext.AllocateCalSizes.Where(w => w.Lot == _data.Lot)
                        .GroupBy( g => new {g.SalesOrderEntry,g.Width,g.ShipToCode,g.SizeNo,g.PurOrder,g.ItemCode,g.Types,g.Total,g.Lot})
                        .Select( s => new {s.Key.SalesOrderEntry,s.Key.Width,s.Key.ShipToCode,s.Key.SizeNo,s.Key.PurOrder,s.Key.ItemCode,s.Key.Types,s.Key.Total,s.Key.Lot, Qty = s.Sum(ss=>ss.Qty) }).ToListAsync();

                        List<SaleOrderD> detorder = await databaseContext.SaleOrderDs.Where(w => w.Sohid == _data.SaleOrderId && w.LineStatus == "A").ToListAsync();

                        

                        var querysize = detsize
                                        .GroupJoin(detorder,
                                        s => new { p1 = s.ItemCode, p2 = s.SalesOrderEntry.ToString(), p3 = s.Width.ToString(), p4 = s.ShipToCode.ToString(), p5 = Convert.ToDecimal(s.SizeNo).ToString(), p6 = s.PurOrder.ToString() },
                                        d => new { p1 = d.ItemCode, p2 = d.Sohid.ToString(), p3 = d.Width.ToString(), p4 = d.ShipToCode.ToString(), p5 = d.SizeNo.Value.ToString(), p6 = d.PoNumber.ToString() },
                                        (s, d) => new { size = s, orderd = d })
                                        .SelectMany(x => x.orderd.DefaultIfEmpty(),
                                            (size, orderd) => new { size.size, orderd = orderd })
                                        .Where(w => w.size.Lot == _data.Lot)
                                        .Select(s => new AllocateLotSize
                                        {
                                           
                                            Lot = s.size.Lot,
                                            AllocateLotId = 0,
                                            ItemCode = s.orderd!.ItemCode,
                                            SaleOrderId = s.size.SalesOrderEntry!.Value,
                                            SaleOrderLineNum = s.orderd.LineNum,
                                            Type = s.size.Types,
                                            SizeNo = s.size.SizeNo,
                                            Qty = s.size.Qty!.Value,
                                            Status = "A",
                                            Receives = 0,
                                            CreateBy = _data.CreateBy,
                                            CreateDate = System.DateTime.Now
                                        }).OrderBy(o => Convert.ToDecimal(o.SizeNo) ).ToList();


                       


                        _data.AllocateLotSizes = querysize;
                        _data.Status = "A";
                        _data.GenerateMc = 0;
                        _data.GeneratePd = 0;

                        databaseContext.AllocateLots.Add(_data);


                    }

                    List<string> getsofromqry = _generateLotTMP.Select(s=> s.SoNumber).Distinct().ToList();


                    var saleOrderH = await databaseContext.SaleOrderHs
                                    .Where(w => getsofromqry.Contains(w.SoNumber))
                                    .ToListAsync();

                    foreach (SaleOrderH saleOrder in saleOrderH)
                    {
                        saleOrder.GenerateLot = 1;
                        saleOrder.GenerateLotBy = CreateBy;

                        databaseContext.SaleOrderHs.Update(saleOrder);
                    }

                    await databaseContext.SaveChangesAsync();


                }
                //await databaseContext.SaveChangesAsync();

                

                transaction.Commit();

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }

        }

          public async Task<(string errorMessage, string Lot)> VerifyDataLot(AllocateLot allocateLot)
        {
             string errorMessage = string.Empty;
             string Lot = string.Empty;



             var chklot = await databaseContext.AllocateLots.Where(w => w.Lot == allocateLot.Lot && w.Status == "A" && w.GenerateMc == 0 && w.GeneratePd == 0 &&  w.StatusIssueMat == "0" && w.StatusPlanning == "0" && w.StatusProduction == "0" && w.StatusReceiveFg == "0" && w.StatusReceiveMat == "0").FirstOrDefaultAsync();

            if ( chklot != null)
            {
                errorMessage = ("Lot has generated (Gen MC, Gen PD,Issuemat,Planning,Production,ReceiveFG,ReceiveMat) ");
                Lot = chklot.Lot;
            }

            var chksaleorder = await databaseContext.SaleOrderHs.Where(w => w.SoNumber == allocateLot.SoNumber && w.ConvertSap == 1).FirstOrDefaultAsync();
            // if ( chksaleorder != null)
            // {
            //     errorMessage = ("SaleOrder has convert to SAP ("+chksaleorder.SoNumber+")... ");
            //     Lot = chksaleorder.SoNumber;
            // }

            var chkqty = allocateLot.AllocateLotSizes.Sum( s=> s.Qty);
            if (chkqty > 504 ){
                errorMessage = ("Quantity over 504 /Lot..");
                Lot = chkqty.ToString();
            }

             if (chkqty == 0){
                errorMessage = ("Quantity is zero..");
                Lot = chkqty.ToString();
            }


            return (errorMessage, Lot);
        }



        public async Task UpdateGenerateLot(AllocateLot allocateLot,IEnumerable<SaleOrderD> saleOrderDs)
        {   
           
            //update allocatelot
            //update allocatelotsize
             //update saleorderD

            using var transaction = databaseContext.Database.BeginTransaction();
            try{

                databaseContext.AllocateLots.Update(allocateLot);

                //databaseContext.SaleOrderDs.UpdateRange(saleOrderDs);

                await databaseContext.SaveChangesAsync();

                transaction.Commit();

            }catch(Exception ex){
                transaction.Rollback();
                throw ex;
            }
            

        }
        public async Task<IEnumerable<AllocateLot>> FindAll()
        {
            return await databaseContext.AllocateLots
                        .Include(s => s.AllocateLotSizes).Where( w=> w.Status == "A")
                        .Include(s => s.AllocateMcs.Where( w=> w.StatusMc == "1"))
                        .Where(w=> w.Status == "A")
                        .OrderByDescending(w => w.Buy).ToListAsync();
        }

        public async Task<AllocateLot> FindById(long id)
        {
            return await databaseContext.AllocateLots
                        .Include(s => s.AllocateLotSizes).Where( w=> w.Status == "A")
                        .Include(s => s.AllocateMcs.Where( w=> w.StatusMc == "1"))
                        .Where(w => w.Id == id && w.Status != "I")
                        .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AllocateLot>> Search(AllocateLot allocateLot)
        {

            return await databaseContext.AllocateLots
                        .Include(s => s.AllocateLotSizes).Where( w=> w.Status == "A")
                        // .Include(s => s.AllocateMcs.Where( w=> w.StatusMc == "1"))
                        .Where(w => (string.IsNullOrEmpty(allocateLot.Buy) ? 1 == 1 : w.Buy == allocateLot.Buy)
                                        && (string.IsNullOrEmpty(allocateLot.SoNumber) ? 1 == 1 : w.SoNumber == allocateLot.SoNumber)
                                        && (string.IsNullOrEmpty(allocateLot.Lot) ? 1 == 1 : w.Lot == allocateLot.Lot)
                                        && (string.IsNullOrEmpty(allocateLot.PurOrder) ? 1 == 1 : w.PurOrder == allocateLot.PurOrder)
                                        && w.Status != "I" )
                        .OrderBy(o => o.Lot)
                        .ToListAsync();
        }

        public async Task Update(AllocateLot allocateLot)
        {
           using var transaction = databaseContext.Database.BeginTransaction();
            try{

           

                databaseContext.AllocateLots.Update(allocateLot);


                await databaseContext.SaveChangesAsync();

                transaction.Commit();

            }catch(Exception ex){
                transaction.Rollback();
                throw ex;
            }
        }

        public async Task<IEnumerable<AllocateLotSize>> FindLotSize(AllocateLot allocateLot)
        {
            return await databaseContext.AllocateLotSizes.Where(w => w.Lot == allocateLot.Lot).ToListAsync();
        }

        public async Task UpdateGeneratePD(AllocateLot allocateLot,string type,VwWebUser account)
        {
            

            using var transaction = databaseContext.Database.BeginTransaction();
            try{

                

                var data = (await databaseContext.AllocateLots.Where(w => w.Lot == allocateLot.Lot).FirstOrDefaultAsync());

                if (data != null){

                    int statuspd  = data.GeneratePd!.Value;

                    if (type == "QueueConvertTOSAP")
                        statuspd = 2;
                    else if (type == "ConvertTOSAP")
                         statuspd = 3;
                    else if (type == "QueueReleasedTOSAP")
                        statuspd = 4;
                    else if (type == "ReleasedTOSAP")
                        statuspd = 5;
                    else if (type == "QueueCloseTOSAP")
                        statuspd = 6;
                    else if (type == "CloseTOSAP")
                        statuspd = 7;
                    else if (type == "QueueCancelTOSAP")
                        statuspd = 8;
                    else if (type == "CancelTOSAP")
                        statuspd = 0;

                    data.GeneratePd = statuspd;
                    data.GeneratePdby = account.EmpUsername;
                }


                databaseContext.AllocateLots.Update(data);

            

                await databaseContext.SaveChangesAsync();

                transaction.Commit();

            }catch(Exception ex){
                transaction.Rollback();
                throw ex;
            }

        }

        public async Task<(string errorMessage, SaleOrderH saleOrderH)> VerifyAllocateLotStatusSO(long saleOrderId)
        {
           
            string errorMessage = string.Empty;
            SaleOrderH _saleorderH = new SaleOrderH();
            
            _saleorderH = (await databaseContext.SaleOrderHs.Where(w=> w.Id == saleOrderId).FirstOrDefaultAsync());


            if (_saleorderH != null){
                var chkallocate = await databaseContext.AllocateLots.Where(w => w.SaleOrderId == saleOrderId & w.Status == "A").ToListAsync();

                if (chkallocate.Count > 0){
                    errorMessage = "There are Lot to allocate.";
                }else{
                    
                    _saleorderH.GenerateLot = 0;
                }
            }else{
                 errorMessage = "Can't find SO Number.";
            }

            return (errorMessage,_saleorderH);


        }

        public async Task UpdateAllocateLotStatusSO(SaleOrderH saleOrderH)
        {
            using var transaction = databaseContext.Database.BeginTransaction();
            try{

                databaseContext.SaleOrderHs.Update(saleOrderH);
                await databaseContext.SaveChangesAsync();

                transaction.Commit();

            }catch(Exception ex){
                transaction.Rollback();
                throw ex;
            }
        }

        public async Task<(string errorMessage, List<GenerateResponse> generateResponses)> VerifyDataDeleteLotList(long saleOrderId)
        {

            string errorMessage = string.Empty;
            List<GenerateResponse> generateResponses = new List<GenerateResponse>();



            var _lotlist = await databaseContext.AllocateLots.Where( w=> w.SaleOrderId == saleOrderId && w.Status == "A").ToListAsync();


            var checkgenmc = _lotlist.Where(w=> w.GenerateMc == 1 || w.GeneratePd == 1 || !String.IsNullOrEmpty(w.StatusIssueMat) || !String.IsNullOrEmpty(w.StatusReceiveMat) || !String.IsNullOrEmpty(w.StatusReceiveFg) ).ToList();

            if (checkgenmc.Count() > 1){
                errorMessage = "There are process after genearate lot";

                foreach(AllocateLot item in checkgenmc){

                    string _errordata = (item.GenerateMc == 1 ? "Gen MC , " : "") + (item.GeneratePd == 1 ? "Gen PD , " : "") + (!String.IsNullOrEmpty(item.StatusIssueMat)?  item.StatusIssueMat+" , " : "") + (!String.IsNullOrEmpty(item.StatusReceiveMat)?  item.StatusReceiveMat+" , " : "") + (!String.IsNullOrEmpty(item.StatusReceiveFg)?  item.StatusReceiveFg+" , " : "");

                     generateResponses.Add(new GenerateResponse{
                    referenceNumber = item.Lot,
                    errorMessage = _errordata,
                });
                }
            }

            return (errorMessage,generateResponses);

        }

        

        public async Task Delete(List<AllocateLot> allocateLots,SaleOrderH saleOrderH)
        {
            using var transaction = databaseContext.Database.BeginTransaction();
            try{

               databaseContext.AllocateLots.UpdateRange(allocateLots);

                databaseContext.SaleOrderHs.Update(saleOrderH);


                await databaseContext.SaveChangesAsync();

                transaction.Commit();

            }catch(Exception ex){
                transaction.Rollback();
                throw ex;
            }
            
        }

         public async Task<(string errorMessage, List<string> fileName)> UploadFile(List<IFormFile> formFiles)
        {
            string path = "UploadFile/AllocateLot/";

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

        public async Task<IEnumerable<AllocateLotExcelLotList>> GetdatafromFile(string fileName)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            string path = "UploadFile/AllocateLot/";

            var dataExcel = new List<AllocateLotExcelLotList>();

            var fullpart = $"{webHostEnvironment.WebRootPath}/{path}/{fileName}";

            using (var stream = System.IO.File.Open(fullpart, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var crow = 0;
                    while (reader.Read()) //Each row of the file
                    {

                        // var _itemNo = reader.IsDBNull(0) ? string.Empty : reader.GetValue(0).ToString();
                        // var _Quanlity = reader.IsDBNull(1) ? string.Empty : reader.GetValue(1).ToString();
                        // var _ShipToCode = reader.IsDBNull(2) ? string.Empty : reader.GetValue(2).ToString();
                        // var _ShitToName = reader.IsDBNull(3) ? string.Empty : reader.GetValue(3).ToString();
                        // var _PoNumber = reader.IsDBNull(4) ? string.Empty : reader.GetValue(4).ToString();
                        // var _Width = reader.IsDBNull(5) ? string.Empty : reader.GetValue(5).ToString();

                        var _lot = reader.IsDBNull(0) ? string.Empty : reader.GetValue(0).ToString();
                        var _total = reader.IsDBNull(1) ? "0" : reader.GetValue(1).ToString();
                        var _035 = reader.IsDBNull(2) ? "0" : reader.GetValue(2).ToString();
                        var _040 = reader.IsDBNull(3) ? "0" : reader.GetValue(3).ToString();
                        var _050 = reader.IsDBNull(4) ? "0" : reader.GetValue(4).ToString();
                        var _055 = reader.IsDBNull(5) ? "0" : reader.GetValue(5).ToString();
                        var _060 = reader.IsDBNull(6) ? "0" : reader.GetValue(6).ToString();
                        var _065 = reader.IsDBNull(7) ? "0" : reader.GetValue(7).ToString();
                        var _070 = reader.IsDBNull(8) ? "0" : reader.GetValue(8).ToString();
                        var _075 = reader.IsDBNull(9) ? "0" : reader.GetValue(9).ToString();
                        var _080 = reader.IsDBNull(10) ? "0" : reader.GetValue(10).ToString();
                        var _085 = reader.IsDBNull(11) ? "0" : reader.GetValue(11).ToString();
                        var _090 = reader.IsDBNull(12) ? "0" : reader.GetValue(12).ToString();
                        var _095 = reader.IsDBNull(13) ? "0" : reader.GetValue(13).ToString();
                        var _100 = reader.IsDBNull(14) ? "0" : reader.GetValue(14).ToString();
                        var _105 = reader.IsDBNull(15) ? "0" : reader.GetValue(15).ToString();
                        var _110 = reader.IsDBNull(16) ? "0" : reader.GetValue(16).ToString();
                        var _115 = reader.IsDBNull(17) ? "0" : reader.GetValue(17).ToString();
                        var _120 = reader.IsDBNull(18) ? "0" : reader.GetValue(18).ToString();
                        var _130 = reader.IsDBNull(19) ? "0" : reader.GetValue(19).ToString();
                        var _140 = reader.IsDBNull(20) ? "0" : reader.GetValue(20).ToString();
                        var _150 = reader.IsDBNull(21) ? "0" : reader.GetValue(21).ToString();
                        var _160 = reader.IsDBNull(22) ? "0" : reader.GetValue(22).ToString();
                        var _170 = reader.IsDBNull(23) ? "0" : reader.GetValue(23).ToString();
                        

                        List<AllocateLotSizeExcelLotList> _allocatelotsize = new List<AllocateLotSizeExcelLotList>();

                             _allocatelotsize.Add(new AllocateLotSizeExcelLotList{Id = 0,Lot = _lot,SizeNo = "035",Qty = Convert.ToInt32(_035)});
                            _allocatelotsize.Add(new AllocateLotSizeExcelLotList{Id = 1,Lot = _lot,SizeNo = "040",Qty = Convert.ToInt32(_040)});
                            _allocatelotsize.Add(new AllocateLotSizeExcelLotList{Id = 2,Lot = _lot,SizeNo = "050",Qty = Convert.ToInt32(_050)});
                            _allocatelotsize.Add(new AllocateLotSizeExcelLotList{Id = 3,Lot = _lot,SizeNo = "055",Qty = Convert.ToInt32(_055)});
                            _allocatelotsize.Add(new AllocateLotSizeExcelLotList{Id = 4,Lot = _lot,SizeNo = "060",Qty = Convert.ToInt32(_060)});
                            _allocatelotsize.Add(new AllocateLotSizeExcelLotList{Id = 5,Lot = _lot,SizeNo = "070",Qty = Convert.ToInt32(_070)});
                            _allocatelotsize.Add(new AllocateLotSizeExcelLotList{Id = 6,Lot = _lot,SizeNo = "075",Qty = Convert.ToInt32(_075)});
                            _allocatelotsize.Add(new AllocateLotSizeExcelLotList{Id = 7,Lot = _lot,SizeNo = "080",Qty = Convert.ToInt32(_080)});
                            _allocatelotsize.Add(new AllocateLotSizeExcelLotList{Id = 8,Lot = _lot,SizeNo = "085",Qty = Convert.ToInt32(_085)});
                            _allocatelotsize.Add(new AllocateLotSizeExcelLotList{Id = 9,Lot = _lot,SizeNo = "090",Qty = Convert.ToInt32(_090)});
                            _allocatelotsize.Add(new AllocateLotSizeExcelLotList{Id = 10,Lot = _lot,SizeNo = "095",Qty = Convert.ToInt32(_095)});
                            _allocatelotsize.Add(new AllocateLotSizeExcelLotList{Id = 11,Lot = _lot,SizeNo = "100",Qty = Convert.ToInt32(_100)});
                            _allocatelotsize.Add(new AllocateLotSizeExcelLotList{Id = 12,Lot = _lot,SizeNo = "105",Qty = Convert.ToInt32(_105)});
                            _allocatelotsize.Add(new AllocateLotSizeExcelLotList{Id = 13,Lot = _lot,SizeNo = "110",Qty = Convert.ToInt32(_110)});
                            _allocatelotsize.Add(new AllocateLotSizeExcelLotList{Id = 14,Lot = _lot,SizeNo = "115",Qty = Convert.ToInt32(_115)});
                            _allocatelotsize.Add(new AllocateLotSizeExcelLotList{Id = 15,Lot = _lot,SizeNo = "120",Qty = Convert.ToInt32(_120)});
                            _allocatelotsize.Add(new AllocateLotSizeExcelLotList{Id = 16,Lot = _lot,SizeNo = "130",Qty = Convert.ToInt32(_130)});
                            _allocatelotsize.Add(new AllocateLotSizeExcelLotList{Id = 17,Lot = _lot,SizeNo = "140",Qty = Convert.ToInt32(_140)});
                            _allocatelotsize.Add(new AllocateLotSizeExcelLotList{Id = 18,Lot = _lot,SizeNo = "150",Qty = Convert.ToInt32(_150)});
                            _allocatelotsize.Add(new AllocateLotSizeExcelLotList{Id = 19,Lot = _lot,SizeNo = "160",Qty = Convert.ToInt32(_160)});
                            _allocatelotsize.Add(new AllocateLotSizeExcelLotList{Id = 20,Lot = _lot,SizeNo = "170",Qty = Convert.ToInt32(_170)});




                        // if (!string.IsNullOrEmpty(_itemNo) && crow != 0)
                        // {

                            dataExcel.Add(new AllocateLotExcelLotList
                            {
                                Id = crow,
                                Lot = _lot.ToString(),
                                Total = Convert.ToInt32(_total),
                                S035 = Convert.ToInt32(_035),
                                S040 = Convert.ToInt32(_040),
                                S050 = Convert.ToInt32(_050),
                                S055 = Convert.ToInt32(_055),
                                S060 = Convert.ToInt32(_060),
                                S070 = Convert.ToInt32(_070),
                                S075 = Convert.ToInt32(_075),
                                S080 = Convert.ToInt32(_080),
                                S085 = Convert.ToInt32(_085),
                                S090 = Convert.ToInt32(_090),
                                S095 = Convert.ToInt32(_095),
                                S100 = Convert.ToInt32(_100),
                                S105 = Convert.ToInt32(_105),
                                S110 = Convert.ToInt32(_110),
                                S115 = Convert.ToInt32(_115),
                                S120 = Convert.ToInt32(_120),
                                S130 = Convert.ToInt32(_130),
                                S140 = Convert.ToInt32(_140),
                                S150 = Convert.ToInt32(_150),
                                S160 = Convert.ToInt32(_160),
                                S170 = Convert.ToInt32(_170),
                                AllocateLotSizes = _allocatelotsize
                               
                            });

                           



                        // }
                        crow++;

                    }
                }
            }

            return dataExcel;
        }

        public async Task<(string errorMessage, List<GenerateResponse> generateResponse)> VerifyDataUpdateLotList(List<AllocateLot> allocateLots)
        {
           string errorMessage = string.Empty;
            List<GenerateResponse> generateResponses = new List<GenerateResponse>();

            List<string> _lotrequest = allocateLots.Select( s=> s.Lot).ToList();

            var _lotlist = await databaseContext.AllocateLots.Where( w=> _lotrequest.Contains(w.Lot) && w.Status == "A").ToListAsync();


            var checkgenmc = _lotlist.Where(w=> w.GenerateMc == 1 || w.GeneratePd == 1 || !String.IsNullOrEmpty(w.StatusIssueMat) || !String.IsNullOrEmpty(w.StatusReceiveMat) || !String.IsNullOrEmpty(w.StatusReceiveFg) ).ToList();

            if (checkgenmc.Count() > 1){
                errorMessage = "There are process after genearate lot";

                foreach(AllocateLot item in checkgenmc){

                    string _errordata = (item.GenerateMc == 1 ? "Gen MC , " : "") + (item.GeneratePd == 1 ? "Gen PD , " : "") + (!String.IsNullOrEmpty(item.StatusIssueMat)?  item.StatusIssueMat+" , " : "") + (!String.IsNullOrEmpty(item.StatusReceiveMat)?  item.StatusReceiveMat+" , " : "") + (!String.IsNullOrEmpty(item.StatusReceiveFg)?  item.StatusReceiveFg+" , " : "");

                     generateResponses.Add(new GenerateResponse{
                    referenceNumber = item.Lot,
                    errorMessage = _errordata,
                });
                }
            }

            return (errorMessage,generateResponses);
           
        }

        public async Task<(string errorMessage, List<GenerateResponse> generateResponses)> VerifyStyleWithSaleOrderD(List<AllocateLotExcelLotList> allocateLotExcelLotLists)
        {
            

            string errorMessage = string.Empty;
            List<GenerateResponse> generateResponses = new List<GenerateResponse>();

            foreach(AllocateLotExcelLotList item in allocateLotExcelLotLists){
                var _allocatelot = await databaseContext.AllocateLots.Where( w=> w.Lot == item.Lot).FirstOrDefaultAsync();
                 List<String> _stylesize = new List<string>();

                foreach(AllocateLotSizeExcelLotList isize in item.AllocateLotSizes){
                    _stylesize.Add(_allocatelot.ItemNo+"_"+isize.SizeNo);
                }

                var SODstylesize = databaseContext.SaleOrderDs.Where(w => w.Id == _allocatelot.SaleOrderId && _stylesize.Contains(w.ItemCode)).ToList();

                var chkstylesize = _stylesize.Where( w=> !SODstylesize.Select(s => s.ItemCode).ToList().Contains(w) ).ToList();
                if (chkstylesize.Count > 0){
                    var detail = string.Join(",", chkstylesize);
                    errorMessage = "Can't find style in SO";
                    generateResponses.Add(new GenerateResponse{
                        errorMessage = "Can't find Style in SO ("+detail+")",
                        referenceNumber = item.Lot
                    });
                }
            }

            return (errorMessage,generateResponses);


        }

        public async Task<(AllocateLot allocateLot, List<AllocateLotSize> newAllocateLotSizes)> PreparedatarequestUpdateLotList(AllocateLotExcelLotList allocateLot, AllocateLot allocateLotDB,VwWebUser account)
        {
            AllocateLot _allocatelotresult = new AllocateLot();
            List<AllocateLotSize> _newAllocatelotresult = new List<AllocateLotSize>();


             //// update qty to Lot
                allocateLotDB.S035 = allocateLot.S035;
                allocateLotDB.S040 = allocateLot.S040;
                allocateLotDB.S050 = allocateLot.S050;
                allocateLotDB.S055 = allocateLot.S055;
                allocateLotDB.S060 = allocateLot.S060;
                allocateLotDB.S075 = allocateLot.S075;

                allocateLotDB.S080 = allocateLot.S080;
                allocateLotDB.S085 = allocateLot.S085;
                allocateLotDB.S090 = allocateLot.S090;
                allocateLotDB.S095 = allocateLot.S095;
                allocateLotDB.S100 = allocateLot.S100;
                allocateLotDB.S105 = allocateLot.S105;
                allocateLotDB.S110 = allocateLot.S110;

                allocateLotDB.S115 = allocateLot.S115;
                allocateLotDB.S120 = allocateLot.S120;
                allocateLotDB.S130 = allocateLot.S130;
                allocateLotDB.S140 = allocateLot.S140;
                allocateLotDB.S150 = allocateLot.S150;
                allocateLotDB.S160 = allocateLot.S160;
                allocateLotDB.S170 = allocateLot.S170;
                allocateLotDB.Total = allocateLot.Total;
                allocateLotDB.UpdateBy = account.EmpUsername;
                allocateLotDB.UpdateDate = System.DateTime.Now;
                



                foreach(AllocateLotSize isize  in allocateLotDB.AllocateLotSizes){
                    var _qtysizenew = allocateLot.AllocateLotSizes.Where( w => w.SizeNo == isize.SizeNo).FirstOrDefault();

                    if (_qtysizenew != null){
                        isize.Qty = _qtysizenew.Qty;
                    }
                }

                var _nsize = allocateLot.AllocateLotSizes.Where( w => !allocateLotDB.AllocateLotSizes.Select( s=> s.SizeNo).ToList().Contains(w.SizeNo)).ToList();

                
                foreach(AllocateLotSizeExcelLotList insize in _nsize){

                    var _saleOrderD = await databaseContext.SaleOrderDs.Where(w => w.Id == allocateLotDB.SaleOrderId && w.ItemCode == allocateLotDB.ItemNo+"_"+insize.SizeNo && w.PoNumber == allocateLotDB.PurOrder && w.Width == allocateLotDB.Width && w.ShipToCode == allocateLotDB.ShipToCode ).FirstOrDefaultAsync();

                
                    _newAllocatelotresult.Add(new AllocateLotSize{
                        Lot = allocateLotDB.Lot,
                        ItemCode = allocateLotDB.ItemNo,
                        AllocateLotId = allocateLotDB.Id,
                        SaleOrderId = allocateLotDB.SaleOrderId,
                        SaleOrderLineNum = _saleOrderD!.LineNum,
                        Type = "Adjust",
                        SizeNo = insize.SizeNo,
                        Qty = insize.Qty,
                        Receives = 0,
                        CreateBy = account.EmpName,
                        CreateDate = System.DateTime.Now
                    });
                }






            return (_allocatelotresult,_newAllocatelotresult);

        }

        public async Task<(string errorMessage, ChangeLotNumber changeLotNumber)> VerifyChangeLotNumber(AllocateLot allocateLotFrom, AllocateLot allocateLotTo)
        {
           string errorMessage = string.Empty;
           ChangeLotNumber changeLotNumber = new ChangeLotNumber { Id = 0, LotFrom = allocateLotFrom.Lot,LotTo = allocateLotTo.Lot};

            if (allocateLotFrom.Lot.Substring(0,7) != allocateLotTo.Lot.Substring(0,7)){
                errorMessage = "The Lot Number type is not match.";
            }

            return (errorMessage,changeLotNumber);

        }

        public async Task UpdateChangeLotNumber(List<AllocateLot> allocateLotold,List<AllocateLot> allocateLotnew)
        {
        
            /// change lot from >>> temp
            using var transaction = databaseContext.Database.BeginTransaction();
            try{

                foreach(AllocateLot itemold in allocateLotold){
                     databaseContext.AllocateLotSizes.RemoveRange(itemold.AllocateLotSizes);
                      databaseContext.AllocateLots.Remove(itemold);  
                      await databaseContext.SaveChangesAsync();
                }
                
              transaction.Commit();

            }catch(Exception ex){  
                transaction.Rollback();
                throw ex;
            }

            using var transaction1 = databaseContext.Database.BeginTransaction();
            try{
                databaseContext.AllocateLots.AddRange(allocateLotnew);
                await databaseContext.SaveChangesAsync();
            transaction1.Commit();

           }catch(Exception ex){  
                transaction1.Rollback();
                throw ex;
            }

        }

        public async Task<List<AllocateLot>> PrepareDataChangeLot(List<AllocateLot> allocateLots,ChangeLotNumber changeLotNumber,VwWebUser account)
        {
            List<AllocateLot> _newlot = new List<AllocateLot>();

             foreach(AllocateLot itnew in allocateLots){

                string _lot = (itnew.Lot == changeLotNumber.LotFrom) ? changeLotNumber.LotTo : changeLotNumber.LotFrom;
  

                List<AllocateLotSize> _lotsize = itnew.AllocateLotSizes.Select( s=> new AllocateLotSize {
                    Lot = _lot,
                    ItemCode = s.ItemCode,
                    SaleOrderId = s.SaleOrderId,
                    SaleOrderLineNum = s.SaleOrderLineNum,
                    Type = s.Type,
                    SizeNo = s.SizeNo,
                    Qty = s.Qty,
                    Receives = s.Receives,
                    CreateBy = s.CreateBy,
                    CreateDate = s.CreateDate,
                    UpdateBy = account.EmpUsername,
                    UpdateDate = System.DateTime.Now,
                    Status = s.Status,
                    BomVersion = s.BomVersion
                }).ToList();


                _newlot.Add(new AllocateLot{
                    
                    Buy = itnew.Buy,
                    SaleOrderId = itnew.SaleOrderId,
                    SoNumber = itnew.SoNumber,
                    SaleDocDate = itnew.SaleDocDate,
                    PurOrder = itnew.PurOrder,
                    Lot = _lot,
                    ItemNo = itnew.ItemNo,
                    ItemName = itnew.ItemName,
                    Width = itnew.Width,
                    ShipToCode = itnew.ShipToCode,
                    ShipToName = itnew.ShipToName,
                    SaleStartDate = itnew.SaleStartDate,
                    Total =itnew.Total,
                    S035 = itnew.S035,
                    S040 = itnew.S040,
                    S050 = itnew.S050,
                    S055 = itnew.S055,
                    S060 = itnew.S060,
                    S065 = itnew.S065,
                    S070 = itnew.S070,
                    S075 = itnew.S075,
                    S080 = itnew.S080,
                    S085 = itnew.S085,
                    S090 = itnew.S090,
                    S095 = itnew.S095,
                    S100 = itnew.S100,
                    S105 = itnew.S105,
                    S110 = itnew.S110,
                    S115 = itnew.S115,
                    S120 = itnew.S120,
                    S130 = itnew.S130,
                    S140 = itnew.S140,
                    S150 = itnew.S150,
                    S160 = itnew.S160,
                    S170 = itnew.S170,
                    CreateBy = itnew.CreateBy,
                    CreateDate = itnew.CreateDate,
                    UpdateBy = account.EmpUsername,
                    UpdateDate = System.DateTime.Now,
                    Status =itnew.Status,
                    StatusIssueMat = itnew.StatusIssueMat,
                    StatusReceiveMat = itnew.StatusReceiveMat,
                    StatusReceiveFg = itnew.StatusReceiveFg,
                    StatusPlanning = itnew.StatusPlanning,
                    GenerateMc = itnew.GenerateMc,
                    GenerateMcby = itnew.GenerateMcby,
                    GeneratePd = itnew.GeneratePd,
                    GeneratePdby = itnew.GeneratePdby,
                    StatusProduction = itnew.StatusProduction,
                    UploadFile = itnew.UploadFile,
                    AllocateLotSizes = _lotsize        


                    
                });
            }


           return  _newlot;
        }

        public async Task<(string errorMessage, List<GenerateResponse> generateResponses)> VerifyLotReleasetoPD(List<AllocateLot> allocateLots)
        {
            string errorMessage = string.Empty;
            List<GenerateResponse> generateResponses = new List<GenerateResponse>();


            var chkgenpd = await databaseContext.AllocateLots.Where( w=> w.Status == "A" && allocateLots.Select(s=> s.Lot).ToList().Contains(w.Lot) && w.GeneratePd < 5).ToListAsync();
            
            if (chkgenpd.Count() != 0){
                errorMessage = "Can't release Lot to Production.";

                foreach(AllocateLot item in chkgenpd){
                     string _errordata = (item.GenerateMc == 1 ? "Gen MC , " : "") 
                     + (item.GeneratePd == 1 ? "Gen PD , " : "") 
                     + (item.GeneratePd == 2 ? "Wait CV , " : "") 
                     + (item.GeneratePd == 3 ? "CV SAP , " : "") 
                     + (item.GeneratePd == 4 ? "Wait Released , " : "") 
                     + (!String.IsNullOrEmpty(item.StatusIssueMat)?  item.StatusIssueMat+" , " : "") 
                     + (!String.IsNullOrEmpty(item.StatusReceiveMat)?  item.StatusReceiveMat+" , " : "") 
                     + (!String.IsNullOrEmpty(item.StatusReceiveFg)?  item.StatusReceiveFg+" , " : "");

                     generateResponses.Add(new GenerateResponse{
                    referenceNumber = item.Lot,
                    errorMessage = _errordata,
                     });
                }

            }
            
            return (errorMessage,generateResponses);
        }

        public async Task<(string errorMessage, List<GenerateResponse> generateResponses)> VerifyDelLotReleasetoPD(List<AllocateLot> allocateLots)
        {
             string errorMessage = string.Empty;
            List<GenerateResponse> generateResponses = new List<GenerateResponse>();


            var chkgenpd = await databaseContext.AllocateLots.Where( w=> w.Status == "A" && allocateLots.Select(s=> s.Lot).ToList().Contains(w.Lot) && w.StatusProduction != "Released").ToListAsync();
            
            if (chkgenpd.Count() > 0){
                errorMessage = "Lot release to production cannot be cancelled.";

                foreach(AllocateLot item in chkgenpd){
                    
                     generateResponses.Add(new GenerateResponse{
                    referenceNumber = item.Lot,
                    errorMessage = (string.IsNullOrEmpty(item.StatusProduction)) ? "Lot release to production cannot be cancelled." : item.StatusProduction,
                     });
                }

            }
            
            return (errorMessage,generateResponses);
        }
    }
}