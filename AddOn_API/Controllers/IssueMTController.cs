using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AddOn_API.DTOs.Picking;
using AddOn_API.Entities;
using AddOn_API.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AddOn_API.DTOs.AllocateLot;
using AddOn_API.DTOs.SAPQuery;

namespace AddOn_API.Controllers;

[Route("api/[controller]")]
public class IssueMTController : ControllerBase
{

    public IAccountService AccountService { get; }
    public ISapSDKService SapSDKService { get; }

    public IIssueMTServices IssueMTServices { get; }

    public IssueMTController(IIssueMTServices issueMTServices, IAccountService accountService, ISapSDKService sapSDKService)
    {
        this.IssueMTServices = issueMTServices;
        this.SapSDKService = sapSDKService;
        this.AccountService = accountService;

    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IssueMaterialHResponse>> FindById(long id)
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");

        IssueMaterialHResponse response = new IssueMaterialHResponse();

        if (accessToken == null)
        {
            return Unauthorized();
        }

        var result = (await IssueMTServices.FindById(id));

        if (result == null)
        {
            return NotFound();
        }
        else
        {
            response = result.Adapt<IssueMaterialHResponse>();
        }

        return StatusCode((int)HttpStatusCode.OK, response);


    }


    [HttpPost("[action]")]
    public async Task<ActionResult<GenerateResponse>> Create(IssueMaterialSearch model)
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");

        IssueMaterialHResponse response = new IssueMaterialHResponse();

        GenerateResponse generateResponse = new GenerateResponse();

        List<GeneratePDResponse> generatePDResponse = new List<GeneratePDResponse>();

        var account = AccountService.GetInfo(accessToken);

        if (accessToken == null)
        {
            return Unauthorized();
        }
        var _issueMTH = await IssueMTServices.FindById(model.Id);

        if (_issueMTH == null)
        {
            return NotFound();
        }


        (string errorMessageVerify,IssueMaterialH issueMaterialH1 )= await IssueMTServices.VerifyDataIssue(_issueMTH);

        if (!string.IsNullOrEmpty(errorMessageVerify)){
            generateResponse.errorMessage = errorMessageVerify;
            generateResponse.referenceNumber = _issueMTH.IssueNumber;

            return StatusCode((int)HttpStatusCode.BadRequest, generateResponse);
        }


        var _productionOrderH = await IssueMTServices.GetProductionH(_issueMTH);

        List<String> _itemCode = new List<string>();

        _itemCode.AddRange(_issueMTH.IssueMaterialDs.Select(s => s.ItemCode).Distinct().ToList());

        var _batchList = await SapSDKService.GetBatNumber(_itemCode);

        if (_batchList.Count() == 0)
        {

            generateResponse.errorMessage = "Can't find batchNumber";
            generateResponse.referenceNumber = _issueMTH.IssueNumber;

            return StatusCode((int)HttpStatusCode.BadRequest, generateResponse);
        }

        try
        {

            (string errorMessage, IssueMaterialH issueMaterialH, List<BatchNumber> batchNumbers) = await SapSDKService.ConvertIssueMaterial(_issueMTH, _productionOrderH.ToList(), _batchList.ToList());

            if (string.IsNullOrEmpty(errorMessage))
            {
                _issueMTH = issueMaterialH;

                _issueMTH.UpdateBy = account.EmpUsername;
                _issueMTH.UpdateDate = System.DateTime.Now;
                _issueMTH.IssueBy = account.EmpUsername;
                _issueMTH.IssueDate = System.DateTime.Now;


                foreach (IssueMaterialD isd in _issueMTH.IssueMaterialDs)
                {
                    isd.IssueQty = isd.PlandQty;
                    isd.UpdateBy = account.EmpUsername;
                    isd.UpdateDate = System.DateTime.Now;
                    isd.Status = "Issued";

                }

                if (_issueMTH.IssueMaterialManuals.Count > 0 && string.IsNullOrEmpty(errorMessage))
                {


                    (string errorMessageML, IssueMaterialH issueMaterialHML) = await SapSDKService.ConvertIssueMaterialML(_issueMTH, batchNumbers);

                    if (string.IsNullOrEmpty(errorMessageML))
                    {

                        _issueMTH = issueMaterialHML;

                        foreach (IssueMaterialManual ism in _issueMTH.IssueMaterialManuals)
                        {
                            ism.UpdateBy = account.EmpUsername;
                            ism.UpdateDate = System.DateTime.Now;
                        }

                    }

                    generatePDResponse.Add( new GeneratePDResponse{
                        errorMessage = errorMessageML,
                        itemCode =  string.Join(",", issueMaterialHML.IssueMaterialManuals.Select(s => s.ItemCode).ToList()),
                        id = issueMaterialHML.Id,

                    });
                  
                }

                ////update issue

                IssueMaterialLog _newLog = new IssueMaterialLog
                {
                    IssueHid = _issueMTH.Id,
                    Users = account.EmpUsername,
                    Status = _issueMTH.Status,
                    LogDate = System.DateTime.Now,
                    Levels = _issueMTH.IssueMaterialLogs.OrderByDescending(o => o.Levels).Select(s => s.Levels).FirstOrDefault(),
                    Comment = "Tranfer Data",
                    Action = "Update",
                    ClientName = "Web"
                };

                await IssueMTServices.Update(_issueMTH, _newLog);

                generateResponse.errorMessage = "OK";
                generateResponse.referenceNumber = _issueMTH.IssueNumber!;
                generateResponse.generatePD = generatePDResponse;

             

                

                return StatusCode((int)HttpStatusCode.OK, generateResponse);

            }
            else
            {
                generateResponse.errorMessage = errorMessage;
                generateResponse.referenceNumber = _issueMTH.IssueNumber!;
                generateResponse.generatePD = generatePDResponse;


                return StatusCode((int)HttpStatusCode.BadRequest, generateResponse);
            }


        }
        catch (Exception ex)
        {

            return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
        }



    }


    [HttpDelete("{id}")]
    public async Task<ActionResult<GenerateResponse>> DeleteIssueById(int id)
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");

        IssueMaterialHResponse response = new IssueMaterialHResponse();

        GenerateResponse generateResponse = new GenerateResponse();

        List<GeneratePDResponse> generatePDResponse = new List<GeneratePDResponse>();

        var account = AccountService.GetInfo(accessToken);

        if (accessToken == null)
        {
            return Unauthorized();
        }
        var _issueMTH = await IssueMTServices.FindById(id);

        if (_issueMTH == null)
        {
            return NotFound();
        }

        (string errorMessage,IssueMaterialH issueMaterialH1 )= await IssueMTServices.VerifyDeleteIssue(_issueMTH);

        if (!string.IsNullOrEmpty(errorMessage)){
            generateResponse.errorMessage = errorMessage;
            generateResponse.referenceNumber = _issueMTH.IssueNumber;

            return StatusCode((int)HttpStatusCode.BadRequest, generateResponse);
        }

        try
        {

          

            if (string.IsNullOrEmpty(errorMessage)){

                _issueMTH.Status = "Delete";
                _issueMTH.UpdateBy = account.EmpUsername;
                _issueMTH.UpdateDate = System.DateTime.Now;

                foreach(IssueMaterialD item in _issueMTH.IssueMaterialDs){
                    item.Status = "D";
                    item.UpdateBy = account.EmpUsername;
                    item.UpdateDate = System.DateTime.Now;
                }

                 foreach(IssueMaterialManual item in _issueMTH.IssueMaterialManuals){
                    item.Status = "D";
                    item.UpdateBy = account.EmpUsername;
                    item.UpdateDate = System.DateTime.Now;
                }


                IssueMaterialLog _newLog = new IssueMaterialLog
                {
                    IssueHid = _issueMTH.Id,
                    Users = account.EmpUsername,
                    Status = _issueMTH.Status,
                    LogDate = System.DateTime.Now,
                    Levels = _issueMTH.IssueMaterialLogs.OrderByDescending(o => o.Levels).Select(s => s.Levels).FirstOrDefault(),
                    Comment = "Delete data",
                    Action = "Update",
                    ClientName = "Web"
                };



                await IssueMTServices.Update(_issueMTH, _newLog);

                generateResponse.errorMessage = "OK";
                generateResponse.referenceNumber = _issueMTH.IssueNumber!;
                generateResponse.generatePD = generatePDResponse;

             


                return StatusCode((int)HttpStatusCode.OK, generateResponse);
            }
            else
            {
                generateResponse.errorMessage = errorMessage;
                generateResponse.referenceNumber = _issueMTH.IssueNumber!;
                 generateResponse.generatePD = generatePDResponse;


                return StatusCode((int)HttpStatusCode.BadRequest, generateResponse);
            }


        }
        catch (Exception ex)
        {

            return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
        }

    
        
    }
    


    
    [HttpPost("[action]")]
    public async Task<ActionResult<IEnumerable<IssueMTGroupD>>> GetissueMTListD(IssueMaterialSearch mdata)
    {
         var accessToken = await HttpContext.GetTokenAsync("access_token");

        List<IssueMTGroupD> response = new List<IssueMTGroupD>();

        if (accessToken == null)
        {
            return Unauthorized();
        }

        var result = (await IssueMTServices.GetissueMTListD(mdata.Id));

        if (result == null)
        {
            return NotFound();
        }
        else
        {
            response = result.Adapt<List<IssueMTGroupD>>();
        }


        


        return StatusCode((int)HttpStatusCode.OK, response);

        

        
        // TODO: Your code here
       
    }
    


}
