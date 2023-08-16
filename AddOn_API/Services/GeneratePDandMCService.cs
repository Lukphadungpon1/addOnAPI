using System.Net.Security;
using System.Text;
using System;
using System.Data;
using System.Security.Cryptography;
using System.Linq;
using System.Xml;
using System.Text.RegularExpressions;
using AddOn_API.Data;
using AddOn_API.Entities;
using AddOn_API.Interfaces;
using Microsoft.EntityFrameworkCore;
using static AddOn_API.Installers.JWTInstaller;
using AddOn_API.DTOs.SAPQuery;

namespace AddOn_API.Services
{
    public class GeneratePDandMCService : IGeneratePDandMCService
    {
        private readonly DatabaseContext databaseContext;
        private readonly JwtSettings jwtSetting;
        private readonly ISapSDKService sapSDKService;

        public GeneratePDandMCService(DatabaseContext databaseContext, JwtSettings jwtSetting, ISapSDKService sapSDKService)
        {
            this.sapSDKService = sapSDKService;

            this.jwtSetting = jwtSetting;
            this.databaseContext = databaseContext;
        }




        public async Task<(string errorMessage, string Lot)> VerifyDataProductionOrder(AllocateLot allocateLot)
        {
            string errorMessage = string.Empty;
            string Lot = string.Empty;

            var _saleOrderID = await databaseContext.AllocateLots.Where(w => w.Lot == allocateLot.Lot).Select(s => s.SaleOrderId).Distinct().FirstOrDefaultAsync();

            var chksaleCVSAP = await databaseContext.SaleOrderHs.Where(w => w.Id == _saleOrderID && w.ConvertSap == 0).FirstOrDefaultAsync();

            if (chksaleCVSAP != null)
            {
                errorMessage = "Sale Order have to Convert to SAP";
                Lot = chksaleCVSAP.SoNumber;
                return (errorMessage, Lot);
            }

            var lotsize = await (databaseContext.AllocateLotSizes.Where(w => w.Status == "A" && w.Lot == allocateLot.Lot ).ToListAsync());


          
            string _itemCode = string.Join(",",lotsize.Select(s =>  "'" + s.ItemCode + "'" ).ToList());

            List<BomofMaterialSAPH> _bomsap = await (GetBomofMaterialSAP(_itemCode));


            // var chkbom = await databaseContext.AllocateLotSizes.Include(s => s.AllocateLot)
            //             .GroupJoin(databaseContext.BomOfMaterialHs,
            //             a => new { p1 = a.ItemCode, p2 = a.BomVersion, p3 = "Active" },
            //             b => new { p1 = b.ItemCode, p2 = b.Version, p3 = b.DefaultBom },
            //             (a, b) => new { al = a, bo = b })
            //             .SelectMany(x => x.bo.DefaultIfEmpty(),
            //             (a, b) => new { al = a, bo = b })
            //             .Where(w => w.bo.ItemCode == null).Select(s => s.al.al.ItemCode).ToListAsync();


            var chkbom1 = lotsize.GroupJoin(_bomsap,
                 a => new { p1 = a.ItemCode},
                 b => new { p1 = b.ItemCode},
                 (a,b) => new {al = a, bo = b})
                 .SelectMany( s => s.bo.DefaultIfEmpty(),
                 (a,b) => new { al = a, bo = b })
                .Where(w => w.bo.ItemCode == null).Select(s => s.al.al.ItemCode).ToList();


            


            if (chkbom1.Count() > 0)
            {
                errorMessage = "Can't Find BOM in system";
                Lot = string.Join(",", chkbom1);
                return (errorMessage, Lot);
            }

            List<BomofMaterialSAPD> bomd = new List<BomofMaterialSAPD>();
            foreach(BomofMaterialSAPH item in _bomsap){
                bomd.AddRange(item.BomOfMaterialD);
            }
            

            var tpdep = await databaseContext.TpstyleWithLocations.Where(w=> w.Status == "A" && _bomsap.Select(s=> s.ItemCode).ToList().Contains(w.ArticleCode!)).Select(s=> new { GroupItem =  s.GroupItem}).Distinct().ToListAsync();


            var gdp = bomd.Select(s=> new { GroupItem = s.ItemCode.Substring(0,4)}).Distinct().ToList();

            if (tpdep.Count > 0){
                 var chkdep = gdp.GroupJoin(tpdep,
                            g => g.GroupItem,
                            t => t.GroupItem,
                            (g,t) => new {g1 = g, t2 = t})
                            .SelectMany( s => s.t2.DefaultIfEmpty(),
                            (g,t) => new {gi = g,tp = t})
                            .Where( w=> w.tp.GroupItem == null).ToList();


                if (chkdep.Count > 0){
                    errorMessage = "There are the department is empty value";

                    Lot = String.Join(",", chkdep.Select(s => s).ToList());
                    return (errorMessage, Lot);
                }
            }else{
                 errorMessage = "There are the department is empty value";
                    Lot = String.Join(",", gdp.Select(s => s.GroupItem).ToList());
                    return (errorMessage, Lot);
            }

           


            // var chkdpbom = bomd.Where(w => String.IsNullOrEmpty(w.DepartmentCode)).ToList();

            // if (chkdpbom.Count > 0){
            //     errorMessage = "There are the department code is empty value";

            //     Lot = String.Join(",", chkdpbom.Select(s => s.ItemCode).ToList());
            //     return (errorMessage, Lot);
            // }
            


            var chkGPD = await databaseContext.AllocateLots.Where(w => w.Lot == allocateLot.Lot && w.GeneratePd == 1).FirstOrDefaultAsync();

            if (chkGPD != null)
            {
                errorMessage = "Lot has been generated Production Order";
                Lot = chkGPD.Lot;
                return (errorMessage, Lot);
            }


            return (errorMessage, Lot);

        }




        public async Task<List<ProductionOrderH>> PreparedatafromLottoPD(AllocateLot allocateLot, VwWebUser account)
        {
            List<ProductionOrderH> prdH = new List<ProductionOrderH>();


           // string _itemCode = string.Join(",", allocateLot.AllocateLotSizes.Select(s => new { ItemCode = "'" + s.ItemCode + "'" }));

            List<AllocateLotSize> lotsize = await (databaseContext.AllocateLotSizes.Where(w => w.Status == "A" && w.Lot == allocateLot.Lot ).ToListAsync());
          
            string _itemCode = string.Join(",",lotsize.Select(s =>  "'" + s.ItemCode + "'" ).ToList());

    
            List<BomofMaterialSAPH> _bomsap = await (GetBomofMaterialSAP(_itemCode));

           
            foreach (AllocateLotSize als in lotsize)
            {
                
                // BomOfMaterialH _bomh = await databaseContext.BomOfMaterialHs.Include(s => s.BomOfMaterialDs).Where(w => w.ItemCode == als.ItemCode.ToString() && w.DefaultBom == "Active").FirstOrDefaultAsync();
                 BomofMaterialSAPH _bomh = _bomsap.Where(w => w.ItemCode == als.ItemCode.ToString()).FirstOrDefault();


                AllocateLot _qrLot = await databaseContext.AllocateLots.Where(w => w.Lot == als.Lot).FirstOrDefaultAsync();
                SaleOrderH _soh = await databaseContext.SaleOrderHs.Where(w => w.Id == als.SaleOrderId).FirstOrDefaultAsync();
              
                
                
                ProductionOrderH _PH = new ProductionOrderH
                {
                    ItemCode = als.ItemCode,
                    ItemName = _bomh.ItemName,
                    PlanQty = als.Qty,
                    Type = "S",
                    Warehouse = _bomh.ToWh,
                    Priority = "100",
                    OrderDate = _qrLot.SaleDocDate,
                    StartDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(5),  // + 5
                    Project = _bomh.ProCode,
                    Remark = als.Lot,
                    AllocateLotSizeId = als.RowId,
                    UomCode = _bomh.Uom,
                    CreateBy = account.EmpUsername,
                    CreateDate = System.DateTime.Now,
                    ConvertSap = 0,
                    Status = "1",
                    Lot = als.Lot,
                    SodocEntry = _soh.DocEntry
                };

                List<ProductionOrderD> _pddetail = new List<ProductionOrderD>();
                foreach (BomofMaterialSAPD bd in _bomh.BomOfMaterialD)
                {

                    DateTime _endDate = Convert.ToDateTime(_PH.StartDate).AddDays(4);

                    _pddetail.Add(new ProductionOrderD
                    {
                        
                        AllocateLotSizeId = _PH.AllocateLotSizeId,
                        LineNum = bd.LineNum,
                        ItemType = "",
                        ItemCode = bd.ItemCode,
                        ItemName = bd.ItemName,
                        BaseQty = bd.Quantity,
                        PlandQty = _PH.PlanQty * bd.Quantity,
                        UomName = bd.UomName,
                        Warehouse = bd.Warehouse,
                        IssueMethod = "Manual",
                        StartDate = _PH.StartDate,
                        EndDate = _endDate,
                        Department = bd.DepartmentCode,
                        Status = "A"
                    });
                }
                
                
                _PH.ProductionOrderDs = _pddetail;
                prdH.Add(_PH);
                
                  ///// create production order for detail FG 

                List<String> listd = new List<string>();
                List<BomofMaterialSAPH> checkbomindetail = new List<BomofMaterialSAPH>();

                listd = _bomh.BomOfMaterialD.Select(s => s.ItemCode).ToList();


                if (listd.Count > 0){
                    checkbomindetail = _bomsap.Where(w => listd.Contains(w.ItemCode)).ToList();
                }
               
    
                int checkloop = 0;

                while(checkloop == 0){
                    try{
                        List<ProductionOrderH> pdhDtFG = await CreateProductionOrderForDetailFG(_PH, account,checkbomindetail,_soh.DocEntry);
                    
                        if (pdhDtFG.Count() > 0)
                        {
                           // prdH.AddRange(pdhDtFG);
                            listd = new List<string>();

                            foreach(ProductionOrderH itemh in pdhDtFG){

                                prdH.Add(itemh);

                                var itemlist = itemh.ProductionOrderDs.Select( s => s.ItemCode).ToList();
                                listd.AddRange(itemlist);
                            }

                            
                            if (listd.Count > 0){
                                checkbomindetail = new List<BomofMaterialSAPH>();
                                checkbomindetail = _bomsap.Where(w => listd.Contains(w.ItemCode)).ToList();
                            }else{
                                checkloop = 1;
                            }
                        }else{
                            checkloop = 1;
                        }
                    }catch{
                        checkloop = 1;
                    }

                }   


                
            }
            return prdH;

        }

        private async Task<List<BomofMaterialSAPH>> GetBomofMaterialSAP(string itemCode)
        {
            List<BomofMaterialSAPH> bomh = new List<BomofMaterialSAPH>();


            string _itemCode = itemCode;
            int checkloop = 0;
            List<String> itemCodeD = new List<string>();

            while(checkloop == 0){
                try{
                    var _getbomh = await (sapSDKService.GetBomofMaterial(_itemCode));

                    if (_getbomh != null ){

                        itemCodeD.Clear();

                        foreach(BomofMaterialSAPH item in _getbomh){
                            bomh.Add(item);

                        itemCodeD.AddRange(item.BomOfMaterialD.Select( s=> s.ItemCode).ToList()!);
                        }

                        var _datadistince = itemCodeD.Distinct();

                        if (itemCodeD.Distinct().Count() > 0){
                            _itemCode = String.Join(",", _datadistince.Select(s => "'" + s + "'"));
                             checkloop = 0;
                        }else{
                            checkloop = 1;
                        }
                    }else{
                        checkloop = 1;
                    }
                }catch{
                    checkloop = 1;
                }
            }

          


            return bomh;
        }


        private async Task<List<ProductionOrderH>> CreateProductionOrderForDetailFG(ProductionOrderH pdhFG, VwWebUser account,List<BomofMaterialSAPH> bomsap,int? SodocEntry)
        {
            List<ProductionOrderH> pdh = new List<ProductionOrderH>();
            
           
            foreach (BomofMaterialSAPH bomh in bomsap)
            {
                List<ProductionOrderD> dtPD = new List<ProductionOrderD>();

                 int _lineNum = 1;

                 DateTime _endDate = Convert.ToDateTime(pdhFG.StartDate).AddDays(4);
                 
                    foreach (BomofMaterialSAPD bd in bomh.BomOfMaterialD)
                    {
                        dtPD.Add(new ProductionOrderD
                        {
                           
                            AllocateLotSizeId = pdhFG.AllocateLotSizeId,
                            LineNum = _lineNum,
                            ItemType = "",
                            ItemCode = bd.ItemCode,
                            ItemName = bd.ItemName,
                            BaseQty = bd.Quantity,
                            PlandQty = pdhFG.PlanQty * bd.Quantity,
                            UomName = bd.UomName,
                            Warehouse = bd.Warehouse,
                            IssueMethod = "Manual",
                            StartDate = pdhFG.StartDate,
                            EndDate = _endDate,
                            Department = bd.DepartmentCode,
                            BomItemCode = bomh.ItemCode,
                            BomVersion = bd.Version,
                            BomLine = bd.LineNum,
                            Status = "A"
                        });
                        _lineNum++;
                    }


                        pdh.Add(new ProductionOrderH
                        {
                            ItemCode = bomh.ItemCode,
                            ItemName = bomh.ItemName,
                            PlanQty = pdhFG.PlanQty,
                            Type = "P",
                            Warehouse = bomh.ToWh,
                            Priority = "100",
                            OrderDate = pdhFG.OrderDate,
                            StartDate = pdhFG.StartDate,
                            DueDate = pdhFG.DueDate,
                            Project = bomh.ProCode,
                            Remark = pdhFG.Lot,
                            AllocateLotSizeId = pdhFG.AllocateLotSizeId,
                            UomCode = bomh.Uom,
                            CreateBy = account.EmpUsername,
                            CreateDate = System.DateTime.Now,
                            Lot = pdhFG.Lot,
                            ConvertSap = 0,
                             Status = "1",
                            ProductionOrderDs = dtPD,
                            SodocEntry = SodocEntry
                        });
                    
                   
            }

            return pdh;
        }

        public async Task<(string errorMessage, string Lot)> VerifyDataConvertPDtoSAP(AllocateLot allocateLot)
        {
            string errorMessage = string.Empty;
            string Lot = string.Empty;

            var chkgenpd = await databaseContext.AllocateLots.Where(s => s.GeneratePd == 0 && s.Lot == allocateLot.Lot && s.Status == "A").FirstOrDefaultAsync();

            if (chkgenpd != null)
            {
                errorMessage = "Lot have to create Production Order";
                Lot = chkgenpd.Lot;
                return (errorMessage, Lot);
            }

            var pdhdata = (await databaseContext.ProductionOrderHs.Where(w => w.Lot == allocateLot.Lot && w.ConvertSap == 0 && w.Status == "1").ToListAsync());

            if (pdhdata.Count == 0)
            {
                errorMessage = "Production Order has been converted to SAP";
                Lot = allocateLot.Lot;
                return (errorMessage, Lot);
            }

            return (errorMessage, Lot);

        }

        public async Task<List<ProductionOrderH>> PreparedataConvertPDToSAP(AllocateLot allocateLot)
        {
            return await databaseContext.ProductionOrderHs.Include(s => s.ProductionOrderDs).Where(w => w.Lot == allocateLot.Lot && w.ConvertSap == 0 && w.Status != "0").ToListAsync();

        }

        public async Task CreatePD(ProductionOrderH productionOrderH)
        {
            using var transaction = databaseContext.Database.BeginTransaction();
            try
            {
                databaseContext.ProductionOrderHs.Add(productionOrderH);

                await databaseContext.SaveChangesAsync();

                transaction.Commit();

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }

        }

        public async Task UpdatePD(ProductionOrderH productionOrderH)
        {
            using var transaction = databaseContext.Database.BeginTransaction();
            try
            {
                databaseContext.ProductionOrderHs.Update(productionOrderH);

                await databaseContext.SaveChangesAsync();

                transaction.Commit();

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }
        }

        public async Task<ProductionOrderH> FindPDById(long id)
        {
            return await databaseContext.ProductionOrderHs.Include(s=> s.ProductionOrderDs).Where(w => w.Id == id && w.Status != "0").FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ProductionOrderH>> SearchPD(AllocateLot allocateLot)
        {

            //var aaaa = await databaseContext.ProductionOrderHs.Include( i => i.ProductionOrderDs).Where( w=> w.Lot == allocateLot.Lot && w.Status != "0").ToListAsync();

            // return await databaseContext.ProductionOrderHs
            // .Include( i => i.ProductionOrderDs)
            // .Where(w => w.Lot == allocateLot.Lot && w.Status != "0" ).ToListAsync();

             return await databaseContext.ProductionOrderHs.Include(s => s.ProductionOrderDs).Where(w => w.Lot.Contains(allocateLot.Lot) && w.Status != "0").ToListAsync();
        }


        public async Task<IEnumerable<ProductionOrderH>> FindAllPD()
        {
            return await databaseContext.ProductionOrderHs.Include(s => s.ProductionOrderDs).Where(w => w.Status != "0").ToListAsync();
        }

        public async Task<AllocateMc> FindMCById(string barcodeId)
        {
            return await databaseContext.AllocateMcs.Where(w => w.BarcodeId == barcodeId).FirstOrDefaultAsync();
        }

        public async Task<(string errorMessage, string Lot)> VerifyDataMainCard(AllocateLot allocateLot)
        {
            string errorMessage = string.Empty;
            string Lot = string.Empty;


            var chkGenMC = await databaseContext.AllocateLots.Where(w => w.Lot == allocateLot.Lot && w.Status == "A").FirstOrDefaultAsync();

            if (chkGenMC == null)
            {
                errorMessage = "Can't find Lot in system";
                Lot = chkGenMC.Lot;
                return (errorMessage, Lot);
            }

            if (chkGenMC.StatusPlanning == "Released")
            {
                errorMessage = "Lot has been Released to Production";
                Lot = chkGenMC.Lot;
                return (errorMessage, Lot);
            }

            if (chkGenMC.GenerateMc == 1)
            {
                errorMessage = "Lot has been generated MainCard";
                Lot = chkGenMC.Lot;
                return (errorMessage, Lot);
            }

            return (errorMessage, Lot);

        }

        public Task<IEnumerable<AllocateMc>> FindAllMC()
        {
            throw new NotImplementedException();
        }

        public async Task CreateMC(List<AllocateMc> allocateMc)
        {
            using var transaction = databaseContext.Database.BeginTransaction();
            try
            {

                foreach (AllocateMc mc in allocateMc)
                {
                    databaseContext.AllocateMcs.Add(mc);
                    await databaseContext.SaveChangesAsync();
                }
                //  databaseContext.AllocateMcs.AddRange(allocateMc);



                transaction.Commit();

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }

        }

        public async Task UpdateMC(List<AllocateMc> allocateMc)
        {
            using var transaction = databaseContext.Database.BeginTransaction();
            try
            {

                databaseContext.AllocateMcs.UpdateRange(allocateMc);
                await databaseContext.SaveChangesAsync();

                // foreach(AllocateMc mc in allocateMc){
                //     databaseContext.AllocateMcs.Update(mc);
                //     await databaseContext.SaveChangesAsync();
                // }
                //  databaseContext.AllocateMcs.AddRange(allocateMc);

                transaction.Commit();

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }
        }

        public async Task<IEnumerable<AllocateMc>> SearchMC(AllocateLot allocateLot)
        {
            return await databaseContext.AllocateMcs.Where(w => w.Lot == allocateLot.Lot && w.StatusMc == "1").ToListAsync();
        }

        public async Task<List<AllocateMc>> PreparedatafromLottoMC(AllocateLot allocateLot, VwWebUser account)
        {
            List<AllocateMc> _dataMC = new List<AllocateMc>();

            List<VwGenerateMcgroupSize> _data = (await databaseContext.VwGenerateMcgroupSizes.Where(w => w.Lot == allocateLot.Lot && w.Project == "UP").ToListAsync());

            /// create UP 
            string barcodeId = "";
            Int64 runBarcodeId = 1;
            int basketSeq = 1;
            string type = string.Empty;
            string splitbarcodeTop = "";
            string splitbarcodeback = "";

            barcodeId = await GenRunningMC(allocateLot);

            splitbarcodeTop = barcodeId.Substring(0, 7);
            splitbarcodeback = barcodeId.Substring(7, (barcodeId.Length - 7));

          
            runBarcodeId = Convert.ToInt64(splitbarcodeback) + runBarcodeId;


            foreach (VwGenerateMcgroupSize dt in _data)
            {
                int qtyPerMC = 6;
                double floor = dt.QtyGsize.Value / qtyPerMC;
                double remain = 0;

                type = dt.Project;
                floor = Math.Floor(floor);
                remain = (dt.QtyGsize.Value - (floor * qtyPerMC));


                for (int i = 0; i < floor; i++)
                {
                
                    if (runBarcodeId > 99999)
                    {
                        barcodeId = splitbarcodeTop + runBarcodeId;
                    }
                    else
                    {
                        string tmrun = ("00000" + runBarcodeId.ToString());
                        barcodeId = splitbarcodeTop + tmrun.Substring(tmrun.Length - 5, 5);
                    }

                    _dataMC.Add(PutdataToMC(dt, account, barcodeId, basketSeq, qtyPerMC, type));
                    basketSeq++;
                    runBarcodeId++;
                }

                if (remain != 0)
                {

                    runBarcodeId = Convert.ToInt64(splitbarcodeback) + runBarcodeId;
                    if (runBarcodeId > 99999)
                    {
                        barcodeId = splitbarcodeTop + runBarcodeId;
                    }
                    else
                    {
                        string tmrun = ("00000" + runBarcodeId.ToString());
                        barcodeId = splitbarcodeTop + tmrun.Substring(tmrun.Length - 5, 5);
                    }

                    _dataMC.Add(PutdataToMC(dt, account, barcodeId.Replace(" ",""), basketSeq, Convert.ToInt16(remain), type));
                    basketSeq++;
                    runBarcodeId++;
                }
            }


            List<AllocateMc> _dataOtherMC = (await PreparedataToOtherMC(_dataMC, allocateLot.Lot));

            _dataMC.AddRange(_dataOtherMC);


            return _dataMC;
        }

        private AllocateMc PutdataToMC(VwGenerateMcgroupSize dt, VwWebUser account, string barcodeId, int basketSeq, int qtyPerMC, string type)
        {
            AllocateMc _dataMC = new AllocateMc
            {
                PlantCode = dt.PlantCode,
                TypeCode = type,
                BarcodeId = barcodeId.Trim(),
                BasketSeq = basketSeq,
                BarcodeQty = qtyPerMC,
                AllocateLotid = dt.AllocateLotid,
                SaleOrderid = dt.SaleOrderId,
                Lot = dt.Lot,
                SizeNo = dt.SizeNo,
                AllocateSizeId = dt.AllocateSizeId,
                ItemNo = dt.ItemNo,
                ItemName = dt.ItemName,
                StatusMc = "1",
                CreateBy = account.EmpUsername,
                CreateDate = System.DateTime.Now,
                Buy = dt.Buy,
                Width = dt.Width,
                Ponumber = dt.PoNumber,
                ShipToCode = dt.ShipToCode,
                ShipToDesc = dt.ShipToDesc,
                Colors = dt.Colors,
                Category = dt.Category,
                Gender = dt.Gender
            };

            return _dataMC;
        }

        private async Task<List<AllocateMc>> PreparedataToOtherMC(List<AllocateMc> allocateMc, string lot)
        {
            List<AllocateMc> _result = new List<AllocateMc>();

            // var getsizeId = (await databaseContext.AllocateLotSizes.Include(s => s.AllocateLot).Where(w => w.Lot == lot).Select(s => s.Id).ToListAsync());

            // var getpdhId = (await databaseContext.ProductionOrderHs.Where(w => getsizeId.Contains(w.AllocateLotSizeId) && w.Status == "1").Select(s => s.Id).ToListAsync());

            // var chkpd = (await databaseContext.ProductionOrderDs
            //                 .Where(w => getpdhId.Contains(w.Id)).Select(s => s.Department).Distinct().ToListAsync());


            /// insert CA
            //  if (chkpd.Contains("CA")){
            foreach (AllocateMc mc in allocateMc)
            {

                var leftbarcodeid = mc.BarcodeId.Substring(0, 7);
                var rightbarcodeid = mc.BarcodeId.Substring(7, mc.BarcodeId.Replace(" ","").Length - 7);

                AllocateMc _dataMC = new AllocateMc
                {
                    PlantCode = mc.PlantCode,
                    TypeCode = "CA",
                    BarcodeId = leftbarcodeid + "1" + rightbarcodeid.Replace(" ",""),
                    BasketSeq = mc.BasketSeq,
                    BarcodeQty = mc.BarcodeQty,
                    AllocateLotid = mc.AllocateLotid,
                    SaleOrderid = mc.SaleOrderid,
                    Lot = mc.Lot,
                    SizeNo = mc.SizeNo,
                    AllocateSizeId = mc.AllocateSizeId,
                    ItemNo = mc.ItemNo,
                    ItemName = mc.ItemName,
                    StatusMc = "1",

                    CreateBy = mc.CreateBy,
                    CreateDate = System.DateTime.Now,
                    Buy = mc.Buy,
                    Width = mc.Width,
                    Ponumber = mc.Ponumber,
                    ShipToCode = mc.ShipToCode,
                    ShipToDesc = mc.ShipToDesc,
                    Colors = mc.Colors,
                    Category = mc.Category,
                    Gender = mc.Gender
                };
                _result.Add(_dataMC);
            }
            // }

            /// insert KD
            // if (chkpd.Contains("KD")){
            foreach (AllocateMc mc in allocateMc)
            {

                var leftbarcodeid = mc.BarcodeId.Substring(0, 7);
                var rightbarcodeid = mc.BarcodeId.Substring(7, mc.BarcodeId.Replace(" ","").Length - 7);

                AllocateMc _dataMC = new AllocateMc
                {
                    PlantCode = mc.PlantCode,
                    TypeCode = "KD",
                    BarcodeId = leftbarcodeid + "2" + rightbarcodeid.Replace(" ",""),
                    BasketSeq = mc.BasketSeq,
                    BarcodeQty = mc.BarcodeQty,
                    AllocateLotid = mc.AllocateLotid,
                    SaleOrderid = mc.SaleOrderid,
                    Lot = mc.Lot,
                    SizeNo = mc.SizeNo,
                    AllocateSizeId = mc.AllocateSizeId,
                    ItemNo = mc.ItemNo,
                    ItemName = mc.ItemName,
                    StatusMc = "1",

                    CreateBy = mc.CreateBy,
                    CreateDate = System.DateTime.Now,
                    Buy = mc.Buy,
                    Width = mc.Width,
                    Ponumber = mc.Ponumber,
                    ShipToCode = mc.ShipToCode,
                    ShipToDesc = mc.ShipToDesc,
                    Colors = mc.Colors,
                    Category = mc.Category,
                    Gender = mc.Gender
                };
                _result.Add(_dataMC);
            }
            //}

            string[] typeUS = { "US", "MS", "OS", "SK" };
            string barcodeId = "";
            Int64 runBarcodeId = 1;
            string splitbarcodeTop = "";
            string splitbarcodeback = "";

            barcodeId = await GenRunningMCUS(lot);
            splitbarcodeTop = barcodeId.Substring(0, 4);
            splitbarcodeback = barcodeId.Substring(4, (barcodeId.Length - 4));

            foreach (var type in typeUS)
            {

                var aaa = allocateMc.ToList();

                foreach (AllocateMc mc in aaa)
                {

                    runBarcodeId = Convert.ToInt64(splitbarcodeback) + runBarcodeId;
                    if (runBarcodeId > 99999)
                    {
                        barcodeId = splitbarcodeTop + runBarcodeId;
                    }
                    else
                    {
                        string tmrun = ("00000" + runBarcodeId.ToString());
                        barcodeId = splitbarcodeTop + tmrun.Substring(tmrun.Length - 5, 5);
                    }
                    AllocateMc _dataMC = new AllocateMc
                    {
                        PlantCode = mc.PlantCode,
                        TypeCode = type,
                        BarcodeId = barcodeId.Trim(),
                        BasketSeq = mc.BasketSeq,
                        BarcodeQty = mc.BarcodeQty,
                        AllocateLotid = mc.AllocateLotid,
                        SaleOrderid = mc.SaleOrderid,
                        Lot = mc.Lot,
                        SizeNo = mc.SizeNo,
                        AllocateSizeId = mc.AllocateSizeId,
                        ItemNo = mc.ItemNo,
                        ItemName = mc.ItemName,
                        StatusMc = "1",

                        CreateBy = mc.CreateBy,
                        CreateDate = System.DateTime.Now,
                        Buy = mc.Buy,
                        Width = mc.Width,
                        Ponumber = mc.Ponumber,
                        ShipToCode = mc.ShipToCode,
                        ShipToDesc = mc.ShipToDesc,
                        Colors = mc.Colors,
                        Category = mc.Category,
                        Gender = mc.Gender
                    };

                    _result.Add(_dataMC);

                    runBarcodeId++;
                }

            }

            return _result;
        }

        public async Task<string> GenRunningMC(AllocateLot allocateLot)
        {
            string lastMaincard = string.Empty;

            var _data = (await databaseContext.SaleOrderHs.Include(s => s.AllocateLots).Where(w => w.AllocateLots.Any(a => a.Lot == allocateLot.Lot)).FirstOrDefaultAsync());

            var _lastmc = await databaseContext.AllocateMcs.Where(w => w.Buy == _data.Buy && w.BarcodeId.Substring(0, 7) == (_data.Buy + _data.SaleTypes) && w.TypeCode == "UP").OrderByDescending(d => d.BarcodeId).FirstOrDefaultAsync();

            if (_lastmc != null)
            {
                lastMaincard = _lastmc.BarcodeId;
            }
            else
            {
                lastMaincard = _data.Buy + _data.SaleTypes + "00000";
            }

            return lastMaincard;
        }

        private async Task<string> GenRunningMCUS(string lot)
        {
            string lastMaincard = string.Empty;

            var _data = (await databaseContext.SaleOrderHs.Include(s => s.AllocateLots).Where(w => w.AllocateLots.Any(a => a.Lot == lot)).FirstOrDefaultAsync());

            var _lastmc = await databaseContext.AllocateMcs.Where(w => w.Buy == _data.Buy && w.BarcodeId.Substring(0, 4) == (_data.Buy.Substring(2, 4))).OrderByDescending(d => d.BarcodeId).FirstOrDefaultAsync();

            if (_lastmc != null)
            {
                lastMaincard = _lastmc.BarcodeId;
            }
            else
            {
                lastMaincard = _data.Buy.Substring(2, 4) + "00000";
            }

            return lastMaincard;
        }

        public async Task<(string errorMessage, string Lot)> VerifyDataMainCardDel(AllocateLot AllocateLot)
        {
            string errorMessage = string.Empty;
            string Lot = string.Empty;


            var chkGenMC = await databaseContext.AllocateLots.Where(w => w.Lot == AllocateLot.Lot && w.Status == "A").FirstOrDefaultAsync();

            if (chkGenMC == null)
            {
                errorMessage = "Can't find Lot in system";
                Lot = chkGenMC.Lot;
                return (errorMessage, Lot);
            }

            if (chkGenMC.StatusPlanning == "Released")
            {
                errorMessage = "Lot has been Released to Production";
                Lot = chkGenMC.Lot;
                return (errorMessage, Lot);
            }

            if (chkGenMC.GenerateMc == 0)
            {
                errorMessage = "Can't find Main Card in System";
                Lot = chkGenMC.Lot;
                return (errorMessage, Lot);
            }

            return (errorMessage, Lot);
        }

        public async Task<(string errorMessage, string Lot)> VerifyDataProductionOrderDel(AllocateLot allocateLot)
        {
            string errorMessage = string.Empty;
            string Lot = string.Empty;

            var _saleOrderID = await databaseContext.AllocateLots.Where(w => w.Lot == allocateLot.Lot).Select(s => s.SaleOrderId).Distinct().FirstOrDefaultAsync();

            var chksaleCVSAP = await databaseContext.SaleOrderHs.Where(w => w.Id == _saleOrderID && w.ConvertSap == 0).FirstOrDefaultAsync();

            if (chksaleCVSAP != null)
            {
                errorMessage = "Sale Order have to Convert to SAP";
                Lot = chksaleCVSAP.SoNumber;
                return (errorMessage, Lot);
            }

             var chkGPD = await databaseContext.AllocateLots.Where(w => w.Lot == allocateLot.Lot).FirstOrDefaultAsync();

            if (chkGPD.GeneratePd == 0)
            {
                errorMessage = "Can't Find Production Order in system";
                Lot = chkGPD.Lot;
                return (errorMessage, Lot);
            }

            if (chkGPD.GeneratePd >= 2  ){
                 errorMessage = "Production Order has convert in system";
                Lot = chkGPD.Lot;
                return (errorMessage, Lot);
            }

            
            if (chkGPD.StatusIssueMat == "Issue"){
                 errorMessage = "There are Issue material transection in system";
                Lot = chkGPD.Lot;
                return (errorMessage, Lot);
            }

            var chkcvtosap = await databaseContext.ProductionOrderHs.Where( w=> w.Lot == allocateLot.Lot && w.ConvertSap == 1).ToListAsync();

            if (chkcvtosap.Count > 0){
                 errorMessage = "There is a transfer of Production Order to SAP";
                Lot = chkGPD.Lot;
                return (errorMessage, Lot);
            }


            return (errorMessage, Lot);

        }

        public async Task<(string errorMessage, string Lot)> VerifyDataReleaseProductionToSAP(AllocateLot allocateLot)
        {

            string errorMessage = string.Empty;
            string Lot = string.Empty;

            var chkgenpd = await databaseContext.AllocateLots.Where(s => s.GeneratePd == 2 && s.Lot == allocateLot.Lot && s.Status == "A").FirstOrDefaultAsync();

            if (chkgenpd != null)
            {
                errorMessage = "Lot have to convert Production Order to SAP";
                Lot = chkgenpd.Lot;
                return (errorMessage, Lot);
            }

            var pdhdata = (await databaseContext.ProductionOrderHs.Where(w => w.Lot == allocateLot.Lot && w.ConvertSap == 1 && w.Status == "P" ).ToListAsync());

            if (pdhdata.Count == 0)
            {
                errorMessage = "Production Order has been Released to SAP";
                Lot = allocateLot.Lot;
                return (errorMessage, Lot);
            }

            return (errorMessage, Lot);
        }

        public Task<(string errorMessage, string Lot)> VerifyDataCloseProductionToSAP(AllocateLot allocateLot)
        {
            // chkeck issue complete

            


            throw new NotImplementedException();
        }

        public async Task<(string errorMessage, string Lot)> VerifyDataCalcelProductionToSAP(AllocateLot allocateLot)
        {
            string errorMessage = string.Empty;
            string Lot = string.Empty;

           

            List<int> valchkgen = new List<int> {3,4,5};
            // valchkgen.Add(2);
            // valchkgen.Add(3);


            var chkgenpd = await databaseContext.AllocateLots.Where(s => !valchkgen.Contains(s.GeneratePd!.Value) && s.Lot == allocateLot.Lot && s.Status == "A").FirstOrDefaultAsync();

            if (chkgenpd != null)
            {
                errorMessage = "Lot have to convert/release Production Order to SAP";
                Lot = chkgenpd.Lot;
                return (errorMessage, Lot);
            }


            var pdhdata = (await databaseContext.ProductionOrderHs.Where(w => w.Lot == allocateLot.Lot && w.ConvertSap >= 3 && w.ConvertSap <= 5  && (w.Status == "P" || w.Status == "R" )).ToListAsync());

            if (pdhdata.Count == 0)
            {
                errorMessage = "Can't Find Production Order..";
                Lot = allocateLot.Lot;
                return (errorMessage, Lot);
            }

            var chkissue = (await databaseContext.ProductionOrderHs.Where(w=> w.Lot == allocateLot.Lot && w.Status == "1").ToListAsync());

            if (chkissue.Count > 0){
                 errorMessage = "There are issue material transection in system.";
                Lot = allocateLot.Lot;
                return (errorMessage, Lot);
            }



            return (errorMessage, Lot);



        }

        public async Task<List<ProductionOrderH>> PreparedataReleasePDToSAP(AllocateLot allocateLot)
        {
            return await databaseContext.ProductionOrderHs.Include(s => s.ProductionOrderDs).Where(w => w.Lot.Contains(allocateLot.Lot) && w.ConvertSap == 1 && w.Status == "P").ToListAsync();
        }

        public async Task<List<ProductionOrderH>> PreparedataCalcelPDToSAP(AllocateLot allocateLot)
        {
            return await databaseContext.ProductionOrderHs.Include(s => s.ProductionOrderDs).Where(w => w.Lot.Contains(allocateLot.Lot) && w.ConvertSap == 1 && (w.Status == "P" || w.Status == "R" )).ToListAsync();
        }

        public async Task<(string errorMessage, string SO)> VerifyDataSOwithAllocate(AllocateLot allocateLot)
        {
            
            String errorMessage = string.Empty;
            String SoNumber = string.Empty;

            List<int> _linenum = allocateLot.AllocateLotSizes.Select(s => s.SaleOrderLineNum).ToList();




            var _sod = await databaseContext.SaleOrderDs.Where( w=> w.Sohid == allocateLot.SaleOrderId && w.LineStatus == "A" && _linenum.Contains(w.LineNum)).ToListAsync();

            List<AllocateLot> _lot = await databaseContext.AllocateLots.Where( w=> w.Lot == allocateLot.Lot && w.Status == "A").ToListAsync();

            List<AllocateLotSize> _lotsize = await databaseContext.AllocateLotSizes.Where( w=> w.SaleOrderId == allocateLot.SaleOrderId && _linenum.Contains(w.SaleOrderLineNum) && w.Status == "A" ).ToListAsync();

            var _ls = _lot.GroupJoin(_lotsize, 
                            l => new {p2 = l.SaleOrderId},
                            s => new {p2 = s.SaleOrderId },
                            (l,s) => new {lot = l, size = s})
                            .SelectMany( s=> s.size.DefaultIfEmpty(),
                                    (t1,t2) => new { t1.lot,t2 = t2})
                                .Select( s=> new {
                                    SOHId = s.lot.SaleOrderId,
                                    LineNume = s.t2!.SaleOrderLineNum,
                                    ItemCode = s.t2.ItemCode,
                                    Width = s.lot.Width,
                                    ShipToCode = s.lot.ShipToCode,
                                    Quanlity = s.t2.Qty!.Value
                                }).ToList()
                            .GroupBy( g=>  new {g.SOHId,g.LineNume,g.ItemCode,g.Width,g.ShipToCode})
                            .Select( ss=> new {
                                    SOHId = ss.Key.SOHId,
                                    LineNume = ss.Key.LineNume,
                                    ItemCode = ss.Key.ItemCode,
                                    Width = ss.Key.Width,
                                    ShipToCode = ss.Key.ShipToCode,
                                    Quanlity = ss.Sum(x => x.Quanlity)
                            }).ToList();
                

            

            var chk = _ls.GroupJoin(_sod,
                            s => new {p1 = s.SOHId ,p2 = s.LineNume,p3 = s.Width,p4 = s.ShipToCode},
                            l => new {p1 = l.Sohid,p2 = l.LineNum,p3 = l.Width,p4 = l.ShipToCode},
                            (s,l) => new {als = s,sod = l})
                            .SelectMany( s=> s.sod.DefaultIfEmpty(),
                                    (t1,t2) => new { t1.als,t2 = t2})
                            .Where( w =>  w.als.Quanlity != w.t2.Quantity)
                            .Select( s=> new  { s.t2.ItemCode}).ToList();
                                 

            




            // var chksosap = saleOrderQuery.Where( w=> w.DocEntry == DocEntry).FirstOrDefault();

            // if (chksosap == null){

            //     errorMessage = "Can't find Sale order in SAP System.";
            //     SoNumber = saleOrderid.ToString();
            //     return (errorMessage,SoNumber);

            // }

            // var _saleorder = await databaseContext.SaleOrderDs.Where( w=> w.Sohid == saleOrderid && w.LineStatus == "A").ToListAsync();
            // var _sapdetail = saleOrderQuery.Where(w=> w.DocEntry == DocEntry).ToList();

            // var chk = _sapdetail.GroupJoin(_saleorder,
            //         s => new { p1 = s.ItemCode, p2 = s.LineNum},
            //         d => new { p1 = d.ItemCode, p2 = d.LineNum},
            //         (s,d) => new { sosap = s, soweb = d})
            //         .SelectMany( x=> x.soweb.DefaultIfEmpty(),
            //             (sosap,soweb) => new { sosap.sosap ,soweb = soweb})
            //         .Where( w=> w.sosap.Quantity != (w.soweb!.Quantity ?? 0))
            //         .Select( s=> new {
            //             ItemCode = s.sosap.ItemCode
            //         }).ToList();


            // var gsize = await databaseContext.AllocateLotSizes.Where( w=> w.SaleOrderId == saleOrderid && w.Status == "A").GroupBy( g=> new { SaleOrderId = g.SaleOrderId, LineNum = g.SaleOrderLineNum})
            //                         .Select( s => new { SaleOrderId = s.Key.SaleOrderId, LineNum = s.Key.LineNum , Qty = s.Sum(su => su.Qty)}).ToListAsync();

            // var chk = databaseContext.SaleOrderDs.GroupJoin(gsize,
            //                 s => new { p1 = s.Sohid ,p2 = s.LineNum},
            //                 d => new {p1 = d.SaleOrderId, p2 = d.LineNum},
            //                 (s,d) => new {so = s, gs =d})
            //                 .SelectMany( x=> x.gs.DefaultIfEmpty(),
            //                     (so,sg) => new { so.so, sg = sg })
            //                 .Where(w=> w.so.Sohid == saleOrderid && w.so.LineStatus == "A" && w.so.Quantity != (w.sg!.Qty ?? 0))
            //                 .Select( s=> new {
            //                     s.so
            //                 }).ToList();


             string stylesize = String.Join(",", chk.Select(s => s.ItemCode).ToList());

                       
            if (chk.Count > 0 ){
                SoNumber = stylesize;
                errorMessage = "There is a difference between orders and allocate.";
            }


            return (errorMessage,SoNumber);



        }

        public async Task<(string errorMessage, string Lot)> VerifyDataReleasedToPD(AllocateLot allocateLot)
        {
            String errorMessage = string.Empty;
            String Lot = string.Empty;

            var chktopd = await databaseContext.AllocateLots.Where(w => w.Lot == allocateLot.Lot && w.Status == "A").FirstOrDefaultAsync();


            if (chktopd == null){
                errorMessage = "Can't find Lot";
                Lot = allocateLot.Lot;
            }

            if (chktopd!.GenerateMc == 0){
                errorMessage = "The barcode have not been generate.";
                Lot = allocateLot.Lot;
            }

            if (chktopd.GeneratePd < 5){
                errorMessage = "The Production Order have not been released.";
                Lot = allocateLot.Lot;
            }





            return (errorMessage,Lot);
         
        }
    }
}