using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AddOn_API.Interfaces;
using AddOn_API.DTOs.AllocateLot;
using Microsoft.AspNetCore.Authentication;
using AddOn_API.Services;
using System.Net;
using Mapster;
using AddOn_API.DTOs.Picking;
using AddOn_API.Entities;
using AddOn_API.DTOs.ReqIssueMT;

namespace AddOn_API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PickingController : ControllerBase
{
    public IPickingMTService PickingMTService { get; }
    public IAccountService AccountService { get; }
    public ISapSDKService SapSDKService { get; }
    public PickingController(IPickingMTService pickingMTService, IAccountService accountService, ISapSDKService sapSDKService)
    {
        this.SapSDKService = sapSDKService;
        this.AccountService = accountService;
        this.PickingMTService = pickingMTService;
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<IEnumerable<AllocateLotResponse>>> GetLotForPicking(AllocateLotRequest model)
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");

        if (accessToken == null)
        {
            return Unauthorized();
        }
        var account = AccountService.GetInfo(accessToken);


        var result = (await PickingMTService.GetLotForPicking(model));

        if (result.Count() == 0)
        {
            return StatusCode((int)HttpStatusCode.NotFound, "Data is not Found");
        }
        else
        {
            return StatusCode((int)HttpStatusCode.OK, result.Adapt<List<AllocateLotResponse>>());
        }

    }

    [HttpPost("[action]")]
    public async Task<ActionResult<IEnumerable<PickingItemH>>> GetItemForLotPicking(List<AllocateLotRequest> model)
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");

        if (accessToken == null)
        {
            return Unauthorized();
        }
        var account = AccountService.GetInfo(accessToken);

        var result = (await PickingMTService.GetItemForPickg(model));

        if (result.Count() == 0)
        {
            return StatusCode((int)HttpStatusCode.NotFound, "Data is not Found");
        }
        else
        {
            return StatusCode((int)HttpStatusCode.OK, result);
        }

    }


    [HttpPost("[action]")]
    public async Task<ActionResult<IssueMaterialHResponse>> CreatePicking(IssueMaterialHResponse model)
    {

        var accessToken = await HttpContext.GetTokenAsync("access_token");

        if (accessToken == null)
        {
            return Unauthorized();
        }
        var account = AccountService.GetInfo(accessToken);

        List<ReqIssueMTResponse> respon = new List<ReqIssueMTResponse>();
        try
        {

            var IssueNumber = await PickingMTService.GetIssueNumber();

            (string errorMessage, List<IssueMaterialD> issueMaterialDs) = await PickingMTService.CheckPickingDetail(model.IssueMaterialDs.Adapt<List<IssueMaterialD>>());

            if (!string.IsNullOrEmpty(errorMessage))
            {

                return StatusCode((int)HttpStatusCode.BadRequest, errorMessage);

            }

            model.IssueNumber = IssueNumber;
            model.PickingBy = account.EmpUsername;
            model.PickingDate = System.DateTime.Now;
            model.CreateBy = account.EmpUsername;
            model.CreateDate = System.DateTime.Now;
            model.UpdateBy = null;
            model.UpdateDate = null;


            await PickingMTService.Create(model.Adapt<IssueMaterialH>());


            return StatusCode((int)HttpStatusCode.Created, model);
        }
        catch (Exception ex)
        {

            return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
        }

    }



    [HttpPost("[action]")]
    public async Task<ActionResult<IssueMaterialHResponse>> GetIssueList(IssueMaterialSearch model)
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");

        if (accessToken == null)
        {
            return Unauthorized();
        }
        var account = AccountService.GetInfo(accessToken);

        try
        {
            var result = await PickingMTService.Search(model);

            if (result.Count() == 0)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Data is not Found.");
            }

            return StatusCode((int)HttpStatusCode.OK, result.Adapt<List<IssueMaterialHResponse>>());
        }
        catch (Exception ex)
        {

            return StatusCode((int)HttpStatusCode.BadRequest, ex.ToString());
        }

        
    }



}
