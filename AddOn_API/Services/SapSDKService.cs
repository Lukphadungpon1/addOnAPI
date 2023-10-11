using System.Data.Common;
using System.Xml;
using System;
using System.Security.Cryptography;
using AddOn_API.DTOs.SaleOrder;
using AddOn_API.DTOs.SAPQuery;
using AddOn_API.Entities;
using AddOn_API.Interfaces;

namespace AddOn_API.Services
{
    public class SapSDKService : ISapSDKService
    {
        private readonly IConfiguration configuration;
        public SapSDKService(IConfiguration configuration)
        {

            this.configuration = configuration;

        }

        public async Task<(string errorMessage, SaleOrderH saleOrderH)> ConvertSaleOrder(SaleOrderH saleOrderHs)
        {
            SAPbobsCOM.Company oCompany = setDefaultString();

            string errorMessage = string.Empty;
            SaleOrderH _saleOrderH = new SaleOrderH();
            _saleOrderH = saleOrderHs;

            var salePersonSAP = await GetSalePerson();
            int salePerson = !(salePersonSAP.FirstOrDefault(w => w.SlpCode == 5) == null) ? salePersonSAP.FirstOrDefault(w => w.SlpCode == 5).SlpCode.Value : -1;

            (string errorMessageSeries, int Series, int SeriesNumber) = await GetSeriesSAP("17");  //sale order = 17

            if (errorMessageSeries != "")
            {
                errorMessage = errorMessageSeries;
                return (errorMessage, _saleOrderH);
            }

            var _currency = saleOrderHs.Currency == "THB" ? "THB" : saleOrderHs.Currency.Substring(0,2) + "S" ;


            try
            {
                if (oCompany.Connect() != 1)
                    oCompany.Connect();

                if (!oCompany.InTransaction)
                    oCompany.StartTransaction();

                SAPbobsCOM.Documents Saleorder = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);

                DateTime DocDueDate = (saleOrderHs.DeliveryDate.Value != null) ? saleOrderHs.DeliveryDate.Value : System.DateTime.Now;

                Saleorder.CardCode = saleOrderHs.CardCode;
                Saleorder.CardName = saleOrderHs.CardName;
                Saleorder.Series = Convert.ToInt32(Series);
                //DraftPO.Series = int.Parse("2132");
                Saleorder.DocNum = SeriesNumber;
                //Saleorder.SalesPersonCode = salePerson_OSLP; /////
                Saleorder.Comments = saleOrderHs.Buy;
                Saleorder.DocDate = System.DateTime.Now;
                Saleorder.DocDueDate = DocDueDate;
                Saleorder.TaxDate = System.DateTime.Now;
                Saleorder.DocCurrency = _currency;
                

                foreach (SaleOrderD data in saleOrderHs.SaleOrderDs)
                {
                    Saleorder.Lines.ItemCode = data.ItemCode;
                    Saleorder.Lines.ItemDescription = data.Dscription;
                    Saleorder.Lines.Quantity = Convert.ToDouble(data.Quantity);
                    Saleorder.Lines.UnitPrice = Convert.ToDouble(1);
                    Saleorder.Lines.DiscountPercent = 0;
                    Saleorder.Lines.MeasureUnit = data.UomCode;
                    Saleorder.Lines.ShipToCode = data.ShipToCode;
                    

                    Saleorder.Lines.UserFields.Fields.Item("U_HMC_PONo").Value = data.PoNumber;
                    Saleorder.Lines.UserFields.Fields.Item("U_HMC_PONumber").Value = data.PoNumber;
                    Saleorder.Lines.UserFields.Fields.Item("U_GCS_Width").Value = data.Width;

                    Saleorder.Lines.Add();
                }

                string _Buy = saleOrderHs.Buy.Substring(0, 4);
                string _month = saleOrderHs.Buy.Substring(4, 2);
                string _Uom = "PRS";

                Saleorder.UserFields.Fields.Item("U_RF_BuyYear").Value = _Buy;
                Saleorder.UserFields.Fields.Item("U_HMC_BuyMonth").Value = _month;
                Saleorder.UserFields.Fields.Item("U_TP2_UOM").Value = _Uom;
                Saleorder.UserFields.Fields.Item("U_GCS_PONo").Value = saleOrderHs.SoNumber;

                Saleorder.Comments = saleOrderHs.Remark;
                Saleorder.SalesPersonCode = salePerson;



                int res = Saleorder.Add();

                if (res != 0)
                {
                    oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
                    errorMessage = oCompany.GetLastErrorDescription().ToString(); //@scope_identity
                }


                if (oCompany.InTransaction)
                {

                    oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                    string objectKey = oCompany.GetNewObjectKey();

                    _saleOrderH.DocNum = Convert.ToString(SeriesNumber);
                    _saleOrderH.DocStatus = "A";
                    _saleOrderH.ConvertSap = 1;
                    _saleOrderH.DocEntry = Convert.ToInt32(objectKey);
                }
                else{
                    errorMessage = oCompany.GetLastErrorDescription().ToString(); //@scope_identity  
                    oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
                }
                    

                
                oCompany.Disconnect();


            }
            catch (Exception ex)
            {
                //ex.ToString();
                
                errorMessage = ex.Message.ToString();
                oCompany.Disconnect();
            }

            return (errorMessage, _saleOrderH);

        }

        public async Task<(string errorMessage, SaleOrderH saleOrderH)> UpdateSaleOrder(SaleOrderH saleOrderHs,List<SaleOrderD> saleOrderDNew)
        {
             SAPbobsCOM.Company oCompany = setDefaultString();

            string errorMessage = string.Empty;
            SaleOrderH _SaleorderH = new SaleOrderH();

            try
            {
                
                if (oCompany.Connect() != 1)
                    oCompany.Connect();

                if (!oCompany.InTransaction)
                    oCompany.StartTransaction();

                SAPbobsCOM.Documents Saleorder = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);
                SAPbobsCOM.Document_Lines SaleOrderLine;

                if ( Saleorder.GetByKey(saleOrderHs.DocEntry!.Value)) {

                    DateTime DocDueDate = (saleOrderHs.DeliveryDate.Value != null) ? saleOrderHs.DeliveryDate.Value : System.DateTime.Now;
                    string _Buy = saleOrderHs.Buy.Substring(0, 4);
                    string _month = saleOrderHs.Buy.Substring(4, 2);
                     var _currency = saleOrderHs.Currency == "THB" ? "THB" : saleOrderHs.Currency.Substring(0,2) + "S" ;

                    Saleorder.CardCode = saleOrderHs.CardCode;
                    Saleorder.CardName = saleOrderHs.CardName;
                    Saleorder.DocCurrency = _currency;
                    Saleorder.Comments = saleOrderHs.Buy;
                    Saleorder.DocDueDate = DocDueDate;
                    Saleorder.UserFields.Fields.Item("U_RF_BuyYear").Value = _Buy;
                    Saleorder.UserFields.Fields.Item("U_HMC_BuyMonth").Value = _month;
                    Saleorder.Comments = saleOrderHs.Remark;

                    int aaa =  Saleorder.Lines.Count;


                    for (int i = 0; i <= aaa; i++)
                    {
                        Saleorder.Lines.SetCurrentLine(0);  
                        Saleorder.Lines.Delete(); //Delete
                          
                    }

                    aaa =  Saleorder.Lines.Count;

                
                   foreach (SaleOrderD data in saleOrderHs.SaleOrderDs)
                    {
                        if (data.LineStatus == "A"){
                            //  SaleOrderLine =  Saleorder.Lines;
                            // SaleOrderLine.Add();

                            // SaleOrderLine.SetCurrentLine(SaleOrderLine.Count - 1);
                            
                            Saleorder.Lines.ItemCode = data.ItemCode;
                            Saleorder.Lines.ItemDescription = data.Dscription;
                            Saleorder.Lines.Quantity = Convert.ToDouble(data.Quantity);
                            Saleorder.Lines.UnitPrice = Convert.ToDouble(1);
                            Saleorder.Lines.DiscountPercent = 0;
                            Saleorder.Lines.MeasureUnit = data.UomCode;
                            Saleorder.Lines.ShipToCode = data.ShipToCode;
                            

                            Saleorder.Lines.UserFields.Fields.Item("U_HMC_PONo").Value = data.PoNumber;
                            Saleorder.Lines.UserFields.Fields.Item("U_HMC_PONumber").Value = data.PoNumber;
                            Saleorder.Lines.UserFields.Fields.Item("U_GCS_Width").Value = data.Width;

                            Saleorder.Lines.Add();
                        }
                       
                    }

                    // add new item detail
                    foreach (SaleOrderD data in saleOrderDNew){
                        // SaleOrderLine =  Saleorder.Lines;
                        // SaleOrderLine.Add();

                        // SaleOrderLine.SetCurrentLine(SaleOrderLine.Count - 1);
                        
                        Saleorder.Lines.ItemCode = data.ItemCode;
                        Saleorder.Lines.ItemDescription = data.Dscription;
                        Saleorder.Lines.Quantity = Convert.ToDouble(data.Quantity);
                        Saleorder.Lines.UnitPrice = Convert.ToDouble(1);
                        Saleorder.Lines.DiscountPercent = 0;
                        Saleorder.Lines.MeasureUnit = data.UomCode;
                        Saleorder.Lines.ShipToCode = data.ShipToCode;
                        

                        Saleorder.Lines.UserFields.Fields.Item("U_HMC_PONo").Value = data.PoNumber;
                        Saleorder.Lines.UserFields.Fields.Item("U_HMC_PONumber").Value = data.PoNumber;
                        Saleorder.Lines.UserFields.Fields.Item("U_GCS_Width").Value = data.Width;

                        Saleorder.Lines.Add();
                    }

                    
                    int res = Saleorder.Update();

                    if (res != 0)
                    {
                        errorMessage = oCompany.GetLastErrorDescription().ToString(); //@scope_identity
                    }

                    if (oCompany.InTransaction)
                    {
                        oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                       
                    }
                    else
                        errorMessage = oCompany.GetLastErrorDescription().ToString(); //@scope_identity  

                    
                }else{
                    errorMessage = "Can not find Sale Order";
                } 

                _SaleorderH = saleOrderHs;
                oCompany.Disconnect();
                
            }
            catch (Exception ex)
            {
                //ex.ToString();
                errorMessage = ex.Message.ToString();
                oCompany.Disconnect();
            }

            return (errorMessage, _SaleorderH);
        }

        public async Task<(string errorMessage, ProductionOrderH productionOrderH)> ConvertProductionOrder(ProductionOrderH productionOrderH)
        {
            SAPbobsCOM.Company oCompany = setDefaultString();

            string errorMessage = string.Empty;
            ProductionOrderH _productionOrderH = new ProductionOrderH();

            try
            {
                var salePersonSAP = await GetSalePerson();
                int salePerson = !(salePersonSAP.FirstOrDefault(w => w.SlpCode == 12) == null) ? salePersonSAP.FirstOrDefault(w => w.SlpCode == 12).SlpCode.Value : -1;

              
                (string errorMessageSeries, int Series, int SeriesNumber) = await GetSeriesSAP("202");  //production order = 202

                if (oCompany.Connect() != 1)
                    oCompany.Connect();

                if (!oCompany.InTransaction)
                    oCompany.StartTransaction();

                SAPbobsCOM.ProductionOrders PDOrder = (SAPbobsCOM.ProductionOrders)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oProductionOrders);

                SAPbobsCOM.BoProductionOrderTypeEnum types = (productionOrderH.Type == "S") ? SAPbobsCOM.BoProductionOrderTypeEnum.bopotStandard : SAPbobsCOM.BoProductionOrderTypeEnum.bopotSpecial;


                PDOrder.ProductionOrderType = types;
                PDOrder.ProductionOrderStatus = SAPbobsCOM.BoProductionOrderStatusEnum.boposPlanned;
                PDOrder.ItemNo = productionOrderH.ItemCode;
                PDOrder.PlannedQuantity = Convert.ToDouble(productionOrderH.PlanQty!.Value);
                PDOrder.Series = Series;
                PDOrder.PostingDate = System.DateTime.Now;
                PDOrder.StartDate = productionOrderH.StartDate!.Value;
                PDOrder.DueDate = productionOrderH.DueDate!.Value;
                PDOrder.UserSignature.Equals(salePerson);
                PDOrder.ProductionOrderOrigin = SAPbobsCOM.BoProductionOrderOriginEnum.bopooSalesOrder;
                PDOrder.ProductionOrderOriginEntry =  productionOrderH.SodocEntry!.Value;
                PDOrder.Project = (productionOrderH.Project != "FG" ? "UP" : productionOrderH.Project);
                PDOrder.Remarks = productionOrderH.Lot;

                foreach (ProductionOrderD _pod in productionOrderH.ProductionOrderDs)
                {
                    PDOrder.Lines.ItemType = SAPbobsCOM.ProductionItemType.pit_Item;
                    PDOrder.Lines.ItemNo = _pod.ItemCode;
                    PDOrder.Lines.BaseQuantity = Convert.ToDouble(_pod.BaseQty);
                    //DOrder.Lines.PlannedQuantity =Convert.ToDouble(_pod.PlandQty.Value);
                    PDOrder.Lines.UserFields.Fields.Item("U_RF_Department").Value = string.IsNullOrEmpty(_pod.Department) ? "" : _pod.Department;
                    PDOrder.Lines.Add();

                }


                int res = PDOrder.Add();

                if (res != 0)
                {
                    oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
                    errorMessage = oCompany.GetLastErrorDescription().ToString(); //@scope_identity
                }


                if (oCompany.InTransaction)
                {
                    oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                    string objectKey = oCompany.GetNewObjectKey();

                    productionOrderH.ConvertSap = 1;

                    productionOrderH.DocEntry = Convert.ToInt32(objectKey);
                    productionOrderH.DocNum = SeriesNumber.ToString();
                    productionOrderH.Status = "P";
                    
                }
                else
                {
                    oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
                    errorMessage = oCompany.GetLastErrorDescription().ToString(); //@scope_identity  

                }
                    

                oCompany.Disconnect();

                _productionOrderH = productionOrderH;

            }
            catch (Exception ex)
            {
                //ex.ToString();
                errorMessage = ex.Message.ToString();
                oCompany.Disconnect();
            }

            return (errorMessage, _productionOrderH);

        }

         public async Task<(string errorMessage, ProductionOrderH productionOrderH)> UpdateStatusdProductionOrder(ProductionOrderH productionOrderH,string type)
        {
            SAPbobsCOM.Company oCompany = setDefaultString();

            string errorMessage = string.Empty;
            ProductionOrderH _productionOrderH = new ProductionOrderH();

            try
            {
                
                if (oCompany.Connect() != 1)
                    oCompany.Connect();

                if (!oCompany.InTransaction)
                    oCompany.StartTransaction();

                SAPbobsCOM.ProductionOrders PDOrder = (SAPbobsCOM.ProductionOrders)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oProductionOrders);
        

                if ( PDOrder.GetByKey(productionOrderH.DocEntry!.Value)) {

                    if (type == "R")
                        PDOrder.ProductionOrderStatus = SAPbobsCOM.BoProductionOrderStatusEnum.boposReleased;
                    else if (type == "L")
                        PDOrder.ProductionOrderStatus = SAPbobsCOM.BoProductionOrderStatusEnum.boposClosed;
                    else if (type == "C")
                        PDOrder.ProductionOrderStatus = SAPbobsCOM.BoProductionOrderStatusEnum.boposCancelled;

                    int res = PDOrder.Update();

                    if (res != 0)
                    {
                        errorMessage = oCompany.GetLastErrorDescription().ToString(); //@scope_identity
                    }

                    if (oCompany.InTransaction)
                    {
                        oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                        productionOrderH.Status = type;

                       
                    }
                    else
                        errorMessage = oCompany.GetLastErrorDescription().ToString(); //@scope_identity  

                    
                }else{
                    errorMessage = "Can not find Production Order";
                } 

                _productionOrderH = productionOrderH;
                oCompany.Disconnect();
                
            }
            catch (Exception ex)
            {
                //ex.ToString();
                errorMessage = ex.Message.ToString();
                oCompany.Disconnect();
            }

            return (errorMessage, _productionOrderH);
        }

      

        public async Task<(string errorMessage, BomOfMaterialH bomOfMaterialH)> ConvertBomOfMaterial(BomOfMaterialH bomOfMaterialH)
        {
            SAPbobsCOM.Company oCompany = setDefaultString();

            string errorMessage = string.Empty;
            BomOfMaterialH _bomMTH = new BomOfMaterialH();


            try
            {
                if (oCompany.Connect() != 1)
                    oCompany.Connect();

                if (!oCompany.InTransaction)
                    oCompany.StartTransaction();

//                SAPbobsCOM.Documents GItoPD = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenExit);
//                SAPbobsCOM.Documents GRtoPD = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenEntry);
//                SAPbobsCOM.Documents GRtoPO = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseDeliveryNotes);

    


                SAPbobsCOM.ProductTrees BomH = (SAPbobsCOM.ProductTrees)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oProductTrees);

                BomH.TreeCode = bomOfMaterialH.ItemCode;
                BomH.Quantity = Convert.ToDouble(bomOfMaterialH.Qauntity);
                BomH.TreeType = SAPbobsCOM.BoItemTreeTypes.iProductionTree;
                BomH.PlanAvgProdSize = 1;
                BomH.Warehouse = bomOfMaterialH.ToWh;
                BomH.PriceList = 0;
                BomH.Project = bomOfMaterialH.ProCode;

                foreach(BomOfMaterialD bd in bomOfMaterialH.BomOfMaterialDs){
                    BomH.Items.ItemType = SAPbobsCOM.ProductionItemType.pit_Item;
                    BomH.Items.ItemCode = bd.ItemCode;
                    BomH.Items.Quantity = Convert.ToDouble(bd.Quantity);
                    BomH.Items.Comment = bd.Comment;
                    BomH.Items.UserFields.Fields.Item("U_RF_Department").Value = bd.Department;
                    BomH.Items.Add();
                }
             
                BomH.Add();

                int res = BomH.Add();

                if (res != 0)
                {
                    errorMessage = oCompany.GetLastErrorDescription().ToString(); //@scope_identity
                }
                if (oCompany.InTransaction)
                {
                    oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                    string objectKey = oCompany.GetNewObjectKey();

                    bomOfMaterialH.ConvertSap = 1;
                    bomOfMaterialH.CenvertSapdate = System.DateTime.Now;

                    _bomMTH = bomOfMaterialH;
                }
                else
                   {
                    oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
                    errorMessage = oCompany.GetLastErrorDescription().ToString(); //@scope_identity  

                }

                oCompany.Disconnect();


            }
            catch (Exception ex)
            {
                //ex.ToString();
                
                errorMessage = ex.Message.ToString();
                oCompany.Disconnect();
            }

             return (errorMessage, _bomMTH);
        }

        public async Task<(string errorMessage, IEnumerable<GCSProdtype>)> InsertGCSProdType(List<GCSProdtype> gCSProdtypes)
        {
             SAPbobsCOM.Company oCompany = setDefaultString();

            string errorMessage = string.Empty;

            List<GCSProdtype> _gcsprodtype = new List<GCSProdtype>();

            try
            {
                if (oCompany.Connect() != 1)
                    oCompany.Connect();

                if (!oCompany.InTransaction)
                    oCompany.StartTransaction();

                SAPbobsCOM.CompanyService oGenS = (SAPbobsCOM.CompanyService)oCompany.GetCompanyService();
                
                SAPbobsCOM.GeneralService oGenService = (SAPbobsCOM.GeneralService)oGenS.GetGeneralService("GCS_PROD_TYPE");

                SAPbobsCOM.GeneralData oGeneralData = (SAPbobsCOM.GeneralData)oGenService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                // SAPbobsCOM.GeneralData oChild;
                // SAPbobsCOM.GeneralDataCollection oChildren;
                // SAPbobsCOM.GeneralDataParams oGeneralParams;

                oGeneralData.SetProperty("Code", "10");
               
                oGeneralData.SetProperty("U_GCS_TypeCode", "TT");

                oGeneralData.SetProperty("U_GCS_TypeName", "Test TT");

                oGeneralData.SetProperty("U_GCS_Status", "A");

                oGeneralData.SetProperty("Name", "T");

            // SAPbobsCOM.UserObjectsMD oUserObjectMD = null;
            // oUserObjectMD = ( ( SAPbobsCOM.UserObjectsMD )( oCompany.GetBusinessObject( SAPbobsCOM.BoObjectTypes.oUserObjectsMD ) ) ); 
            // oUserObjectMD.CanCancel = SAPbobsCOM.BoYesNoEnum.tYES; 
            // oUserObjectMD.CanClose = SAPbobsCOM.BoYesNoEnum.tNO; 
            // oUserObjectMD.CanCreateDefaultForm = SAPbobsCOM.BoYesNoEnum.tNO; 
            // oUserObjectMD.CanDelete = SAPbobsCOM.BoYesNoEnum.tYES; 
            // oUserObjectMD.CanFind = SAPbobsCOM.BoYesNoEnum.tYES; 
            // oUserObjectMD.CanYearTransfer = SAPbobsCOM.BoYesNoEnum.tNO; 
            
            // oUserObjectMD.Code = "10"; 
            // oUserObjectMD.ManageSeries = SAPbobsCOM.BoYesNoEnum.tYES; 
            // oUserObjectMD.Name = "T"; 
            // oUserObjectMD.ObjectType = SAPbobsCOM.BoUDOObjType.boud_MasterData; 
            // oUserObjectMD.TableName = "GCS_PROD_TYPE"; 

            //     SAPbobsCOM.UserTable tbprodtype = oCompany.UserTables.Item("GCS_PROD_TYPE");
            //     tbprodtype.Code = "10";
            //    tbprodtype.Name = "T";
            //    tbprodtype.UserFields.Fields.Item("U_GCS_TypeCode").Value = "TT";
            //    tbprodtype.UserFields.Fields.Item("U_GCS_TypeName").Value = "Test T";
            //    tbprodtype.UserFields.Fields.Item("U_GCS_Status").Value = "1";


                oGenService.Add(oGeneralData);
                
              

                if (oCompany.InTransaction)
                {

                    oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                    string objectKey = oCompany.GetNewObjectKey();

                    _gcsprodtype.Add(new GCSProdtype {
                        U_GCS_TypeCode = "TT",
                        U_GCS_TypeName = "Test T",
                        U_GCS_Status = "1",
                        message = objectKey,
                    });
                    
                }
                else
                    errorMessage = oCompany.GetLastErrorDescription().ToString(); //@scope_identity  


                oCompany.Disconnect();



            }catch (Exception ex)
            {
                //ex.ToString();
                errorMessage = ex.Message.ToString();
                oCompany.Disconnect();
            }




            return (errorMessage,_gcsprodtype);

        }
          public async Task<(string errorMessage, IEnumerable<GCSAllocate>)> InsertGCSAllocate(List<GCSAllocate> gCSAllocates)
        {
            SAPbobsCOM.Company oCompany = setDefaultString();

            string errorMessage = string.Empty;

            List<GCSAllocate> _gcsAllocate = new List<GCSAllocate>();

            try
            {
                if (oCompany.Connect() != 1)
                    oCompany.Connect();

                // if (!oCompany.InTransaction)
                //     oCompany.StartTransaction();
                // get last record code
                string lastRecord = "0";
                
                SAPbobsCOM.Recordset oIS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                oIS.DoQuery("select top 1 Cast(Code as bigint) as Code from [@GCS_ALLOCATE] order by Code desc ");

                while (!oIS.EoF)
                {
                    lastRecord = oIS.Fields.Item("Code").Value.ToString();
                    oIS.MoveNext();
                }

                SAPbobsCOM.CompanyService oGenS = (SAPbobsCOM.CompanyService)oCompany.GetCompanyService();
                
                SAPbobsCOM.GeneralService oGenService = (SAPbobsCOM.GeneralService)oGenS.GetGeneralService("GCS_ALLOCATE");

                SAPbobsCOM.GeneralData oGeneralData = (SAPbobsCOM.GeneralData)oGenService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);


              int i =1;
                foreach(GCSAllocate item in gCSAllocates){
                    string Code = (Convert.ToInt64(lastRecord) + i).ToString();

                    try{
                        oGeneralData.SetProperty("Code", Code);
                        oGeneralData.SetProperty("Name", item.SoNumber);
                        oGeneralData.SetProperty("U_GCS_SalesOrderEntry", item.GCS_SalesOrderEntry);
                        oGeneralData.SetProperty("U_GCS_SalesOrder", item.GCS_SalesOrder);
                        oGeneralData.SetProperty("U_GCS_DocDate", item.GCS_DocDate);
                        oGeneralData.SetProperty("U_GCS_PurOrder", item.GCS_PurOrder);
                        oGeneralData.SetProperty("U_GCS_ItemCode", item.GCS_ItemCode);
                        oGeneralData.SetProperty("U_GCS_ItemName", item.GCS_ItemName);
                        oGeneralData.SetProperty("U_GCS_Width", item.GCS_Width);
                        oGeneralData.SetProperty("U_GCS_StartDate", item.GCS_StartDate);
                        oGeneralData.SetProperty("U_GCS_Total", item.GCS_Total);

                        oGeneralData.SetProperty("U_GCS_035", item.GCS_035);
                        oGeneralData.SetProperty("U_GCS_040", item.GCS_040);
                        oGeneralData.SetProperty("U_GCS_045", item.GCS_045);
                        oGeneralData.SetProperty("U_GCS_050", item.GCS_050);
                        oGeneralData.SetProperty("U_GCS_055", item.GCS_055);
                        oGeneralData.SetProperty("U_GCS_060", item.GCS_060);
                        oGeneralData.SetProperty("U_GCS_065", item.GCS_065);
                        oGeneralData.SetProperty("U_GCS_070", item.GCS_070);
                        oGeneralData.SetProperty("U_GCS_075", item.GCS_075);
                        oGeneralData.SetProperty("U_GCS_080", item.GCS_080);
                        oGeneralData.SetProperty("U_GCS_085", item.GCS_085);
                        oGeneralData.SetProperty("U_GCS_090", item.GCS_090);
                        oGeneralData.SetProperty("U_GCS_095", item.GCS_095);
                        oGeneralData.SetProperty("U_GCS_100", item.GCS_100);
                        oGeneralData.SetProperty("U_GCS_105", item.GCS_105);
                        oGeneralData.SetProperty("U_GCS_110", item.GCS_110);
                        oGeneralData.SetProperty("U_GCS_115", item.GCS_115);
                        oGeneralData.SetProperty("U_GCS_120", item.GCS_120);
                        oGeneralData.SetProperty("U_GCS_130", item.GCS_130);
                        oGeneralData.SetProperty("U_GCS_140", item.GCS_140);
                        oGeneralData.SetProperty("U_GCS_150", item.GCS_150);
                        oGeneralData.SetProperty("U_GCS_160", item.GCS_160);
                        oGeneralData.SetProperty("U_GCS_170", item.GCS_170);
                        oGeneralData.SetProperty("U_GCS_Generate", item.GCS_Generate);
                        oGeneralData.SetProperty("U_RF_PRD_LINE", item.RF_PRD_LINE);
                    
                        i++;
                       oGenService.Add(oGeneralData);
                        string objectKey = oCompany.GetNewObjectKey();

                        item.message = objectKey;

                    }
                    catch (Exception ex)
                    {
                        errorMessage = ex.ToString();
                       item.message = oCompany.GetLastErrorDescription().ToString();
                    }
                    
                    _gcsAllocate.Add(item);
                    
                }

                oCompany.Disconnect();

            }catch (Exception ex)
            {
                //ex.ToString();
                errorMessage = ex.Message.ToString();
                oCompany.Disconnect();
            }

            return (errorMessage,_gcsAllocate);

        }

        public async Task<(string errorMessage, IEnumerable<GCSAllocateMC>)> InsertGCSAllocateMC(List<GCSAllocateMC> gCSAllocateMC)
        {

            SAPbobsCOM.Company oCompany = setDefaultString();

            string errorMessage = string.Empty;

            List<GCSAllocateMC> _gcsAllocateMC = new List<GCSAllocateMC>();

            try
            {
                if (oCompany.Connect() != 1)
                    oCompany.Connect();

                // if (!oCompany.InTransaction)
                //     oCompany.StartTransaction();
                // get last record code
                string lastRecord = "0";
                
                SAPbobsCOM.Recordset oIS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                oIS.DoQuery("select top 1 Cast(Code as bigint) as Code from [@GCS_MC] order by Code desc ");

                while (!oIS.EoF)
                {
                    lastRecord = oIS.Fields.Item("Code").Value.ToString();
                    oIS.MoveNext();
                }

                SAPbobsCOM.CompanyService oGenS = (SAPbobsCOM.CompanyService)oCompany.GetCompanyService();
                
                SAPbobsCOM.GeneralService oGenService = (SAPbobsCOM.GeneralService)oGenS.GetGeneralService("GCS_MC");

                SAPbobsCOM.GeneralData oGeneralData = (SAPbobsCOM.GeneralData)oGenService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);


              int i =1;
                foreach(GCSAllocateMC item in gCSAllocateMC){
                    string Code = (Convert.ToInt64(lastRecord) + i).ToString();

                    try{
                        oGeneralData.SetProperty("Code", Code);
                        oGeneralData.SetProperty("Name", item.SoNumber);
                        oGeneralData.SetProperty("U_GCS_MainCardCode", item.GCS_MainCardCode);
                        oGeneralData.SetProperty("U_GCS_MainCardDesc", item.GCS_MainCardDesc);
                        oGeneralData.SetProperty("U_GCS_JobOrderEntry", item.GCS_JobOrderEntry);
                        oGeneralData.SetProperty("U_GCS_JobOrder", item.GCS_JobOrder);
                        oGeneralData.SetProperty("U_GCS_SalesOrderEntry", item.GCS_SalesOrderEntry);
                        oGeneralData.SetProperty("U_GCS_SalesOrder", item.GCS_SalesOrder);
                        oGeneralData.SetProperty("U_GCS_PurOrder", item.GCS_PurOrder);
                        oGeneralData.SetProperty("U_GCS_ItemCode", item.GCS_ItemCode);
                        oGeneralData.SetProperty("U_GCS_ItemName", item.GCS_ItemName);
                        oGeneralData.SetProperty("U_GCS_SizeCode", item.GCS_SizeCode);
                        oGeneralData.SetProperty("U_GCS_Basket", item.GCS_Basket);
                        oGeneralData.SetProperty("U_GCS_Qty", item.GCS_Qty);
                        oGeneralData.SetProperty("U_GCS_IssueDate", item.GCS_IssueDate);
                        oGeneralData.SetProperty("U_GCS_ReceiptDate", item.GCS_ReceiptDate);
                        oGeneralData.SetProperty("U_GCS_Status", item.GCS_Status);
                        oGeneralData.SetProperty("U_GCS_Width", item.GCS_Width);
                        oGeneralData.SetProperty("U_GCS_Lot", item.GCS_Lot);
                        oGeneralData.SetProperty("U_RF_MC_QTY", item.RF_MC_QTY);
                        oGeneralData.SetProperty("U_RF_Accum_Rc_Qty", item.RF_Accum_Rc_Qty);
                       
                    
                        i++;
                       oGenService.Add(oGeneralData);
                        string objectKey = oCompany.GetNewObjectKey();

                        item.message = objectKey;

                    }
                    catch (Exception ex)
                    {
                        errorMessage = ex.ToString();
                       item.message = oCompany.GetLastErrorDescription().ToString();
                    }
                    
                    _gcsAllocateMC.Add(item);
                    
                }

                oCompany.Disconnect();

            }catch (Exception ex)
            {
                //ex.ToString();
                errorMessage = ex.Message.ToString();
                oCompany.Disconnect();
            }

            return (errorMessage,_gcsAllocateMC);
        }

        public async Task<IEnumerable<DetailItem>> GetItemSale(List<SaleOrderD> aosaleOrderD)
        {
            SAPbobsCOM.Company oCompany = setDefaultString();

            var _detailItem = new List<DetailItem>();

            try
            {

                if (oCompany.Connect() != 1)
                    oCompany.Connect();

                if (!oCompany.InTransaction)
                    oCompany.StartTransaction();

                string whitem = "";

                List<string> _itemL = new List<string>();
                foreach (var item in aosaleOrderD)
                {
                    string[] splitem = item.ItemCode.Split("_");

                    if (!_itemL.Contains(splitem[0]))
                        _itemL.Add(splitem[0]);



                    //whitem += "'" + item.ItemCode + "',";
                }
                whitem = string.Join("','",_itemL);

                

                SAPbobsCOM.Recordset oIS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                oIS.DoQuery("select ItemCode,ItemName,ltrim(right(CS.Name,(Len(CS.Name) - CHARINDEX(' ',CS.Name)))) as Color " +
                            ",CS.U_RF_Category,CS.U_RF_Gender,CS.U_RF_Style,U_HMC_Size,'' as ItemNo " +
                            ",OUGP.UgpCode " +
                            "from OITM OITM " +
                            "left join [@HMC_COLOR_SHOES] CS " +
                            "on OITM.U_HMC_ColorShoes = CS.Code " +
                            "left join OUGP OUGP " +
                            "on OITM.UgpEntry = OUGP.UgpEntry " +
                            "where 1=1 " +
                            "and  SUBSTRING(ItemCode,0,CHARINDEX('_',ItemCode)) in ('" + whitem + "') " +
                            "and SellItem = 'Y' " +
                            "and Canceled = 'N' "+
                            "and frozenFor = 'N' ");

                

                while (!oIS.EoF)
                {

                    string _style = oIS.Fields.Item("U_RF_Style").Value.ToString().Replace("\"",string.Empty).ToString();

                    _detailItem.Add(new DetailItem
                    {


                        ItemCode = oIS.Fields.Item("ItemCode").Value.ToString(),
                        Dscription = oIS.Fields.Item("ItemName").Value.ToString(),
                        Colors = oIS.Fields.Item("Color").Value.ToString(),
                        Category = oIS.Fields.Item("U_RF_Category").Value.ToString(),
                        Gender = oIS.Fields.Item("U_RF_Gender").Value.ToString(),
                        Style = _style,
                        SizeNo = oIS.Fields.Item("U_HMC_Size").Value.ToString(),
                        UomCode = oIS.Fields.Item("UgpCode").Value.ToString()

                    });

                    oIS.MoveNext();
                }

                if (oCompany.InTransaction)
                {
                    oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                }
                oCompany.Disconnect();

            }
            catch (Exception ex)
            {
                ex.ToString();

                oCompany.Disconnect();
            }


            return _detailItem;
        }

        public async Task<IEnumerable<SalePersonSAP>> GetSalePerson()
        {
            var _detailItem = new List<SalePersonSAP>();

            try
            {
                string sql = "select SlpCode,SlpName from OSLP where Active = 'Y' and SlpCode > 0 ";
                SAPbobsCOM.Recordset oIS = QueryFromSAP(sql);

                while (!oIS.EoF)
                {

                    _detailItem.Add(new SalePersonSAP
                    {
                        SlpCode = Convert.ToInt32(oIS.Fields.Item("SlpCode").Value.ToString()),
                        SlpName = oIS.Fields.Item("SlpName").Value.ToString()

                    });
                    oIS.MoveNext();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();

            }


            return _detailItem;
        }

        public async Task<IEnumerable<BusinessPartner>> GetBusinessPartner(string type)
        {
            var _detailItem = new List<BusinessPartner>();

            string _cardtype = (type == "Customer") ? "C" : "S";
            try
            {
                string sql = "select CardCode,CardName,isnull(FrgnName,Currency) as CurrencyName ,Currency as Currency " +
                                  "from [dbo].OCRD " +
                                  "left join [dbo].OCRN " +
                                  "on Currency = CurrCode " +
                                  "where CardType = '" + _cardtype + "' " +
                                  "and CardName is not null " +
                                  "and OCRD.frozenFor = 'N' ";


                SAPbobsCOM.Recordset oIS = QueryFromSAP(sql);
                int i =0;
                while (!oIS.EoF)
                {
                    i++;
                    _detailItem.Add(new BusinessPartner
                    {
                        id = i,
                        CardCode = oIS.Fields.Item("CardCode").Value.ToString(),
                        CardName = oIS.Fields.Item("CardName").Value.ToString(),
                        CurrencyName = oIS.Fields.Item("CurrencyName").Value.ToString(),
                        Currency = oIS.Fields.Item("Currency").Value.ToString()

                    });
                    oIS.MoveNext();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return _detailItem;
        }

         public async Task<IEnumerable<BomofMaterialSAPH>> GetBomofMaterial(string Code)
        {
            var bomH = new List<BomofMaterialSAPH>();
            var bomD = new List<BomofMaterialSAPD>();

            var bomsap = new List<BomSAPQurry>();

 
            try
            {
                string sql = "select OITM.U_GCS_ProCode,OITM.ItemCode,OITM.ItemName,OITM.InvntryUom as InvntryUomH " + 
                                ",OITT.Qauntity,OITT.ToWH,OITT.TreeType,OITT.PlAvgSize,OITT.PriceList " + 
                                ",ITT1.VisOrder,ITT1.ChildNum,ITT1.Code,OITMD.ItemName as ItemNameD,ITT1.Quantity as QTYD,OITMD.InvntryUom,ITT1.Warehouse,ITT1.Comment as Comments,ITT1.U_HMC_Comment as Comment,ITT1.U_RF_Department as DepartmentCode " + 
                                ",RFD.Name as DepartmentName " + 
                                "from OITM OITM " + 
                                "left join OITB OITB " + 
                                "on OITM.ItmsGrpCod = OITB.ItmsGrpCod " + 
                                "left join OITT OITT " + 
                                "on OITM.ItemCode = OITT.Code " + 
                                "left join ITT1 ITT1 " + 
                                "on OITT.Code = ITT1.Father " + 
                                "left join OITM OITMD " + 
                                "on ITT1.Code = OITMD.ItemCode " + 
                                "left join dbo.[@RF_DEPARTMENT] RFD " + 
                                "on ITT1.U_RF_Department = RFD.Code " + 
                                "where OITM.ItemCode in ("+Code+") and ITT1.Quantity > 0 and ITT1.Code is not null ";


                SAPbobsCOM.Recordset oIS = QueryFromSAP(sql);
                int i =0;
                var objbomh = new BomofMaterialSAPH();
               
                int aaa = oIS.RecordCount;

                while (!oIS.EoF)
                {
                   
                   bomsap.Add(new BomSAPQurry{
                        Id = i,
                    ItemCode = oIS.Fields.Item("ItemCode").Value.ToString(),
                    ItemName = oIS.Fields.Item("ItemName").Value.ToString(),
                    Type = oIS.Fields.Item("ItemName").Value.ToString(),
                    Qauntity = Convert.ToInt16(oIS.Fields.Item("Qauntity").Value.ToString()),
                    ToWh = oIS.Fields.Item("ToWH").Value.ToString(),
                    ProCode = oIS.Fields.Item("U_GCS_ProCode").Value.ToString(),
                    Uom = oIS.Fields.Item("InvntryUomH").Value.ToString(),
                      Version = oIS.Fields.Item("VisOrder").Value.ToString(),
                        LineNum = Convert.ToInt32(oIS.Fields.Item("ChildNum").Value.ToString()),
                        ItemCodeD = oIS.Fields.Item("Code").Value.ToString(),
                        ItemNameD = oIS.Fields.Item("ItemNameD").Value.ToString(),
                        QuantityD = Convert.ToDecimal(oIS.Fields.Item("QTYD").Value.ToString()) ,
                        UomName = oIS.Fields.Item("InvntryUom").Value.ToString(),
                        Warehouse = oIS.Fields.Item("Warehouse").Value.ToString(),
                        Comment = oIS.Fields.Item("Comments").Value.ToString(),
                        Comment2 = oIS.Fields.Item("Comment").Value.ToString(),
                        DepartmentCode = oIS.Fields.Item("DepartmentCode").Value.ToString(),
                        DepartmentName = oIS.Fields.Item("DepartmentName").Value.ToString()


                   });

                    oIS.MoveNext();
                }

                // List<BomofMaterialSAPH> gitemcode = bomsap.Select( s => new BomofMaterialSAPH{ ItemCode =  s.ItemCode, ItemName = s.ItemName,Type = s.Type,Qauntity = s.Qauntity, ToWh = s.ToWh,ProCode = s.ProCode,Uom = s.Uom }).ToList();

                // var itemcodedisc = gitemcode.Distinct();

                List<BomofMaterialSAPH> gitemcode = bomsap.GroupBy( g => new {g.ItemCode,g.ItemName,g.Type,g.Qauntity,g.ToWh,g.ProCode,g.Uom})
                .Select(s => new BomofMaterialSAPH{ ItemCode =  s.Key.ItemCode, ItemName = s.Key.ItemName,Type = s.Key.Type,Qauntity = s.Key.Qauntity, ToWh = s.Key.ToWh,ProCode = s.Key.ProCode,Uom = s.Key.Uom }).ToList();



                bomH.AddRange(gitemcode);

                foreach (BomofMaterialSAPH item in bomH)
                {
                         List<BomofMaterialSAPD> detail = bomsap.Where( w => w.ItemCode == item.ItemCode)
                    .Select( s => new BomofMaterialSAPD { ItemCodeH = s.ItemCode
                    ,Version = s.Version
                    ,LineNum = s.LineNum
                    ,ItemCode = s.ItemCodeD
                    ,ItemName = s.ItemNameD
                    ,Quantity = s.QuantityD
                    ,UomName=s.UomName
                    ,Warehouse = s.Warehouse
                    ,Comment = s.Comment
                    ,Comment2 = s.Comment2
                    ,DepartmentCode =s.DepartmentCode
                    ,DepartmentName = s.DepartmentName}).ToList();


                    if (detail.Count > 0){
                        item.BomOfMaterialD = detail;
                    }
                }
                  
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            return bomH;
        }
        
          public async Task<IEnumerable<SaleOrderQuery>> GetSaleOrder(List<long> DocEntry)
        {
            string sqlwhare = string.Empty;

            foreach(long it in DocEntry){
                sqlwhare += it.ToString()+",";
            }

            
    
            var salesap = new List<SaleOrderQuery>();

             try
            {
                 string sql = "select ORDR.DocEntry,ORDR.DocNum,ORDR.CardCode,ORDR.CardName,ORDR.U_RF_BuyYear,ORDR.U_HMC_BuyMonth " + 
                            ",RD1.ItemCode,RD1.LineNum + 1 as LineNum,RD1.Dscription,RD1.Quantity,RD1.U_HMC_PONo,RD1.U_GCS_Width,RD1.ShipToCode,RD1.ShipToDesc " + 
                            "from ORDR ORDR " + 
                            "left join RDR1 RD1 " + 
                            "on ORDR.DocEntry = RD1.DocEntry " + 
                            "where ORDR.DocEntry in ("+ sqlwhare.Substring(0,sqlwhare.Length-1) +") " + 
                            "and RD1.ItemType = 4 ";

                SAPbobsCOM.Recordset oIS = QueryFromSAP(sql);
                int i =0;
                var objbomh = new BomofMaterialSAPH();
               
                int aaa = oIS.RecordCount;

                while (!oIS.EoF)
                {    
                   salesap.Add(new SaleOrderQuery{
                        Id = i,
                        DocEntry = Convert.ToInt32(oIS.Fields.Item("DocEntry").Value.ToString()),
                        DocNum = oIS.Fields.Item("DocNum").Value.ToString(),
                        CardCode = oIS.Fields.Item("CardCode").Value.ToString(),
                        CardName = oIS.Fields.Item("CardName").Value.ToString(),
                        Buy = oIS.Fields.Item("U_RF_BuyYear").Value.ToString() + oIS.Fields.Item("U_HMC_BuyMonth").Value.ToString() ,
                        ItemCode = oIS.Fields.Item("ItemCode").Value.ToString(),
                        LineNum =  Convert.ToInt32(oIS.Fields.Item("LineNum").Value.ToString()),
                        Dscription = oIS.Fields.Item("Dscription").Value.ToString(),
                        Quantity = Convert.ToInt32(oIS.Fields.Item("Quantity").Value.ToString()),
                        PoNumber = oIS.Fields.Item("U_HMC_PONo").Value.ToString(),
                        Width = oIS.Fields.Item("U_GCS_Width").Value.ToString(),
                        ShipToCode = oIS.Fields.Item("ShipToCode").Value.ToString(),
                        ShipToDesc = oIS.Fields.Item("ShipToDesc").Value.ToString()
                      
                   });

                    oIS.MoveNext();
                }

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            
            return salesap;

        }

        public async Task<IEnumerable<ItemOnhand>> GetItemOnhand(List<string> ItemCodeList)
        {
            SAPbobsCOM.Company oCompany = setDefaultString();

            var _detailItem = new List<ItemOnhand>();

            try
            {

                if (oCompany.Connect() != 1)
                    oCompany.Connect();

                if (!oCompany.InTransaction)
                    oCompany.StartTransaction();

                string whitem = "";
                whitem = string.Join("','",ItemCodeList);

                SAPbobsCOM.Recordset oIS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                oIS.DoQuery("select ROW_NUMBER() over (order by ITM.ItemCode) as Id ,ITB.ItmsGrpCod,ITB.ItmsGrpNam " + 
                                ",ITM.ItemCode,ITM.ItemName,ITM.IUoMEntry,UOM.UomName,ITM.InvntryUom,ITM.OnHand " + 
                                ",ITW.WhsCode,WHS.WhsName,ITW.OnHand as OnHandDFwh,CL.Name as Color " + 
                                "from OITM ITM " + 
                                "left join OITB ITB " + 
                                "on ITM.ItmsGrpCod = ITB.ItmsGrpCod " + 
                                "left join OITW ITW " + 
                                "on ITM.DfltWH = ITW.WhsCode " + 
                                "and ITM.ItemCode = ITW.ItemCode " + 
                                "left join OWHS WHS " + 
                                "on ITW.WhsCode = WHS.WhsCode " + 
                                "left join OUOM UOM " + 
                                "on ITM.IUoMEntry = UOM.UomEntry " + 
                                "left join [@HMC_COLOR] CL " + 
                                "on ITM.U_HMC_Color = CL.Code " +
                                "where ITB.ItmsGrpCod not in (104,114,115,116,117,112) " + 
                                "and ITM.ItemCode in ('" + whitem + "') ");

                while (!oIS.EoF)
                { 
                    _detailItem.Add(new ItemOnhand
                    {
                        Id= Convert.ToInt32(oIS.Fields.Item("Id").Value.ToString()),
                        ItmsGrpCod = Convert.ToInt32(oIS.Fields.Item("ItmsGrpCod").Value.ToString()),
                        ItmsGrpNam = oIS.Fields.Item("ItmsGrpNam").Value.ToString(),
                        ItemCode = oIS.Fields.Item("ItemCode").Value.ToString(),
                        ItemName = oIS.Fields.Item("ItemName").Value.ToString(),
                        IUoMEntry = Convert.ToInt32(oIS.Fields.Item("IUoMEntry").Value.ToString()),
                        UomName = oIS.Fields.Item("UomName").Value.ToString(),
                        InvntryUom = oIS.Fields.Item("InvntryUom").Value.ToString(),
                        OnHand  = Convert.ToDecimal(oIS.Fields.Item("OnHand").Value.ToString()),
                        WhsCode = oIS.Fields.Item("WhsCode").Value.ToString(),
                        WhsName = oIS.Fields.Item("WhsName").Value.ToString(),
                        OnHandDFwh = Convert.ToDecimal(oIS.Fields.Item("OnHandDFwh").Value.ToString()),
                        Color = oIS.Fields.Item("Color").Value.ToString()
                    });

                    oIS.MoveNext();
                }

                if (oCompany.InTransaction)
                {
                    oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                }
                oCompany.Disconnect();

            }
            catch (Exception ex)
            {
                ex.ToString();

                oCompany.Disconnect();
            }

            return _detailItem;
        }

          public async Task<IEnumerable<BatchNumber>> GetBatNumber(List<string> ItemCodeList)
        {
            SAPbobsCOM.Company oCompany = setDefaultString();

            var _detailItem = new List<BatchNumber>();

            try
            {
                if (oCompany.Connect() != 1)
                    oCompany.Connect();

                if (!oCompany.InTransaction)
                    oCompany.StartTransaction();

                string whitem = "";
                whitem = string.Join("','",ItemCodeList);

                SAPbobsCOM.Recordset oIS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                oIS.DoQuery("select OIBT.ItemCode,OIBT.WhsCode,OIBT.Quantity,0 as usedQty,OBTN.DistNumber,OBTN.InDate " + 
                            "from OIBT OIBT " + 
                            "left join OBTN  OBTN " + 
                            "on OBTN.ItemCode = OIBT.ItemCode " + 
                            "and OBTN.DistNumber = OIBT.BatchNum " + 
                            "where OIBT.Quantity <> 0 " + 
                            "and OIBT.ItemCode in ('" + whitem + "') " + 
                            "order by OIBT.ItemCode,OIBT.WhsCode,OIBT.InDate asc ");

                int id = 0;
                while (!oIS.EoF)
                { 
                    _detailItem.Add(new BatchNumber
                    {
                        Id= id,
                        ItemCode = oIS.Fields.Item("ItemCode").Value.ToString(),
                        WhsCode = oIS.Fields.Item("WhsCode").Value.ToString(),
                        Quantity = Convert.ToDecimal(oIS.Fields.Item("Quantity").Value.ToString()),
                        UsedQty = Convert.ToDecimal(oIS.Fields.Item("usedQty").Value.ToString()),
                        DistNumber = oIS.Fields.Item("DistNumber").Value.ToString(),
                        InDate  = Convert.ToDateTime(oIS.Fields.Item("InDate").Value.ToString())
                    });
                    id ++;
                    oIS.MoveNext();


                }

                if (oCompany.InTransaction)
                {
                    oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                }
                oCompany.Disconnect();

            }
            catch (Exception ex)
            {
                ex.ToString();

                oCompany.Disconnect();
            }

            return _detailItem;
        }





        public async Task<(string errorMessage, int Series, int SeriesNumber)> GetSeriesSAP(string documentType)
        {
            string errorMessage = string.Empty;
            int _series = 0;
            int _seriesNumber = 0;
            SAPbobsCOM.Company oCompany = setDefaultString();
            try
            {
                if (oCompany.Connect() != 1)
                    oCompany.Connect();


                SAPbobsCOM.SeriesService seriesService = (SAPbobsCOM.SeriesService)oCompany.GetCompanyService().GetBusinessService(SAPbobsCOM.ServiceTypes.SeriesService);
                SAPbobsCOM.DocumentTypeParams typeParams = (SAPbobsCOM.DocumentTypeParams)seriesService.GetDataInterface(SAPbobsCOM.SeriesServiceDataInterfaces.ssdiDocumentTypeParams);

                typeParams.Document = documentType;  ////po document

  
                SAPbobsCOM.Series se = seriesService.GetDefaultSeries(typeParams);
                _series = se.Series;
                _seriesNumber = se.NextNumber;


                oCompany.Disconnect();

            }
            catch (Exception ex)
            {

                oCompany.Disconnect();
                errorMessage = ex.ToString();
            }


            return (errorMessage, _series, _seriesNumber);

        }
        public SAPbobsCOM.Company setDefaultString()
        {
            var sapConnect = new SAPConnect();
            configuration.Bind(nameof(SAPConnect), sapConnect);


            SAPbobsCOM.Company oCompany = new SAPbobsCOM.Company();

            oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2019;
            oCompany.DbUserName = sapConnect.DBUsername;
            oCompany.DbPassword = sapConnect.DBPassword;
            oCompany.Server = sapConnect.Server;
            oCompany.CompanyDB = sapConnect.CompanyDB;
            oCompany.UserName = sapConnect.Username;
            oCompany.Password = sapConnect.Password;
            oCompany.UseTrusted = false;
            oCompany.language = SAPbobsCOM.BoSuppLangs.ln_English;
            return oCompany;
        }

        public dynamic QueryFromSAP(string sql)
        {
            SAPbobsCOM.Company oCompany = setDefaultString();

            try
            {
                if (oCompany.Connect() != 1)
                    oCompany.Connect();

                if (!oCompany.InTransaction)
                    oCompany.StartTransaction();

                SAPbobsCOM.Recordset oIS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                oIS.DoQuery(sql);

                if (oCompany.InTransaction)
                {
                    oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                }
                oCompany.Disconnect();
                return oIS;

            }
            catch (Exception ex)
            {
                oCompany.Disconnect();

                return ex.ToString();
            }
        }

        public async Task<(string errorMessage, IssueMaterialH issueMaterialH,List<BatchNumber> batchNumbersRT)> ConvertIssueMaterial(IssueMaterialH issueMaterialH,List<ProductionOrderH> productionOrderHs,List<BatchNumber> batchNumbers)
        {

            SAPbobsCOM.Company oCompany = setDefaultString();

            string errorMessage = string.Empty;
            IssueMaterialH _issueMaterialH = new IssueMaterialH();
            _issueMaterialH = issueMaterialH;

            List<BatchNumber> _batchNumberRT = new List<BatchNumber>();
            _batchNumberRT.AddRange(batchNumbers.ToList());



            var salePersonSAP = await GetSalePerson();
            int salePerson = !(salePersonSAP.FirstOrDefault(w => w.SlpCode == 5) == null) ? salePersonSAP.FirstOrDefault(w => w.SlpCode == 5).SlpCode.Value : -1;

            (string errorMessageSeries, int Series, int SeriesNumber) = await GetSeriesSAP("60");  //Issue For Production (OIGE)= 60

            if (errorMessageSeries != "")
            {
                errorMessage = errorMessageSeries;
                return (errorMessage, _issueMaterialH,_batchNumberRT);
            }

             try
            {
                if (oCompany.Connect() != 1)
                    oCompany.Connect();

                if (!oCompany.InTransaction)
                    oCompany.StartTransaction();

                SAPbobsCOM.Documents GIFPD = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenExit);

                GIFPD.Series = Convert.ToInt32(Series);
                //DraftPO.Series = int.Parse("2132");
                GIFPD.DocNum = SeriesNumber;
                //Saleorder.SalesPersonCode = salePerson_OSLP; /////
                GIFPD.Comments = "Automatically created by Receipt from Production "+issueMaterialH.Lotlist;
               // GIFPD.Reference2 = issueMaterialH.IssueNumber;
                GIFPD.DocDate = System.DateTime.Now;
                GIFPD.JournalMemo = "";

                foreach (IssueMaterialD data in issueMaterialH.IssueMaterialDs)
                {
                    ProductionOrderH _PH = productionOrderHs.Where(w=> w.Id == data.Pdhid).FirstOrDefault();

                    GIFPD.Lines.BaseEntry = _PH.DocEntry!.Value;
                    GIFPD.Lines.BaseType = 202; 
                    GIFPD.Lines.BaseLine = data.LineNum;
                   // GIFPD.Lines.ItemCode = data.ItemCode;
                   // GIFPD.Lines.ItemDescription = data.ItemName;
                    GIFPD.Lines.Quantity = Convert.ToDouble(data.PickQty!.Value);
                    GIFPD.Lines.UseBaseUnits = SAPbobsCOM.BoYesNoEnum.tYES;
                   // GIFPD.Lines.Currency = "THB";
                   // GIFPD.Lines.Rate = 0;
                    GIFPD.Lines.WarehouseCode = data.Warehouse!.Trim();

                    //int batLineNum = GIFPD.Lines.BatchNumbers.BaseLineNumber;
                    //GIFPD.Lines.BatchNumbers.BaseLineNumber = batLineNum - 1;
                   decimal _qtybatch = Convert.ToDecimal(data.PickQty!.Value);
                   int checkloop = 0;
                    do
                    {
                       
                        BatchNumber _batchNumber = _batchNumberRT.Where(w=> w.ItemCode == data.ItemCode && w.Quantity != w.UsedQty).OrderBy(o=> o.InDate).FirstOrDefault()!;

                        if ((_batchNumber.Quantity - _batchNumber.UsedQty) >= (_qtybatch !=  Convert.ToDecimal(data.PickQty!.Value) ? Convert.ToDecimal(data.PickQty!.Value) - _qtybatch : _qtybatch)){
                            _qtybatch = (_qtybatch !=  Convert.ToDecimal(data.PickQty!.Value) ? Convert.ToDecimal(data.PickQty!.Value) - _qtybatch : _qtybatch);
                            checkloop = 1;
                            
                        }else{
                            _qtybatch =  (_batchNumber.Quantity - _batchNumber.UsedQty);
                        }
                        _batchNumber.UsedQty = _batchNumber.UsedQty + _qtybatch ;
                        
                        GIFPD.Lines.BatchNumbers.BatchNumber = _batchNumber.DistNumber;
                        GIFPD.Lines.BatchNumbers.Quantity =  Convert.ToDouble(_qtybatch);
                        GIFPD.Lines.BatchNumbers.Add();
                   
                    } while (checkloop != 1);

                    //GIFPD.Lines.BatchNumbers.BaseLineNumber = 0;
                    // GIFPD.Lines.BatchNumbers.BatchNumber = "202308";
                    // GIFPD.Lines.BatchNumbers.Quantity =  Convert.ToDouble(data.PickQty!.Value);
                    // GIFPD.Lines.BatchNumbers.Add();

                    GIFPD.Lines.Add();
                }
                int res = GIFPD.Add();

                if (res != 0)
                {
                   
                    errorMessage = oCompany.GetLastErrorDescription().ToString(); //@scope_identity
                    oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
                    oCompany.Disconnect();

                     return (errorMessage, _issueMaterialH,_batchNumberRT);
                   
                }

                if (oCompany.InTransaction)
                {

                    oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                    string objectKey = oCompany.GetNewObjectKey();

                    _issueMaterialH.DocNum = Convert.ToString(SeriesNumber);
                    _issueMaterialH.Status = "Issued";
                    _issueMaterialH.ConvertSap = 1;
                    _issueMaterialH.DocEntry = Convert.ToInt32(objectKey);
                }
                else
                {
                   
                    errorMessage = oCompany.GetLastErrorDescription().ToString(); //@scope_identity  
                     oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                }

                oCompany.Disconnect();


            }
            catch (Exception ex)
            {
                //ex.ToString();
                errorMessage = ex.Message.ToString();
                oCompany.Disconnect();
            }


           return (errorMessage, _issueMaterialH,_batchNumberRT);
            
        }

        public async Task<(string errorMessage, IssueMaterialH issueMaterialH)> ConvertIssueMaterialML(IssueMaterialH issueMaterialH,List<BatchNumber> batchNumbers)
        {
             SAPbobsCOM.Company oCompany = setDefaultString();

            string errorMessage = string.Empty;
            IssueMaterialH _issueMaterialH = new IssueMaterialH();
            _issueMaterialH = issueMaterialH;

            List<BatchNumber> _batchNumberRT = new List<BatchNumber>();
            _batchNumberRT.AddRange(batchNumbers.ToList());


            var salePersonSAP = await GetSalePerson();
            int salePerson = !(salePersonSAP.FirstOrDefault(w => w.SlpCode == 5) == null) ? salePersonSAP.FirstOrDefault(w => w.SlpCode == 5).SlpCode.Value : -1;

            (string errorMessageSeries, int Series, int SeriesNumber) = await GetSeriesSAP("60");  //Issue For Production (OIGE)= 60

            if (errorMessageSeries != "")
            {
                errorMessage = errorMessageSeries;
                return (errorMessage, _issueMaterialH);
            }

             try
            {
                if (oCompany.Connect() != 1)
                    oCompany.Connect();

                if (!oCompany.InTransaction)
                    oCompany.StartTransaction();

                SAPbobsCOM.Documents GIFPD = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenExit);

                GIFPD.Series = Convert.ToInt32(Series);
                //DraftPO.Series = int.Parse("2132");
                GIFPD.DocNum = SeriesNumber;
                //Saleorder.SalesPersonCode = salePerson_OSLP; /////
                GIFPD.Comments = "Automatically created by Receipt from Production (Manual) "+issueMaterialH.Lotlist;
              //  GIFPD.Reference2 = issueMaterialH.IssueNumber;
                GIFPD.DocDate = System.DateTime.Now;
  

                foreach (IssueMaterialManual data in issueMaterialH.IssueMaterialManuals)
                {
                  
                    GIFPD.Lines.ItemCode = data.ItemCode;
                    GIFPD.Lines.ItemDescription = data.ItemName;
                    GIFPD.Lines.Quantity = Convert.ToDouble(data.PickQty!.Value);
                    GIFPD.Lines.UseBaseUnits = SAPbobsCOM.BoYesNoEnum.tYES;
                    //GIFPD.Lines.Currency = "THB";
                    //GIFPD.Lines.Rate = 0;
                    GIFPD.Lines.WarehouseCode = data.Warehouse!.Trim();

                    decimal _qtybatch = Convert.ToDecimal(data.PickQty!.Value);
                   int checkloop = 0;
                    do
                    {
                       
                        BatchNumber _batchNumber = _batchNumberRT.Where(w=> w.ItemCode == data.ItemCode && w.Quantity != w.UsedQty).OrderBy(o=> o.InDate).FirstOrDefault()!;

                        if ((_batchNumber.Quantity - _batchNumber.UsedQty) >= (_qtybatch !=  Convert.ToDecimal(data.PickQty!.Value) ? Convert.ToDecimal(data.PickQty!.Value) - _qtybatch : _qtybatch)){

                            _qtybatch = (_qtybatch !=  Convert.ToDecimal(data.PickQty!.Value) ? Convert.ToDecimal(data.PickQty!.Value) - _qtybatch : _qtybatch);
                            checkloop = 1;
                            
                        }else{
                            _qtybatch =  (_batchNumber.Quantity - _batchNumber.UsedQty);
                        }

                        _batchNumber.UsedQty = _batchNumber.UsedQty + _qtybatch ;
                        
                        GIFPD.Lines.BatchNumbers.BatchNumber = _batchNumber.DistNumber;
                        GIFPD.Lines.BatchNumbers.Quantity =  Convert.ToDouble(_qtybatch);
                        GIFPD.Lines.BatchNumbers.Add();

                    } while (checkloop != 1);
                    
                    GIFPD.Lines.Add();

                }
                int res = GIFPD.Add();

                if (res != 0)
                {
                    errorMessage = oCompany.GetLastErrorDescription().ToString(); //@scope_identity
                    oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                    oCompany.Disconnect();
                    return (errorMessage, _issueMaterialH);
                }


                if (oCompany.InTransaction)
                {

                    oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                    string objectKey = oCompany.GetNewObjectKey();

                    foreach(IssueMaterialManual data in _issueMaterialH.IssueMaterialManuals){
                        data.DocEntry = Convert.ToInt32(objectKey);
                        data.DocNum  = SeriesNumber.ToString();
                        data.ConvertSap = 1;
                        data.Status = "Issued";
                        data.IssueQty = data.PickQty;
                    }
                   
                }
                else{
                    oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
                    errorMessage = oCompany.GetLastErrorDescription().ToString(); //@scope_identity  

                }


                oCompany.Disconnect();
                

            }
            catch (Exception ex)
            {
                //ex.ToString();
                errorMessage = ex.Message.ToString();
                oCompany.Disconnect();
            }

            return (errorMessage, _issueMaterialH);

        }

      
        public class SAPConnect
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Server { get; set; }
            public string CompanyDB { get; set; }
            public string DBUsername { get; set; }
            public string DBPassword { get; set; }

        }
    }
}