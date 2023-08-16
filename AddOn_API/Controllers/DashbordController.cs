using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AddOn_API.DTOs.Dashboard;
using AddOn_API.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
//using AddOn_API.Models;

namespace AddOn_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashbordController : ControllerBase
    {
        private readonly ISaleOrderService saleOrderService;
        private readonly IAllocateService allocateService;
        private readonly IAccountService accountService;
        public DashbordController(ISaleOrderService saleOrderService, IAllocateService allocateService, IAccountService accountService)
        {
            this.accountService = accountService;
            this.allocateService = allocateService;
            this.saleOrderService = saleOrderService;
        }

        [HttpGet("")]
        public async Task<ActionResult<DashboardCard>> GetDashboardCard()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            string access = (await accountService.CheckPermissionAccess(accessToken, "Allocate"));
            if (access == "Null")
                return Unauthorized();

            var _saleorder = (await saleOrderService.FindAll());

            var _allocatelot = (await allocateService.FindAll());
            // _allocatelot.Where(w=> w.GenerateMc == 0).Count();

            DashboardCard _data = new DashboardCard
            {
                Id = 0,
                DraftSO = _saleorder.Where(w => w.DocStatus == "D").Count(),
                WTConvertSO = _saleorder.Where(w => w.ConvertSap == 0).Count(),
                WTAllocate = _saleorder.Where(w => w.GenerateLot == 0).Count(),
                Allocate = _allocatelot.Count(),
                WTGenMC = _allocatelot.Where(w => w.GenerateMc == 0).Count(),
                WTGenPD = _allocatelot.Where(w => w.GeneratePd == 0).Count(),
                WTConvertPD = _allocatelot.Where(w => w.GenerateMc == 1).Count(),
                WTReleasedPD = _allocatelot.Where(w => w.GenerateMc == 2).Count()
            };

            return _data;

        }


    }
}